namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    partial class DatabaseQuery
    {
        internal const bool DESC = true;
        public List<OrderByPart> OrderByParts = new List<OrderByPart>();

        public class OrderByPart
        {
            public string Property;
            public bool Descending;

            public override string ToString() => Property + (DESC ? ".DESC" : null);
        }

        public DatabaseQuery OrderBy(string property, bool descending)
        {
            if (OrderByParts.Any())
                throw new Exception("There is already an OrderBy part added. Use ThenBy().");

            return ThenBy(property, descending);
        }

        public DatabaseQuery ThenBy(string property, bool descending)
        {
            OrderByParts.Add(new OrderByPart { Property = property, Descending = descending });
            return this;
        }
    }

    partial class DatabaseQuery<TEntity>
    {
        public DatabaseQuery<TEntity> ThenBy(Expression<Func<TEntity, object>> property, bool descending = false)
        {
            var propertyPath = property.Body.GetPropertyPath();
            if (propertyPath.IsEmpty() || propertyPath.Contains("."))
                throw new Exception($"Unsupported OrderBy expression. The only supported format is \"x => x.Property\". You provided: {property}");

            return (DatabaseQuery<TEntity>)ThenBy(propertyPath, descending);
        }

        public DatabaseQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> property) => OrderBy(property, DESC);

        public DatabaseQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> property, bool descending = false)
        {
            if (OrderByParts.Any())
                throw new Exception("There is already an OrderBy part added. Use ThenBy().");

            return ThenBy(property, descending);
        }

        public DatabaseQuery<TEntity> ThenByDescending(Expression<Func<TEntity, object>> property) => ThenBy(property, DESC);
    }
}
