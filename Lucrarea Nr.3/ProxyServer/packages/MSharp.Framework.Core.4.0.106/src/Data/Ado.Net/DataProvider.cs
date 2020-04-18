namespace MSharp.Framework.Data.Ado.Net
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET.
    /// </summary>
    public abstract class DataProvider<TConnection, TDataAdapter, TDataParameter> : IDataProvider
        where TConnection : IDbConnection, new()
        where TDataAdapter : IDbDataAdapter, new()
        where TDataParameter : IDbDataParameter, new()
    {
        protected abstract IDataAccessor GetAccessor();

        static string[] ExtractIdsSeparator = new[] { "</Id>", "<Id>", "," };

        string connectionStringKey, connectionString;

        protected DataProvider()
        {
            connectionStringKey = GetDefaultConnectionStringKey();
        }

        static string GetDefaultConnectionStringKey() => "AppDatabase";

        public virtual void BulkInsert(IEntity[] entities, int batchSize)
        {
            foreach (var item in entities)
                Database.Save(item, SaveBehaviour.BypassAll);
        }

        public void BulkUpdate(IEntity[] entities, int batchSize)
        {
            foreach (var item in entities)
                Database.Save(item, SaveBehaviour.BypassAll);
        }

        public static List<string> ExtractIds(string idsXml)
        {
            return idsXml.Split(ExtractIdsSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public bool SupportValidationBypassing() => true;

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(string command) => ExecuteNonQuery(command, CommandType.Text);

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
            {
                try
                {
                    return GetAccessor().ExecuteNonQuery(command, commandType, @params);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(command);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public int ExecuteNonQuery(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands)
        {
            using (new DatabaseContext(ConnectionString))
            {
                try
                {
                    return GetAccessor().ExecuteNonQuery(commandType, commands);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(commands.ToLinesString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and builds an IDataReader.  Make sure you close the data reader after finishing the work.
        /// </summary>
        public IDataReader ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
            {
                try
                {
                    return GetAccessor().ExecuteReader(command, commandType, @params);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(command);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public object ExecuteScalar(string command) => ExecuteScalar(command, CommandType.Text);

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public object ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
            {
                try
                {
                    return GetAccessor().ExecuteScalar(command, commandType, @params);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(command);
                    throw;
                }
            }
        }

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var result = new Dictionary<string, Tuple<string, string>>();

            var type = original.GetType();
            var propertyNames = type.GetProperties().Distinct().Select(p => p.Name.Trim()).ToArray();

            Func<string, PropertyInfo> getProperty = name => type.GetProperties().Except(p => p.IsSpecialName || p.GetGetMethod().IsStatic).Where(p => p.GetSetMethod() != null && p.GetGetMethod().IsPublic).OrderByDescending(x => x.DeclaringType == type).FirstOrDefault(p => p.Name == name);

            var dataProperties = propertyNames.Select(getProperty).ExceptNull()
                .Except(CalculatedAttribute.IsCalculated)
                .Where(LogEventsAttribute.ShouldLog)
                .ToArray();

            foreach (var p in dataProperties)
            {
                var propertyType = p.PropertyType;
                // Get the original value:
                string originalValue, updatedValue = null;
                if (propertyType == typeof(IList<Guid>))
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IList<Guid>).ToString(",");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IList<Guid>).ToString(",");
                    }
                    catch
                    {
                        continue;
                    }
                }
                else if (propertyType.IsGenericType && !propertyType.IsNullable())
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IEnumerable<object>).ToString(", ");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IEnumerable<object>).ToString(", ");
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        originalValue = $"{p.GetValue(original)}";
                        if (updated != null)
                            updatedValue = $"{p.GetValue(updated)}";
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (updated == null || originalValue != updatedValue)
                    if (result.LacksKey(p.Name))
                        result.Add(p.Name, new Tuple<string, string>(originalValue, updatedValue));
            }

            return result;
        }

        /// <summary>
        /// Creates a data parameter with the specified name and value.
        /// </summary>
        public IDataParameter CreateParameter(string parameterName, object value)
        {
            if (value == null) value = DBNull.Value;

            return new TDataParameter { ParameterName = parameterName.Remove(" "), Value = value };
        }

        /// <summary>
        /// Creates a data parameter with the specified name and value and type.
        /// </summary>
        public IDataParameter CreateParameter(string parameterName, object value, DbType columnType)
        {
            if (value == null) value = DBNull.Value;

            return new TDataParameter { ParameterName = parameterName.Remove(" "), Value = value, DbType = columnType };
        }

        /// <summary>
        /// Deletes the specified record.
        /// </summary>
        public abstract void Delete(IEntity record);

        /// <summary>
        /// Gets the specified record by its type and ID.
        /// </summary>
        public IEntity Get(object objectID)
        {
            var command = $"SELECT {GetFields()} FROM {GetTables()} WHERE {MapColumn("ID")} = @ID";

            using (var reader = ExecuteReader(command, CommandType.Text, CreateParameter("ID", objectID)))
            {
                var result = new List<IEntity>();

                if (reader.Read()) return Parse(reader);
                else throw new DataException($"There is no record with the the ID of '{objectID}'.");
            }
        }

        /// <summary>
        /// Reads the many to many relation.
        /// </summary>
        public abstract IEnumerable<string> ReadManyToManyRelation(IEntity instance, string property);

        /// <summary>
        /// Saves the specified record.
        /// </summary>
        public abstract void Save(IEntity record);

        /// <summary>
        /// Generates data provider specific parameters for the specified data items.
        /// </summary>
        public IDataParameter[] GenerateParameters(Dictionary<string, object> parametersData)
        {
            return parametersData.Select(GenerateParameter).ToArray();
        }

        /// <summary>
        /// Generates a data provider specific parameter for the specified data.
        /// </summary>
        public virtual IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            return new TDataParameter { Value = data.Value, ParameterName = data.Key.Remove(" ").Remove("`") };
        }

        public virtual object Aggregate(Type type, Database.AggregateFunction function, string propertyName, IEnumerable<ICriterion> conditions, params QueryOption[] options)
        {
            throw new NotImplementedException("Rebuild your project in M#");
        }

        #region Connection String

        /// <summary>
        /// Gets or sets the connection string key used for this data provider.
        /// </summary>
        public string ConnectionStringKey
        {
            get
            {
                return connectionStringKey;
            }
            set
            {
                if (value.HasValue()) LoadConnectionString(value);

                connectionStringKey = value;
            }
        }

        void LoadConnectionString(string key)
        {
            var settingInConfig = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().FirstOrDefault(s => s.Name == key);

            if (settingInConfig == null)
            {
                throw new ArgumentException("Thre is no connectionString defined in the app.config or web.config with the key '{0}'.".FormatWith(key));
            }
            else
            {
                connectionString = settingInConfig.ConnectionString;
            }
        }

        /// <summary>
        /// Returns a direct database criterion used to eager load associated objects.
        /// Gets the list of specified records.
        /// </summary>        
        public virtual DirectDatabaseCriterion GetAssociationInclusionCriteria(DatabaseQuery masterQuery, PropertyInfo association)
        {
            var whereClause = GenerateAssociationLoadingCriteria((DatabaseQuery)masterQuery, association);

            return new DirectDatabaseCriterion(whereClause)
            {
                Parameters = masterQuery.Parameters
            };
        }

        string GenerateAssociationLoadingCriteria(DatabaseQuery masterQuery, PropertyInfo association)
        {
            if (masterQuery.PageSize.HasValue && masterQuery.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var masterProvider = masterQuery.Provider as DataProvider<TConnection, TDataAdapter, TDataParameter>;

            var uniqueItems = masterProvider.GenerateSelectCommand(masterQuery,
                masterProvider.MapColumn(association.Name));

            return GenerateAssociationLoadingCriteria(MapColumn("ID"), uniqueItems, association);
        }

        protected virtual string GenerateAssociationLoadingCriteria(string id, string uniqueItems, PropertyInfo association)
        {
            return $"{id} IN ({uniqueItems})";
        }

        // public virtual IEnumerable<IEntity> GetList(IQueryOption query)
        // {
        //    using (var reader = ExecuteGetListReader(query))
        //    {
        //        var result = new List<IEntity>();
        //        while (reader.Read()) result.Add(Parse(reader));
        //        return result;
        //    }
        // }

        public virtual string GenerateSelectCommand(DatabaseQuery iquery)
        {
            return GenerateSelectCommand(iquery, GetFields());
        }

        public abstract string GenerateSelectCommand(DatabaseQuery iquery, string fields);

        public IDataReader ExecuteGetListReader(DatabaseQuery query)
        {
            var command = GenerateSelectCommand(query);
            return ExecuteReader(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        // public abstract IEntity Parse(IDataReader reader);

        public abstract string MapColumn(string propertyName);

        public int Count(DatabaseQuery query)
        {
            var command = GenerateCountCommand(query);
            return Convert.ToInt32(ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters)));
        }

        public abstract string GetTables(string prefix = null);

        public abstract string GenerateWhere(DatabaseQuery query);

        public string GenerateCountCommand(DatabaseQuery query)
        {
            if (query.PageSize.HasValue)
                throw new ArgumentException("PageSize cannot be used for Count().");

            if (query.TakeTop.HasValue)
                throw new ArgumentException("TakeTop cannot be used for Count().");

            return $"SELECT Count(*) FROM {GetTables()} {GenerateWhere(query)}";
        }

        public object Aggregate(DatabaseQuery query, QueryAggregateFunction function, string propertyName)
        {
            var command = GenerateAggregateQuery(query, function, propertyName);
            return ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        public string GenerateAggregateQuery(DatabaseQuery query, QueryAggregateFunction function, string propertyName)
        {
            var sqlFunction = function.ToString();

            var columnValueExpression = MapColumn(propertyName);

            if (function == QueryAggregateFunction.Average)
            {
                sqlFunction = "AVG";

                var propertyType = query.EntityType.GetProperty(propertyName).PropertyType;

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                    columnValueExpression = $"CAST({columnValueExpression} AS decimal)";
            }

            return $"SELECT {sqlFunction}({columnValueExpression}) FROM {GetTables()}" +
                GenerateWhere(query);
        }

        public abstract IEntity Parse(IDataReader reader);

        public virtual IEnumerable<IEntity> GetList(DatabaseQuery query)
        {
            using (var reader = ExecuteGetListReader(query))
            {
                var result = new List<IEntity>();
                while (reader.Read()) result.Add(Parse(reader));
                return result;
            }
        }

        public abstract string GetFields();

        public virtual string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => MapColumn(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return parts.ToString(", ") + offset;
        }

        public virtual string MapSubquery(string path, string parent)
        {
            throw new NotSupportedException($"{GetType().Name} does not provide a sub-query mapping for '{path}'.");
        }

        /// <summary>
        /// Gets or sets the connection string key used for this data provider.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (connectionString.HasValue()) return connectionString;

                if (connectionStringKey.HasValue())
                {
                    LoadConnectionString(connectionStringKey);
                }

                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }

        public abstract Type EntityType { get; }

        #endregion
    }
}