using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BeetleX.EFCore.Extension
{
    public static class SqlExtensionMethods
    {
        public static bool ASC<T>(this T obj)
        {
            return true;
        }

        public static bool DESC<T>(this T obj)
        {
            return true;
        }

        public static bool In<T>(this T obj, params T[] data)
        {
            return true;
        }

        public static bool NotIn<T>(this T obj, params T[] data)
        {
            return true;
        }

        public static int Delete<T>(this DbSet<T> entity, Expression<Func<T, bool>> filter) where T : class
        {
            DELETE<T> del = new DELETE<T>();
            del.Where(filter);
            var db = entity.GetService<ICurrentDbContext>().Context;
            return del.Execute(db);
        }
    }
}
