using System.Net.Mime;
using SqlSugar;

namespace SqlSugarIssues;

public class Program4
{
    static void Main4(string[] args)
    {
        var db = new SqlSugarScope(new SqlSugar.ConnectionConfig()
        {
            ConnectionString = $@"DataSource={Environment.CurrentDirectory}\test.db",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true
        });
        db.Aop.OnLogExecuted = (s, parameters) => Console.WriteLine(s);

        db.DbMaintenance.CreateDatabase();

        //建表 
        db.CodeFirst.InitTables<bank_t_cash_master>();
        db.CodeFirst.InitTables<bank_t_cash_detail>();
        db.CodeFirst.InitTables<OrderMain>();
        db.CodeFirst.InitTables<OrderChild>();
        db.CodeFirst.InitTables<ItemInfo>();

        if (!db.Queryable<bank_t_cash_master>().Any())
        {
            db.Insertable<bank_t_cash_master>(new bank_t_cash_master()
                {
                    sheet_no = "SR012212030216",
                    voucher_no = "PB20221203201021",
                })
                .ExecuteCommand();
        }

        if (!db.Queryable<bank_t_cash_detail>().Any())
        {
            db.Insertable<bank_t_cash_detail>(new bank_t_cash_detail()
                {
                    sheet_no = "SR012212030216",
                    req_sheet_no = "PP012212030988",
                    req_sheet_sort = "1"
                })
                .ExecuteCommand();
        }

        //用例代码 
        var result = db.Queryable<bank_t_cash_master>()
            .Includes(s => s.Order)
            .Includes(s => s.Details, s => s.Order
                    .MappingField(q => q.sheet_no, () => s.req_sheet_no)
                    .MappingField(q => q.sheet_sort, () => s.req_sheet_sort)
                    .ToList(),
                s => s.Item)
            .Where(s => s.sheet_no == "SR012212030216")
            .First();

        Console.WriteLine(result);
        Console.WriteLine("用例跑完");
        Console.ReadKey();
    }


    public class bank_t_cash_master
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string sheet_no { get; set; }

        public string voucher_no { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(voucher_no))]
        public OrderMain Order { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(sheet_no), nameof(bank_t_cash_detail.sheet_no))]
        public List<bank_t_cash_detail> Details { get; set; }
    }

    public class bank_t_cash_detail
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int flow_id { get; set; }

        public string sheet_no { get; set; }
        public string req_sheet_no { get; set; }
        public string req_sheet_sort { get; set; }

        [Navigate(NavigateType.Dynamic, null)]
        public OrderChild Order { get; set; }
    }

    [SugarTable("co_t_order_main")]
    public class OrderMain
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string sheet_no { get; set; }

        public string branch_no { get; set; }

        public string p_sheet_no { get; set; } = "";
        public string voucher_no { get; set; }

        public string trans_no { get; set; }
        public string order_status { get; set; } = "0";
        public string sup_no { get; set; }

        public string coin_code { get; set; } = "RMB";
        public decimal total_amount { get; set; }
        public decimal paid_amount { get; set; }

        public DateTime valid_date { get; set; }
        public DateTime oper_date { get; set; } = DateTime.Now;
        public DateTime update_time { get; set; } = DateTime.Now;
        public string approve_flag { get; set; } = "0";
        public DateTime approve_date { get; set; }

        public string memo { get; set; }

        public string order_man { get; set; }
        public string dept_no { get; set; }

        public string ex_id { get; set; }
        public string ex_sheet { get; set; }
        public string ex_database { get; set; }

        public string is_push { get; set; }

        public string workshop_no { get; set; }


        [Navigate(NavigateType.OneToMany, nameof(OrderChild.sheet_no))]
        public List<OrderChild> Child { get; set; }
    }

    [SugarTable("co_t_order_child")]
    public class OrderChild
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int flow_id { get; set; }

        public string sheet_no { get; set; }

        public string item_no { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(item_no), nameof(ItemInfo.ItemNo))]
        public ItemInfo Item { get; set; }

        public string unit_no { get; set; }

        public decimal in_price { get; set; }
        public decimal notax_price { get; set; }

        public decimal order_qnty { get; set; }

        public decimal sub_amount { get; set; }
        public decimal notax_amount { get; set; }

        public int sheet_sort { get; set; }
        public int head_sheet_sort { get; set; }

        public string uuid { get; set; } = Guid.NewGuid().ToString().ToUpper();

        public string other1 { get; set; }

        public string ex_id { get; set; }
        public string ex_sheet { get; set; }
        public string ex_flowId { get; set; }
    }

    [SugarTable("bi_t_item_info")]
    public class ItemInfo
    {
        [SugarColumn(ColumnName = "item_no", IsPrimaryKey = true)]
        public string ItemNo { get; set; }

        [SugarColumn(ColumnName = "item_clsno")]
        public string ItemClsNo { get; set; }


        [SugarColumn(ColumnName = "item_subno")]
        public string ItemSubNo { get; set; }

        [SugarColumn(ColumnName = "item_name")]
        public string Name { get; set; }

        [SugarColumn(ColumnName = "item_subname")]
        public string PhoneticCode { get; set; }

        [SugarColumn(ColumnName = "unit_no")]
        public string Unit { get; set; }

        public string item_size { get; set; }

        [SugarColumn(ColumnName = "barcode")]
        public string Barcode { get; set; }

        [SugarColumn(ColumnName = "valid_day")]
        public int ValidDay { get; set; }

        public string item_bom { get; set; }

        public string branch_no { get; set; }

        /// <summary>
        /// 0:外购,1:自制
        /// </summary>
        public string item_property { get; set; }

        public string process_type { get; set; } = "1";

        public string is_mrp { get; set; } = "1";

        public int min_stock { get; set; } = 0;

        public decimal pu_tax_rate1 { get; set; }
        public decimal pu_tax_rate2 { get; set; }

        /// <summary>
        /// 外来字段
        /// </summary>
        public string ex_field { get; set; }

        /// <summary>
        /// 外来来源
        /// </summary>
        public string ex_database { get; set; }
    }
}