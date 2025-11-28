using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<CategoryResponse> AddCategoryAsync(CategoryRequest request);
        Task<CategoryResponse> UpdateCategoryAsync(CategoryRequest request);
        Task Delete(int id);
    }
}
