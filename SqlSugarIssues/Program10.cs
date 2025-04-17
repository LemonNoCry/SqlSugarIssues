using System;
using System.Collections.Generic;
using Autofac;
using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program10
{
    public static void Main10(string[] args)
    {
        var configs = new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test9.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        };
        var db = new SqlSugarScope(configs,
            client =>
            {
                client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s);
            });

        db.CodeFirst.InitTables<Student>();
        db.CodeFirst.InitTables<ClassRoom>();
        db.CodeFirst.InitTables<Teacher>();

        //例子1
        {
            var query = db.Queryable<Student>()
                .Includes(s => s.ClassRoom)
                .Includes(s => s.ClassRoom, s => s.Teacher)
                .Where(s => s.Name != "张三");
            //拼接动态条件
            query.Where(p => p.ClassRoom.Name == "高一");
            //拼接动态条件
            query.Where(s => s.ClassRoom.Name == "高一");
        }



        //例子2
        {
            var query = db.Queryable<Student>()
                .Includes(s => s.ClassRoom)
                .Includes(s => s.ClassRoom, s => s.Teacher)
                .Where(s => s.Name != "张三")
                .Where(s => s.ClassRoom.TeacherId == 1);
            //拼接动态条件
            query.Where(p => p.ClassRoom.Name == "高一");
        }

    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        public string Name { get; set; }
        public long ClassRoomId { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }

        public bool IsDelete { get; set; }
    }

    public class ClassRoom
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        //班主任
        public long TeacherId { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(TeacherId))]
        public Teacher Teacher { get; set; }

        public string Name { get; set; }
        public bool IsDelete { get; set; }
    }

    public class Teacher
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}