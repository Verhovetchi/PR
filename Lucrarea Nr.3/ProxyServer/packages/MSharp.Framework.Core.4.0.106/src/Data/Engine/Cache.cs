﻿namespace MSharp.Framework.Data
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;

    enum CacheContainerStrategy { Universal, PerHttpRequest }

    /// <summary>
    /// Provides a cache of objects retrieved from the database.
    /// </summary>
    public class Cache
    {
        object SyncLock = new object();
        public static Cache Instance = new Cache();
        Dictionary<Type, Dictionary<string, IEntity>> Types = new Dictionary<Type, Dictionary<string, IEntity>>();
        Dictionary<Type, Dictionary<string, IEnumerable>> Lists = new Dictionary<Type, Dictionary<string, IEnumerable>>();

        #region Row Version

        // Note: This is to solve the following concurrency issue:
        // In highly concurrent systems the following scenario can happen.
        //      A GET call loads a record from DB.
        //      It then adds it to the cache.
        //      If that record is updated in between the two steps above, then bad data is added to the cache.        

        internal ConcurrentDictionary<Type, ConcurrentDictionary<string, long>> RowVersionCache
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();

        static Cache()
        {
            IsCachingEnabled = Config.Get<bool>("Database.Cache.Enabled", defaultValue: true);

            switch (Config.Get("Database.Cache.Strategy", "Universal"))
            {
                case "Universal":
                    Strategy = CacheContainerStrategy.Universal;
                    break;
                case "PerHttpRequest":
                    Strategy = CacheContainerStrategy.PerHttpRequest;
                    break;
                default:
                    throw new Exception("Invalid Database.Cache.Strategy setting is configured.");
            }
        }

        public virtual bool IsUpdatedSince(IEntity instance, DateTime since)
        {
            var type = instance.GetType();
            if (!CanCache(type)) return false;

            var cache = RowVersionCache.GetOrDefault(type);

            return cache?.GetOrDefault(instance.GetId().ToString()) > since.Ticks;
        }

        public virtual void UpdateRowVersion(IEntity entity)
        {
            var type = entity.GetType();
            if (!CanCache(type)) return;

            var cache = RowVersionCache.GetOrAdd(type, t => new ConcurrentDictionary<string, long>());
            cache[entity.GetId().ToString()] = DateTime.UtcNow.Ticks;
        }

        #endregion

        #region IsEnabled property

        static bool IsCachingEnabled;

        static CacheContainerStrategy Strategy = CacheContainerStrategy.Universal;

        public static bool CanCache(Type type) => CacheObjectsAttribute.IsEnabled(type) ?? IsCachingEnabled;

        #endregion

        /// <summary>
        /// Gets the current cache.
        /// </summary>
        public static Cache Current
        {
            get
            {
                switch (Strategy)
                {
                    case CacheContainerStrategy.Universal:
                        return Instance;
                    case CacheContainerStrategy.PerHttpRequest:
                        if (!Context.Current.HttpContextItemsAccessor.ItemsAvaiable)
                        {
                            // A not-null Cache is relied upon in many places. So return an empty cache:
                            return new Cache();
                        }
                        else
                        {
                            // Create a new one if it doesn't exist, or just return the one from current Http context.
                            return Context.Current.HttpContextItems["Current.Database.Cache"] as Cache ??
                               (Cache)(Context.Current.HttpContextItems["Current.Database.Cache"] = new Cache());
                        }
                    default:
                        throw new NotSupportedException(Strategy + " is not supported!");
                }
            }
        }

        Dictionary<string, IEntity> GetEntities(Type type)
        {
            var result = Types.TryGet(type);

            if (result == null)
            {
                lock (SyncLock)
                {
                    result = Types.TryGet(type);

                    if (result == null)
                    {
                        result = new Dictionary<string, IEntity>();
                        Types.Add(type, result);
                    }
                }
            }

            return result;
        }

        Dictionary<string, IEnumerable> GetLists(Type type, bool autoCreate = true)
        {
            var result = Lists.TryGet(type);

            if (result == null && autoCreate)
            {
                lock (SyncLock)
                {
                    result = Lists.TryGet(type);
                    if (result == null)
                    {
                        result = new Dictionary<string, IEnumerable>();
                        Lists.Add(type, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual IEntity Get(string id)
        {
            try
            {
                foreach (var type in Types.Keys.ToArray().Where(t => t.IsA<GuidEntity>()))
                {
                    var result = Get(type, id);
                    if (result != null) return result;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual TEntity Get<TEntity>(object id) where TEntity : IEntity
        {
            return (TEntity)Get(typeof(TEntity), id.ToStringOrEmpty());
        }

        /// <summary>
        /// Gets an entity from cache. Returns null if not found.
        /// </summary>
        public virtual IEntity Get(Type entityType, string id)
        {
            if (!CanCache(entityType)) return null;

            var entities = GetEntities(entityType);

            if (entities.ContainsKey(id))
            {
                try
                {
                    return entities[id];
                }
                catch (KeyNotFoundException)
                {
                    // A threading issue.
                    return Get(entityType, id);
                }
            }
            else
            {
                foreach (var type in entityType.Assembly.GetSubTypes(entityType))
                {
                    var result = Get(type, id);
                    if (result != null) return result;
                }

                return null;
            }
        }

        /// <summary>
        /// Adds a given entity to the cache.
        /// </summary>
        public virtual void Add(IEntity entity)
        {
            if (!CanCache(entity.GetType())) return;

            var entities = GetEntities(entity.GetType());

            lock (entities)
            {
                var id = entity.GetId().ToString();
                if (entities.ContainsKey(id))
                {
                    entities.GetOrDefault(id).Perform(x => x.InvalidateCachedReferences());
                    entities.Remove(id);
                }

                entities.Add(id, entity);

                ExpireLists(entity.GetType());
            }
        }

        /// <summary>
        /// Removes a given entity from the cache.
        /// </summary>
        public virtual void Remove(IEntity entity)
        {
            entity.InvalidateCachedReferences();

            SessionMemory.Remove(entity);

            if (!(entity is IApplicationEvent))
                foreach (var type in CacheDependentAttribute.GetDependentTypes(entity.GetType()))
                    Remove(type, invalidateCachedReferences: true);

            if (!CanCache(entity.GetType())) return;

            var entities = GetEntities(entity.GetType());

            lock (entities)
            {
                var id = entity.GetId().ToString();

                if (entities.ContainsKey(id)) entities.Remove(id);

                ExpireLists(entity.GetType());
            }

            if (this != Current) Current.Remove(entity);
        }

        /// <summary>
        /// Removes all entities of a given types from the cache.
        /// </summary>
        public virtual void Remove(Type type, bool invalidateCachedReferences = false)
        {
            if (!CanCache(type)) return;

            lock (SyncLock)
            {
                foreach (var inherited in Types.Keys.Where(t => t.BaseType == type).ToList())
                    Remove(inherited, invalidateCachedReferences);

            }

            if (Types.ContainsKey(type))
            {
                lock (SyncLock)
                {
                    if (Types.ContainsKey(type))
                    {
                        var entities = Types[type];
                        lock (entities)
                        {
                            Types.Remove(type);
                            ExpireLists(type);

                            if (invalidateCachedReferences)
                                entities.Do(e => e.Value.InvalidateCachedReferences());
                        }
                    }
                }
            }

            if (this != Current)
                Current.Remove(type, invalidateCachedReferences);
        }

        public virtual void ExpireLists(Type type)
        {
            if (!CanCache(type)) return;

            for (var parentType = type; parentType != typeof(Entity); parentType = parentType.BaseType)
            {
                var lists = GetLists(parentType, autoCreate: false);

                if (lists != null) lock (lists) lists.Clear();

            }

            if (this != Current) Current.ExpireLists(type);
        }

        public virtual IEnumerable GetList(Type type, string key)
        {
            if (!CanCache(type)) return null;

            var lists = GetLists(type);
            lock (lists)
            {
                if (lists.ContainsKey(key)) return lists[key];
                else return null;
            }
        }

        public virtual void AddList(Type type, string key, IEnumerable list)
        {
            if (!CanCache(type)) return;

            var lists = GetLists(type);

            lock (lists) lists[key] = list;

        }

        static internal string BuildQueryKey(Type type, IEnumerable<ICriterion> conditions, int? numberOfRecords)
        {
            var r = new StringBuilder();
            r.Append(type.GetCachedAssemblyQualifiedName());

            r.Append(':');

            if (conditions != null)
                foreach (var c in conditions)
                {
                    r.Append(c.ToString());
                    r.Append('|');
                }

            if (numberOfRecords.HasValue)
            {
                r.Append("|N:");
                r.Append(numberOfRecords);
            }

            return r.ToString();
        }

        public virtual void ClearAll()
        {
            lock (SyncLock)
            {
                RowVersionCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, long>>();
                Types.Clear();
                Lists.Clear();
            }
        }

        internal int CountAllObjects() => Types.Sum(t => t.Value.Count);

        internal static bool IsConcurrencyAware = Config.Get("Database:Concurrency.Aware.Cache", defaultValue: true);

        internal static DateTime? GetQueryTimestamp() => IsConcurrencyAware ? DateTime.UtcNow : default(DateTime?);
    }
}