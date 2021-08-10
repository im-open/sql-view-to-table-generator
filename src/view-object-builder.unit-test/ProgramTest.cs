using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using viewObjectBuilder;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace viewObjectBuilder.unitTest
{
    public class ProgramTest
    {
        private readonly string _currentDirectory;

        public ProgramTest() => _currentDirectory = Directory.GetCurrentDirectory();

        [Theory,
         InlineData("test"),
         InlineData("test\\subtest")]
        public void build_directories_creates_single_folder(string folder)
            => build_folders(new string[] { folder });

        [Fact]
        public void build_directories_creates_multiple_folders()
        {
            var testFolderArray = (new string[]
            {
                Path.Combine(_currentDirectory, "test"),
                Path.Combine(_currentDirectory, "test\\subTest"),
            });

            build_folders(testFolderArray);
        }

        private void build_folders(string[] folders)
        {
            try
            {
                Program.BuildDirectories(folders);
                foreach (var folder in folders)
                {
                    Directory.Exists(folder).ShouldBe(true);
                }
            }
            finally
            {
                //clean up folder tests
                var deleteFolders = folders
                    .OrderByDescending(f => f.Split('\\').Count());

                foreach (string deleteFolder in deleteFolders)
                {
                    if(Directory.Exists((deleteFolder)))
                        Directory.Delete(deleteFolder, true);
                }
            }
        }

        [Theory,
         InlineData("a:\\temp")]
        public void does_not_build_folder_for(string directory)
        {
            //It should throw an error, but that's okay
            try
            {
                Program.BuildDirectories(new string[] { directory });
            }
            catch (Exception) { }
            Directory.Exists(directory).ShouldBe(false);
        }

        [Theory,
         InlineData(new string[] { }, "Command successful"),
         InlineData(new string[] { "Processed Request" }, "Results: Processed Request")]
        public void display_results_given_this(string[] messages, string displays)
        {
            var writer = new StringWriter();
            Console.SetOut(writer);
            Program.DisplayResults(writer.WriteLine, messages);
            writer.ToString().ShouldBe($"{displays}\r\n");
        }

        [Fact]
        public void setting_main_to_verbose()
        {
            var results = Program.Main(new string[] {"-v", "true"});
            results.ShouldBe(1);
        }
    }
}
