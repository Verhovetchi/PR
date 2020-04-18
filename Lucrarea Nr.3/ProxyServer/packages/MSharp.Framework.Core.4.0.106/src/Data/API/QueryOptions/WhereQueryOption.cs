namespace MSharp.Framework.Data
{
    using System;

    public class WhereQueryOption : QueryOption
    {
        public string SqlCriteria { get; set; }
        public ICriterion[] Criterion { get; set; }

        public WhereQueryOption() { }

        public WhereQueryOption(string sqlCriteria)
        {
            SqlCriteria = sqlCriteria;
        }

        public WhereQueryOption(ICriterion[] criteria)
        {
            Criterion = criteria;
        }

        internal override void Configure(DatabaseQuery query)
        {
            if (SqlCriteria.HasValue())
                query.Where(new DirectDatabaseCriterion(SqlCriteria));

            foreach (var item in Criterion.OrEmpty()) query.Where(item);
        }
    }
}
