using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace viewObjectBuilder.Data
{
    public interface IDbQueryConnection: IDisposable
    {
        ConnectionState State { get; }

        IEnumerable<T> Query<T>(string sql);
    }

    public class DbQueryConnection : IDbQueryConnection
    {
        private IDbConnection _dbConnection;

        public ConnectionState State => _dbConnection.State;

        public DbQueryConnection(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        
        public IEnumerable<T> Query<T>(string sql)
            => _dbConnection.Query<T>(sql);

        public void Dispose()
        {
            if(_dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
            }
            _dbConnection.Dispose();
        }
    }
}