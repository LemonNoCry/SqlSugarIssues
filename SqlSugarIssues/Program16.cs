using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program16
{
    public static void Main16(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test16.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        db.CodeFirst.InitTables<Student>();

        //生成测试数据
        db.Insertable(new Student() { Name = "张三", Age = "18" })
           .ExecuteCommand();

        var student = db.Queryable<Student>()
           .Select(s => new{ s })
           .First();

        Console.WriteLine(JsonConvert.SerializeObject(student));
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Age { get; set; }

        public long ParentId { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(ParentId))]
        public Student Parent { get; set; }
    }

    public class StudentView
    {
        public Student Student { get; set; }

        //其他属性
    }
}