using SqlSugar;

namespace SqlSugarIssues;

public class Program13
{
    public static void Main13(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test12.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        db.CodeFirst.InitTables<Student,StarStudent>();

        db.Queryable<Student>()
            .IncludeLeftJoin(s => s.StartStudent)
            .Where(s => s.Name.StartsWith("张"))
            .Select(s => new StudentDto()
            {
                StarLevel = s.StartStudent.StarLevel,
            }, true)
            .ToList();
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Age { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(Id), nameof(StarStudent.StudentId))]
        public StarStudent StartStudent { get; set; }
    }

    public class StarStudent
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int StarLevel { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Age { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }

        public int StarLevel { get; set; }
    }
}