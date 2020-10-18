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

    class SqlHelper
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

        [ThreadStatic]
        private static int mID = 1;

        public string Prefix { get; set; } = "@";

        private string GetParameterName()
        {
            if (mID > 1000)
                mID = 1;
            else
                mID++;
            var id = mID;
            return $"{Prefix}P{id}";
        }

        internal static string GetTableName(Type type)
        {
            var table = type.GetCustomAttribute<TableAttribute>(false);
            return table != null ? table.Name : type.Name;

        }

        internal static string GetPropertyName(Type type, PropertyInfo property)
        {
            string table = GetTableName(type);
            ColumnAttribute col = property.GetCustomAttribute<ColumnAttribute>(false);
            string name = col != null ? col.Name : property.Name;
            return $"{table}.{name}";
        }

        public void AddUpdateExpression(SQL sql, Expression exp)
        {
            if (exp is UnaryExpression unary)
            {
                AddUpdateExpression(sql, unary.Operand);
                return;
            }
            BinaryExpression binaryExpression = (BinaryExpression)exp;
            MemberExpression left = (MemberExpression)binaryExpression.Left;
            var property = (PropertyInfo)left.Member;
            object value = null;
            if (binaryExpression.Right is ConstantExpression constant)
            {
                value = constant.Value;
            }
            else if (binaryExpression.Right is MemberExpression member)
            {
                value = GetValue(member);
            }
            else
            {
                throw new Exception($"Unsupported update expression {binaryExpression.Right}");
            }
            string name = property.Name;//GetPropertyName(property.DeclaringType, property);
            if (exp.NodeType == ExpressionType.Equal)//=
            {
                sql.AddSpace().Add(name).Add("=");
                var pname = GetParameterName();
                sql.Add(pname, (pname, value));
            }
            else if (exp.NodeType == ExpressionType.Add)//+
            {
                sql.AddSpace().Add(name).Add("=").Add(name).Add("+");
                var pname = GetParameterName();
                sql.Add(pname, (pname, value));
            }
            else if (exp.NodeType == ExpressionType.Divide)// /
            {
                sql.AddSpace().Add(name).Add("=").Add(name).Add("/");
                var pname = GetParameterName();
                sql.Add(pname, (pname, value));
            }
            else if (exp.NodeType == ExpressionType.Multiply)// *
            {
                sql.AddSpace().Add(name).Add("=").Add(name).Add("*");
                var pname = GetParameterName();
                sql.Add(pname, (pname, value));
            }
            else if (exp.NodeType == ExpressionType.Negate) //-
            {
                sql.AddSpace().Add(name).Add("=").Add(name).Add("-");
                var pname = GetParameterName();
                sql.Add(pname, (pname, value));
            }
            else
            {
                throw new Exception($"Unsupported update expression {exp}");
            }
        }

        private void OnBuilderOrderByMethodExpression(SQL sql, MethodCallExpression methodCall)
        {
            MemberExpression member = (MemberExpression)methodCall.Arguments[0];
            var propert = (PropertyInfo)member.Member;
            string name = GetPropertyName(member.Expression.Type, propert);
            if (methodCall.Method.Name == "ASC")
            {
                sql.OrderByASC(name);
                return;
            }
            else if (methodCall.Method.Name == "DESC")
            {
                sql.OrderByDESC(name);
                return;
            }
            throw new Exception("Unsupported order by method " + methodCall.Method.Name);
        }

        public void AddOrderBy(SQL sql, Expression exp)
        {
            if (!sql.HasOrderBy)
            {
                sql.AddSpace().Add("ORDER BY");
                sql.HasOrderBy = true;
            }
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
            {
                sql.AddSpace().Add("WHERE");
                sql.HasWhere = true;
            }
            OnBuilderExpression(sql, exp.Body);
        }



        private void OnBuilderMethodExpression(SQL sql, MethodCallExpression methodCall)
        {
            MemberExpression member = (MemberExpression)methodCall.Object;


            if (methodCall.Method == mContains)
            {
                var propert = (PropertyInfo)member.Member;
                string name = GetPropertyName(propert.DeclaringType, propert);
                sql.AddSpace().Add(name);
                sql.AddSpace().Add("LIKE");
                object value = GetExpressValue(methodCall.Arguments[0]);
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"%{value}%"));
                return;

            }
            if (methodCall.Method == mStartWith)
            {
                var propert = (PropertyInfo)member.Member;
                string name = GetPropertyName(propert.DeclaringType, propert);
                sql.AddSpace().Add(name);
                sql.AddSpace().Add("LIKE");
                object value = GetExpressValue(methodCall.Arguments[0]);
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"{value}%"));
                return;
            }
            if (methodCall.Method == mEndsWith)
            {
                var propert = (PropertyInfo)member.Member;
                string name = GetPropertyName(propert.DeclaringType, propert);
                sql.AddSpace().Add(name);
                sql.AddSpace().Add("LIKE");
                object value = GetExpressValue(methodCall.Arguments[0]);
                string pname = GetParameterName();
                sql.AddSpace().Add(pname, (pname, $"%{value}"));
                return;
            }
            if (methodCall.Method.Name == "In")
            {
                MemberExpression memberExpression = (MemberExpression)methodCall.Arguments[0];
                var propert = (PropertyInfo)memberExpression.Member;
                var type = memberExpression.Expression.Type;
                var value = (IEnumerable)GetExpressValue(methodCall.Arguments[1]);
                string name = GetPropertyName(type, propert);
                sql.AddSpace().Add(name).AddSpace().Add("IN(");
                int i = 0;
                foreach (var item in value)
                {
                    if (i > 0)
                        sql.Add(",");
                    string pname = GetParameterName();
                    sql.Add(pname, (pname, item));
                    i++;
                }
                sql.AddSpace().Add(")");
                return;
            }
            if (methodCall.Method.Name == "NotIn")
            {
                MemberExpression memberExpression = (MemberExpression)methodCall.Arguments[0];
                var propert = (PropertyInfo)memberExpression.Member;
                var type = memberExpression.Expression.Type;
                var value = (IEnumerable)GetExpressValue(methodCall.Arguments[1]);
                string name = GetPropertyName(type, propert);
                sql.AddSpace().Add(name).AddSpace().Add("NOT IN(");
                int i = 0;
                foreach (var item in value)
                {
                    if (i > 0)
                        sql.Add(",");
                    string pname = GetParameterName();
                    sql.Add(pname, (pname, item));
                    i++;
                }
                sql.AddSpace().Add(")");
                return;
            }
            throw new Exception($"Unsupported method call: {methodCall}");
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
                string name = GetPropertyName(propert.DeclaringType, propert);
                sql.AddSpace().Add(name);
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
                    return rightIsNull ? "IS NULL" : "=";
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
