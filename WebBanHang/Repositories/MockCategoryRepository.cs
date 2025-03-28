using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories = new List<Category>
        {
            new Category { Id = 1, Name = "Laptop" },
            new Category { Id = 2, Name = "Desktop" }
        };

        public Task<IEnumerable<Category>> GetAllAsync()
        {
            return Task.FromResult(_categories.AsEnumerable());
        }

        public Task<Category?> GetByIdAsync(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(category);
        }

        public Task AddAsync(Category category)
        {
            category.Id = _categories.Max(c => c.Id) + 1;
            _categories.Add(category);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Category category)
        {
            var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _categories.Remove(category);
            }
            return Task.CompletedTask;
        }
    }
}
