using SolutionTransform.Files;
using SolutionTransform.Solutions;
using System.Collections.Generic;

namespace SolutionTransform.Api17
{
    public class RemoveProjectFromSolution
    {
        IFileStorage _fileStorage;
        string _solutionFilePath;

        public RemoveProjectFromSolution(IFileStorage fileStorage, 
            string solutionFilePath)
        {
            _fileStorage = fileStorage;
            _solutionFilePath = solutionFilePath;
        }

        public void Execute()
        {
            var api10 = new Api10.Api(null, _fileStorage);
            var parser = new SolutionFileParser(_fileStorage);
            var transformer = api10.GetSolutionFileTransformer(_solutionFilePath);
            transformer.Transform(
                 api10.StandardRename("-Modified"),
                 api10.Modify(new List<string>(), new List<string> { "Pdf" }),
                 api10.RebaseAssemblies(transformer, @"Pdf\Pdf.csproj"));
        }
    }
}
