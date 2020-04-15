using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.EFCore.Extension
{
    public class SQL
    {
        public SQL(string sql)
        {
            mBaseSql = sql;
            mCommand.AddSqlText(sql);
        }
        public static SQL operator +(string subsql, SQL sql)
        {
            sql.AddSql(subsql);
            return sql;
        }
        public static SQL operator +(SQL sql, string subsql)
        {
            sql.AddSql(subsql);
            return sql;
        }

        public SQL this[string name, object value]
        {
            get
            {
                return Parameter(name, value);
            }
        }

        public static implicit operator SQL(string sql)
        {
            return new SQL(sql);
        }

        private string mBaseSql;

        private Command mCommand = new Command("");

        public Command Command
        {
            get
            {
                return mCommand;
            }
        }

        public SQL AddSql(string sql)
        {
            mCommand.AddSqlText(sql);
            return this;
        }
        public SQL Parameter(string name, object value)
        {
            mCommand.AddParameter(name, value);
            return this;
        }

        public int Execute(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            var cmd = mCommand.CreateCommand(conn);
            return cmd.ExecuteNonQuery();
        }

        public T GetValue<T>(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            var cmd = mCommand.CreateCommand(conn);
            return (T)cmd.ExecuteScalar();
        }
        public T ListFirst<T>(DbContext db) where T : new()
        {
            IList<T> result = (IList<T>)List(typeof(T), db, new Region(0, 2));
            if (result.Count > 0)
                return result[0];
            return default(T);

        }

        internal object ListFirst(Type type, DbContext db)
        {
            IList result = List(type, db, null);
            if (result.Count > 0)
                return result[0];
            return null;
        }

        /// <summary>
        /// 在指定数据库上执行SQL并返回记录列表
        /// </summary>
        /// <typeparam name="T">记录类型</typeparam>
        /// <param name="cc">数据库访问上下文</param>
        /// <returns>对象列表</returns>
        public IList<T> List<T>(DbContext db) where T : new()
        {
            return (IList<T>)List(typeof(T),db, null);
        }

        internal IList List(Type type, DbContext db, Region region)
        {

            System.Type itemstype = System.Type.GetType("System.Collections.Generic.List`1");
            itemstype = itemstype.MakeGenericType(type);
            IList result;
            if (region == null)
            {
                region = new Region(0, 10);
            }
            result = (IList)Activator.CreateInstance(itemstype, region.Size);
            CommandReader cr = CommandReader.GetReader(mBaseSql, type);
            int index = 0;
            Command cmd = GetCommand();
            var conn = db.Database.GetDbConnection();
            var dbcmd = cmd.CreateCommand(conn);
            using (IDataReader reader = dbcmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    if (index >= region.Start)
                    {
                        object item = Activator.CreateInstance(type);
                        cr.ReaderToObject(reader, item);
                        result.Add(item);
                        if (result.Count >= region.Size)
                        {
                            cmd.DbCommand.Cancel();
                            reader.Dispose();
                            break;
                        }
                    }
                    index++;
                }

            }

            return result;

        }

        private Command GetCommand()
        {
            return mCommand;
        }
    }
}
