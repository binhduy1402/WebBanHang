using WebBanHang.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MockProductRepository : IProductRepository
{
    private List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Nike Air Max", Price = 5000, Description = "Men's Shoes", CategoryId = 2, ImageUrl = "/uploads/nike.jpg" },
        new Product { Id = 2, Name = "Adidas UltraBoost", Price = 4000, Description = "Running Shoes", CategoryId = 3, ImageUrl = "/uploads/adidas.jpg" }
    };

    public Task<IEnumerable<Product>> GetAllAsync() =>
        Task.FromResult(_products.AsEnumerable());

    public Task<Product?> GetByIdAsync(int id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<int> AddAsync(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return Task.FromResult(product.Id);
    }

    public Task<bool> UpdateAsync(Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct != null)
        {
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImageUrl = product.ImageUrl;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null) _products.Remove(product);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Product>> GetAllWithCategoriesAsync() =>
        Task.FromResult(_products.AsEnumerable());

    public Task<Product?> GetByIdWithCategoryAsync(int id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
}
