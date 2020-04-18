namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    partial class DatabaseQuery
    {
        bool IsCacheable()
        {
            if (PageSize.HasValue) return false;

            if (Criteria.Except(typeof(DirectDatabaseCriterion)).Any(c => c.PropertyName.Contains(".")))
                return false; // This doesn't work with cache expiration rules.

            if (Criteria.OfType<DirectDatabaseCriterion>().Any(x => !x.IsCacheSafe))
                return false;

            // Do not cache a polymorphic call:
            if (NeedsTypeResolution()) return false;

            return true;
        }

        string GetCacheKey()
        {
            var r = new StringBuilder();
            r.Append(EntityType.GetCachedAssemblyQualifiedName());

            r.Append(':');

            foreach (var c in Criteria)
            {
                r.Append(c.ToString());
                r.Append('|');
            }

            if (TakeTop.HasValue) r.Append("|N:" + TakeTop);

            r.Append(OrderByParts.Select(x => x.ToString()).ToString(",").WithPrefix("|S:"));

            return r.ToString();
        }

        public IEnumerable<IEntity> GetList()
        {
            if (!IsCacheable()) return LoadFromDatabase();

            var cacheKey = GetCacheKey();

            var result = Cache.Current.GetList(EntityType, cacheKey)?.Cast<IEntity>();
            if (result != null)
            {
                LoadIncludedAssociations(result);
                return result;
            }

            result = LoadFromDatabaseAndCache();

            // If there is no transaction open, cache it:
            if (!Database.AnyOpenTransaction())
                Cache.Current.AddList(EntityType, cacheKey, result);

            return result;
        }

        List<IEntity> LoadFromDatabase()
        {
            List<IEntity> result;
            if (NeedsTypeResolution())
            {
                var queries = ResolveDataProviders().Select(p => p.GetList(this));
                result = queries.SelectMany(x => x).ToList();
            }
            else
                result = (Provider.GetList(this)).ToList();

            if (OrderByParts.None())
            {
                // TODO: If the entity is sortable by a single DB column, then automatically add that to the DB call.
                result.Sort();
            }

            LoadIncludedAssociations(result);

            return result;
        }

        void LoadIncludedAssociations(IEnumerable<IEntity> mainResult)
        {
            foreach (var associationHeirarchy in Includes)
            {
                associationHeirarchy.LoadAssociations(this, mainResult);
            }
        }

        List<IEntity> LoadFromDatabaseAndCache()
        {
            var timestamp = Cache.GetQueryTimestamp();

            var result = new List<IEntity>();

            foreach (var item in LoadFromDatabase())
            {
                var inCache = Cache.Current.Get(item.GetType(), item.GetId().ToString());
                if (inCache != null) result.Add(inCache);
                else
                {
                    EntityManager.RaiseOnLoaded(item);
                    Database.TryCache(item, timestamp);
                    result.Add(item);
                }
            }

            return result;
        }

        public int Count() => Provider.Count(this);

        public IEntity FirstOrDefault()
        {
            TakeTop = 1;
            return (Provider.GetList(this)).FirstOrDefault();
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public new IEnumerable<TEntity> GetList() => (base.GetList()).Cast<TEntity>();

        public new TEntity FirstOrDefault() => (TEntity)(base.FirstOrDefault());
    }
}
