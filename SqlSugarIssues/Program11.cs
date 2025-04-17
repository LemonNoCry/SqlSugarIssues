using System;
using System.Collections.Generic;
using Autofac;
using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program11
{
    public static void Main11(string[] args)
    {
        var configs = new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                //不用配置正确的 为了模拟故障转移
                ConnectionString =
                    $@"user id=xxxx;password=xx;initial catalog=xxx; data source=xxxx;TrustServerCertificate=true;Connect Timeout=1",
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            },
            new()
            {
                ConfigId = "Main2",
                //配置正确的连接
                ConnectionString = $@"user id=sa;password=123456;initial catalog=demo; data source=127.0.0.1;TrustServerCertificate=true;Connect Timeout=1",
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            }
        };

        var db = new SqlSugarScope(configs,
            s =>
            {
                if (!s.Ado.IsValidConnection())
                {
                    Console.WriteLine("故障转移,切换备用库");
                    s.ChangeDatabase("Main2");
                    s.RemoveConnection("Main");
                }
            });
        //只是模拟创建一个备用库 实际不会写这个
        var db2 = db.GetConnection("Main2");
        db2.DbMaintenance.CreateDatabase();
        db2.CodeFirst.InitTables<Student>();

        //CURD 都没问题
        Console.WriteLine("=====CURD======");
        var tasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var count = db.Queryable<Student>().Count();
                Console.WriteLine($"Student:{count}");
            });
        }

        Task.WaitAll(tasks);


        db.Deleteable<Student>().Where(s => s.Id == 10).ExecuteCommand();

        //事务出错
        Console.WriteLine("=====事务======");
        db.BeginTran();
        db.CommitTran();
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        public string Name { get; set; }
        public long ClassRoomId { get; set; }

        public bool IsDelete { get; set; }
    }
}