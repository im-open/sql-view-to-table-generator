using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using viewObjectBuilder.Data;


namespace viewObjectBuilder.Configuration
{
    [HelpOption,
     Command(Description = "Build ORM project to access the table objects created by this tool.")]
    public class BuildOrmConfiguration : DataBaseSourceConfiguration
    {
        public BuildOrmConfiguration() { }

        public BuildOrmConfiguration(ISchemaRepository schemaRepository, IDbQuery dbQuery) :
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
                var ormTablePath = Path.Combine(basePath, "tables");
                var ormPropertiesPath = Path.Combine(basePath, "properties");
                var folders = new[] { basePath, ormTablePath, ormPropertiesPath };
                var writeView = new Action<ViewDefinition>(view =>
                {
                    var path = Path.Combine(ormTablePath, $"{view.Name}.cs");
                    File.WriteAllText(path, view.ClassText);
                });

                BuildFiles(Schema, folders, writeView);
                WriteOrmProjectFiles(this, basePath, ormPropertiesPath);
                resultMessages.Add("ORM files built.");
                resultCode = 0;
            }
            catch (Exception exc)
            {
                resultMessages.Add("Error building orm files.");
                resultMessages.Add(exc.ToString());
            }
            finally
            {
                Configuration.ProcessResults = resultMessages;
            }

            return resultCode;
        }

        public void WriteOrmProjectFiles(BuildOrmConfiguration config, string projectPath, string propertiesPath)
        {
            File.WriteAllText(Path.Combine(projectPath, $"{config.Schema}.csproj"), OrmProjectTools.OrmProjectFile);
            File.WriteAllText(Path.Combine(projectPath, "DbQuery.cs"),
                OrmProjectTools.DbConnectionFile(config.Schema));
            File.WriteAllText(Path.Combine(propertiesPath, "AppSettings.cs"),
                OrmProjectTools.AppSettingsClassFile(config.Schema));
            File.WriteAllText(Path.Combine(propertiesPath, "appSettings.json"), OrmProjectTools.AppSettingsFile);
        }
    }
}
