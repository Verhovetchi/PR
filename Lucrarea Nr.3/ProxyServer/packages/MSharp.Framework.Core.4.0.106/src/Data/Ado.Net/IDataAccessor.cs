namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;

    public interface IDataAccessor
    {
        string GetCurrentConnectionString();
        DataSet ReadData(string databaseQuery, params IDataParameter[] @params);
        DataSet ReadData(string databaseQuery, CommandType commandType, params IDataParameter[] @params);
        int ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params);
        int ExecuteNonQuery(string command);
        IDbConnection CreateConnection(string connectionString);
        IDbConnection CreateConnection();
        int ExecuteNonQuery(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands);
        IDataReader ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params);
        T ExecuteScalar<T>(string commandText);
        object ExecuteScalar(string commandText);
        object ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params);

        string GetMasterConnectionString();
        void DetachDatabase(string databaseName);
        void TakeDatabaseOffline(string databaseName);
        void TakeDatabaseOnline(string databaseName);
        void DeleteDatabase(string databaseName);
        bool DatabaseExists(string databaseName);
        void TestDatabaseConnection(string databaseName);
        void ClearAllPools();

        IDataParameter CreateParameter(string name, object value);
    }

    /// <summary>
    /// Provides a DataAccessor implementation for System.Data.SqlClient 
    /// </summary>
    public static partial class DataAccessor
    {
        static IDataAccessor current = new SqlDataAccessor();

        public static IDataAccessor Current
        {
            get => current ?? throw new InvalidOperationException("The current instance should be set prior to use.");
            private set => current = value;
        }

        public static void SetCurrent(IDataAccessor accessor) => Current = accessor;

        public static string GetCurrentConnectionString() => Current.GetCurrentConnectionString();

        public static DataSet ReadData(string databaseQuery, params IDataParameter[] @params)
            => Current.ReadData(databaseQuery, @params);

        public static DataSet ReadData(string databaseQuery, CommandType commandType, params IDataParameter[] @params)
            => Current.ReadData(databaseQuery, commandType, @params);

        public static int ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params)
            => Current.ExecuteNonQuery(command, commandType, @params);

        public static int ExecuteNonQuery(string command) => Current.ExecuteNonQuery(command);

        public static IDbConnection CreateConnection(string connectionString)
            => Current.CreateConnection(connectionString);

        public static IDbConnection CreateConnection() => Current.CreateConnection();

        public static int ExecuteNonQuery(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands)
            => Current.ExecuteNonQuery(commandType, commands);

        public static IDataReader ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params)
            => Current.ExecuteReader(command, commandType, @params);

        public static T ExecuteScalar<T>(string commandText) => Current.ExecuteScalar<T>(commandText);

        public static object ExecuteScalar(string commandText) => Current.ExecuteScalar(commandText);

        public static object ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params)
            => Current.ExecuteScalar(command, commandType, @params);
    }
}