using MSharp.Framework.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace MSharp.Framework.UI
{
    class DatabaseChangeWatcher
    {
        static List<XElement> Changes = new List<XElement>();
        const string NullObject = "¦¦nil¦¦";

        static DatabaseChangeWatcher()
        {
            Data.DatabaseStateChangeCommand.ExecutedChangeCommand += DatabaseStateChangeCommand_ExecutedChangeCommand;
        }

        static void DatabaseStateChangeCommand_ExecutedChangeCommand(DatabaseStateChangeCommand change)
        {
            var node = new XElement("Change");
            if (change.CommandType != CommandType.Text)
                node.Add(new XAttribute("Type", change.CommandType.ToString()));

            node.Add(new XAttribute("Command", change.CommandText));

            foreach (var p in change.Params)
                node.Add(new XElement("Param",
                    new XAttribute("Name", p.ParameterName),
                    new XAttribute("Value", p.Value == DBNull.Value ? NullObject : p.Value),
                    new XAttribute("Type", p.DbType)));

            Changes.Add(node);
        }

        internal static void Restart() => Changes.Clear();

        internal static void DispatchChanges()
        {
            var response = new XElement("Changes", Changes).ToString();
            Changes.Clear();
            HttpContext.Current.Response.EndWith(response, "text/xml");
        }

        internal static void RunChanges()
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(HttpContext.Current.Request.Unvalidated["Data"])));
                var connectionStringKey = xmlDocument.GetElementsByTagName("ConnectionStringKey")[0].FirstChild.Value;
                var dataProviderType = xmlDocument.GetElementsByTagName("DataProviderType")[0].FirstChild.Value;
                var dataAccessor = DataAccessor.GetDataAccessor(dataProviderType);
                var changesNodeList = xmlDocument.GetElementsByTagName("Changes")[0];

                foreach (var xmlElement in changesNodeList.ChildNodes.OfType<XmlElement>())
                {
                    var command = xmlElement.GetAttribute("Command").Replace("&#xD;&#xA;", Environment.NewLine);
                    var commandType = CommandType.Text;
                    var dataParameters = new List<IDataParameter>();
                    if (!xmlElement.GetAttribute("Type").IsEmpty())
                        commandType = xmlElement.GetAttribute("Type").To<CommandType>();

                    foreach (var innerXmlElement in xmlElement.ChildNodes.OfType<XmlElement>())
                    {
                        var value = innerXmlElement.GetAttribute("Value");
                        var sqlDbType = innerXmlElement.GetAttribute("Type").To<DbType>();
                        var sqlParameter = dataAccessor.CreateParameter(innerXmlElement.GetAttribute("Name"), value == NullObject ? (object)DBNull.Value : value);
                        var param = sqlParameter as SqlParameter;
                        void perform(SqlParameter sqlP, Action<SqlParameter> action)
                        {
                            if (sqlP != null) action(sqlP);
                        }

                        switch (sqlDbType)
                        {
                            case DbType.DateTime:
                                sqlParameter.DbType = DbType.DateTime;
                                perform(param, p => p.SqlDbType = SqlDbType.DateTime);
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToDateTimeOffset(sqlParameter.Value.ToString()).DateTime;
                                break;
                            case DbType.Guid:
                                sqlParameter.DbType = DbType.Guid;
                                perform(param, p => p.SqlDbType = SqlDbType.UniqueIdentifier);
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : sqlParameter.Value?.ToString().To<Guid>();
                                break;
                            case DbType.DateTime2:
                                sqlParameter.DbType = DbType.DateTime2;
                                perform(param, p => p.SqlDbType = SqlDbType.DateTime2);
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToDateTimeOffset(sqlParameter.Value.ToString()).DateTime;
                                break;
                            case DbType.Time:
                                sqlParameter.DbType = DbType.Time;
                                perform(param, p => p.SqlDbType = SqlDbType.Time);
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToTimeSpan(sqlParameter.Value.ToString());
                                break;
                            case DbType.Boolean:
                                sqlParameter.DbType = DbType.Boolean;
                                perform(param, p => p.SqlDbType = SqlDbType.Bit);
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : sqlParameter.Value?.ToString().To<bool>();
                                break;
                            default:
                                perform(param, p => p.DbType = sqlDbType);
                                break;
                        }

                        dataParameters.Add(sqlParameter);
                    }

                    using (new DatabaseContext(Config.GetConnectionString(connectionStringKey)))
                        dataAccessor.ExecuteNonQuery(command, commandType, dataParameters.ToArray());
                }

                Cache.Current.ClearAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}