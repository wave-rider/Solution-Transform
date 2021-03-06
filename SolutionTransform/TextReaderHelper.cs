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

using System;

namespace SolutionTransform
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static class TextReaderHelper
	{
		public static IEnumerable<string> AsLines(this string content) {
			using (var reader = new StringReader(content)) {
				return reader.AsLines().ToList();
			}
		}

		public static IEnumerable<string> AsLines(this TextReader reader) {
			string line;
			while (null != (line = reader.ReadLine())) {
				yield return line;
			}
		}

		public static string FileContent(this FilePath path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			using (var stream = new StreamReader(path.Path)) {
				return stream.ReadToEnd();
			}
		}
	}
}
