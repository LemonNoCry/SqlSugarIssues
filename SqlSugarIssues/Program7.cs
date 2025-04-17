using System.Data;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SqlSugarIssues;

public class Program7
{
    static async Task Main7(string[] args)
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
        var db = new SqlSugarScope(configs,
            s =>
            {
                foreach (var c in configs)
                {
                    var db = s.GetConnectionScope((string) c.ConfigId);
                    db.Aop.OnLogExecuted = (sql, parameters) =>
                    {
                        Console.WriteLine($"======================={c.ConfigId}=============================");
                        Console.WriteLine(UtilMethods.GetSqlString(DbType.Sqlite, sql, parameters));
                        Console.WriteLine("====================================================");
                    };
                }
            });

        //5.1.4.86 不报错
        //5.1.4.94 报错

        var flowQuery = db.Queryable<ProductStockFlow>()
            .GroupBy(s => new {s.ProductId})
            .Select(s => new
            {
                s.ProductId,
                LastOutTime = SqlFunc.AggregateMax(s.CreateTime)
            });

        var data = await db.Queryable<ProductStock>()
            .LeftJoin(flowQuery, (s, f) => s.ProductId == f.ProductId)
            .Where(s => s.StockQty > 0)
            .Select((s, f) => new ProductStockMonitorDto(), true)
            .ToListAsync();
    }
}

public class ProductStockFlow
{
    [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    /// <summary>
    /// 产品货号
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 发生数量
    /// </summary>
    [SugarColumn(Length = 18, DecimalDigits = 4)]
    public decimal SettleQty { get; set; }

    /// <summary>
    /// 发生类型
    /// </summary>
    public string SettleType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Remark { get; set; }

    public DateTime CreateTime { get; set; }
}

public class ProductStock
{
    [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    /// <summary>
    /// 产品货号
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    [SugarColumn(Length = 18, DecimalDigits = 4)]
    public decimal StockQty { get; set; }
}

public class ProductStockMonitorDto
{
    public long ProductId { get; set; }

    /// <summary>
    /// 产品货号
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal StockQty { get; set; }

    /// <summary>
    /// 最后出库时间
    /// </summary>
    public DateTime? LastOutTime { get; set; }
}