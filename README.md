# BeetleX.EFCore.Extension
BeetleX.EFCore.Extension
## sql
``` csharp
string select = "select * from employees";
SQL sql = select;
var items = sql.List<employees, NorthWind>();

sql = select;
sql.Add(" where id in (@p1,@p2) order by id asc", ("@p1", 1), ("@p2", 2));
items = sql.List<employees, NorthWind>();

sql = select;
sql.Where<employees>(e => e.id == 3);
var item = sql.ListFirst<employees, NorthWind>();

sql = select;
sql.OrderBy<employees>(p => p.id.DESC());
item = sql.ListFirst<employees, NorthWind>();
```

## update
``` csharp
UpdateSql<employees> update = new UpdateSql<employees>();
update.Set(f => f.fax_number == "123").Where(f => f.id == 1).Execute<NorthWind>();

using (var db = new NorthWind())
{
    db.Customers.Update(c => c.city == "gz").Execute();
}
```

## delete
``` csharp
DeleteSql<employees> del = new DeleteSql<employees>();
del.Where(f => f.id.In(1, 2, 3));
del.Execute<NorthWind>();

using (var db = new NorthWind())
{
    db.Customers.Delete(f => f.id.In(1, 2, 3));
}
```
