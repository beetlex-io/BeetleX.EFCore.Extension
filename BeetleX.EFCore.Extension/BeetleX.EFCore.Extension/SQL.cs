#if NETCOREAPP2_1
using BeetleX.Tracks;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.EFCore.Extension
{

    public class Update<T>
    {
        public Update()
        {
            mTable = SqlHelper.GetTableName(typeof(T));
        }

        private List<Expression> mUpdateExpress = new List<Expression>();

        private Expression<Func<T, bool>> mWhere;

        private string mTable;

        public Update<T> Set(params Expression<Func<T, object>>[] exp)
        {
            mUpdateExpress.AddRange(exp);
            return this;
        }

        internal DbContext DB { get; set; }

        public Update<T> Where(Expression<Func<T, bool>> filter)
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
            if (mWhere != null)
                sql.Where<T>(mWhere);
            return sql.Execute(db);
        }
    }


    public class Delete<T>
    {
        private string mTable;

        private Expression<Func<T, bool>> mFilter;

        public string Sql { get; private set; }

        public Delete()
        {
            mTable = SqlHelper.GetTableName(typeof(T));
        }

        public Delete<T> Where(Expression<Func<T, bool>> filter)
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


    public class Select<Entity> where Entity : new()
    {
        private string mFields;

        private string mTable;

        private List<Filter> mFilters = new System.Collections.Generic.List<Filter>();

        private Expression<Func<Entity, bool>> mOrderBy;

        public Select(params string[] fields)
        {
            if (fields == null || fields.Length == 0)
                mFields = "*";
            else
                mFields = string.Join(",", fields);
            mTable = SqlHelper.GetTableName(typeof(Entity));
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
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            return sql.ExecuteScalar<int>(db);

        }

        public static Select<Entity> operator &(Select<Entity> sql, Expression<Func<Entity, bool>> filter)
        {
            sql.And(filter);
            return sql;
        }

        public static Select<Entity> operator |(Select<Entity> sql, Expression<Func<Entity, bool>> filter)
        {
            sql.Or(filter);
            return sql;
        }

        public Select<Entity> Or(Expression<Func<Entity, bool>> filter)
        {
            mFilters.Add(new Filter { Type = "Or", Expression = filter });
            return this;
        }
        public Select<Entity> And(Expression<Func<Entity, bool>> filter)
        {
            mFilters.Add(new Filter { Type = "And", Expression = filter });
            return this;
        }

        public Select<Entity> Or(bool exp, Expression<Func<Entity, bool>> filter)
        {
            if (exp)
                mFilters.Add(new Filter { Type = "Or", Expression = filter });
            return this;
        }

        public Select<Entity> And(bool exp, Expression<Func<Entity, bool>> filter)
        {
            if (exp)
                mFilters.Add(new Filter { Type = "And", Expression = filter });
            return this;
        }

        public Select<Entity> OrderBy(Expression<Func<Entity, bool>> order)
        {
            mOrderBy = order;
            return this;
        }

        public Entity ListFirst<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ListFirst(db);
            }
        }

        public Entity ListFirst(DbContext db)
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            if (mOrderBy != null)
                sql.OrderBy<Entity>(mOrderBy);
            return sql.ListFirst<Entity>(db);

        }

        public IList<T> List<T, DB>(Region region = null) where T : new()
           where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return List<T>(db, region);
            }
        }

        public IList<Entity> List<DB>(Region region = null)
            where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return List<Entity>(db, region);
            }
        }

        public IList<Entity> List(DbContext db, Region region = null)
        {
            return List<Entity>(db, region);
        }

        public IList<T> List<T>(DbContext db, Region region = null)
            where T : new()
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            if (mOrderBy != null)
                sql.OrderBy<Entity>(mOrderBy);
            return sql.List<T>(db, region);
        }


        #region async

        public Task<int> CountAsync<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return CountAsync(db);
            }
        }

        public async Task<int> CountAsync(DbContext db)
        {
            SQL sql = $"SELECT count(*) FROM {mTable}";
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            return await sql.ExecuteScalarAsync<int>(db);

        }

        public Task<Entity> ListFirstAsync<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ListFirstAsync(db);
            }
        }

        public Task<Entity> ListFirstAsync(DbContext db)
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            if (mOrderBy != null)
                sql.OrderBy<Entity>(mOrderBy);
            return sql.ListFirstAsync<Entity>(db);

        }

        public Task<IList<T>> ListAsync<T, DB>(Region region = null) where T : new()
           where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ListAsync<T>(db, region);
            }
        }

        public Task<IList<Entity>> ListAsync<DB>(Region region = null)
            where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ListAsync<Entity>(db, region);
            }
        }

        public Task<IList<Entity>> ListAsync(DbContext db, Region region = null)
        {
            return ListAsync<Entity>(db, region);
        }

        public async Task<IList<T>> ListAsync<T>(DbContext db, Region region = null)
            where T : new()
        {
            SQL sql = $"SELECT {mFields} FROM {mTable}";
            if (mFilters.Count > 0)
                sql += " where ";
            SqlHelper where = new SqlHelper();
            for (int i = 0; i < mFilters.Count; i++)
            {
                var item = mFilters[i];
                if (i > 0)
                {
                    sql += $" {item.Type} ";
                }
                where.AddWhere(sql, item.Expression);
            }
            if (mOrderBy != null)
                sql.OrderBy<Entity>(mOrderBy);
            return await sql.ListAsync<T>(db, region);
        }

        #endregion
    }


    class Filter
    {
        public LambdaExpression Expression { get; set; }

        public string Type { get; set; }
    }


    public class SQL
    {

        public SQL(string sql)
        {
            mBaseSql = sql;
            mCommand.AddSqlText(sql);
            if(sql.IndexOf("where", StringComparison.OrdinalIgnoreCase)>=0)
            {
                HasWhere = true;
            }
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
        public static SQL operator +(SQL sql, ValueTuple<string, object> parameter)
        {
            return sql[parameter.Item1, parameter.Item2];

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
            if (!HasOrderBy)
            {
                AddSpace().Add("ORDER BY");
                HasOrderBy = true;
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
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);
                return cmd.ExecuteNonQuery();
#if NETCOREAPP2_1
            }
#endif
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
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);
                return (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T));
#if NETCOREAPP2_1
            }
#endif

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

        public void List(DbContext db, Region region, Action<IDataReader> handler)
        {
            if (region == null)
            {
                region = new Region(0, 100);
            }
            int index = 0;
            Command cmd = GetCommand();

            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var dbcmd = cmd.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(dbcmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteToObject"))
            {
#endif
                dbcmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(dbcmd);
                int count = 0;
                using (IDataReader reader = dbcmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (index >= region.Start)
                        {
                            handler?.Invoke(reader);
                            count++;
                            if (count >= region.Size)
                            {
                                cmd.DbCommand.Cancel();
                                reader.Dispose();
                                break;
                            }
                        }
                        index++;
                    }

                }
#if NETCOREAPP2_1
            }
#endif
        }

        internal IList List(Type type, DbContext db, Region region)
        {
            System.Type itemstype = System.Type.GetType("System.Collections.Generic.List`1");
            itemstype = itemstype.MakeGenericType(type);
            IList result;
            if (region == null)
            {
                region = new Region(0, 100);
            }
            result = (IList)Activator.CreateInstance(itemstype, region.Size);
            EntityReader cr = EntityReader.GetReader(mBaseSql, type);
            int index = 0;
            Command cmd = GetCommand();
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var dbcmd = cmd.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(dbcmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteToObject"))
            {
#endif
                dbcmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(dbcmd);
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
#if NETCOREAPP2_1
            }
#endif

        }

        private Command GetCommand()
        {
            return mCommand;
        }

        public SQL And()
        {
            Add(" AND ");
            return this;
        }

        public SQL Or()
        {
            Add(" OR ");
            return this;
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

        private void ModifyCommanToCount(DbCommand cmd)
        {
            var index = cmd.CommandText.IndexOf("select", 0, StringComparison.OrdinalIgnoreCase);
            if (index != 0)
                throw new Exception("The current sql statement is not select！");
            var fromindex = cmd.CommandText.IndexOf("from", 0, StringComparison.OrdinalIgnoreCase);
            if (fromindex < 1)
                throw new Exception("The current sql statement is not select！");
            StringBuilder sb = new StringBuilder();
            sb.Append(cmd.CommandText.Substring(0, 6));
            sb.Append(" count(*) ");
            sb.Append(cmd.CommandText.Substring(fromindex));
            cmd.CommandText = sb.ToString();

        }

        public int Count<DB>() where DB : DbContext, new()
        {
            using (DB db = new DB())
            {
                return Count(db);
            }
        }

        public int Count(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            var cmd = mCommand.CreateCommand(conn);
            ModifyCommanToCount(cmd);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);
                return (int)Convert.ChangeType(cmd.ExecuteScalar(), typeof(int));
#if NETCOREAPP2_1
            }
#endif
        }

        public override string ToString()
        {
            return Command.Text.ToString();
        }

        public Action<DbCommand> SQLExecuting { get; set; }

        #region async

        public Task<int> ExecuteAsync<DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ExecuteAsync(db);
            }
        }

        public async Task<int> ExecuteAsync(DbContext db)
        {

            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            var cmd = mCommand.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);
                return await cmd.ExecuteNonQueryAsync();
#if NETCOREAPP2_1
            }
#endif
        }

        public Task<T> ExecuteScalarAsync<T, DB>() where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return ExecuteScalarAsync<T>(db);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(DbContext db)
        {

            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            var cmd = mCommand.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);

                return (T)Convert.ChangeType(await cmd.ExecuteScalarAsync(), typeof(T));
#if NETCOREAPP2_1
            }
#endif

        }


        public Task<T> ListFirstAsync<T, DB>() where DB : DbContext, new()
    where T : new()
        {
            using (var db = new DB())
            {
                return ListFirstAsync<T>(db);
            }
        }

        public async Task<T> ListFirstAsync<T>(DbContext db) where T : new()
        {
            IList<T> result = (IList<T>)await ListAsync(typeof(T), db, new Region(0, 1));
            if (result.Count > 0)
                return result[0];
            return default(T);

        }

        internal async Task<object> ListFirstAsync(Type type, DbContext db)
        {
            IList result = await ListAsync(type, db, null);
            if (result.Count > 0)
                return result[0];
            return null;
        }

        public async Task<IList<T>> ListAsync<T, DB>(Region region = null) where T : new()
            where DB : DbContext, new()
        {
            using (var db = new DB())
            {
                return await ListAsync<T>(db, region);
            }
        }

        public async Task<IList<T>> ListAsync<T>(DbContext db, Region region = null) where T : new()
        {
            return (IList<T>)await ListAsync(typeof(T), db, region);
        }

        public async Task ListAsync(DbContext db, Region region, Action<IDataReader> handler)
        {
            if (region == null)
            {
                region = new Region(0, 100);
            }
            int index = 0;
            Command cmd = GetCommand();

            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            var dbcmd = cmd.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(dbcmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteToObject"))
            {
#endif
                dbcmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(dbcmd);
                int count = 0;
                using (DbDataReader reader = await dbcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (index >= region.Start)
                        {
                            handler?.Invoke(reader);
                            count++;
                            if (count >= region.Size)
                            {
                                cmd.DbCommand.Cancel();
                                reader.Dispose();
                                break;
                            }
                        }
                        index++;
                    }

                }
#if NETCOREAPP2_1
            }
#endif
        }

        internal async Task<IList> ListAsync(Type type, DbContext db, Region region)
        {
            System.Type itemstype = System.Type.GetType("System.Collections.Generic.List`1");
            itemstype = itemstype.MakeGenericType(type);
            IList result;
            if (region == null)
            {
                region = new Region(0, 100);
            }
            result = (IList)Activator.CreateInstance(itemstype, region.Size);
            EntityReader cr = EntityReader.GetReader(mBaseSql, type);
            int index = 0;
            Command cmd = GetCommand();
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            var dbcmd = cmd.CreateCommand(conn);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(dbcmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteToObject"))
            {
#endif
                dbcmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(dbcmd);
                using (DbDataReader reader = await dbcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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
#if NETCOREAPP2_1
            }
#endif

        }

        public Task<int> CountAsync<DB>() where DB : DbContext, new()
        {
            using (DB db = new DB())
            {
                return CountAsync(db);
            }
        }

        public async Task<int> CountAsync(DbContext db)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            var cmd = mCommand.CreateCommand(conn);
            ModifyCommanToCount(cmd);
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track(cmd.CommandText, CodeTrackLevel.Function, null, "EFCore", "ExecuteSQL"))
            {
#endif
                cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
                SQLExecuting?.Invoke(cmd);
                return (int)Convert.ChangeType(await cmd.ExecuteScalarAsync(), typeof(int));
#if NETCOREAPP2_1
            }
#endif
        }

        #endregion
    }
}
