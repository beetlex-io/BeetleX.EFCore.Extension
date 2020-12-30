using System;
using Xunit;
using System.Linq;
using System.Linq.Expressions;
using NorthwindEFCoreSqlite;
using BeetleX.Tracks;
using Xunit.Abstractions;

namespace BeetleX.EFCore.Extension.XUnitTest
{
    public class UnitTest1
    {
        readonly ITestOutputHelper Console;
        public UnitTest1(ITestOutputHelper output)
        {
            this.Console = output;

        }
        [Fact]
        public void BaseSQL()
        {
            SQL sql = "select * from employees";
            var employees = sql.List<Employee, NorthwindContext>();

            sql = "select * from customers where country=@country";
            sql += ("@country", "UK");
            var customers = sql.List<Customer, NorthwindContext>();

        }
        [Fact]
        public void update()
        {
            using (NorthwindContext db = new NorthwindContext())
            {
                var cmd = db.Customers.Update(c => c.Region == "uk")
                    .Where(c => c.Country == "UK").Execute();
                var items = db.Customers.Where(c => c.Country == "UK").Select(c => c).ToArray();

            }
        }
        class CustomerName
        {
            public string CustomerID { get; set; }

            public string CompanyName { get; set; }
        }
        [Fact]
        public void AutoExecute()
        {
            CodeTrackFactory.Level = CodeTrackLevel.All;
            using (CodeTrackFactory.TrackReport("AutoExecute", CodeTrackLevel.Bussiness, null, "EFCore", "BeetleX"))
            {
                using (NorthwindContext db = new NorthwindContext())
                {
                    DBValueList<string> values = (db, "select customerid from customers");
                    DBObjectList<CustomerName> items = (db, "select CustomerID,CompanyName from customers");
                    DBExecute<string> id = (db, "select CompanyName from customers where CustomerID='ALFKI'");
                    DBExecute execute = (db, "delete from customers", " delete from orders");
                }
            }
            this.Console.WriteLine(CodeTrackFactory.Activity?.GetReport());
        }
        [Fact]
        public void selectObject()
        {

            Select<Customer> select = new Select<Customer>("CustomerID", "CompanyName");
            select &= c => c.Country == "UK";
            var items = select.List<CustomerName, NorthwindContext>();


        }
        [Fact]
        public void Delete()
        {
            using (NorthwindContext db = new NorthwindContext())
            {
                var cmd = db.Customers.Delete(c => c.Country == "UK");
                var items = db.Customers.Where(c => c.Country == "UK").Select(c => c).ToArray();

            }
        }
    }
}
