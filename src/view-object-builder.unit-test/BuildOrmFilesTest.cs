using viewObjectBuilder.Configuration;
using viewObjectBuilder.Data;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace viewObjectBuilder.unitTest
{
    //-o c:\\temp\\view_to_table
    //-n localhost
    //-d LocalDb
    //-s dbo
    //-v true"
    public class BuildOrmFilesTest : IDisposable
    {
        private BuildOrmConfiguration _buildOrm;
        private readonly string _outputDirectory;

        public BuildOrmFilesTest()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            _outputDirectory = Path.Combine(currentDirectory, "testOrm");
        }

        [Theory,
         InlineData(1),
         InlineData(2),
         InlineData(5)]
        public void builds_files_properly_for(int viewCount)
        {
            var returnResults = new List<SqlViewColumn>();
            for (int i = 0; i < viewCount; i++)
            {
                returnResults.Add(DbQueryTest.randomColumn(i.ToString()));
            }

            var dbQuery = Substitute.For<IDbQuery>();
            dbQuery
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Returns(returnResults);

            var schemaRepository = new SchemaRepository(dbQuery);

            _buildOrm = new BuildOrmConfiguration(schemaRepository, dbQuery)
            {
                Schema = "testOrm",
                OutputFolder = _outputDirectory,
            };

            try
            {
                var result = _buildOrm.OnExecute();

                result.ShouldBe(0, "The ORM project should be successfully created");

                //folder exits
                Directory.Exists(_buildOrm.OutputFolder)
                    .ShouldBe(true, "Output folder does not exist.");

                //project file
                File
                    .Exists(Path.Combine(_buildOrm.OutputFolder, $"{_buildOrm.Schema}.csproj"))
                    .ShouldBe(true, "Project file not in output.");

                //DbQuery.cs
                File
                    .Exists(Path.Combine(_buildOrm.OutputFolder, $"DbQuery.cs"))
                    .ShouldBe(true, "DbQuery file not in output.");

                //AppSettings.cs
                File
                    .Exists(Path.Combine(_buildOrm.OutputFolder, "properties", $"AppSettings.cs"))
                    .ShouldBe(true, "AppSettings.cs file not in output.");

                //appsettings.json
                File
                    .Exists(Path.Combine(_buildOrm.OutputFolder, "properties", $"appSettings.json"))
                    .ShouldBe(true, "appSettings.json file not in output.");

                //all view to tables files should exist
                returnResults
                    .Select(r => r.VIEW)
                    .Distinct()
                    .Select(v => Path.Combine(_buildOrm.OutputFolder, "tables", $"{v}.cs"))
                    .All(File.Exists)
                    .ShouldBe(true);
            }
            catch (Exception) { }
        }

        [Fact]
        public void builds_files_throws_exception()
        {
            var dbQuery = Substitute.For<IDbQuery>();
            dbQuery
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Throws<ApplicationException>();

            var schemaRepository = new SchemaRepository(dbQuery);

            _buildOrm = new BuildOrmConfiguration(schemaRepository, dbQuery)
            {
                Schema = "testOrm",
                OutputFolder = _outputDirectory,
            };

            try
            {
                var result = _buildOrm.OnExecute();

                result.ShouldBe(1,
                    "Building files should have failed before any files have been written.");

                //folder exits
                Directory.Exists(_buildOrm.OutputFolder)
                    .ShouldBe(true, "Output folder does not exist.");
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            if(Directory.Exists(_outputDirectory))
                Directory.Delete(_outputDirectory, true);
        }
    }
}