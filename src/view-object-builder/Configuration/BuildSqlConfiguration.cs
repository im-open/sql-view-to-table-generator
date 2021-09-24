using McMaster.Extensions.CommandLineUtils;
using viewObjectBuilder.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace viewObjectBuilder.Configuration
{
    [HelpOption,
     Command(Description = "Build View_To_Sql sql files.")]
    public class BuildSqlConfiguration : DataBaseSourceConfiguration
    {
        [Option("-m|--MetaData",
            "The name of the branch and SHA hash key being processed. No value will default to main.",
            CommandOptionType.SingleValue)]
        public string MetaData { get; }

        public BuildSqlConfiguration() { }

        public BuildSqlConfiguration(ISchemaRepository schemaRepository, IDbQuery dbQuery) :
            base(schemaRepository, dbQuery)
        { }

        public int OnExecute()
        {
            var resultCode = 1;
            var resultMessages = new List<string>();
            try
            {
                Configuration.Verbose = Verbose;
                var basePath = OutputFolder;
                var folders = new string[] { basePath };
                var branchName = (string.IsNullOrWhiteSpace(MetaData) || MetaData.ToLower() == "main")
                    ? string.Empty
                    : $"-{MetaData}";
                var writeView = new Action<ViewDefinition>(view =>
                {
                    var packageId = $"{Schema}.{view.Name}.{view.Version ?? "1.0"}{branchName}";

                    File.WriteAllText(Path.Combine(basePath, $"{packageId}.sql"),
                        DbSqlTools.BuildSql(Schema, view.Name, view.Version, view.SqlText), Encoding.UTF8);
                });

                BuildFiles(Schema, folders, writeView);
                resultMessages.Add("SQL files built.");
                resultCode = 0;
            }
            catch (Exception exc)
            {
                resultMessages.Add("Error building sql files.");
                resultMessages.Add(exc.ToString());
            }
            finally
            {
                Configuration.ProcessResults = resultMessages;
            }

            return resultCode;
        }
    }
}
