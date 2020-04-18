using MSharp.Framework;
using MSharp.Framework.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    partial class MSharpExtensions
    {
        public static object CompileAndInvoke(this Expression expression)
        {
            return Expression.Lambda(typeof(Func<>).MakeGenericType(expression.Type),
                expression).Compile().DynamicInvoke();
        }

        public static object GetValue(this Expression expression)
        {
            var result = expression.ExtractValue();
            if (result is IntEntity intEnt && intEnt.IsNew) return -1;
            if (result is Entity ent) return ent.GetId();
            return result;
        }

        static object ExtractValue(this Expression expression)
        {
            if (expression == null) return null;

            if (expression is ConstantExpression)
                return (expression as ConstantExpression).Value;

            if (expression is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;

                if (member is PropertyInfo prop)
                    return prop.GetValue(memberExpression.Expression.ExtractValue());

                else if (member is FieldInfo field)
                    return field.GetValue(memberExpression.Expression.ExtractValue());

                else
                    return expression.CompileAndInvoke();
            }
            else if (expression is MethodCallExpression methodExpression)
            {
                var method = methodExpression.Method;
                var instance = methodExpression.Object.ExtractValue();

                return method.Invoke(instance, methodExpression.Arguments.Select(a => ExtractValue(a)).ToArray());
            }
            else
            {
                return expression.CompileAndInvoke();
            }
        }

        public static FilterFunction ToFilterFunction(this ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal: return FilterFunction.Is;
                case ExpressionType.NotEqual: return FilterFunction.IsNot;
                case ExpressionType.GreaterThan: return FilterFunction.MoreThan;
                case ExpressionType.GreaterThanOrEqual: return FilterFunction.MoreThanOrEqual;
                case ExpressionType.LessThan: return FilterFunction.LessThan;
                case ExpressionType.LessThanOrEqual: return FilterFunction.LessThanOrEqual;
                default: throw new NotSupportedException(type + " is still not supported as a FilterFunction.");
            }
        }

        static string GetColumnName(PropertyInfo property)
        {
            var name = property.Name;
            if (!name.EndsWith("Id")) return name;
            if (property.PropertyType.IsAnyOf(typeof(Guid), typeof(Guid?)))
                name = name.TrimEnd(2);
            return name;
        }

        public static PropertyInfo[] GetPropertiesInPath(this MemberExpression memberInfo)
        {
            var empty = new PropertyInfo[0];

            // Handle the member:
            var property = memberInfo.Member as PropertyInfo;
            if (property == null) return empty;

            // Fix for overriden properties:
            try { property = memberInfo.Expression.Type.GetProperty(property.Name) ?? property; }
            catch { }

            if (CalculatedAttribute.IsCalculated(property)) return empty;

            if (memberInfo.Expression.IsSimpleParameter()) return new[] { property };

            if (memberInfo.Expression is MemberExpression exp)
            {
                // The expression is itself a member of something.
                var parentProperty = exp.GetPropertiesInPath();
                if (parentProperty.None()) return empty;

                return parentProperty.Concat(property).ToArray();
            }

            if (memberInfo.Expression.Type.IsNullable()) return new[] { property };

            return empty;
        }

        public static string GetDatabaseColumnPath(this MemberExpression memberInfo)
        {
            var path = memberInfo.GetPropertiesInPath();
            if (path.None()) return null;

            if (path.ExceptLast().Any(x => !x.PropertyType.Implements<IEntity>())) return null;

            return path.Select(GetColumnName).ToString(".");
        }

        static bool IsSimpleParameter(this Expression expression)
        {
            if (expression is ParameterExpression) return true;

            if (expression is UnaryExpression && (expression.NodeType == ExpressionType.Convert))
                return true;

            return false;
        }
    }
}