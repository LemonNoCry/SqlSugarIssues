using SqlSugar;

namespace SqlSugarIssues;

public class Program5
{
    static void Main5(string[] args)
    {
        var DB = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new ConnectionConfig()
            {
                ConfigId = "Main",
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true,
                ConnectionString =
                    "user id=1;password=1;initial catalog=DataHubCloud_std; data source=127.0.0.1"
            }
        }, client => { client.Aop.OnLogExecuting = (s, parameters) => Console.WriteLine(s); });

        DB.CodeFirst.InitTables<TempTest01>();

        DB.Insertable<TempTest01>(new TempTest01() { Name = "张三" }).ExecuteCommand();

        DB.CodeFirst.InitTables<TempTest02>();

        Console.WriteLine("用例跑完");
        Console.ReadKey();
    }

    public enum ModelType
    {
        None,
        One
    }

    public class TempTest01
    {
        [SugarColumn(IsNullable = true)]
        public string Name { get; set; }
    }

    [SugarTable("TempTest01")]
    public class TempTest02
    {
        [SugarColumn(IsNullable = true)]
        public string Name { get; set; }

        [SugarColumn(IsNullable = true, DefaultValue = "0")]
        public ModelType Type { get; set; } = ModelType.None;
    }
}