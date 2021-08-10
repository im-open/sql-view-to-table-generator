using System;
using System.IO;
using System.Linq;
using System.Text;
using viewObjectBuilder.Configuration;
using Shouldly;
using Xunit;
namespace viewObjectBuilder.unitTest
{
    public class CompareFilesTest : IDisposable
    {
        private readonly CompareFilesConfiguration _compareFiles;
        private readonly string _compareDirectory;
        private static readonly Random _random = new Random();

        private static char[] _alphaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static char[] _characters;
        public static char[] characters
        {
            get
            {
                if (_characters == null)
                {
                    var chars = _alphaChars.ToList();
                    chars.Add('\n');
                    _characters = chars.ToArray();
                }

                return _characters;
            }
        }

        public CompareFilesTest()
        {
            _compareFiles = new CompareFilesConfiguration();
            _compareDirectory = Path.Combine(Directory.GetCurrentDirectory(), "compareFiles");
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public void two_files_compared(bool shouldBeTheSame)
            => multiple_files_compared(2, uniqueString(_alphaChars, 10), shouldBeTheSame,
                result => result.ShouldBe(shouldBeTheSame ? 0: 1));

        [Fact]
        public void two_files_missing_second()
        {
            var filePath1 = Path.Combine(_compareDirectory, $"fileMissing_{uniqueString(5)}_1.txt");
            var filePath2 = Path.Combine(_compareDirectory, $"fileMissing_{uniqueString(5)}_2.txt");
            buildTempFile(filePath1, null);
            _compareFiles.Files = new[] {filePath1, filePath2};

            var result = _compareFiles.OnExecute();
            result.ShouldBe(1);
            File.Delete(filePath1);
        }

        [Theory,
         InlineData(0),
         InlineData(1),
         InlineData(3)]
        public void compare_files_count_for(int fileCount)
            => multiple_files_compared(fileCount, uniqueString(10), false, result => result.ShouldBe(1));
        
        private void multiple_files_compared(
            int fileCount,
            string uniqueString,
            bool shouldBeTheSame,
            Action<object> shouldBe)
        {
            var files = Enumerable.Range(1, fileCount)
                .Select(i => Path.Combine(_compareDirectory, $"file_{uniqueString}_{i}.txt"))
                .ToArray();
            try
            {
                var text = (string)null;
                foreach (string file in files)
                {
                    text = buildTempFile(file, shouldBeTheSame ? text : null);
                }
                
                _compareFiles.Files = files ;
                var result = _compareFiles.OnExecute();
                shouldBe(result);
            }
            finally
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }

        public static string uniqueString(int length)
            => uniqueString(_alphaChars, length);

        public static  string uniqueString(char[] charSet, int length)
        {
            var textBody = new StringBuilder(length);
            textBody.Append(
                string.Join(string.Empty, Enumerable.Range(0, length)
                        .Select(c => charSet[_random.Next(charSet.Length)])));

            return textBody.ToString();
        }

        private string buildTempFile(string path, string text = null)
        {
            var fileLength = _random.Next(1_000);
            var fileText = text ?? uniqueString(characters, fileLength);
            if (!Directory.Exists(_compareDirectory)) Directory.CreateDirectory(_compareDirectory);
            File.WriteAllText(path, fileText);
            return  fileText;
        }

        public void Dispose()
        {
            if (Directory.Exists(_compareDirectory))
                Directory.Delete(_compareDirectory, true);
        }
    }
}
