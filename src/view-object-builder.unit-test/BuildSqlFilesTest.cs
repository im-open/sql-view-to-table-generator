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
    //-v true
    //-m Metadata

    public class BuildSqlFilesTest : IDisposable
    {
        private BuildSqlConfiguration _buildSql;
        private readonly string _outputDirectory;

        public BuildSqlFilesTest()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            _outputDirectory = Path.Combine(currentDirectory, "testSql");
        }

        [Theory,
         InlineData(1),
         InlineData(2),
         InlineData(5)]
        public void builds_files_properly_for(int viewCount)
        {
            var testDirectory = Path.Combine(_outputDirectory, CompareFilesTest.uniqueString(10));

            try
            {
                var returnResults = new List<SqlViewColumn>();
                for (int i = 0; i < viewCount; i++)
                {
                    returnResults.Add(DbQueryTest.randomColumn($"proper_{CompareFilesTest.uniqueString(10)}_{i}"));
                }

                var dbQuery = Substitute.For<IDbQuery>();
                dbQuery
                    .Query<SqlViewColumn>(Arg.Any<string>())
                    .Returns(returnResults);

                var schemaRepository = new SchemaRepository(dbQuery);

                _buildSql = new BuildSqlConfiguration(schemaRepository, dbQuery)
                {
                    Schema = "testSql",
                    OutputFolder = testDirectory,
                };

                var files = returnResults
                    .Select(r => r.VIEW)
                    .Distinct()
                    .Select(v =>
                        Path.Combine(_buildSql.OutputFolder,
                            $"{_buildSql.Schema}.{v}.1.0.sql"))
                    .ToArray();

                var result = _buildSql.OnExecute();

                //folder exits
                Directory.Exists(_buildSql.OutputFolder)
                    .ShouldBe(true, "Output folder does not exist.");

                //all view to tables files should exist
                //Have to do this with a for loop,
                // because when I do it with  a single statement
                // the unit test gets locked up on going through each file.
                var allExists = true;
                foreach (var file in files)
                {
                    if (!File.Exists(file))
                        allExists = false;
                }

                allExists.ShouldBe(true, "All view files should exist.");

                result.ShouldBe(0);
            }
            finally
            {
                if(Directory.Exists(testDirectory))
                    Directory.Delete(testDirectory, true);
            }
        }

        [Fact]
        public void builds_files_throws_exception()
        {
            var dbQuery = Substitute.For<IDbQuery>();
            dbQuery
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Throws<ApplicationException>();

            var schemaRepository = new SchemaRepository(dbQuery);

            _buildSql = new BuildSqlConfiguration(schemaRepository, dbQuery)
            {
                Schema = "testSql",
                OutputFolder = _outputDirectory,
            };

            try
            {
                var result = _buildSql.OnExecute();

                result.ShouldBe(1,
                    "Building files should have failed before any files have been written.");

                //folder exits
                Directory.Exists(_buildSql.OutputFolder)
                    .ShouldBe(true, "Output folder does not exist.");
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            if (Directory.Exists(_outputDirectory))
                Directory.Delete(_outputDirectory, true);
        }
    }
}