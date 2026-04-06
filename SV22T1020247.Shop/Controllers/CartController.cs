using Microsoft.AspNetCore.Mvc;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Shop.Models;
using System.Text.Json;

namespace SV22T1020247.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string SHOPPING_CART = "ShoppingCart";

        private List<CartItem> GetShoppingCart()
        {
            var sessionData = HttpContext.Session.GetString(SHOPPING_CART);
            if (!string.IsNullOrEmpty(sessionData))
            {
                return JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }
        private void SaveShoppingCart(List<CartItem> cart)
        {
            string jsonCart = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(SHOPPING_CART, jsonCart);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(30) };
                Response.Cookies.Append($"SavedCart_{userIdClaim}", jsonCart, cookieOptions);
            }
        }

        private void ClearSavedCart()
        {
            HttpContext.Session.Remove(SHOPPING_CART);
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                Response.Cookies.Delete($"SavedCart_{userIdClaim}");
            }
        }

        public IActionResult Index()
        {
            var cart = GetShoppingCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId = 0, int quantity = 1)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = await CatalogDataService.GetProductAsync(productId);
                if (product != null)
                {
                    cart.Add(new CartItem()
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo ?? "",
                        SalePrice = product.Price,
                        Quantity = quantity
                    });
                }
            }
            SaveShoppingCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveShoppingCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RedirectToAction("RemoveFromCart", new { id = productId });
            }

            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveShoppingCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            ClearSavedCart();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = GetShoppingCart();
            if (cart.Count == 0) return RedirectToAction("Index");

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> InitOrder(string deliveryProvince, string deliveryAddress)
        {
            var cart = GetShoppingCart();
            if (cart.Count == 0) return RedirectToAction("Index");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            int customerID = int.Parse(userIdClaim);
            int orderID = await SalesDataService.AddOrderAsync(customerID, deliveryProvince, deliveryAddress);
            if (orderID > 0)
            {
                foreach (var item in cart)
                {
                    await SalesDataService.SaveOrderDetailAsync(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }
                ClearSavedCart();
                return RedirectToAction("Confirm");
            }

            ModelState.AddModelError("", "Không thể tạo đơn hàng. Vui lòng thử lại.");
            return View("Checkout", cart);
        }

        public IActionResult Confirm()
        {
            return View();
        }
    }
}