using System;
using System.Linq;
using System.Linq.Expressions;
using MSharp.Framework.Data;
using static MSharp.Framework.Database;

namespace MSharp.Framework
{
    //partial class Database<TEntity>
    //{
    //    /// <summary>
    //    /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
    //    /// If no criteria is specified, the count of all instances will be returned.
    //    /// </summary>        
    //    public TOutput? Aggregate<TProperty, TOutput>(AggregateFunction function, Expression<Func<TEntity, TProperty>> property,
    //        params ICriterion[] criteria) where TOutput : struct
    //    {
    //        var criteriaItems = criteria.ToList();

    //        if (SoftDeleteAttribute.RequiresSoftdeleteQuery<TEntity>())
    //            criteriaItems.Add(new Criterion("IsMarkedSoftDeleted", false));

    //        var result = GetProvider(typeof(TEntity)).Aggregate(typeof(TEntity), function, property.GetPropertyName(), criteriaItems);

    //        return result.ToStringOrEmpty().TryParseAs<TOutput>();
    //    }

    //    /// <summary>
    //    /// Gets a list of entities of the given type from the database.
    //    /// </summary>
    //    public TOutput? Aggregate<TProperty, TOutput>(AggregateFunction function, Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, bool>> criteria) where TOutput : struct
    //    {
    //        var runner = ExpressionRunner<TEntity>.CreateRunner(criteria);

    //        if (runner.DynamicCriteria == null)
    //        {
    //            var conditions = runner.Conditions.OfType<ICriterion>().ToArray();
    //            return Aggregate<TProperty, TOutput>(function, property, conditions);
    //        }
    //        else throw new InvalidOperationException("Database Aggregate functions can be used only if the criteria can be evaluated in the database. If your criteria has to execute in CLR, then use Database.GetList() and run the LINQ aggregation on it.");
    //    }

    //    public TProperty? Max<TProperty>(Expression<Func<TEntity, TProperty>> property,
    //        params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Max, property, criteria);
    //    }

    //    public TProperty? Max<TProperty>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, bool>> criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Max, property, criteria);
    //    }

    //    public TProperty? Min<TProperty>(Expression<Func<TEntity, TProperty>> property,
    //        params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Min, property, criteria);
    //    }

    //    public TProperty? Min<TProperty>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, bool>> criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Min, property, criteria);
    //    }

    //    public TProperty? Sum<TProperty>(Expression<Func<TEntity, TProperty>> property,
    //        params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Sum, property, criteria);
    //    }

    //    public TProperty? Sum<TProperty>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, bool>> criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Sum, property, criteria);
    //    }

    //    #region Average

    //    public TProperty? Average<TProperty>(Expression<Func<TEntity, TProperty>> property,
    //        params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Average, property, criteria);
    //    }

    //    public TProperty? Average<TProperty>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, bool>> criteria) where TProperty : struct
    //    {
    //        return Aggregate<TProperty, TProperty>(AggregateFunction.Average, property, criteria);
    //    }

    //    public decimal? Average<TProperty>(Expression<Func<TEntity, int>> property,
    //        params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<int, decimal>(AggregateFunction.Average, property, criteria);
    //    }

    //    public decimal? Average<TProperty>(Expression<Func<TEntity, int?>> property,
    //       params ICriterion[] criteria) where TProperty : struct
    //    {
    //        return Aggregate<int?, decimal>(AggregateFunction.Average, property, criteria);
    //    }

    //    public decimal? Average(Expression<Func<TEntity, int>> property, Expression<Func<TEntity, bool>> criteria)
    //    {
    //        return Aggregate<int, decimal>(AggregateFunction.Average, property, criteria);
    //    }

    //    public decimal? Average(Expression<Func<TEntity, int?>> property, Expression<Func<TEntity, bool>> criteria)
    //    {
    //        return Aggregate<int?, decimal>(AggregateFunction.Average, property, criteria);
    //    }

    //    #endregion
    //}
}