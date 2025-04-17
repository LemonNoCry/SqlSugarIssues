using System.Data.Common;
using SqlSugar;

namespace SqlSugarIssues;

public class Program12
{
    public static void Main12(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test11.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        db.CodeFirst.InitTables<Teacher, TeacherClass, ClassRoom, Student, StudentClass>();
        db.CodeFirst.InitTables<StarClass>();
        db.DbMaintenance.TruncateTable<Teacher>();
        db.DbMaintenance.TruncateTable<TeacherClass>();
        db.DbMaintenance.TruncateTable<ClassRoom>();
        db.DbMaintenance.TruncateTable<Student>();
        db.DbMaintenance.TruncateTable<StudentClass>();
        db.DbMaintenance.TruncateTable<StarClass>();

        db.Insertable(new Teacher() { Id = 10, Name = "张三老师" }).ExecuteCommand();
        db.Insertable(new ClassRoom() { Id = 100, Name = "一班" }).ExecuteCommand();
        db.Insertable(new ClassRoom() { Id = 101, Name = "二班" }).ExecuteCommand();
        db.Insertable(new TeacherClass() { TeacherId = 10, ClassRoomId = 100 }).ExecuteCommand();
        db.Insertable(new TeacherClass() { TeacherId = 10, ClassRoomId = 101 }).ExecuteCommand();

        db.Queryable<Teacher>()
            .Select(s => new TeacherView
            {
                Id = s.Id, Name = s.Name,
                ClassRoom = SqlFunc.Subqueryable<ClassRoom>()
                    .LeftJoin<StarClass>((c, sc) => c.Id == sc.ClassId)
                    .Where((c, sc) => SqlFunc.Subqueryable<TeacherClass>().Where(tc => tc.TeacherId == s.Id && tc.ClassRoomId == c.Id).Any())
                    .ToList((c, sc) => new ClassRoomView()
                    {
                        HasStudent = SqlFunc.Subqueryable<StudentClass>().Where(st => st.ClassId == c.Id).Any(),
                    }, true)
            })
            .ToList();
    }

    public class Teacher
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class TeacherView
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<ClassRoomView> ClassRoom { get; set; }
    }

    public class ClassRoom
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        [Navigate(NavigateType.ManyToMany, nameof(StudentClass.ClassId), nameof(StudentClass.StudentId))]
        public List<Student> Students { get; set; }
    }

    public class ClassRoomView
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool HasStudent { get; set; }

        public List<Student> Students { get; set; }
    }

    public class TeacherClass
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public int ClassRoomId { get; set; }
    }

    public class StarClass
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public int ClassId { get; set; }
    }

    public class Student
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public int ClassId { get; set; }
    }

    public class StudentClass
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int ClassId { get; set; }
    }
}