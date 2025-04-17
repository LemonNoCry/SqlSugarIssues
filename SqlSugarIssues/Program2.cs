// See https://aka.ms/new-console-template for more information

using SqlSugar;

public class Program2
{
    public string Name = "asd";
    public void Main()
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
        });

        var db = DB.AsTenant().GetConnection("Main");
        JsonClient jsonToSqlClient = new JsonClient
        {
            Context = db
        };

        var json = "{\"Table\":\"CustomizeSqlConfig\",\"PageNumber\":\"1\",\"PageSize\":\"100\"}";

        var sql = jsonToSqlClient.Queryable(json).ToSql();

        Console.WriteLine("Hello, World!");
    }
}