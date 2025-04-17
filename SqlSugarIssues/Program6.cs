using System.Net.Mime;
using SqlSugar;

namespace SqlSugarIssues;

public class Program6
{
    static void Main6(string[] args)
    {
        var db = new SqlSugarScope(new SqlSugar.ConnectionConfig()
        {
            ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test.db",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true
        });
        db.Aop.OnLogExecuted = (s, parameters) => Console.WriteLine(s);

        db.DbMaintenance.CreateDatabase();

        //建表 
        db.CodeFirst.InitTables<Student>();
        db.CodeFirst.InitTables<ClassRoom>();

        var dto = db.Queryable<Student>()
            .Includes(s => s.ClassRoom)
            .Where(s => s.Id == 1)
            .Select<StudentDto>()
            .First();

        Console.WriteLine(dto);
        Console.WriteLine("用例跑完");
        Console.ReadKey();
    }

    public class ClassRoom
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public long ClassRoomId { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public long ClassRoomId { get; set; }
    }
}