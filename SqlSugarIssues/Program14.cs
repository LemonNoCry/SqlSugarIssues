using System.Reflection;
using SqlSugar;

namespace SqlSugarIssues;

public class Program14
{
    public static void Main14(string[] args)
    {
        var db = new SqlSugarScope(new List<ConnectionConfig>()
        {
            new()
            {
                ConfigId = "Main",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test13.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                AopEvents = new AopEvents()
                {
                    OnLogExecuting = (s, parameters) => Console.WriteLine(s)
                }
            },
            new()
            {
                ConfigId = "Log",
                ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test13_log.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                AopEvents = new AopEvents()
                {
                    OnLogExecuting = (s, parameters) => Console.WriteLine(s)
                }
            }
        }, client => { });

        var lis = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            lis.Add(Task.Run(() =>
            {
                using var uow = db.UseTran();
                var accessLog = db.QueryableWithAttr<AccessTrendLog>()
                    .TranLock(DbLockType.Wait)
                    .Where(s => s.UserId == 1)
                    .SpiltTableNow()
                    .First();
                if (accessLog is null)
                {
                    accessLog = new AccessTrendLog() { UserId = 1 };
                    var flag = db.InsertableWithAttr(accessLog).SplitTable().ExecuteCommand();
                    if (flag == 0)
                    {
                        Console.WriteLine("Insert failed");
                    }
                    else
                    {
                        Console.WriteLine(1);
                    }

                    return;
                }

                Console.WriteLine(accessLog.Count);
                accessLog.Count += 1;
                db.UpdateableWithAttr(accessLog)
                    .SplitTable()
                    .ExecuteCommand();

                uow.CommitTran();
            }));
        }

        Task.WaitAll(lis.ToArray());
    }

    /// <summary>
    /// 用户访问趋势日志
    /// </summary>
    [Tenant("Log")]
    [SplitTable(SplitType.Month)] //按年分表 （自带分表支持 年、季、月、周、日）
    [SugarTable($@"{nameof(AccessTrendLog)}_{{year}}{{month}}{{day}}")]
    public class AccessTrendLog
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
        public long UserId { get; set; }

        /// <summary>
        /// 平台Id
        /// </summary>
        [SugarColumn(IsNullable = true, DefaultValue = "0")]
        public long PlatformId { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 150)]
        public string UserName { get; set; }

        /// <summary>
        /// 次数
        /// </summary>
        public int Count { get; set; }

        [SplitField]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}

public static class QueryExtension
{
    public static DateTime GetNowWeekStart(this DateTime time)
    {
        return time.AddDays(1 - Convert.ToInt32(time.DayOfWeek.ToString("d"))).Date;
    }

    public static DateTime GetNowWeekEnd(this DateTime time)
    {
        return time.AddDays(1 - Convert.ToInt32(time.DayOfWeek.ToString("d"))).Date.AddDays(7).AddSeconds(-1);
    }

    public static ISugarQueryable<T> SpiltTableNow<T>(this ISugarQueryable<T> queryable)
    {
        var entity = queryable.Context.EntityMaintenance.GetEntityInfo<T>();
        return queryable.SplitTable(source =>
        {
            var customAttribute = entity.Type.GetCustomAttribute<SplitTableAttribute>();
            if (customAttribute is null) throw new Exception($"{entity.EntityName} 未找到分表特性");

            return customAttribute.SplitType switch
            {
                SplitType.Month => source
                    .Where(y => y.Date.Year == DateTime.Now.Year && y.Date.Month == DateTime.Now.Month).ToList(),
                SplitType.Year => source.Where(y => y.Date.Year == DateTime.Now.Year).ToList(),
                SplitType.Week => source.Where(y =>
                        y.Date >= DateTime.Now.GetNowWeekStart() && y.Date <= DateTime.Now.GetNowWeekEnd())
                    .ToList(),
                _ => source.Take(1).ToList()
            };
        });
    }
}