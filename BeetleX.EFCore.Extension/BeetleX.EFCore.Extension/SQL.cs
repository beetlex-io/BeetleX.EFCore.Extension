using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.EFCore.Extension
{

    public class UpdateSql<T>
    {
        public UpdateSql()
        {
            mTable = SqlHelper.GetTableName(typeof(T));
        }


        private List<Expression> mUpdateExpress = new List<Expression>();

        private Expression<Func<T, bool>> mWhere;

        private string mTable;

        public UpdateSql<T> Set(params Expression<Func<T, object>>[] exp)
        {
            mUpdateExpress.AddRange(exp);
            return this;
        }

        internal DbContext DB { get; set; }

        public UpdateSql<T> Where(Expression<Func<T, bool>> filter)
        {
            mWhere = filter;
            return this;
        }

        public int Execute<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return Execute(db);
            }
        }
        public int Execute()
        {
            if (DB == null)
                throw new Exception("The dbcontext cannot be null!");
            return Execute(DB);
        }
        public int Execute(DbContext db)
        {
            SqlHelper helper = new SqlHelper();
            SQL sql = new SQL($"UPDATE {mTable} SET ");
            for (int i = 0; i < mUpdateExpress.Count; i++)
            {
                if (i > 0)
                    sql.Add(",");
                helper.AddUpdateExpression(sql, ((LambdaExpression)mUpdateExpress[i]).Body);
            }
            if(mWhere !=null)
                sql.Where<T>(mWhere);
            return sql.Execute(db);
        }
    }


    public class DeleteSql<T>
    {
        private string mTable;

        private Expression<Func<T, bool>> mFilter;

        public string Sql { get; private set; }

        public DeleteSql()
        {
            mTable = SqlHelper.GetTableName(typeof(T));
        }

        public DeleteSql<T> Where(Expression<Func<T, bool>> filter)
        {
            mFilter = filter;
            return this;
        }

        public int Count<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return Count(db);
            }
        }

        public int Count(DbContext db)
        {
            SQL sql = $"SELECT count(*) FROM {mTable}";
            if (mFilter != null)
                sql.Where<T>(mFilter);
            return sql.ExecuteScalar<int>(db);

        }

        public int Execute<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return Execute(db);
            }
        }

        public int Execute(DbContext db)
        {
            SQL sql = $"DELETE FROM {mTable}";
            if (mFilter != null)
                sql.Where<T>(mFilter);
            return sql.Execute(db);

        }
    }


    public class SelectSql<T> where T : new()
    {
        private string mFields;

        private string mTable;

        private Expression<Func<T, bool>> mFilter;

        private Expression<Func<T, bool>> mOrderBy;

        public SelectSql(string fields = "*")
        {
            mFields = fields;
            mTable = SqlHelper.GetTableName(typeof(T));
        }

        public int Count<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return Count(db);
            }
        }

        public int Count(DbContext db)
        {
            SQL sql = $"SELECT count(*) FROM {mTable}";
            if (mFilter != null)
                sql.Where<T>(mFilter);
            return sql.ExecuteScalar<int>(db);

        }

        public SelectSql<T> Where(Expression<Func<T, bool>> filter)
        {
            mFilter = filter;
            return this;
        }

        public SelectSql<T> OrderBy(Expression<Func<T, bool>> order)
        {
            mOrderBy = order;
            return this;
        }

        public T ListFirst<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ListFirst(db);
            }
        }

        public T ListFirst(DbContext db)
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilter != null)
                sql.Where<T>(mFilter);
            if (mOrderBy != null)
                sql.OrderBy<T>(mOrderBy);
            return sql.ListFirst<T>(db);

        }

        public IList<T> List<DB>(Region region = null)
            where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return List(db, region);
            }
        }

        public IList<T> List(DbContext db, Region region = null)
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilter != null)
                sql.Where<T>(mFilter);
            if (mOrderBy != null)
                sql.OrderBy<T>(mOrderBy);
            return sql.List<T>(db);
        }

    }


    public class SQL
    {

        public SQL(string sql)
        {
            mBaseSql = sql;
            mCommand.AddSqlText(sql);
        }

        public static SQL operator +(string subsql, SQL sql)
        {
            sql.Add(subsql);
            return sql;
        }

        public static SQL operator +(SQL sql, string subsql)
        {
            sql.Add(subsql);
            return sql;
        }

        internal bool HasWhere { get; set; } = false;

        internal bool HasOrderBy { get; set; } = false;

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

        public SQL Valid(object data)
        {
            if (data == null)
                return null;
            else if (data is string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;
            }
            return this;
        }

        public SQL AddSpace()
        {
            Command.AddSqlText(" ");
            return this;
        }

        public SQL OrderByASC(params string[] fields)
        {
            if (!HasWhere)
            {
                AddSpace().Add("ORDER BY");
            }
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                    Add(",");
                AddSpace().Add(fields[i]).AddSpace().Add("ASC");
            }
            return this;
        }

        public SQL OrderByDESC(params string[] fields)
        {
            if (!HasOrderBy)
            {
                AddSpace().Add("ORDER BY");
            }
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                    Add(",");
                AddSpace().Add(fields[i]).AddSpace().Add("DESC");
            }
            return this;
        }

        public SQL OrderBy<T>(Expression<Func<T, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddOrderBy(this, filter.Body);
            return this;
        }

        public SQL OrderBy<T, T1>(Expression<Func<T, T1, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddOrderBy(this, filter.Body);
            return this;
        }

        public SQL OrderBy<T, T1, T2>(Expression<Func<T, T1, T2, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddOrderBy(this, filter.Body);
            return this;
        }

        public SQL Add(string sql, params (string, object)[] parameters)
        {
            mCommand.AddSqlText(sql);
            if (!HasWhere)
            {
                if (sql.IndexOf("where", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    HasWhere = true;
                }
            }
            if (!HasOrderBy)
            {
                if (sql.IndexOf("order by", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    HasOrderBy = true;
                }
            }
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    this.Parameter(p.Item1, p.Item2);
                }
            }
            return this;
        }

        public SQL Parameter(string name, object value)
        {
            mCommand.AddParameter(name, value);
            return this;
        }

        public int Execute<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return Execute(db);
            }
        }

        public int Execute(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var cmd = mCommand.CreateCommand(conn);
            return cmd.ExecuteNonQuery();
        }

        public T ExecuteScalar<T, DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ExecuteScalar<T>(db);
            }
        }

        public T ExecuteScalar<T>(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var cmd = mCommand.CreateCommand(conn);
            return (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T));
        }

        public T ListFirst<T, DB>() where DB : DbContext, new()
            where T : new()
        {
            using (var db = new DB())
            {
                return ListFirst<T>(db);
            }
        }

        public T ListFirst<T>(DbContext db) where T : new()
        {
            IList<T> result = (IList<T>)List(typeof(T), db, new Region(0, 1));
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

        public IList<T> List<T, DB>(Region region = null) where T : new()
            where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return List<T>(db, region);
            }
        }

        public IList<T> List<T>(DbContext db, Region region = null) where T : new()
        {
            return (IList<T>)List(typeof(T), db, region);
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
            EntityReader cr = EntityReader.GetReader(mBaseSql, type);
            int index = 0;
            Command cmd = GetCommand();
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
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

        public SQL Where<T>(Expression<Func<T, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddWhere(this, filter);
            return this;
        }

        public SQL Where<T, T1>(Expression<Func<T, T1, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddWhere(this, filter);
            return this;
        }

        public SQL Where<T, T1, T2>(Expression<Func<T, T1, T2, bool>> filter)
        {
            SqlHelper where = new SqlHelper();
            where.AddWhere(this, filter);
            return this;
        }

        public override string ToString()
        {
            return Command.Text.ToString();
        }
    }
}
