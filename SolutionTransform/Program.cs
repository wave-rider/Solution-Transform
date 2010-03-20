// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Boo.Lang;
using Boo.Lang.Compiler;
using SolutionTransform.Api10;
using SolutionTransform.Files;
using SolutionTransform.Scripts;
using SolutionTransform.Solutions;

[assembly:InternalsVisibleTo("SolutionTransform.Tests")]

namespace SolutionTransform {
	using System;

	public class Program {
		internal static int Main(IFileStorage fileSystem, string[] args)
		{
			return Main(fileSystem, new LegacyScriptProvider(), args);
		}

		public FilePath ExecutingAssemblyLocation {
			get {
				var executablePath = Assembly.GetExecutingAssembly().CodeBase;
				return ToLocal(executablePath);
			}
		}

		static FilePath ToLocal(string uriStyle) {
			string result = uriStyle.Replace("/", "\\");
			return new FilePath(result.Substring(6), true);
		}

        internal static int Main(IFileStorage fileSystem, IScriptProvider provider,  string[] args)
        {
			if (fileSystem == null) {
				throw new ArgumentNullException("No file system was provided.", "fileSystem");
			}
			if (args == null) {
				throw new ArgumentNullException("No args were provided.", "args");
			}
			if (args.Length == 0) {
                Console.WriteLine("Usage:\tSolutionTransform <Script Name or Path> <Other Arguments>");
                Console.WriteLine("\t\tSolutionTransform <Script Name or Path> --help");
                Console.WriteLine("\t\t\tfor script argument help");
                Console.WriteLine("\t\t\tNote that paths nearly always have to be absolute.");
                return 0;
            }
        	var file = provider.FindScript(args[0]);
			if (file == null)
			{
				Console.WriteLine("Could not find script named '{0}'.", args[0]);
				Console.WriteLine("Valid scripts:");
				foreach (var script	in provider.AllScripts)
				{
					Console.WriteLine("\t{0:20}{1}", script.Name, script.Location);
				}
			}
            return file.Execute(args, fileSystem);
        }

	    public static int Main(string[] args)
		{
			return Main(new FileSystem(), new LegacyScriptProvider(), args);
		}

        

	    

	    
	}
}
