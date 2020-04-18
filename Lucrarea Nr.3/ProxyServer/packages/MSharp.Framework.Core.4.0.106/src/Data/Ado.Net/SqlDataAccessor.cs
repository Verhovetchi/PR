namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class SqlDataAccessor : DataAccessor<SqlConnection, SqlDataAdapter>
    {
        public override void DetachDatabase(string databaseName)
        {
            var script = @"
USE Master;
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [{0}] SET MULTI_USER;
exec sp_detach_db '{0}'".FormatWith(databaseName);


            try
            {
                using (new DatabaseContext(GetMasterConnectionString()))
                    ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Could not detach database '" + databaseName + "' becuase '" + ex.Message + "'", ex);
            }
        }

        internal void AttachDatabase(FileInfo mdfFile, FileInfo ldfFile, string databaseName)
        {
            var script = @"USE Master; CREATE DATABASE [{0}] ON (FILENAME = '{1}'), (FILENAME = '{2}') FOR ATTACH"
                .FormatWith(databaseName, mdfFile.FullName, ldfFile.FullName);

            try
            {
                using (new DatabaseContext(GetMasterConnectionString()))
                    ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Could not attach database '" + databaseName + "' becuase '" + ex.Message + "'", ex);
            }
        }

        public override void DeleteDatabase(string databaseName)
        {
            var script = @"
IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}')
BEGIN
    ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [{0}] SET MULTI_USER;
    DROP DATABASE [{0}];
END".FormatWith(databaseName);

            try
            {
                using (new DatabaseContext(GetMasterConnectionString()))
                    ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not drop database '" + databaseName + "'.", ex);
            }
        }

        public override bool DatabaseExists(string databaseName)
        {
            var script = $"SELECT count(name) FROM master.dbo.sysdatabases WHERE name = N'{databaseName}'";

            using (var connection = new SqlConnection(GetMasterConnectionString()))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = script;

                    try { return (int)cmd.ExecuteScalar() > 0; }
                    catch (Exception ex) { throw EnrichError(ex, script); }
                }
            }
        }

        public override void TakeDatabaseOffline(string databaseName)
        {
            SqlConnection.ClearAllPools();

            var script = $"USE Master; ALTER DATABASE [{databaseName}] SET OFFLINE WITH ROLLBACK IMMEDIATE;"
                .FormatWith(databaseName);
            try
            {
                using (new DatabaseContext(GetMasterConnectionString()))
                    ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not drop database '" + databaseName + "'.", ex);
            }
        }

        public override void TakeDatabaseOnline(string databaseName)
        {
            var script = $"USE Master; ALTER DATABASE [{databaseName}] SET ONLINE;"
                .FormatWith(databaseName);
            try
            {
                using (new DatabaseContext(GetMasterConnectionString()))
                    ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not drop database '" + databaseName + "'.", ex);
            }
        }

        public override void TestDatabaseConnection(string databaseName)
        {
            Exception error = null;

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    ReadData("SELECT TABLE_NAME FROM [{0}].INFORMATION_SCHEMA.TABLES".FormatWith(databaseName));
                    return;
                }
                catch (Exception ex)
                {
                    SqlConnection.ClearAllPools();

                    error = ex;
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            throw new Exception("Could not access the new database:" + error.Message, error);
        }

        public override string GetMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(GetCurrentConnectionString())
            {
                InitialCatalog = "master"
            };

            return builder.ToString();
        }

        public override void ClearAllPools() => SqlConnection.ClearAllPools();

        public override IDataParameter CreateParameter(string name, object value) => new SqlParameter(name, value);
    }
}