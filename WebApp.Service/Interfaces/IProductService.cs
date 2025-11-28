using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetProductsAsync();
        Task<ProductResponse> GetByIDAsync(int id);
        Task<ProductResponse> AddAsync(ProductRequest request, int userId);
        Task<ProductResponse> UpdateAsync(ProductRequest request);
        Task DeleteAsync(int id);
    }
}
