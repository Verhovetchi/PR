using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Framework.Data.Ado.Net
{

    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET based on the OleDb provider.
    /// </summary>
    public abstract class OleDbDataProvider : DataProvider<System.Data.OleDb.OleDbConnection, System.Data.OleDb.OleDbDataAdapter, System.Data.OleDb.OleDbParameter> { }

    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET based on the ODBC provider.
    /// </summary>
    public abstract class OdbcDataProvider : DataProvider<System.Data.Odbc.OdbcConnection, System.Data.Odbc.OdbcDataAdapter, System.Data.Odbc.OdbcParameter> { }
}
