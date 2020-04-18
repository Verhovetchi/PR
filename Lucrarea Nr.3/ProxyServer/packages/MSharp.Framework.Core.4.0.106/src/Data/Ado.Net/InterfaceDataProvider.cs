namespace MSharp.Framework.Data.Ado.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides data access for Interface types.
    /// </summary>
    public class InterfaceDataProvider : IDataProvider
    {
        static ConcurrentDictionary<Type, List<Type>> ImplementationsCache = new ConcurrentDictionary<Type, List<Type>>();
        Type InterfaceType;

        public InterfaceDataProvider(Type type) => InterfaceType = type;

        static List<Type> GetImplementers(Type interfaceType)
        {
            return ImplementationsCache.GetOrAdd(interfaceType, FindImplementers);
        }

        static List<Type> FindImplementers(Type interfaceType)
        {
            var result = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.References(interfaceType.Assembly))
                .Concat(interfaceType.Assembly).ToArray();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == interfaceType) continue;
                        if (type.IsInterface) continue;

                        if (type.Implements(interfaceType))
                        {
                            result.Add(type);
                        }
                    }
                }
                catch
                {
                    // Can't load assembly
                }
            }

            // For any type, if it's parent is in the list, exclude it:

            var typesWithParentsIn = result.Where(x => result.Contains(x.BaseType)).ToArray();

            foreach (var item in typesWithParentsIn)
                result.Remove(item);

            return result;
        }

        public int Count(Type type, IEnumerable<ICriterion> conditions, params QueryOption[] options)
        {
            return GetList(type, conditions, options).Count();
        }

        public IEnumerable<IEntity> GetList(Type type, IEnumerable<ICriterion> criteria, params QueryOption[] options)
        {
            return GetImplementers(type).SelectMany(x => Database.GetList(x, criteria, options)).ToList();
        }

        public object Aggregate(Type type, Database.AggregateFunction function, string propertyName, IEnumerable<ICriterion> conditions, params QueryOption[] options)
        {
            throw new NotSupportedException("Database.Aggregate doesn't work on interfaces.");
        }

        public IEntity Get(object objectID)
        {
            foreach (var actual in GetImplementers())
            {
                try
                {
                    var result = Database.Get(objectID, actual) as Entity;
                    if (result != null) return result;
                }
                catch
                {
                    continue;
                }
            }

            throw new Exception("There is no {0} record with the ID of '{1}'".FormatWith(InterfaceType.Name, objectID));
        }

        public IEnumerable<string> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new NotSupportedException("IDataProvider.ReadManyToManyRelation() is not supported for Interfaces");
        }

        public void Save(IEntity record)
        {
            throw new NotSupportedException("IDataProvider.Save() is irrelevant to Interfaces");
        }

        public void Delete(IEntity record)
        {
            throw new NotSupportedException("IDataProvider.Delete() is irrelevant to Interfaces");
        }

        #region IDataProvider Members

        public IEnumerable<object> GetIdsList(Type type, IEnumerable<ICriterion> criteria)
        {
            throw new NotSupportedException("IDataProvider.Delete() is irrelevant to Interfaces");
        }

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated)
        {
            throw new NotSupportedException("GetUpdatedValues() is irrelevant to Interfaces");
        }

        public int ExecuteNonQuery(string command)
        {
            throw new NotSupportedException("ExecuteNonQuery() is irrelevant to Interfaces");
        }

        public object ExecuteScalar(string command)
        {
            throw new NotSupportedException("ExecuteScalar() is irrelevant to Interfaces");
        }

        public bool SupportValidationBypassing()
        {
            throw new NotSupportedException("SupportValidationBypassing() is irrelevant to Interfaces");
        }

        public void BulkInsert(IEntity[] entities, int batchSize)
        {
            throw new NotSupportedException("BulkInsert() is irrelevant to Interfaces");
        }

        public void BulkUpdate(IEntity[] entities, int batchSize)
        {
            throw new NotSupportedException("BulkInsert() is irrelevant to Interfaces");
        }

        public DirectDatabaseCriterion GetAssociationInclusionCriteria(QueryOption query, PropertyInfo association)
        {
            throw new InvalidOperationException("Oops! GetAssociationInclusionCriteria() is not meant to be ever called on " + GetType().Name);
        }

        public IEnumerable<IEntity> GetList(DatabaseQuery query)
        {
            if (query.TakeTop.HasValue)
                throw new Exception("Top() criteria is not allowed when querying based on Interfaces.");

            if (query.OrderByParts.Any())
                throw new Exception("OrderBy() is not allowed when querying based on Interfaces.");

            var providers = FindProviders();
            var results = providers.Select(x => x.GetList(query.CloneFor(x.EntityType)));
            return results.SelectMany(x => x);
        }

        List<IDataProvider> FindProviders()
        {
            var implementers = GetImplementers();
            return implementers.Select(x => Database.GetProvider(x)).ToList();
        }

        List<Type> GetImplementers() => ImplementationsCache.GetOrAdd(InterfaceType, FindImplementers);

        public string MapColumn(string propertyName)
        {
            var options = FindProviders().Select(p => new { provider = p, name = p.MapColumn(propertyName) })
                .Distinct(x => x.name);

            if (options.IsSingle()) return options.First().name;

            if (options.None())
                throw new Exception("MapColumn is not supported for Interface data provider: " +
                    InterfaceType?.Name + ". No implementors found!");

            throw new Exception("MapColumn() for Interface data provider: " + InterfaceType?.Name +
                " results in more than one option:\n\n" +
                options.Select(x => x.provider.EntityType?.Name + "->" + x.name).ToLinesString());
        }

        public int Count(DatabaseQuery query)
        {
            var providers = FindProviders();
            var results = providers.Select(x => x.Count(query.CloneFor(x.EntityType)));
            return results.Sum();
        }

        public object Aggregate(DatabaseQuery query, QueryAggregateFunction function, string propertyName) =>
            throw new NotSupportedException("Database.Aggregate doesn't work on interfaces.");

        public DirectDatabaseCriterion GetAssociationInclusionCriteria(DatabaseQuery query, PropertyInfo association)
        {
            throw new InvalidOperationException("Oops! GetAssociationInclusionCriteria() is not meant to be ever called on " + GetType().Name);
        }

        public string GenerateSelectCommand(DatabaseQuery iquery, string fields)
        {
            throw new InvalidOperationException("Oops! GenerateSelectCommand() is not meant to be ever called on " + GetType().Name);
        }

        public string ConnectionString { get; set; }

        public string ConnectionStringKey { get; set; }

        Type IDataProvider.EntityType => InterfaceType;

        public string MapSubquery(string path, string parent) =>
            throw new NotSupportedException("IDataProvider.MapSubquery() is irrelevant to Interfaces");

        #endregion
    }
}