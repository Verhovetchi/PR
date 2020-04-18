using System;
using System.Linq;
using System.Linq.Expressions;
using MSharp.Framework.Data;

namespace MSharp.Framework
{
    public static partial class Database
    {
        /// <summary>
        /// Gets a list of entities of the given type from the database with the specified type matching the specified criteria.
        /// If no criteria is specified, the count of all instances will be returned.
        /// </summary>        
        public static int Count<T>(params Criterion[] criteria) where T : IEntity
        {
            var criteriaItems = criteria.ToList();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery<T>())
                criteriaItems.Add(new Criterion("IsMarkedSoftDeleted", false));

            return GetProvider(typeof(T)).Count(new DatabaseQuery(typeof(T)).Where(criteriaItems));
        }

        /// <summary>
        /// Gets a list of entities of the given type from the database.
        /// </summary>
        public static int Count<T>(Expression<Func<T, bool>> criteria, params QueryOption[] options) where T : IEntity
        {
            var runner = new ExpressionRunner<T>(criteria);

            if (runner.DynamicCriteria == null)
            {
                var conditions = runner.Conditions.OfType<ICriterion>().ToList();

                if (SoftDeleteAttribute.RequiresSoftdeleteQuery<T>())
                    conditions.Add(new Criterion("IsMarkedSoftDeleted", false));

                return GetProvider(typeof(T)).Count(new DatabaseQuery<T>().Where(conditions).Config(options));
            }
            else
            {
                return GetList<T>(criteria, options).Count();
            }
        }
    }
}