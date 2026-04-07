using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Shop.Models;
using SV22T1020247.Shop;
using System.Security.Claims;
using System.Text.Json;

namespace SV22T1020247.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Email và Mật khẩu!");
                return View();
            }

            // GỌI HÀM TỪ CRYPTHELPER
            string encryptedPassword = CryptHelper.HashMD5(password);

            var searchInput = new Models.Common.PaginationSearchInput() { Page = 1, PageSize = 10, SearchValue = email.Trim() };
            var customersResult = await PartnerDataService.ListCustomersAsync(searchInput);

            var customer = customersResult.DataItems.FirstOrDefault(c =>
                !string.IsNullOrEmpty(c.Email) &&
                c.Email.Trim().ToLower() == email.Trim().ToLower() &&
                !string.IsNullOrEmpty(c.Password) &&
                c.Password.Trim() == encryptedPassword); // SO SÁNH VỚI PASS ĐÃ MÃ HÓA

            if (customer != null)
            {
                if (customer.IsLocked == true)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên!");
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                    new Claim(ClaimTypes.Name, customer.CustomerName),
                    new Claim(ClaimTypes.Email, customer.Email ?? ""),
                    new Claim("Role", "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                var currentSessionCartStr = HttpContext.Session.GetString("ShoppingCart");
                var currentCart = string.IsNullOrEmpty(currentSessionCartStr)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(currentSessionCartStr);

                string cookieName = $"SavedCart_{customer.CustomerID}";
                string? savedCartJson = Request.Cookies[cookieName];

                if (!string.IsNullOrEmpty(savedCartJson))
                {
                    var savedCart = JsonSerializer.Deserialize<List<CartItem>>(savedCartJson);
                    if (savedCart != null)
                    {
                        foreach (var item in savedCart)
                        {
                            var existingItem = currentCart?.FirstOrDefault(c => c.ProductID == item.ProductID);
                            if (existingItem == null)
                            {
                                currentCart?.Add(item);
                            }
                        }
                    }
                }

                string finalCartJson = JsonSerializer.Serialize(currentCart);
                HttpContext.Session.SetString("ShoppingCart", finalCartJson);
                Response.Cookies.Append(cookieName, finalCartJson, new CookieOptions { Expires = DateTime.Now.AddDays(30) });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tài khoản không tồn tại hoặc sai mật khẩu!");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var cartData = HttpContext.Session.GetString("ShoppingCart");
                if (!string.IsNullOrEmpty(cartData))
                {
                    var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(30) };
                    Response.Cookies.Append($"SavedCart_{userIdClaim}", cartData, cookieOptions);
                }
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string customerName, string email, string phone, string address, string province, string password)
        {
            if (string.IsNullOrWhiteSpace(customerName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(province))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ các trường có dấu sao (*)");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View();
            }

            email = email.Trim();
            if (!email.Contains("@") || !email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Email phải chứa ký tự '@' và có đuôi '.com' (Ví dụ: abc@gmail.com)");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View();
            }
            if (!await PartnerDataService.ValidatelCustomerEmailAsync(email, 0))
            {
                ModelState.AddModelError("", "Email này đã được sử dụng bởi một tài khoản khác!");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View();
            }
            var newCustomer = new Models.Partner.Customer()
            {
                CustomerName = customerName.Trim(),
                ContactName = customerName.Trim(),
                Email = email,
                Phone = (phone ?? "").Trim(),
                Address = (address ?? "").Trim(),
                Province = province,
                IsLocked = false,
                Password = CryptHelper.HashMD5(password) // GỌI HÀM TỪ CRYPTHELPER
            };
            int result = await PartnerDataService.AddCustomerAsync(newCustomer);
            if (result > 0)
            {
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu dữ liệu. Vui lòng thử lại!");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(int.Parse(userId));
            if (customer == null) return NotFound();

            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Models.Partner.Customer data)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
            {
                ModelState.AddModelError("", "Tên không được để trống");
                ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
                return View(data);
            }

            var currentCustomer = await PartnerDataService.GetCustomerAsync(data.CustomerID);
            if (currentCustomer != null)
            {
                data.Password = currentCustomer.Password;
            }

            bool result = await PartnerDataService.UpdateCustomerAsync(data);
            if (result)
            {
                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Cập nhật thất bại!");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(data);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới không khớp!");
                return View();
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var customer = await PartnerDataService.GetCustomerAsync(int.Parse(userId));
            if (customer == null) return RedirectToAction("Login");

            // GỌI HÀM TỪ CRYPTHELPER KIỂM TRA PASS CŨ
            if (customer.Password?.Trim() != CryptHelper.HashMD5(oldPassword ?? ""))
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác!");
                return View();
            }

            // GỌI HÀM TỪ CRYPTHELPER CẬP NHẬT PASS MỚI
            customer.Password = CryptHelper.HashMD5(newPassword ?? "");
            await PartnerDataService.UpdateCustomerAsync(customer);

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}