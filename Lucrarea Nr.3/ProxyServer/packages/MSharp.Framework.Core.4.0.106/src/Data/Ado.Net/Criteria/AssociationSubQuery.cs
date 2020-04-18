using System;
using System.Linq;
using System.Reflection;

namespace MSharp.Framework.Data
{
    public class AssociationSubQuery
    {
        public PropertyInfo Property { get; set; }
        public string ColumnName { get; set; }
        public string MappedSubquery { get; set; }
        public string TableAlias { get; set; }
        public string RecordIdExpression { get; set; }
        public string SelectClause { get; set; }

        public AssociationSubQuery(PropertyInfo property, string mappedSubquery)
        {
            Property = property;
            var subquerySplitted = mappedSubquery.Split(new[] { " WHERE " }, StringSplitOptions.RemoveEmptyEntries).Trim().ToArray();

            SelectClause = subquerySplitted[0].ToLines().Trim().ToString(" ");

            var whereClause = subquerySplitted[1];

            // In case it adds a soft delete filter:
            whereClause = whereClause.Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries).First();

            var whereClauseSplitted = whereClause.Split('=').Trim().ToArray();

            ColumnName = whereClauseSplitted[0];
            TableAlias = whereClauseSplitted.Last().Split('.').ExceptLast().ToString(".").Trim();

            RecordIdExpression = whereClauseSplitted[1];
        }

        public override string ToString() => SelectClause + " WHERE " + ColumnName + " = " + RecordIdExpression;
    }
}