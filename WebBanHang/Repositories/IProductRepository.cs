using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Models;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<int> AddAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<IEnumerable<Product>> GetAllWithCategoriesAsync();
    Task<Product?> GetByIdWithCategoryAsync(int id);

}
