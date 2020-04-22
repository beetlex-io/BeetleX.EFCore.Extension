using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace BeetleX.EFCore.Extension
{

    public class SqlHelper
    {
        static SqlHelper()
        {
            mContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            mStartWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            mEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        }

        private static MethodInfo mContains;

        private static MethodInfo mStartWith;

        private static MethodInfo mEndsWith;

        private int mID = 1;

        public string Prefix { get; set; } = "@";

        private string GetParameterName()
        {
            var id = System.Threading.Interlocked.Increment(ref mID);
            return $"{Prefix}P{id}";
        }

        private void OnBuilderOrderByMethodExpression(SQL sql, MethodCallExpression methodCall)
        {
            MemberExpression member = (MemberExpression)methodCall.Object;
            var propert = (PropertyInfo)member.Member;
            string name = $"{GetTableName(propert.PropertyType)}.{propert.Name}";
            if(methodCall.Method.Name== "ASC")
            {
                sql.OrderByASC(name);
            }
            else if(methodCall.Method.Name == "DESC")
            {
                sql.OrderByDESC(name);
            }
            throw new Exception("Unsupported order by method " + methodCall.Method.Name);
        }

        public void AddOrderBy(SQL sql, Expression exp)
        {
            if (exp is BinaryExpression binaryExpression)
            {
                AddOrderBy(sql, binaryExpression.Left);
                AddOrderBy(sql, binaryExpression.Right);
            }
            else if (exp is MethodCallExpression methodCall)
            {
                OnBuilderOrderByMethodExpression(sql, methodCall);
            }
            else
            {
                throw new Exception($"Unsupported {exp}");
            }
        }

        public void AddWhere(SQL sql, LambdaExpression exp)
        {
            if (!sql.HasWhere)
                sql.AddSpace().Add("WHERE");
            OnBuilderExpression(sql, exp.Body);
        }

        internal static string GetTableName(Type type)
        {
            var table = type.GetCustomAttribute<TableAttribute>(false);
            return table != null ? table.Name : type.Name;

        }

        private void OnBuilderMethodExpression(SQL sql, MethodCallExpression methodCall)
        {
            MemberExpression member = (MemberExpression)methodCall.Object;
            var propert = (PropertyInfo)member.Member;
            sql.AddSpace().Add(GetTableName(propert.DeclaringType)).Add(".").Add(propert.Name);
            sql.AddSpace().Add("LIKE");
            object value = GetExpressValue(methodCall.Arguments[0]);
            if (methodCall.Method == mContains)
            {
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"%{value}%"));
                return;

            }
            if (methodCall.Method == mStartWith)
            {
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"{value}%"));
                return;
            }
            if (methodCall.Method == mEndsWith)
            {
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"%{value}"));
                return;
            }
            throw new Exception("Unsupported method call: " + methodCall.Method.Name);
        }

        private void OnBuilderLeftExpression(SQL sql, Expression left)
        {
            if (left is BinaryExpression binaryExpression)
            {
                sql.AddSpace().Add("(");
                OnBuilderLeftExpression(sql, binaryExpression.Left);
                OnBuilderRightExpression(sql, left, binaryExpression.Right);
                sql.AddSpace().Add(")");
            }
            else if (left is MemberExpression member)
            {
                var propert = (PropertyInfo)member.Member;
                sql.AddSpace().Add(GetTableName(propert.DeclaringType)).Add(".");
                sql.AddSpace().Add(propert.Name);
            }
            else if (left is MethodCallExpression methodCall)
            {
                OnBuilderMethodExpression(sql, methodCall);
            }
            else
            {
                throw new Exception($"Unsupported {left}");
            }
        }
        private void OnBuilderRightExpression(SQL sql, Expression left, Expression right)
        {
            if (right is BinaryExpression binaryExpression)
            {
                sql.AddSpace().Add(NodeTypeToString(left.NodeType, false));
                sql.AddSpace().Add("(");
                OnBuilderLeftExpression(sql, binaryExpression.Left);
                OnBuilderRightExpression(sql, binaryExpression, binaryExpression.Right);
                sql.AddSpace().Add(")");
            }
            else if (right is ConstantExpression constant)
            {
                if (constant.Value == null)
                {
                    sql.AddSpace().Add(NodeTypeToString(left.NodeType, true));
                }
                else
                {
                    sql.AddSpace().Add(NodeTypeToString(left.NodeType, false));
                    string name = GetParameterName();
                    sql.AddSpace().Add(name, (name, constant.Value));

                }
            }
            else if (right is MemberExpression member)
            {
                var data = GetValue(member);
                if (data == null)
                {
                    sql.AddSpace().Add(NodeTypeToString(left.NodeType, true));
                }
                else
                {
                    sql.AddSpace().Add(NodeTypeToString(left.NodeType, false));
                    string name = GetParameterName();
                    sql.AddSpace().Add(name, (name, data));
                }
            }
            else if (right is MethodCallExpression methodCall)
            {
                sql.AddSpace().Add(NodeTypeToString(left.NodeType, false));
                OnBuilderMethodExpression(sql, methodCall);
            }
            else
            {
                throw new Exception($"Unsupported {right}");
            }
        }

        private void OnBuilderExpression(SQL sql, Expression exp)
        {
            sql.AddSpace().Add("(");
            if (exp is BinaryExpression binaryExpression)
            {
                sql.AddSpace().Add("(");
                OnBuilderLeftExpression(sql, binaryExpression.Left);
                OnBuilderRightExpression(sql, exp, binaryExpression.Right);
                sql.AddSpace().Add(")");
            }
            else
            {
                if (exp is MethodCallExpression methodCall)
                {
                    OnBuilderMethodExpression(sql, methodCall);
                }
            }
            sql.AddSpace().Add(")");
        }


        public string ToSql(SQL sql, LambdaExpression expression)
        {
            return Recurse(sql, expression.Body, true);
        }

        private string Recurse(SQL sql, Expression expression, bool isUnary = false, bool quote = true)
        {
            if (expression is UnaryExpression)
            {
                var unary = (UnaryExpression)expression;
                var right = Recurse(sql, unary.Operand, true);
                return "(" + NodeTypeToString(unary.NodeType, right == "NULL") + " " + right + ")";
            }
            if (expression is BinaryExpression)
            {
                var body = (BinaryExpression)expression;
                var right = Recurse(sql, body.Right);
                return "(" + Recurse(sql, body.Left) + " " + NodeTypeToString(body.NodeType, right == "NULL") + " " + right + ")";
            }
            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                return ValueToString(constant.Value, isUnary, quote);
            }
            if (expression is MemberExpression)
            {
                var member = (MemberExpression)expression;

                if (member.Member is PropertyInfo)
                {
                    var property = (PropertyInfo)member.Member;
                    var colName = property.Name;
                    if (isUnary && member.Type == typeof(bool))
                    {
                        return "([" + colName + "] = 1)";
                    }
                    return "[" + colName + "]";
                }
                if (member.Member is FieldInfo)
                {
                    return ValueToString(GetValue(member), isUnary, quote);
                }
                throw new Exception($"Expression does not refer to a property or field: {expression}");
            }
            if (expression is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)expression;
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return "(" + Recurse(sql, methodCall.Object) + " LIKE '%" + Recurse(sql, methodCall.Arguments[0], quote: false) + "%')";
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return "(" + Recurse(sql, methodCall.Object) + " LIKE '" + Recurse(sql, methodCall.Arguments[0], quote: false) + "%')";
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return "(" + Recurse(sql, methodCall.Object) + " LIKE '%" + Recurse(sql, methodCall.Arguments[0], quote: false) + "')";
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                    {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    }
                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                    {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    }
                    else
                    {
                        throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                    }
                    var values = (IEnumerable)GetValue(collection);
                    var concated = "";
                    foreach (var e in values)
                    {
                        concated += ValueToString(e, false, true) + ", ";
                    }
                    if (concated == "")
                    {
                        return ValueToString(false, true, false);
                    }
                    return "(" + Recurse(sql, property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
                }
                throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }
            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        public string ValueToString(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "(1=1)" : "(1=0)";
                }
                return (bool)value ? "1" : "0";
            }
            return value.ToString();
        }

        private static bool IsEnumerableType(Type type)
        {
            return type
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private static object GetExpressValue(Expression exp)
        {
            if (exp is ConstantExpression)
            {
                var constant = (ConstantExpression)exp;
                return constant.Value;
            }
            return GetValue(exp);
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static string NodeTypeToString(ExpressionType nodeType, bool rightIsNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return rightIsNull ? "IS" : "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
            }
            throw new Exception($"Unsupported node type: {nodeType}");
        }
    }
}
