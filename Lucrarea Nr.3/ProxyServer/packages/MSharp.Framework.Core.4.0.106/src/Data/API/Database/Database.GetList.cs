namespace MSharp.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using MSharp.Framework.Data;

    partial class Database
    {
        /// <summary>
        /// This is invoked for every Database.GetList() call. You can use this to inject additional criteria or query options globally.
        /// </summary>
        public static event Action<Type, List<ICriterion>, List<QueryOption>> GettingList;

        static List<T> GetConcreteList<T>(IEnumerable<ICriterion> conditions, params QueryOption[] options) where T : IEntity
        {
            DateTime? timestamp = null;

            if (IsCacheConcurrencyAware) timestamp = DateTime.UtcNow;

            #region Load the instances

            List<T> rawObjects;
            var objectType = typeof(T);

            if (NeedsTypeResolution(objectType))
            {
                rawObjects = new List<T>();

                var takeOption = options.OfType<ResultSetSizeQueryOption>().FirstOrDefault();
                options = options.Except(i => i is ResultSetSizeQueryOption).ToArray();

                foreach (var provider in ResolveDataProviders(objectType))
                {
                    var query = new DatabaseQuery(provider.EntityType).Where(conditions).Config(options);
                    rawObjects.AddRange(provider.GetList(query).Cast<T>().ToList());
                }

                if (takeOption != null && takeOption.Number.HasValue)
                    rawObjects = rawObjects.Take(takeOption.Number.Value).ToList();
            }
            else
            {
                var query = new DatabaseQuery(objectType).Where(conditions).Config(options);
                rawObjects = GetProvider<T>().GetList(query).Cast<T>().ToList();
            }

            #endregion

            var result = new List<T>();

            foreach (var item in rawObjects)
            {
                // In-session objects has higher priority:
                var inSession = SessionMemory.Get(typeof(T), item.GetId());

                if (inSession != null) result.Add((T)inSession);
                else
                {
                    var inCache = (T)(object)Cache.Current.Get(item.GetType(), item.GetId().ToString());

                    if (inCache == null)
                    {
                        var asEntity = item as Entity;
                        EntityManager.RaiseOnLoaded(asEntity);

                        if (!AnyOpenTransaction()) // Don't cache the result if it is fetched in a transaction.
                        {
                            if (IsCacheConcurrencyAware)
                            {
                                if (!Cache.Current.IsUpdatedSince(asEntity, timestamp.Value))
                                    Cache.Current.Add(asEntity);
                            }
                            else Cache.Current.Add(asEntity);
                        }

                        result.Add(item);
                    }
                    else result.Add(inCache);
                }
            }

            if (options.OfType<SortQueryOption>().None() && options.OfType<PagingQueryOption>().None())
            {
                // Sort the collection if T is a generic IComparable:
                if (typeof(T).Implements<IComparable<T>>() || typeof(T).Implements<IComparable>()) // Note: T is always IComparable! 
                    result.Sort();
            }

            return result;
        }

        /// <summary>
        /// Returns a list of entities with the specified type.
        /// </summary>
        public static IEnumerable<T> GetList<T>(IEnumerable<ICriterion> criteria) where T : IEntity
        {
            return GetList<T>(criteria, null);
        }

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public static IEnumerable<T> GetList<T>() where T : IEntity => GetList<T>(new ICriterion[0]);

        /// <summary>
        /// Returns a list of entities with the specified type.
        /// </summary>
        public static IEnumerable<T> GetList<T>(params QueryOption[] options) where T : IEntity
        {
            return GetList<T>(new ICriterion[0], options);
        }

        /// <summary>
        /// Returns a list of entities with the specified type.
        /// </summary>
        public static IEnumerable<T> GetList<T>(IEnumerable<ICriterion> criteria, params QueryOption[] options) where T : IEntity
        {
            var optionsList = options?.ToList() ?? new List<QueryOption>();

            var criteriaList = criteria?.ToList() ?? new List<ICriterion>();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery<T>())
                criteriaList.Add(new Criterion(nameof(Entity.IsMarkedSoftDeleted), false));

            GettingList?.Invoke(typeof(T), criteriaList, optionsList);

            return DoGetList<T>(criteriaList, optionsList);
        }

        static IEnumerable<T> DoGetList<T>(List<ICriterion> criteria, List<QueryOption> options) where T : IEntity
        {
            List<T> result = null;
            string cacheKey = null;

            var numberOfRecords = options.GetResultsToFetch();

            var canCache = options.None() || (options.IsSingle() && numberOfRecords == 1);

            canCache &= criteria.OfType<DirectDatabaseCriterion>().All(x => x.IsCacheSafe);

            if (criteria.Except(typeof(DirectDatabaseCriterion)).Any(c => c.PropertyName.Contains(".")))
                canCache = false; // This doesn't work with cache expiration rules.

            if (canCache)
            {
                // Standard query, try the cache first:
                cacheKey = Cache.BuildQueryKey(typeof(T), criteria, numberOfRecords);

                if (result != null) return result;

                result = Cache.Current.GetList(typeof(T), cacheKey) as List<T>;
                if (result != null) return result;
            }

            result = GetConcreteList<T>(criteria, options.ToArray());

            if (canCache)
            {
                if (!NeedsTypeResolution(typeof(T))) // Do not cache a polymorphic call:
                {
                    if (!AnyOpenTransaction()) // Don't cache the result if it is fetched in a transaction.
                        Cache.Current.AddList(typeof(T), cacheKey, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the list of objects with the specified type matching the specified criteria.
        /// If no criteria is specified, all instances will be returned.
        /// </summary>        
        public static IEnumerable<T> GetList<T>(params Criterion[] criteria) where T : IEntity
        {
            return GetList<T>(criteria.OfType<ICriterion>());
        }

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public static IEnumerable<T> GetList<T>(Expression<Func<T, bool>> criteria) where T : IEntity
        {
            return GetList<T>(criteria, null);
        }

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        /// <param name="orderBy">The order by expression to run at the database level. It supports only one property.</param>
        /// <param name="desc">Specified whether the order by is descending.</param>
        public static IEnumerable<T> GetList<T>(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> orderBy, bool desc = false) where T : IEntity
        {
            return GetList<T>(criteria, QueryOption.OrderBy<T>(orderBy, desc));
        }

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public static IEnumerable<T> GetList<T>(Expression<Func<T, bool>> criteria, params QueryOption[] options) where T : IEntity
        {
            options = options ?? new QueryOption[0];

            var runner = new ExpressionRunner<T>(criteria);

            if (runner.DynamicCriteria == null)
            {
                return GetList<T>(runner.Conditions.Cast<ICriterion>(), options);
            }
            else
            {
                var result = GetList<T>(runner.Conditions);
                result = result.Where(r => runner.DynamicCriteria(r)).ToArray();

                var resultsToFetch = options.GetResultsToFetch();
                if (resultsToFetch.HasValue && resultsToFetch.HasValue && result.Count() > resultsToFetch)
                    result = result.Take(resultsToFetch.Value).ToArray();

                return result;
            }
        }

        /// <summary>
        /// Returns a list of entities with the specified type.
        /// </summary>
        public static IEnumerable<T> GetList<T>(IEnumerable<Criterion> criteria) where T : IEntity
        {
            return GetList<T>(criteria.ToArray());
        }

        public static IEnumerable<Entity> GetList(Type type, QueryOption[] queryOptions)
        {
            return GetList(type, new Criterion[0], queryOptions);
        }

        public static IEnumerable<Entity> GetList(Type type, IEnumerable<ICriterion> criteria, QueryOption[] queryOptions)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (criteria == null) criteria = new ICriterion[0];

            var method = typeof(Database).GetMethod(nameof(GetList),
                new[] { typeof(IEnumerable<ICriterion>), typeof(QueryOption[]) }).MakeGenericMethod(type);

            var result = new List<Entity>();

            foreach (Entity item in (IEnumerable)method.InvokeStatic(criteria, queryOptions))
                result.Add(item);

            return result;
        }

        public static IEnumerable<Entity> GetList(Type type, params Criterion[] conditions)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var method = typeof(Database).GetMethod(nameof(GetList), new[] { typeof(Criterion[]) }).MakeGenericMethod(type);

            var result = new List<Entity>();

            foreach (Entity item in (IEnumerable)method.InvokeStatic(new object[] { conditions }))
                result.Add(item);

            return result;
        }

        /// <summary>
        /// Gets the list of T objects from their specified IDs.
        /// </summary>
        public static IEnumerable<T> GetList<T>(IEnumerable<Guid> ids) where T : IEntity => ids.Select(Get<T>);

        /// <summary>
        /// Gets the list of objects from their specified IDs.
        /// </summary>
        public static IEnumerable<IEntity> GetList(Type type, IEnumerable<Guid> ids)
        {
            return ids.Select(id => Get(id, type));
        }
    }
}