namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static partial class DatabaseQueryExtensions
    {
        public static T Page<T>(this T query, int startIndex, int pageSize)
            where T : DatabaseQuery
        {
            if (pageSize < 1)
                throw new ArgumentException("Invalid PagingQueryOption specified. PageSize should be a positive number.");

            query.PageSize = pageSize;
            query.PageStartIndex = startIndex;
            return query;
        }

        public static T OrderBy<T>(this T query, string property, bool descending = false)
          where T : DatabaseQuery
        {
            return (T)query.OrderBy(property, descending);
        }

        public static T OrderByDescending<T>(this T query, string property)
            where T : DatabaseQuery
        {
            return OrderBy(query, property, descending: true);
        }

        public static T ThenBy<T>(this T query, string property, bool descending = false)
            where T : DatabaseQuery
        {
            return (T)query.ThenBy(property, descending);
        }

        public static T ThenByDescending<T>(this T query, string property)
            where T : DatabaseQuery
        {
            return (T)query.ThenBy(property, descending: true);
        }

        public static T Where<T>(this T query, string sqlCriteria)
            where T : DatabaseQuery
        {
            query.Criteria.Add(new DirectDatabaseCriterion(sqlCriteria));
            return query;
        }

        public static T Where<T>(this T query, IEnumerable<ICriterion> criteria)
           where T : DatabaseQuery
        {
            query.Where(criteria.ToArray());
            return query;
        }

        public static T Where<T>(this T query, IEnumerable<Criterion> criteria)
           where T : DatabaseQuery
        {
            query.Where(criteria.Cast<ICriterion>().ToArray());
            return query;
        }

        #region WhereIn 

        public static T WhereIn<T>(this T query, string myField, DatabaseQuery subquery, string targetField)
               where T : DatabaseQuery
               => (T)query.WhereIn(myField, subquery, targetField);

        public static T WhereIn<T>(this T query, DatabaseQuery subquery, string targetField)
               where T : DatabaseQuery
               => query.WhereIn<T>("ID", subquery, targetField);

        public static DatabaseQuery<T> WhereIn<T>(this DatabaseQuery<T> query, Expression<Func<T, object>> property, DatabaseQuery subquery, string targetField)
             where T : IEntity
             => (DatabaseQuery<T>)query.WhereIn(property.GetPropertyPath(), subquery, targetField);

        public static DatabaseQuery<T> WhereIn<T, K>(this DatabaseQuery<T> query, Expression<Func<T, object>> property,
            DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
             where T : IEntity
            where K : IEntity
        {
            return (DatabaseQuery<T>)query.WhereIn(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());
        }

        public static T WhereIn<T, K>(this T query, string property, DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : DatabaseQuery
           where K : IEntity
            => (T)query.WhereIn(property, subquery, targetProperty.GetPropertyPath());

        public static T WhereIn<T, K>(this T query, DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
            where T : DatabaseQuery
            where K : IEntity
            => query.WhereIn(subquery, targetProperty.GetPropertyPath());

        #endregion

        #region WhereNotIn
        public static T WhereNotIn<T>(this T query, string myField, DatabaseQuery subquery, string targetField)
            where T : DatabaseQuery
            => (T)query.WhereNotIn(myField, subquery, targetField);

        public static T WhereNotIn<T>(this T query, DatabaseQuery subquery, string targetField)
               where T : DatabaseQuery
               => query.WhereNotIn<T>("ID", subquery, targetField);

        public static DatabaseQuery<T> WhereNotIn<T>(this DatabaseQuery<T> query, Expression<Func<T, object>> property, DatabaseQuery subquery, string targetField)
             where T : IEntity
             => (DatabaseQuery<T>)query.WhereNotIn(property.GetPropertyPath(), subquery, targetField);

        public static DatabaseQuery<T> WhereNotIn<T, K>(this DatabaseQuery<T> query, Expression<Func<T, object>> property,
            DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
             where T : IEntity
            where K : IEntity
             => (DatabaseQuery<T>)query.WhereNotIn(property.GetPropertyPath(), subquery, targetProperty.GetPropertyPath());

        public static T WhereNotIn<T, K>(this T query, string property, DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : DatabaseQuery
           where K : IEntity
            => (T)query.WhereNotIn(property, subquery, targetProperty.GetPropertyPath());

        public static T WhereNotIn<T, K>(this T query, DatabaseQuery<K> subquery, Expression<Func<K, object>> targetProperty)
           where T : DatabaseQuery
           where K : IEntity
           => query.WhereNotIn(subquery, targetProperty.GetPropertyPath());
        #endregion

        public static string GetColumnExpression(this IDataProvider provider,
            string propertyName,
            string tableAlias = null)
        {
            if (propertyName.EndsWith("Id") && propertyName.Length > 2)
            {
                var association = provider.EntityType.GetProperty(propertyName.TrimEnd("Id"));

                if (association != null && !association.Defines<CalculatedAttribute>() &&
                    association.PropertyType.IsA<IEntity>())
                    propertyName = propertyName.TrimEnd("Id");
            }

            var result = provider.MapColumn(propertyName);

            if (tableAlias.HasValue()) result = tableAlias + "." + result.Split('.').Last();

            return result;
        }
    }
}
