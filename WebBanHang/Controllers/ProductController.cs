using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebBanHang.Models;
using WebBanHang.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;

namespace WebBanHang.Controllers
{
    [Authorize] // Yêu cầu phải đăng nhập mới truy cập controller này
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile? imageFile)
        {
            var categoryExists = await _categoryRepository.GetByIdAsync(product.CategoryId);
            if (categoryExists == null)
            {
                ModelState.AddModelError("CategoryId", "Danh mục không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string imageUrl = await SaveImage(imageFile);
                    if (imageUrl == null)
                    {
                        ModelState.AddModelError("ImageUrl", "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png, .gif");
                        ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
                        return View(product);
                    }
                    product.ImageUrl = imageUrl;
                }

                int newProductId = await _productRepository.AddAsync(product);
                if (newProductId <= 0)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu sản phẩm, vui lòng thử lại.");
                    return View(product);
                }

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllWithCategoriesAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }

            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                string imageUrl = await SaveImage(imageFile, existingProduct.ImageUrl);
                if (imageUrl != null)
                {
                    existingProduct.ImageUrl = imageUrl;
                }
            }

            bool updateResult = await _productRepository.UpdateAsync(existingProduct);
            if (!updateResult)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm, vui lòng thử lại.");
                return View(product);
            }

            return RedirectToAction("Index");
        }

        private async Task<string?> SaveImage(IFormFile imageFile, string? oldImageUrl = null)
        {
            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return null;
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
