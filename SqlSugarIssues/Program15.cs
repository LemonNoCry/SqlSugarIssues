using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program15
{
    public static void Main15(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test15.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        db.CodeFirst.InitTables<Student>();

        //生成测试数据
        db.Insertable(new Student()
                { Name = "张三", Age = "18", Phone = "123456", Address = new AddressInfo() { Province = "广东", City = "广州", Street = "天河" } })
           .ExecuteCommand();

        var student = db.Queryable<Student>()
           .Select(s => new StudentDto()
            {
                Address = s.Address,
            }, true)
           .First();

        Console.WriteLine(JsonConvert.SerializeObject(student));
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Age { get; set; }

        public string Phone { get; set; }

        [SugarColumn(IsJson = true, IsNullable = true, SqlParameterDbType = typeof(SqlSugarSerializeService))]
        public AddressInfo Address { get; set; }
    }

    public class AddressInfo
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }


    public class StudentDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Age { get; set; }

        public string Phone { get; set; }

        [SugarColumn(IsJson = true, IsNullable = true, SqlParameterDbType = typeof(SqlSugarSerializeService))]
        public AddressInfo Address { get; set; }
    }
}