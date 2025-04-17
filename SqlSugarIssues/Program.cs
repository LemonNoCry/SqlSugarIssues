using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Threading.Tasks;
using SqlSugar;

namespace SqlSugarIssues;

public static class Program
{
    static async Task Main(string[] args)
    {
        var db = new SqlSugarScope(new SqlSugar.ConnectionConfig()
        {
            ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test18.db",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true
        });
        db.Aop.OnLogExecuted = (s, parameters) => Console.WriteLine(s);

        db.DbMaintenance.CreateDatabase();

        //建表 
        db.CodeFirst.InitTables<Students>();

        var data = new List<Students>()
        {
            new Students() { Id = 1, Name = "张三" },
            new Students() { Id = 2, Name = "张三2", ParentId = 1 },
            new Students() { Id = 3, Name = "张三3", ParentId = 1 },
            new Students() { Id = 4, Name = "张三4", ParentId = 1 },
        };

        await db.Storageable(data).ExecuteCommandAsync();

        var flag = true;
        
   
        var query = await db.Queryable<Students>()
           .IncludesIf(flag, d => d.Child)
           .ToListAsync();

        Console.WriteLine("用例跑完");
        Console.ReadKey();
    }

    private static ISugarQueryable<T> IncludesIf<T, TReturn>(this ISugarQueryable<T> queryable, bool condition,
        Expression<Func<T, TReturn>> expression) where T : class
    {
        return condition ? queryable.Includes(expression) : queryable;
    }

    public class Students
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public long ParentId { get; set; }

        [Navigate(NavigateType.OneToMany, nameof(ParentId))]
        public List<Students> Child { get; set; }
    }
}