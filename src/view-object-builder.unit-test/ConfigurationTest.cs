using System.Collections.Generic;
using System.Linq;
using viewObjectBuilder.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NSubstitute;
using Shouldly;
using Xunit;

namespace viewObjectBuilder.unitTest
{
    public class Configuration
    {
        private readonly IDbQuery _dbConnection;

        public Configuration()
        {
            _dbConnection = Substitute.For<IDbQuery>();
        }

        [Fact]
        public void schema_has_no_views()
        {
            _dbConnection
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Returns(new SqlViewColumn[] { });

            var dbSchemaViews = new SchemaRepository(_dbConnection);

            var views = dbSchemaViews.GetSchemas("schema");
            views.ShouldBeEmpty();
        }

        [Theory,
         InlineData(1, 1),
         InlineData(2, 1),
         InlineData(2, 4),
         InlineData(3, 6),
         InlineData(6, 12)]
        public void schema_returns_appropriate_view_count(int viewCount, int columnCount)
        {
            var returnResults = new List<SqlViewColumn>();
            for (int i = 0; i < viewCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    returnResults.Add(randomColumn(i.ToString()));
                }
            }

            _dbConnection
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Returns(returnResults);

            var dbSchemaViews = new SchemaRepository(_dbConnection);

            var views = dbSchemaViews.GetSchemas("schema");
            views.Count().ShouldBe(viewCount);
        }

        private SqlViewColumn randomColumn(string view)
            => new SqlViewColumn()
            {
                COLUMN_NAME = string.Empty,
                DATA_TYPE = string.Empty,
                CHARACTER_MAXIMUM_LENGTH = 0,
                IS_NULLABLE = true,
                COLUMN_DEFAULT = string.Empty,
                VIEW = view,
                VERSION = null,
            };
}
}