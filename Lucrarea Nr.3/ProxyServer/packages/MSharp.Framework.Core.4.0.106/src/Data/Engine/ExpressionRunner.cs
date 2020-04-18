namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    internal class ExpressionRunner<T> where T : IEntity
    {
        static bool ShouldRecordDynamicQueries = Config.Get("Data.Access.Log.Custom.Queries", defaultValue: false);
        static bool EnforceOptimizedQueries = Config.Get("Data.Access.Enforce.Optimized.Queries", defaultValue: false);
        static List<string> CustomQueriesLog = new List<string>();
        IList<Expression> DynamicExpressions = new List<Expression>();

        internal List<Criterion> Conditions;
        internal Func<T, bool> DynamicCriteria;

        public ExpressionRunner(Expression<Func<T, bool>> criteria)
        {
            var extractor = new CriteriaExtractor<T>(criteria, EnforceOptimizedQueries);

            Conditions = extractor.Extract();
            DynamicExpressions.AddRange(extractor.NotConverted);

            if (DynamicExpressions.Any())
            {
                DynamicCriteria = GetDynamicCriteria(criteria);
                if (ShouldRecordDynamicQueries) RecordCustomQuery(criteria);
            }
        }

        Func<T, bool> GetDynamicCriteria(Expression<Func<T, bool>> criteria)
        {
            if (Conditions.None()) return criteria.Compile();

            var dynamicCriteria = DynamicExpressions.Skip(1).Aggregate<Expression, Expression<Func<T, bool>>>(
                    Expression.Lambda<Func<T, bool>>(DynamicExpressions.First(), criteria.Parameters),
                    (lamda, expression) =>
                    {
                        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(lamda.Body, expression), criteria.Parameters);
                    });

            return dynamicCriteria.Compile();
        }

        void RecordCustomQuery(Expression<Func<T, bool>> criteria)
        {
            if (CustomQueriesLog.Contains(criteria.ToString())) return;

            CustomQueriesLog.Add(criteria.ToString());
            var sb = new StringBuilder();
            sb.AppendLine(criteria.ToString());
            var st = new StackTrace(true);

            for (var i = 0; i < st.FrameCount; i++)
            {
                var frame = st.GetFrame(i);
                var ns = frame.GetMethod().DeclaringType.Namespace;
                if (ns == "App" || ns.IsEmpty()) // null for web pages
                    sb.Append(frame.ToString());
            }

            sb.AppendLine("--------------");
            System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Custom.Data.Access.Queries.txt", sb.ToString());
        }
    }
}