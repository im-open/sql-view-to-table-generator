using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using viewObjectBuilder.Data;
using System.IO;

namespace viewObjectBuilder.Configuration
{
    [Command(Name = "dotnet view_to_sql.dll"),
     Subcommand("BuildSql", typeof(BuildSqlConfiguration)),
     Subcommand("BuildOrm", typeof(BuildOrmConfiguration)),
     Subcommand("CompareFiles", typeof(CompareFilesConfiguration))]
    public class Configuration
    {
        public static bool Verbose { get; set; }

        public static IEnumerable<string> ProcessResults { get; set; }
    }
}