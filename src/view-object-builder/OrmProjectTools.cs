using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace viewObjectBuilder
{
    public static class OrmProjectTools
    {
        public static string OrmProjectFile => $@"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <OutputType>Library</OutputType>
        <TargetFramework>netcoreapp2.1</TargetFramework>
    </PropertyGroup>
    <ItemGroup>
       <PackageReference Include=""Dapper"" Version=""1.50.5"" />
       <PackageReference Include=""Microsoft.AspNetCore.App"" Version=""2.1.2"" />
    </ItemGroup>
    <ItemGroup>
        <None Update=""properties\appSettings.json"">
            <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </None>
    </ItemGroup>
</Project>";

        public static string DbConnectionFile(string schema) =>
            $@"using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using {schema}.Properties;

namespace {schema}.Data
{{
    public interface IDbQuery
    {{
        IEnumerable<T> Query<T>(string sql);
    }}

    public class DbQuery : IDbQuery
    {{
        private readonly IDbConnection _connection;
        private readonly string _server;
        private readonly int _port;
        private readonly string _database;

        private string _connectionString
            => string.Format(AppSettings.ConnectionStringTemplate, _server, _port, _database);

        public DbQuery(IDbConnection connection) => _connection = connection;

        public DbQuery(string server, int port, string database)
        {{
            _server = server;
            _port = port;
            _database = database;
            _connection = GetConnection();
        }}

        public IDbConnection GetConnection(bool multipleResultSets = false)
        {{
            var cs = _connectionString;
            if (multipleResultSets)
            {{
                var scsb = new SqlConnectionStringBuilder(cs)
                {{
                    MultipleActiveResultSets = true
                }};
                cs = scsb.ConnectionString;
            }}
            var buildConnection = new SqlConnection(cs);
            buildConnection.Open();
            return buildConnection;
        }}

        public IEnumerable<T> Query<T>(string sql)
            => _connection.Query<T>(sql);
    }}
}}";

        public static string AppSettingsClassFile(string schema) => $@"using System.IO;
using Microsoft.Extensions.Configuration;

namespace {schema}.Properties
{{
    public static class AppSettings
    {{
        private static IConfiguration _configuration;

        private static IConfiguration configuration
            => _configuration ?? (_configuration = (new ConfigurationBuilder()
                   .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), ""properties""))
        .AddJsonFile(""appSettings.json"")).Build());

        public static string ConnectionStringTemplate => configuration[""ConnectionStringTemplate""];
    }}
}}";

        public static string AppSettingsFile => @"{ ""exclude"": [
    ""**/bin"",
    ""**/bower_components"",
    ""**/jspm_packages"",
    ""**/node_modules"",
    ""**/obj"",
    ""**/platforms""
    ],
    ""ConnectionStringTemplate"": ""Data Source={0}SqlAg;Initial Catalog={1}ExtendHealth;Integrated Security=True""
}";

        public static string buildTableClass(string schema, string tableName, IEnumerable<IViewColumn> columns)
        {
            var pluralizedTableName = tableName.PluralizeName();

            return $@"using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace {schema}	{{
	public class {pluralizedTableName} {{
        public IEnumerable<{tableName}> records {{ get; }}

		public {pluralizedTableName} (SqlConnection connection) {{
            var selectSql = ""select * from [{schema}].[{tableName}];"";
            records = connection.Query<{tableName}>(selectSql);
        }}
	}}

	public class {tableName} {{
		{string.Join("\r\n\t\t", columns.Select(c => c.columnToProperty()))}
	}}
}}";
        }

        public static string columnToProperty(this IViewColumn column)
            => $"public {sqlToCSharpType(column.DATA_TYPE, column.IS_NULLABLE)} {column.COLUMN_NAME} {{ get; set; }}";

        public static string sqlToCSharpType(string sqlDataType, bool nullable)
        {
            var dataType = "object";
            switch (sqlDataType.ToLower())
            {
                case "nvarchar":
                case "varchar":
                case "char":
                    dataType = "string";
                    break;

                case "tinyint":
                case "int":
                    dataType = "int" + (nullable ? "?" : "");
                    break;

                case "bigint":
                    dataType = "long" + (nullable ? "?" : "");
                    break;

                case "bit":
                    dataType = "bool" + (nullable ? "?" : "");
                    break;

                case "datetime":
                case "datetime2":
                case "date":
                case "datetimeoffset":
                    dataType = "DateTime" + (nullable ? "?" : "");
                    break;

                case "money":
                case "decimal":
                    dataType = "decimal" + (nullable ? "?" : "");
                    break;
            }

            return dataType;
        }
    }
}