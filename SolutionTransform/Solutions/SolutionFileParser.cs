using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SolutionTransform.Files;

namespace SolutionTransform.Solutions {
    public class SolutionFileParser {
        private readonly IFileStorage fileSystem;

        public SolutionFileParser(IFileStorage fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public SolutionFile Parse(FilePath solutionPath, IEnumerable<string> lines) {
            Func<string, bool> chapter = l => l.StartsWith("Project") || l.StartsWith("Global");
            var chapterLines = Split(lines, chapter).ToList();
            var preamble = chapterLines[0];
            chapterLines.RemoveAt(0);
            return new SolutionFile(solutionPath, fileSystem, preamble, chapterLines.Select(l => ParseChapter(solutionPath, l)));

        }

        SolutionChapter ParseChapter(FilePath basePath, List<string> lines) {
            SolutionChapter result = null;
            var start = lines[0];
            var end = lines[lines.Count - 1];

            if (start.StartsWith("Project")) {
                result = new SolutionProject(start, end, basePath, fileSystem);
            } else if (start.StartsWith("Global")) {
                result = new GlobalChapter();
            } else {
                result = new SolutionChapter(start, end);  // Unknown, just keep it
            }
            var sectionLines = Split(Inner(lines), l => l[1] != 'E' && l[1] != '\t');
            foreach (var section in sectionLines.Select(z => ParseSection(z, result))) {
                result.Sections.Add(section);
            }
            return result;
        }

        IEnumerable<string> Inner(List<string> outer) {
            return outer.Skip(1).Take(outer.Count - 2);
        }

        Regex sectionStartRegex = new Regex(@"(\w+)[(]([^)]+)[)]\s+=\s+(\w+)");

        SolutionSection CreateSection(string sectionType, SolutionChapter solutionChapter)
        {
            if (sectionType == "ProjectConfigurationPlatforms")
            {
                return new ProjectConfigurationPlatforms((GlobalChapter)solutionChapter);
            }
            if (sectionType == "NestedProjects")
            {
                return new NestedProjects((GlobalChapter)solutionChapter);
            }
            return new SolutionSection();
        }

        SolutionSection ParseSection(List<string> lines, SolutionChapter solutionChapter) {
            
            var match = sectionStartRegex.Match(lines[0]);
            var sectionType = match.Groups[2].Value;
            var result = CreateSection(sectionType, solutionChapter);
            result.ChapterSectionType = match.Groups[1].Value;
            result.SectionType = sectionType;
            result.Position = match.Groups[3].Value;
            foreach (var line in Inner(lines)) {
                var items = line.Split('=');
                result.Values.Add(new KeyValuePair<string, string>(items[0].Trim(), items[1].Trim()));
            }
            return result;
        }

        IEnumerable<List<TValue>> Split<TValue>(IEnumerable<TValue> values, Func<TValue, bool> isStart) {
            List<TValue> result = new List<TValue>();
            foreach (var value in values) {
                if (isStart(value)) {
                    if (result.Count > 0) {
                        yield return result;
                    }
                    result = new List<TValue>();
                }
                result.Add(value);
            }
            if (result.Count > 0) {
                yield return result;
            }
        }

    }

    internal class GlobalChapter : SolutionChapter {
        public GlobalChapter()
            : base("Global", "EndGlobal") {

        }

        public IEnumerable<string> SolutionConfigurationPlatforms {
            get {
                return Sections
                       .Where(s => s.SectionType == "SolutionConfigurationPlatforms")
                       .SelectMany(s => s.Keys());
            }
        }

        public ProjectConfigurationPlatforms ProjectConfigurationPlatforms {
            get {
                return Sections.OfType<ProjectConfigurationPlatforms>().Single();
            }
        }

        public NestedProjects NestedProjects
        {
            get
            {
                return Sections.OfType<NestedProjects>().Single();
            }
        }
    }

    class ProjectConfigurationPlatforms : SolutionSection {
        private readonly GlobalChapter globalChapter;

        public ProjectConfigurationPlatforms(GlobalChapter globalChapter) {
            this.globalChapter = globalChapter;
        }

        public void Add(SolutionProject project) {
            foreach (var platform in globalChapter.SolutionConfigurationPlatforms) {
                AddValue(project, platform, "ActiveCfg");
                AddValue(project, platform, "Build.0");
            }
        }

        public void Remove(SolutionProject project)
        {
            var regex = new Regex(Regex.Escape(project.Id.ToString()), RegexOptions.IgnoreCase);
            foreach (var value in Values.ToList().Where(v => regex.IsMatch(v.Key)))
            {
                Values.Remove(value);
            }
        }

        void AddValue(SolutionProject project, string platform, string build) {
            Values.Add(new KeyValuePair<string, string>(string.Format("{0}.{1}.{2}", project, platform, build), platform));
        }
    }

    class NestedProjects : SolutionSection
    {
        private readonly GlobalChapter globalChapter;

        public NestedProjects(GlobalChapter globalChapter)
        {
            this.globalChapter = globalChapter;
        }

        public void Remove(SolutionProject project)
        {
            var regex = new Regex(Regex.Escape(project.Id.ToString()), RegexOptions.IgnoreCase);
            foreach (var value in Values.ToList().Where(v => regex.IsMatch(v.Key)))
            {
                Values.Remove(value);
            }
        }
    }
}
