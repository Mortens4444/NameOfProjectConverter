using System;
using System.IO;
using System.Linq;

namespace NameOfProjectConverter
{
    class MainClass
    {
        public static void Main(string[] args)
        {
			if (args.Length != 1)
            {
                throw new NotSupportedException("Provide a folder path. For example: C:\\Projects\\MyApplication");
            }

            var folder = args.First();
            if (!Directory.Exists(folder))
            {
                throw new NotSupportedException($"Folder not found: {folder}");
            }

			var sourceFileNames = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
			foreach (var sourceFileName in sourceFileNames)
            {
				var fileContent = File.ReadAllText(sourceFileName);
				var fixedFileContent = FixFileContent(fileContent);
				if (fileContent != fixedFileContent)
				{
					File.WriteAllText(sourceFileName, fixedFileContent);
					Console.WriteLine($"File fixed: {sourceFileName}");
				}
            }

			Console.ReadKey();
        }

		private static string FixFileContent(string fileContent)
		{
			var fixableExceptions = new[] { nameof(ArgumentException), nameof(ArgumentNullException) };
			foreach (var fixableException in fixableExceptions)
			{
				int index;

				while ((index = fileContent.IndexOf($"{fixableException}(\"", StringComparison.Ordinal)) > -1)
				{
					var startIndex = index + fixableException.Length + 2;
					var endIndex = fileContent.IndexOf('"', startIndex);
					var lineEndIndex = fileContent.IndexOf(';', startIndex);
                    var commaIndex = fileContent.IndexOf(',', startIndex);

					if (commaIndex != -1 && commaIndex < lineEndIndex)
					{
						if (fixableException == nameof(ArgumentException))
						{
							var message = fileContent.Substring(startIndex, endIndex - startIndex);
							startIndex = fileContent.IndexOf('"', commaIndex + 1) + 1;
							endIndex = fileContent.IndexOf('"', startIndex + 1);

							if (endIndex != -1)
							{
								fileContent = FixArgumentException(fileContent, fixableException, startIndex, endIndex, message);
							}

							break;
						}

						fileContent = FixArgumentNullException(fileContent, fixableException, startIndex, endIndex);
					}
					else
					{
						fileContent = FixArgumentNullException(fileContent, fixableException, startIndex, endIndex);
					}
				}
			}

			return fileContent;
		}

		private static string FixArgumentException(string fileContent, string fixableException, int startIndex, int endIndex, string message)
		{
			var parameterName = fileContent.Substring(startIndex, endIndex - startIndex);
			return fileContent.Replace(
				$"{fixableException}(\"{message}\", {parameterName}\")",
				$"{fixableException}(\"{message}\", nameof({parameterName}))");
		}

		private static string FixArgumentNullException(string fileContent, string fixableException, int startIndex, int endIndex)
        {
            var parameterName = fileContent.Substring(startIndex, endIndex - startIndex);
            return fileContent.Replace(
                $"{fixableException}(\"{parameterName}\")",
                $"{fixableException}(nameof({parameterName}))");
        }
	}
}
