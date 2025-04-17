using Autofac;
using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program8
{
    public static void Main8(string[] args)
    {
        var configs = new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        };
        var DB = new SqlSugarScope(configs,
            client =>
            {
                client.QueryFilter.AddTableFilter<Student>(s => s.IsDelete == false);
                client.QueryFilter.AddTableFilter<ClassRoom>(s => s.IsDelete == false);
                client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s);
            });

        DB.CodeFirst.InitTables<Student>();
        DB.CodeFirst.InitTables<ClassRoom>();

        DB.DbMaintenance.TruncateTable<Student>();
        DB.DbMaintenance.TruncateTable<ClassRoom>();
        DB.Insertable(new List<ClassRoom>() {new ClassRoom() {Id = 1, Name = "一年级", IsDelete = true}})
            .ExecuteReturnSnowflakeId();
        DB.Insertable(new List<ClassRoom>() {new ClassRoom() {Id = 2, Name = "而年级", IsDelete = true}})
            .ExecuteReturnSnowflakeId();

        DB.Storageable(new List<Student>() {new Student() {Id = 1, Name = "张三", ClassRoomId = 1}}).ExecuteCommand();
        DB.Storageable(new List<Student>() {new Student() {Id = 2, Name = "李四", ClassRoomId = 2}}).ExecuteCommand();


        var data = DB.Queryable<Student>().ClearFilter<Student, ClassRoom>().Includes(s => s.ClassRoom).ToList();
        Console.WriteLine(JsonConvert.SerializeObject(data));
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