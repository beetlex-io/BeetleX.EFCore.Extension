using BeetleX.Tracks;
using NorthwindEFCoreSqlite;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BeetleX.EFCore.Extension.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SQL sql = @"select Orders.*,Employees.FirstName,Employees.LastName,Customers.CompanyName 
from Orders inner join Employees on Employees.EmployeeID=Orders.EmployeeID inner join Customers on Customers.CustomerID= Orders.CustomerID where 1=1";
            int i = 3;
            int i1 = 4;
            sql.And().Where<Employee>(e => e.EmployeeID.In(i, i1));
            DBRegionData<ExpandoObject> result = new DBRegionData<ExpandoObject>(0, 50);
            await result.ExecuteAsync<NorthwindContext>(sql);
            await Task.Delay(-1);
        }
    }
    class CustomerName
    {
        public string CustomerID { get; set; }

        public string CompanyName { get; set; }
    }
}
