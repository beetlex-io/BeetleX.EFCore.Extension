using System;
using System.Collections.Generic;
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
    }
}
