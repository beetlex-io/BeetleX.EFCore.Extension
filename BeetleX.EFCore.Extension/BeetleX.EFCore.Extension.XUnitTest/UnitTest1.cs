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
        public void sqlbuilder()
        {
            SQL sql = new SQL("select * from customers");
            sql.Where<customers>(c => c.first_name.StartsWith("a"));
            var items = sql.List<customers, NorthWind>();
            Console.WriteLine(items.Count);
        }
    }
}
