using System;
using Xunit;
using System.Linq;
using System.Linq.Expressions;

namespace BeetleX.EFCore.Extension.XUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void BaseSQL()
        {
            string select = "select * from employees";
            SQL sql = select;
            var items = sql.List<employees, NorthWind>();
            Assert.Equal<int>(9, items.Count);

            sql = select;
            sql.Add(" where id in (@p1,@p2) order by id asc", ("@p1", 1), ("@p2", 2));
            items = sql.List<employees, NorthWind>();
            Assert.Equal<int>(1, items[0].id);
            Assert.Equal<int>(2, items[1].id);


            sql = select;
            sql.Where<employees>(e => e.id == 3);
            var item = sql.ListFirst<employees, NorthWind>();
            Assert.Equal<int>(3, item.id);

            sql = select;
            sql.OrderBy<employees>(p => p.id.DESC());
            item = sql.ListFirst<employees, NorthWind>();
            Assert.Equal<int>(9, item.id);
        }
        [Fact]
        public void Update()
        {
            UpdateSql<employees> update = new UpdateSql<employees>();
            update.Set(f => f.fax_number == "123").Where(f => f.id == 1).Execute<NorthWind>();

            SelectSql<employees> list = new SelectSql<employees>();
            list.Where(f => f.id == 1);
            var item = list.ListFirst<NorthWind>();
            Assert.Equal("123", item.fax_number);

            using (var db = new NorthWind())
            {
                db.Customers.Update(c => c.city == "gz").Execute();
                SelectSql<customers> lstCustomers = new SelectSql<customers>();
                foreach (var customer in lstCustomers.List(db))
                {
                    Assert.Equal("gz", customer.city);
                }

            }
          
        }
        [Fact]
        public void Delete()
        {
            using (var db = new NorthWind())
            {
                db.Customers.Delete(f => f.id.In(1, 2, 3));
            }
        }
    }
}
