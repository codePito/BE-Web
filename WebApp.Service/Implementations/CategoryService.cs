using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task AddCategoryAsync(Category category)
        {
            _repo.AddCategory(category);
            await _repo.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            _repo.DeleteCategory(id);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return _repo.GetCategories();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _repo.UpdateCategory(category);
            await _repo.SaveChangesAsync();
        }
    }
}
