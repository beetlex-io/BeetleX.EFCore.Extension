using System;
using Xunit;
using System.Linq;
using System.Linq.Expressions;

namespace BeetleX.EFCore.Extension.XUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void select()
        {
            SQL sql = "select * from customers";
            sql.Add(" where city='Seattle'");
            var items = sql.List<customers, NorthWind>();
            Console.WriteLine(items.Count);
        }
        [Fact]
        public void where()
        {
            string city = "asdfsdf%";
            SQL sql = "select * from customers where true ";
            sql.Valid(city)?.Add("and city like @city", ("@city", city));
            var items = sql.List<customers, NorthWind>();
            Console.WriteLine(items.Count);
        }
        [Fact]
        public void Expression()
        {
            var db = new NorthWind();
            db.Customers.Where(p => p.company == "asdf");
        }
        [Fact]
        public void SELECT()
        {
            int[] ids = new int[] { 1, 2 };
            SELECT<customers> customer = new SELECT<customers>();
            customer.Where(c => c.id.NotIn(ids)).OrderBy(c => c.first_name.ASC());
            int count = customer.Count<NorthWind>();
            var items = customer.List<NorthWind>();
            Console.WriteLine(items.Count);
        }
        [Fact]
        public void DELETE()
        {

            SELECT<customers> customer = new SELECT<customers>();
            customer.Where(c => c.id > 10).OrderBy(c => c.first_name.ASC());
            int count = customer.Count<NorthWind>();
            var items = customer.List<NorthWind>();
            Console.WriteLine(items.Count);

            DELETE<customers> del = new DELETE<customers>();
            del.Where(c => c.id > 50);
            count = del.Count<NorthWind>();
            count = del.Execute<NorthWind>();


        }
        [Fact]
        public void Delete_with_dbset()
        {

            using (var db = new NorthWind())
            {
                var count = db.Customers.Delete(d => d.id > 5);
            }
        }
    }
}
