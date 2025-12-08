using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Response
{
    public class MomoPaymentResponse
    {
        public int OrderId { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Message {  get; set; } = string.Empty;

    }
}
