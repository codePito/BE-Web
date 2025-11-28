using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Request
{
    public class CategoryRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
