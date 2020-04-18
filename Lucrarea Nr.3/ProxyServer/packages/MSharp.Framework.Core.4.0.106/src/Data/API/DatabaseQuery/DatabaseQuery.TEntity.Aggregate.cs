namespace MSharp.Framework.Data
{
    using System;
    using System.Linq.Expressions;

    partial class DatabaseQuery
    {
        public object Aggregate(QueryAggregateFunction function, string propertyName)
        {
            return Provider.Aggregate(this, function, propertyName);
        }
    }

    public enum QueryAggregateFunction
    {
        Max,
        Min,
        Sum,
        Average
    }

    partial class DatabaseQuery<TEntity>
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
        /// If no criteria is specified, the count of all instances will be returned.
        /// </summary>
        public TOutput? Aggregate<TProperty, TOutput>(QueryAggregateFunction function,
            Expression<Func<TEntity, TProperty>> property) where TOutput : struct
        {
            var result = Aggregate(function, property.GetPropertyPath());
            return result.ToStringOrEmpty().TryParseAs<TOutput>();
        }

        public TProperty? Max<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            Aggregate<TProperty, TProperty>(QueryAggregateFunction.Max, property);

        public TProperty? Min<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            Aggregate<TProperty, TProperty>(QueryAggregateFunction.Min, property);

        public TProperty? Sum<TProperty>(Expression<Func<TEntity, TProperty>> property) where TProperty : struct =>
            Aggregate<TProperty, TProperty>(QueryAggregateFunction.Sum, property);

        public TProperty? Average<TProperty>(Expression<Func<TEntity, TProperty>> property)
            where TProperty : struct =>
            Aggregate<TProperty, TProperty>(QueryAggregateFunction.Average, property);

        public decimal? Average<TProperty>(Expression<Func<TEntity, int>> property)
            where TProperty : struct =>
            Aggregate<int, decimal>(QueryAggregateFunction.Average, property);

        public decimal? Average<TProperty>(Expression<Func<TEntity, int?>> property)
            where TProperty : struct =>
            Aggregate<int?, decimal>(QueryAggregateFunction.Average, property);

        public decimal? Average(Expression<Func<TEntity, int>> property) =>
            Aggregate<int, decimal>(QueryAggregateFunction.Average, property);

        public decimal? Average(Expression<Func<TEntity, int?>> property) =>
            Aggregate<int?, decimal>(QueryAggregateFunction.Average, property);
    }
}
