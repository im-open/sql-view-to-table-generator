using System;
using System.Collections.Generic;
using System.Linq;

namespace viewObjectBuilder
{
    public static class DbSqlTools

    {
        public static string BuildSql(string schema, string view, string version, string sql)
        {
            var propertyVersion = version ?? "1.0";
            var downstreamServiceRoleName = schema.Replace("_Public", string.Empty, StringComparison.OrdinalIgnoreCase) + "_DownstreamService";

            return $@"
IF DATABASE_PRINCIPAL_ID('{schema}Owner') IS NULL
    CREATE USER [{schema}Owner] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[{schema}]
GO
IF SCHEMA_ID('{schema}') IS NULL
    EXEC ('CREATE SCHEMA [{schema}] AUTHORIZATION [{schema}Owner]')
GO
IF DATABASE_PRINCIPAL_ID('{downstreamServiceRoleName}') IS NULL
    CREATE ROLE [{downstreamServiceRoleName}]
GO

{sql}

{$"EXEC sys.sp_addextendedproperty @name=N'Version', @value=N'{propertyVersion}', @level0type=N'SCHEMA', @level0name=N'{schema}', @level1type=N'TABLE', @level1name=N'{view}'"}
GO
";
        }

        public static string columnToSql(this IViewColumn column)
            => $"{column.COLUMN_NAME} {column.DATA_TYPE.ToUpper()}" +
               $"{(column.CHARACTER_MAXIMUM_LENGTH != null ? $"({(column.CHARACTER_MAXIMUM_LENGTH != -1 ? column.CHARACTER_MAXIMUM_LENGTH.ToString() : "max")})" : "")} " +
               $"{(column.COLLATION_NAME != null ? $" COLLATE {column.COLLATION_NAME} " : "")}" +
               $"{(column.IS_NULLABLE ? "NULL" : "NOT NULL")}{(column.COLUMN_DEFAULT != null ? " DEFAULT " + column.COLUMN_DEFAULT : string.Empty)}";

        public static string buildTableSql(string schema, string tableName, IEnumerable<IViewColumn> columns)
            => $@"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = N'{schema}' AND TABLE_NAME = N'{tableName}')
BEGIN
	DROP TABLE [{schema}].[{tableName}]
END

CREATE TABLE [{schema}].[{tableName}](
    {string.Join(",\r\n\t", columns.Select(c => c.columnToSql()))}
)
GO";
    }
}