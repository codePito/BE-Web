using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Request
{
    public class MomoPaymentRequest
    {
        public int OrderId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl {  get; set; } = string.Empty;
    }
}
