using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolutionTransform
{
    [DebuggerDisplay("{path}")]
    public class FilePath : IEquatable<FilePath>
    {
        private readonly string path;
        private readonly bool isDirectory;
        private readonly bool isAbsolute;

		public FilePath(string path, bool isDirectory)
			: this(path, isDirectory, 
			path.Contains(System.IO.Path.VolumeSeparatorChar) || path.StartsWith(System.IO.Path.PathSeparator.ToString()))
        {
            
        }
        public FilePath(string path, bool isDirectory, bool isAbsolute)
        {
            this.path = Normalize(path);
            this.isDirectory = isDirectory;
            this.isAbsolute = isAbsolute;
        }


        public bool IsDirectory
        {
            get { return isDirectory; }
        }

        public string Path {
            get {
                return path;
            }
        }

        public FilePath File(string fileName)
        {
            if (!isDirectory)
            {
                return Parent.File(fileName);
            }
            return new FilePath(System.IO.Path.Combine(path, fileName), false, isAbsolute);
        }

        public FilePath Directory(string directoryName) {
            if (!isDirectory) {
                return Parent.Directory(directoryName);
            }
            return new FilePath(PathCombine(path, directoryName), true, isAbsolute);
        }

        public FilePath ToAbsolutePath(FilePath from) {
            if (isAbsolute)
            {
                return this;
            }
            if (!from.isDirectory)
            {
                return ToAbsolutePath(from.Parent);
            }
            return new FilePath(PathCombine(from.Path, this.Path), this.IsDirectory, true);
        }

		internal static string PathCombine(string path1, string path2)
		{
			return System.IO.Path.Combine(path1, path2);
		}


    	internal static string Normalize(string path)
		{
			var components = path.Split(System.IO.Path.DirectorySeparatorChar).ToList();
			for (int index = 0; index < components.Count; index++)
			{
				if (components[index] == ".." && index > 0)
				{
					components.RemoveRange(index-1, 2);
					index -= 2;
				}
			}
			return string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), components.ToArray());
		}

		// a b .. d
		// a d

        public FilePath PathFrom(FilePath from) {
            if (!from.isDirectory)
            {
                return PathFrom(from.Parent);
            }
            if (isAbsolute)
            {
                return new FilePath(WorkOutRelativePath(from.Path, this.Path, 0), this.IsDirectory, false);
            }
            return this;
        }

        public FilePath Parent
        {
            get
            {
                var parentPath = System.IO.Path.GetDirectoryName(Path);
                if (parentPath == null)
                {
                    return null;
                }
                return new FilePath(parentPath, true, isAbsolute);
            }
        }

        internal bool IsAbsolute
        {
            get { return isAbsolute; }
        }

        static string WorkOutRelativePath(string from, string to, int directoriesUp) {
            var match = Regex.Match(to, Regex.Escape(from));
            if (match == Match.Empty) {
                return WorkOutRelativePath(System.IO.Path.GetDirectoryName(from), to, directoriesUp + 1);
            }
            var result = new string[directoriesUp + 1];
            for (int index = 0; index < directoriesUp; index++) {
                result[index] = "..";
            }
            result[directoriesUp] = to.Substring(match.Length + 1);
            return string.Join("\\", result);
        }

        public bool Equals(FilePath other)
        {
            return other.isDirectory == isDirectory
                && other.isAbsolute == isAbsolute
                && StringComparer.InvariantCultureIgnoreCase.Equals(other.path, path);
        }

        public override bool Equals(object obj) {
            return base.Equals((FilePath) obj);
        }

        public override int GetHashCode() {
            return unchecked(
                isDirectory ? 3 : 0
                + (isAbsolute ? 7 : 0)
                + 13 * StringComparer.InvariantCultureIgnoreCase.GetHashCode(path)
                );
        }
    }
}
