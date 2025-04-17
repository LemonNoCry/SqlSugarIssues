using Newtonsoft.Json;
using SqlSugar;

namespace SqlSugarIssues;

public class Program17
{
    public static async Task Main17(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test17.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine($"DbContextId:{client.ContextID}\r\nSQL:" + s); });

        db.CodeFirst.InitTables<Student>();

        //生成测试数据
        await db.Insertable(new Student() { Name = "张三", Age = "18" })
           .ExecuteCommandAsync();

        await new StudentJob().ExecutionAsync(db);

        Console.WriteLine("Over");
    }

    public interface IJob
    {
        Task ExecutionAsync(ISqlSugarClient db);
    }

    public class StudentJob : IJob
    {
        public async Task ExecutionAsync(ISqlSugarClient db)
        {
            await new StudentService(db).ExecAsync(1);
        }
    }

    public interface IStudentService
    {
        Task ExecAsync(long id);
    }

    public class StudentService : IStudentService
    {
        private readonly ISqlSugarClient _db;

        public StudentService(ISqlSugarClient db)
        {
            this._db = db;
        }

        public async Task ExecAsync(long id)
        {
            try
            {
                Console.WriteLine("Begin");
                Console.WriteLine(_db.ContextID);
                await _db.AsTenant().BeginTranAsync();

                Console.WriteLine(_db.ContextID);
                var students = await _db.Queryable<Student>()
                   .TranLock()
                   .Where(s => s.Id > 0)
                   .ToListAsync();
                Console.WriteLine(_db.ContextID);


                students.ForEach(s => s.Age += 1);
                await _db.Updateable(students).ExecuteCommandHasChangeAsync();
                Console.WriteLine(_db.ContextID);

                await _db.AsTenant().CommitTranAsync();
                Console.WriteLine("Commit");
            }
            catch (Exception e)
            {
                await _db.AsTenant().RollbackTranAsync();
            }
        }
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
}