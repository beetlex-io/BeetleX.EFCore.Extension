using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BeetleX.EFCore.Extension
{
    public static class WhereBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, bool exp,
                                                           Expression<Func<T, bool>> expr2)
        {
            if (exp)
            {
                var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
                return Expression.Lambda<Func<T, bool>>
                      (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
            }
            return expr1;
        }


        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, bool exp,
                                                             Expression<Func<T, bool>> expr2)
        {
            if (exp)
            {
                var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
                return Expression.Lambda<Func<T, bool>>
                      (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
            }
            return expr1;
        }
    }
}
