using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;

namespace WebApp.Repository.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts();
        Product? GetByID(int id);
        void Add(Product product);
        void Update(Product product);
        Task<bool> Delete(int id);
        Task SaveChangesAsync();
    }
}
