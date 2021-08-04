using Dapper;
using viewObjectBuilder.Properties;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace viewObjectBuilder.Data
{
    public interface IDbQuery
    {
        IEnumerable<T> Query<T>(string sql);
    }

    public class DbQuery : IDbQuery
    {
        private IDbQueryConnection _connection;
        private readonly string _server;
        private readonly int _port;
        private readonly string _database;

        public string ConnectionString
            => string.Format(AppSettings.ConnectionStringTemplate, _server, _port, _database);

        public DbQuery(IDbQueryConnection connection)
            => _connection = connection;

        public DbQuery(string server, int port, string database)
        {
            _server = server;
            _port = port;
            _database = database;
        }

        public IDbConnection GetConnection(bool multipleResultSets = false)
        {
            var cs = ConnectionString;
            if (multipleResultSets)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var buildConnection = new SqlConnection(cs);
            buildConnection.Open();
            return buildConnection;
        }

        public IEnumerable<T> Query<T>(string sql)
            => (_connection ?? (_connection = new DbQueryConnection(GetConnection())))
                .Query<T>(sql);
    }
}