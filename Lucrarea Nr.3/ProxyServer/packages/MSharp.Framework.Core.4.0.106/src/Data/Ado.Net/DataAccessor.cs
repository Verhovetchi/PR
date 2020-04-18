namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// ADO.NET Facade for submitting single method commands.
    /// </summary>
    public abstract class DataAccessor<TConnection, TDataAdapter> : IDataAccessor
        where TConnection : IDbConnection, new()
        where TDataAdapter : IDbDataAdapter, new()
    {
        #region Manage connection

        /// <summary>
        /// Creates a new DB Connection to database with the given connection string.
        /// </summary>		
        public TConnection CreateConnection(string connectionString)
        {
            var result = new TConnection { ConnectionString = connectionString };

            result.Open();

            return result;
        }

        /// <summary>
        /// Creates a connection object.
        /// </summary>
        public TConnection CreateConnection()
        {
            var result = DbTransactionScope.Root?.GetDbConnection();
            if (result != null)
                if (DbTransactionScope.Current?.ScopeOption != DbTransactionScopeOption.Suppress)
                    return (TConnection)result;

            return CreateActualConnection();
        }

        public string GetCurrentConnectionString()
        {
            string result;

            if (DatabaseContext.Current != null) result = DatabaseContext.Current.ConnectionString;
            else result = Config.GetConnectionString("AppDatabase");

            if (result.IsEmpty())
                throw new ConfigurationErrorsException("No 'AppDatabase' connection string is specified in the application config file.");

            return result;
        }

        /// <summary>
        /// Creates a connection object.
        /// </summary>       
        internal TConnection CreateActualConnection()
        {
            return CreateConnection(GetCurrentConnectionString());
        }
        #endregion

        IDbCommand CreateCommand(CommandType type, string commandText, params IDataParameter[] @params)
        {
            return CreateCommand(type, commandText, default(TConnection), @params);
        }

        IDbCommand CreateCommand(CommandType type, string commandText, TConnection connection, params IDataParameter[] @params)
        {
            if (connection == null) connection = CreateConnection();

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = type;

            var scopeTransaction = DbTransactionScope.Root?.GetDbTransaction();
            if (scopeTransaction != null)
            {
                if (DbTransactionScope.Current?.ScopeOption != DbTransactionScopeOption.Suppress)
                {
                    if (scopeTransaction.Connection is TConnection)
                        command.Transaction = scopeTransaction;
                    else Debug.WriteLine("ERROR: Distributed transactions are not supported! Failed to run a " +
                        GetType().Name + " command when the open transaction scope is " + scopeTransaction.GetType().Name);
                }
            }

            command.CommandTimeout = DatabaseContext.Current?.CommandTimeout ?? (Config.TryGet<int?>("Sql.Command.TimeOut")) ?? command.CommandTimeout;

            foreach (var param in @params)
                command.Parameters.Add(param);

            return command;
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(string commandText) => ExecuteNonQuery(commandText, CommandType.Text);

        DataAccessProfiler.Watch StartWatch(string command)
        {
            if (DataAccessProfiler.IsEnabled) return DataAccessProfiler.Start(command);
            else return null;
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params)
        {
            var dbCommand = CreateCommand(commandType, command, @params);

            var watch = StartWatch(command);

            try
            {
                var result = dbCommand.ExecuteNonQuery();
                DatabaseStateChangeCommand.Raise(command, commandType, @params);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Non-Query SQL command.", ex).AddData("Command", command)
                    .AddData("Parameters", @params.Get(l => l.Select(p => p.ParameterName + "=" + p.Value).ToString(" | ")))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand.Parameters?.Clear();

                CloseConnection(dbCommand.Connection);

                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        void CloseConnection(IDbConnection connection)
        {
            if (DbTransactionScope.Root == null)
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands)
        {
            var connection = CreateConnection();
            var result = 0;

            try
            {
                foreach (var c in commands)
                {
                    var watch = StartWatch(c.Key);

                    IDbCommand dbCommand = null;
                    try
                    {
                        dbCommand = CreateCommand(commandType, c.Key, connection, c.Value);
                        result += dbCommand.ExecuteNonQuery();

                        DatabaseStateChangeCommand.Raise(c.Key, commandType, c.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error in executing SQL command.", ex).AddData("Command", c.Key)
                            .AddData("Parameters", c.Value.Get(l => l.Select(p => p.ParameterName + "=" + p.Value).ToString(" | ")));
                    }
                    finally
                    {
                        dbCommand?.Parameters?.Clear();

                        if (watch != null) DataAccessProfiler.Complete(watch);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Non-Query SQL commands.", ex).AddData("ConnectionString", connection.ConnectionString);
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and builds an IDataReader.
        /// Make sure you close the data reader after finishing the work.
        /// </summary>
        public IDataReader ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params)
        {
            var watch = StartWatch(command);

            var dbCommand = CreateCommand(commandType, command, @params);

            try
            {
                if (DbTransactionScope.Root != null) return dbCommand.ExecuteReader();
                else return dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running SQL Query.", ex).AddData("Command", command)
                    .AddData("Parameters", @params.Get(l => l.Select(p => p.ParameterName + "=" + p.Value).ToString(" | ")))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand?.Parameters?.Clear();
                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value of the type specified.
        /// </summary>
        public T ExecuteScalar<T>(string commandText) => (T)ExecuteScalar(commandText);

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(commandText, CommandType.Text);
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public object ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params)
        {
            var watch = StartWatch(command);
            var dbCommand = CreateCommand(commandType, command, @params);

            try
            {
                var result = dbCommand.ExecuteScalar();

                if (command.Contains("UPDATE ") || !command.ToLowerOrEmpty().StartsWith("select "))
                    DatabaseStateChangeCommand.Raise(command, commandType, @params);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Scalar SQL Command.", ex).AddData("Command", command)
                    .AddData("Parameters", @params.Get(l => l.Select(p => p.ParameterName + "=" + p.Value).ToString(" | ")))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand.Parameters?.Clear();

                CloseConnection(dbCommand.Connection);

                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        /// <summary>
        /// Executes a database query and returns the result as a data set.
        /// </summary>        
        public DataSet ReadData(string databaseQuery, params IDataParameter[] @params)
        {
            return ReadData(databaseQuery, CommandType.Text, @params);
        }

        /// <summary>
        /// Executes a database query and returns the result as a data set.
        /// </summary>        
        public DataSet ReadData(string databaseQuery, CommandType commandType, params IDataParameter[] @params)
        {
            using (var command = CreateCommand(commandType, databaseQuery, @params))
            {
                var watch = StartWatch(databaseQuery);
                try
                {
                    var result = new DataSet();
                    var adapter = new TDataAdapter { SelectCommand = command };
                    adapter.Fill(result);
                    command?.Parameters?.Clear();

                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error in running SQL Query.", ex).AddData("Command", command.CommandText)
                    .AddData("Parameters", @params.Get(l => l.Select(p => p.ParameterName + "=" + p.Value).ToString(" | ")))
                    .AddData("ConnectionString", command.Connection.ConnectionString);
                }
                finally
                {
                    command.Parameters?.Clear();

                    CloseConnection(command.Connection);

                    if (watch != null) DataAccessProfiler.Complete(watch);
                }
            }
        }

        IDbConnection IDataAccessor.CreateConnection(string connectionString) => CreateConnection(connectionString);

        IDbConnection IDataAccessor.CreateConnection() => CreateConnection();

        protected Exception EnrichError(Exception ex, string command)
        {
            var error = "Could not execute SQL command: \r\n-----------------------\r\n{0}\r\n-----------------------\r\n Because:\r\n\r\n{1}".FormatWith(command.Trim(), ex.Message);

            if (ex.Message.Contains(" permission ", caseSensitive: false))
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name.Or("?");

                error += Environment.NewLine + Environment.NewLine + "Current IIS process model Identity: " + identity;

                if (identity != "NT AUTHORITY\\SYSTEM")
                {
                    error += Environment.NewLine + Environment.NewLine +
                        "Recommended action: If using IIS, update the Application Pool (Advanced Settings) and set Identity to LocalSystem or a specific user with full admin permission.";
                }
            }

            throw new Exception(error);
        }

        public abstract string GetMasterConnectionString();

        public abstract void DetachDatabase(string databaseName);

        public abstract void TakeDatabaseOffline(string databaseName);

        public abstract void TakeDatabaseOnline(string databaseName);

        public abstract void DeleteDatabase(string databaseName);

        public abstract bool DatabaseExists(string databaseName);

        public abstract void TestDatabaseConnection(string databaseName);

        public abstract void ClearAllPools();
        public abstract IDataParameter CreateParameter(string name, object value);
    }
}