using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Data;
using WebApp.Model.Entities;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Implementations
{
    public class CategoryRepository :ICategoryRepository
    {
        private readonly WebContext _context;
        public CategoryRepository(WebContext context)
        {
            _context = context;
        }
        public void AddCategory(Category category) => _context.Categories.Add(category);

        public void DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
        }
        public IEnumerable<Category> GetCategories() => _context.Categories.ToList();
        
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        
        public void UpdateCategory(Category category) => _context.Categories.Update(category);
    }
}
