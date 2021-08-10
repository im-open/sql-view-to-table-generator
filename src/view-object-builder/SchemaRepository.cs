using viewObjectBuilder.Data;
using System.Collections.Generic;
using System.Linq;

namespace viewObjectBuilder
{
    public interface ISchemaRepository
    {
        IEnumerable<ViewDefinition> GetSchemas(string schema);
    }

    public class SchemaRepository : ISchemaRepository
    {
        private readonly IDbQuery _dbQuery;

        public SchemaRepository(IDbQuery dbQuery)
        {
            _dbQuery = dbQuery;
        }

        public IEnumerable<ViewDefinition> GetSchemas(string schema)
        {
            var viewSql = $@"
DECLARE @p0 NVarChar(1000) = '{schema}'
DECLARE @p1 VarChar(1000) = 'YES'

SELECT views.name [VIEW],
	[COLUMNS].[COLUMN_NAME], [COLUMNS].[DATA_TYPE], [COLUMNS].[CHARACTER_MAXIMUM_LENGTH],
    (CASE
        WHEN [COLUMNS].[IS_NULLABLE] = @p1 THEN 1
        WHEN NOT ([COLUMNS].[IS_NULLABLE] = @p1) THEN 0
        ELSE NULL
     END) AS [IS_NULLABLE], [COLUMNS].[COLUMN_DEFAULT],
	 extended_properties.value [VERSION],
	 [COLUMNS].[COLLATION_NAME]
FROM sys.schemas
	INNER JOIN sys.views on schemas.schema_id = views.schema_id
	INNER JOIN INFORMATION_SCHEMA.COLUMNS
		ON schemas.name = COLUMNS.TABLE_SCHEMA AND
			views.name = COLUMNS.TABLE_NAME
	LEFT OUTER JOIN
		(SELECT *
		FROM sys.extended_properties
		WHERE extended_properties.name = 'Version') extended_properties
			 ON views.object_id = extended_properties.major_id
WHERE schemas.name = @p0
";

            var viewResults = _dbQuery.Query<SqlViewColumn>(viewSql);

            return viewResults
                .GroupBy(v => new { v.VIEW, v.VERSION })
                .Select(v => new ViewDefinition()
                {
                    Name = v.Key.VIEW,
                    Version = v.Key.VERSION,

                    SqlText = DbSqlTools.buildTableSql(schema, v.Key.VIEW, v),
                    ClassText = OrmProjectTools.buildTableClass(schema, v.Key.VIEW, v),

                    Columns = v.Select(c => (ViewColumn)c)
                });
        }
    }
}
