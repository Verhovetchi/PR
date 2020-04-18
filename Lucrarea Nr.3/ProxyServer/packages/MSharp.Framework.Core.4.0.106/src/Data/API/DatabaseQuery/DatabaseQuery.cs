namespace MSharp.Framework.Data
{
    using MSharp.Framework.Data.QueryOptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public partial class DatabaseQuery
    {
        Dictionary<string, AssociationInclusion> include = new Dictionary<string, AssociationInclusion>();

        public IDataProvider Provider { get; }

        public Type EntityType { get; private set; }

        public List<ICriterion> Criteria { get; } = new List<ICriterion>();

        public IEnumerable<AssociationInclusion> Includes => include.Values.ToArray();

        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public int PageStartIndex { get; set; }

        public int? PageSize { get; set; }

        public int? TakeTop { get; set; }

        public DatabaseQuery() { }

        public DatabaseQuery(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));

            if (!entityType.IsA<IEntity>())
                throw new ArgumentException(entityType.Name + " is not an IEntity.");

            EntityType = entityType;

            if (entityType.IsInterface)
                Provider = new Ado.Net.InterfaceDataProvider(entityType);
            else
                Provider = Database.GetProvider(entityType);
        }

        public DatabaseQuery Where(params ICriterion[] criteria)
        {
            Criteria.AddRange(criteria);
            return this;
        }

        public DatabaseQuery Include(string associations)
        {
            var immediateAssociation = associations.Split('.').First();
            var nestedAssociations = associations.Split('.').ExceptFirst().ToString(".");

            var property = EntityType.GetProperty(immediateAssociation)
                ?? throw new Exception(EntityType.Name + " does not have a property named " + immediateAssociation);

            if (!property.PropertyType.IsA<IEntity>())
                throw new Exception(EntityType.Name + "." + immediateAssociation + " is not an Entity type.");

            if (!include.ContainsKey(immediateAssociation))
                include.Add(immediateAssociation, AssociationInclusion.Create(property));

            if (nestedAssociations.HasValue())
                include[immediateAssociation].IncludeNestedAssociation(nestedAssociations);

            // TODO: Support one-to-many too
            return this;
        }

        public DatabaseQuery Include(IEnumerable<string> associations)
        {
            foreach (var item in associations)
                Include(item);

            return this;
        }

        public DatabaseQuery Top(int rows)
        {
            TakeTop = rows;
            return this;
        }

        public DatabaseQuery OrderBy(string property) => this.OrderBy(property, descending: false);

        public DatabaseQuery CloneFor(Type type)
        {
            var result = new DatabaseQuery(type)
            {
                PageStartIndex = PageStartIndex,
                TakeTop = TakeTop,
                PageSize = PageSize
            };

            result.Criteria.AddRange(Criteria);
            result.include.Add(include);
            result.Parameters.Add(Parameters);

            return result;
        }

        bool NeedsTypeResolution() => EntityType.IsInterface || EntityType == typeof(Entity);

        internal DatabaseQuery Config(QueryOption[] options)
        {
            options?.Do(x => x.Configure(this));
            return this;
        }
    }
}