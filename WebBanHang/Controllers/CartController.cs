using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private const string CART_KEY = "cart";

        public CartController(
            IProductRepository productRepo,
            ApplicationDbContext context,
            IHttpContextAccessor httpContext)
        {
            _productRepo = productRepo;
            _context = context;
            _httpContext = httpContext;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(p => p.ProductId == id);
            if (existingItem != null)
                existingItem.Quantity++;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl ?? "",
                    Price = product.Price,
                    Quantity = 1
                });

            HttpContext.Session.SetObject(CART_KEY, cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            cart.RemoveAll(p => p.ProductId == id);
            HttpContext.Session.SetObject(CART_KEY, cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY);
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            var userEmail = User.Identity?.Name ?? "Khách";

            var order = new Order
            {
                UserEmail = userEmail,
                OrderDate = DateTime.Now
            };

            foreach (var item in cart)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove(CART_KEY);

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int id, string action)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart") ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.ProductId == id);
            if (item != null)
            {
                if (action == "increase") item.Quantity++;
                else if (action == "decrease")
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                        cart.Remove(item);
                }

                HttpContext.Session.SetObject("cart", cart);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddToCartSilent(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.ProductId == id);
            if (item != null)
                item.Quantity++;
            else
            {
                var product = _productRepo.GetByIdAsync(id).Result;
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ImageUrl = product.ImageUrl ?? "",
                        Price = product.Price,
                        Quantity = 1
                    });
                }
            }

            HttpContext.Session.SetObject(CART_KEY, cart);
            TempData["SuccessMessage"] = "Đã thêm vào giỏ hàng!";
            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> AddToCartAndRedirect(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(p => p.ProductId == id);

            if (item != null)
                item.Quantity++;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl ?? "",
                    Price = product.Price,
                    Quantity = 1
                });

            HttpContext.Session.SetObject(CART_KEY, cart);
            return RedirectToAction("Index", "Cart");
        }
        [HttpGet]
        public IActionResult CheckoutForm()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY);
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            return View(new OrderInfoViewModel());
        }

        [HttpPost]
        public IActionResult CheckoutForm(OrderInfoViewModel model)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_KEY);
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            if (!ModelState.IsValid) return View(model);

            var order = new Order
            {
                UserEmail = User.Identity?.Name ?? "Khách",
                OrderDate = DateTime.Now,
                FullName = model.FullName,
                Address = model.Address,
                Phone = model.Phone,
                PaymentMethod = model.PaymentMethod,
                OrderDetails = cart.Select(p => new OrderDetail
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    Quantity = p.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();
            HttpContext.Session.Remove(CART_KEY);

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "Home");
        }



    }
}
