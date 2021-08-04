using System;
using System.ComponentModel.DataAnnotations;
using viewObjectBuilder.Data;
using McMaster.Extensions.CommandLineUtils;

namespace viewObjectBuilder.Configuration
{
    public class DataBaseSourceConfiguration : IVerbose
    {
        private ISchemaRepository _schemaRepository;

        public ISchemaRepository SchemaRepository
            => _schemaRepository ?? (_schemaRepository = new SchemaRepository(DbQuery));

        private IDbQuery _dbQuery;
        public IDbQuery DbQuery
            => _dbQuery ?? (_dbQuery = new DbQuery(Server, Port, Database));


        public DataBaseSourceConfiguration() { }

        public DataBaseSourceConfiguration(
                    ISchemaRepository schemaRepository, IDbQuery dbQuery)
        {
            _schemaRepository = schemaRepository;
            _dbQuery = dbQuery;
        }

        [Required, Option("-o|--OutputFolder <FolderPath>",
             "Required: The folder path where the sql scripts will be written to.",
             CommandOptionType.SingleValue)]
        public string OutputFolder { get; set; }

        [Required, Option("-n|--Server <IPAddressOrServerName>",
             "Required: The IP address or name of the sql server.",
             CommandOptionType.SingleValue)]
        public string Server { get; set; }

        [Required, Option("-d|--Database <Name>",
             "Required: The name of the database that schema and views are defined.",
             CommandOptionType.SingleValue)]
        public string Database { get; set; }

        [Required, Option("-s|--Schema <Name>",
             "Required: The schema where the views are defined.",
             CommandOptionType.SingleValue)]
        public string Schema { get; set; }

        [Option("-p|--Port <Port>",
            "The port that the sql server is listening on.  Defaults to 1433.",
            CommandOptionType.SingleValue)]
        public int Port { get; set; } = 1433;

        [Option("-v|--Verbose",
            "Extended/detailed output messaging. Defaults to false.",
            CommandOptionType.SingleValue)]
        public bool Verbose { get; set; }

        public void BuildFiles(
            string schema,
            string[] folders,
            Action<ViewDefinition> writeFile)
        {
            Program.BuildDirectories(folders);
            var views = SchemaRepository.GetSchemas(schema);
            foreach (var view in views) writeFile(view);
        }
    }
}
