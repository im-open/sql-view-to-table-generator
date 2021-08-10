using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using viewObjectBuilder.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;

namespace viewObjectBuilder.unitTest
{
    public class DbQueryTest
    {
        private IDbQueryConnection _dbConnection;
        private viewObjectBuilder.Data.DbQuery _dbQuery;
        private readonly AppSettingsData _appSettingsFile;
        private string _testServer = "TestServer";
        private int _testPort = 1433;
        private string _testDataBase = "TestDb";
        

        public DbQueryTest()
        {
            _appSettingsFile = AppSettingsData.LoadFromFile(AppSettingsData.DefaultAppSettingsFile);
            _dbQuery = new Data.DbQuery(_testServer, _testPort, _testDataBase);
        }

        [Fact]
        public void connection_string_match()
        {
            _dbQuery.ConnectionString
                .ShouldBe(
                    string.Format(_appSettingsFile.ConnectionStringTemplate,
                        _testServer, _testPort, _testDataBase));
        }
        
        [Theory,
         InlineData(0, 0),
         InlineData(1, 1),
         InlineData(2, 1),
         InlineData(2, 4),
         InlineData(3, 6),
         InlineData(6, 12)]
        public void schema_returns_appropriate_view_count(int viewCount, int columnCount)
        { 
            var results = new List<SqlViewColumn>();
            for (int i = 0; i < viewCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    results.Add(randomColumn($"view{i}"));
                }
            }

            _dbConnection = Substitute.For<IDbQueryConnection>();
            _dbConnection
                .Query<SqlViewColumn>(Arg.Any<string>())
                .Returns(results.AsQueryable());
            _dbQuery = new Data.DbQuery(_dbConnection);

            var query = _dbQuery.Query<SqlViewColumn>("test");
            query.Count().ShouldBe(viewCount * columnCount);
        }

        [Fact]
        public void db_connection_returns_state()
        {
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.State
                .Returns(ConnectionState.Open);
            var dbQueryConnection = new DbQueryConnection(dbConnection);
            dbQueryConnection.State.ShouldBe(ConnectionState.Open);
        }

       public static SqlViewColumn randomColumn(string view)
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
