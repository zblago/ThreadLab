# ThreadLab

Showcase for demonstrating communication between threads. You can run a console application without any setup, which is the default option. However, if you want a DB setup
follow one of the following options to create Microsoft SQL Server database.

Available `StorageType` values:

* `SQLServerEfDbContext` - physical DB instance
* `InMemoryEfDbContext` - in-memory DB context
* `InMemoryCollection` - raw C# (CLR list)

## DB setup

1. Run the [database setup script](https://github.com/zblago/ThreadLab/blob/main/ThreadLab/ThreadLab/DatabaseScripts/script.sql).

2. Update the connection string and storage type.

For example:

```json
{
  "ConnectionStrings": {
    "ThreadLab": "Server=(LocalDB)\\MSSQLLocalDB;Database=ThreadLab1;Trusted_Connection=True;"
  },
  "StorageType": "SQLServerEfDbContext" //SQLServerEfDbContext, InMemoryEfDbContext, InMemoryCollection
}
```
## EF Core Migrations

1. Visual Studio
2. Package Manager Console
3. ```update-database```
