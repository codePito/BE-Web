using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;
using WebApp.Repository.Interfaces;
using WebApp.Service.Interfaces;

namespace WebApp.Service.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CategoryResponse> AddCategoryAsync(CategoryRequest request)
        {
            var entity = _mapper.Map<Category>(request);
            
            _repo.AddCategory(entity);

            await _repo.SaveChangesAsync();

            return _mapper.Map<CategoryResponse>(entity);
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

        public async Task<CategoryResponse> UpdateCategoryAsync(CategoryRequest request)
        {
            var entity = _mapper.Map<Category>(request);
            _repo.UpdateCategory(entity);
            await _repo.SaveChangesAsync();
            return _mapper.Map <CategoryResponse>(entity);
        }
    }
}
