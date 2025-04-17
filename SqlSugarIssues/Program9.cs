using Autofac;
using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program9
{
    public static void Main9(string[] args)
    {
        var configs = new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test8.db",
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

        db.DbMaintenance.TruncateTable<Student>();
        db.DbMaintenance.TruncateTable<ClassRoom>();
        db.Insertable(new List<ClassRoom>() {new ClassRoom() {Id = 1, Name = "一年级"}}).ExecuteCommand();
        db.Insertable(new List<ClassRoom>() {new ClassRoom() {Id = 2, Name = "而年级"}}).ExecuteCommand();
        db.Insertable(new List<Student>() {new Student() {Id = 1, Name = "张三", ClassRoomId = 1}}).ExecuteCommand();
        db.Insertable(new List<Student>() {new Student() {Id = 2, Name = "李四", ClassRoomId = 2}}).ExecuteCommand();

        //更新学生
        var student = db.Queryable<Student>().Where(s => s.Id == 1).First();
        Console.WriteLine(student.Name);
        db.Tracking(student);

        student.Name += "1";
        db.Updateable(student).ExecuteCommand();
        student = db.Queryable<Student>().Where(s => s.Id == 1).First();
        Console.WriteLine("修改后:" + student.Name);

        //更新班级
        var room = db.Queryable<ClassRoom>().Where(s => s.Id == 1).First();
        room.Name += "1";
        db.Updateable(room).ExecuteCommand();
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

        public string Name { get; set; }
        public bool IsDelete { get; set; }
    }
}