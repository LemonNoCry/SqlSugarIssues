// See https://aka.ms/new-console-template for more information

using SqlSugar;

namespace SqlSugarIssues;

public class Program3
{
    public static void Main2()
    {
        var DB = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new ConnectionConfig()
            {
                ConfigId = "Main",
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                ConnectionString = "server=localhost;Database=SqlSugar4xTest;Uid=root;Pwd=haosql"
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        var entity = new ULockEntity() {Id = 1, Name = "a", Ver = 0};

        DB.Updateable(entity)
            .UpdateColumns(s => new {s.Name, s.Enable})
            .ExecuteCommandWithOptLock(true);
        Console.WriteLine("Hello");
    }

    class ULockEntity
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Enable { get; set; }


        [SqlSugar.SugarColumn(IsEnableUpdateVersionValidation = true)] //标识版本字段
        public long Ver { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ShowName => $@"{Id}_{Name}";
    }
}