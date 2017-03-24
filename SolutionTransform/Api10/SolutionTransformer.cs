using System;
using System.Collections.Generic;
using System.Linq;
using SolutionTransform.Model;
using SolutionTransform.ProjectFile;
using SolutionTransform.Solutions;
using SolutionTransform.Solutions.Commands;
using SolutionTransform.Common;

namespace SolutionTransform.Api10
{
    public class SolutionTransformer
    {
        private readonly SolutionFile _solutionFile;

        public SolutionTransformer(SolutionFile solutionFile)
        {
            this._solutionFile = solutionFile;
        }

        public void Transform(IRename rename, params ISolutionCommand[] solutionCommands) {
            var originalProjects = _solutionFile.Projects.ToList();
            var solutionCommand = solutionCommands.Length == 1
                ? solutionCommands[0]
                : new CompositeCommand(solutionCommands);
            solutionCommand.Process(_solutionFile);
            SynchronizeProjectReferences(originalProjects);

            new TransformFinalizer(this._solutionFile).FinalizeTransform(rename);           
        }

        private void SynchronizeProjectReferences(IEnumerable<SolutionProject> originalProjects)
        {
            var currentProjects = _solutionFile.Projects.ToList();
            var addedProjects = currentProjects.Except(originalProjects);
            var removedProjects = originalProjects.Except(currentProjects);
            foreach (var project in addedProjects)
            {
                ApplyTransform(new ConvertAssemblyToProject(project));
            }
            foreach (var project in removedProjects)
            {
                ApplyTransform(new ConvertProjectToAssembly(project));
            }
        }

        // Extract it into the class
        void ApplyTransform(ITransform transform)
        {
            var command = Api.TransformCommand(transform);
            command.Process(_solutionFile);
        }

        internal FilePath BasePath
        {
            get
            {
                return _solutionFile.FullPath.Parent;
            }
        }

        internal SolutionFile Solution
        {
            get { return _solutionFile; }
        }
    }
}