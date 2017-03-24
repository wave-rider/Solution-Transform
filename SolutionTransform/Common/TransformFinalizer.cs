using SolutionTransform.Api10;
using SolutionTransform.Model;
using SolutionTransform.ProjectFile;
using SolutionTransform.Solutions;
using System;
using System.Linq;

namespace SolutionTransform.Common
{
    public class TransformFinalizer
    {
        SolutionFile _solutionFile;
        public TransformFinalizer(SolutionFile solutionFile)
        {
            _solutionFile = solutionFile;
        }

        public void FinalizeTransform(IRename rename)
        {
            ApplyTransform(new NameTransform(rename));  // Rename the projects and solution
            SaveProjectsAndSolution(rename);
        }
        
        void ApplyTransform(ITransform transform)
        {
            var command = Api.TransformCommand(transform);
            command.Process(_solutionFile);
        }

        private void SaveProjectsAndSolution(IRename rename)
        {
            foreach (var project in _solutionFile.Projects.Where(p => !p.IsFolder))
            {
                project.Name = rename.RenameSolutionProjectName(project.Name);
                project.Path = new FilePath(rename.RenameCsproj(project.Path.Path), false);
                // Note that project.Path and project.XmlFile.Path have different values....
                var from = project.XmlFile.Path.Path;
                var to = rename.RenameCsproj(from);
                project.XmlFile.Save(to);
                Duplicate(from, to, ".cspscc");
                Duplicate(from, to, ".user");

            }
            var destination = rename.RenameSln(_solutionFile.FullPath.Path);
            var filePath = new FilePath(destination, false, true);
            _solutionFile.Save(filePath);
        }

        private void Duplicate(string from, string to, string extension)
        {

            string fromWithExtension = from + extension;
            var toWithExtension = to + extension;
            if (StringComparer.InvariantCultureIgnoreCase.Equals(fromWithExtension, toWithExtension))
            {
                return;
            }
            if (System.IO.File.Exists(fromWithExtension))
            {
                System.IO.File.Copy(fromWithExtension, toWithExtension, true);
            }
        }
    }
}
