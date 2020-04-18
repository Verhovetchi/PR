namespace MSharp.Framework.Data
{
    using System;
    using System.Linq.Expressions;

    public partial class DatabaseQuery<TEntity> : DatabaseQuery where TEntity : IEntity
    {
        public DatabaseQuery() : base(typeof(TEntity)) { }

        public DatabaseQuery<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null) return this;
            Criteria.AddRange(new CriteriaExtractor<TEntity>(criteria, throwOnNonConversion: true).Extract());
            return this;
        }

        public DatabaseQuery<TEntity> Include(Expression<Func<TEntity, object>> property)
        {
            base.Include(property.GetPropertyPath());
            return this;
        }

        public new DatabaseQuery<TEntity> Top(int rows)
        {
            TakeTop = rows;
            return this;
        }

        public new DatabaseQuery<TEntity> OrderBy(string property)
            => DatabaseQueryExtensions.OrderBy(this, property, descending: false);

        public new DatabaseQuery<TEntity> Where(params ICriterion[] criteria)
        {
            Criteria.AddRange(criteria);
            return this;
        }

        public TEntity WithMax(string property) => this.OrderByDescending(property).FirstOrDefault();

        public TEntity WithMin(string property) => OrderBy(property).FirstOrDefault();

        public TEntity WithMax(Expression<Func<TEntity, object>> property)
            => OrderByDescending(property).FirstOrDefault();

        public TEntity WithMin(Expression<Func<TEntity, object>> property)
            => OrderBy(property).FirstOrDefault();
    }
}
