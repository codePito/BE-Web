using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Response
{
    public class ProductSalesMonthlyStatsResponse
    {
        public string Month { get; set; } = string.Empty;   
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int Year { get; set; }   
    }
}
