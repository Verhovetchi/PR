namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class DatabaseQuery
    {
        public string MapColumn(string propertyName) => AliasPrefix + Provider.GetColumnExpression(propertyName);

        public string AliasPrefix { get; set; }

        DatabaseQuery WhereIn(string myField, DatabaseQuery subquery, string targetField)
        {
            return WhereSubquery(myField, subquery, targetField, "IN");
        }

        DatabaseQuery WhereNotIn(string myField, DatabaseQuery subquery, string targetField)
        {
            return WhereSubquery(myField, subquery, targetField, "NOT IN");
        }

        DatabaseQuery WhereSubquery(string myField, DatabaseQuery subquery, string targetField, string @operator)
        {
            subquery.AliasPrefix = "Subq" + Guid.NewGuid().ToString().Remove("-").Substring(0, 6);

            var sql = subquery.Provider
                .GenerateSelectCommand(subquery, subquery.MapColumn(targetField));

            sql = $"{MapColumn(myField)} {@operator} ({sql})";
            Criteria.Add(Criterion.FromSql(sql));

            foreach (var subQueryParam in subquery.Parameters)
                Parameters.Add(subQueryParam.Key, subQueryParam.Value);

            return this;
        }
    }
}