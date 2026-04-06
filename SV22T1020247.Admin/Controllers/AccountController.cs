using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Security;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020247.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến tài khoản
    /// </summary>
   [Authorize]
    public class AccountController : Controller
    {
       [AllowAnonymous]
       [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous] 
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đủ tên và mật khẩu");
                return View();
            }

            password = CryptHelper.HashMD5(password);

            var userAccount = await SecurityDataService.EmployeeAuthorizeAsync(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng!");
                return View();
            }

            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            await HttpContext.SignInAsync
            (
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal()
            );

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Giao diện đổi mật khẩu
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Xử lý dữ liệu khi bấm nút Đổi mật khẩu
        /// </summary>
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            // 1. Kiểm tra dữ liệu rỗng
            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin!");
                return View();
            }

            // 2. Kiểm tra mật khẩu mới và xác nhận có khớp không
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp với mật khẩu mới!");
                return View();
            }

            // 3. Lấy tên đăng nhập (Email) của người dùng đang đăng nhập
            // Trong pattern của bạn, hàm GetUserData() thường được mở rộng để lấy thông tin session
            string userName = User.GetUserData()?.UserName ?? "";

            // 4. Mã hóa mật khẩu cũ và mới để so sánh/lưu vào database
            string hashedOldPassword = CryptHelper.HashMD5(oldPassword);
            string hashedNewPassword = CryptHelper.HashMD5(newPassword);

            // 5. Gọi Service thực hiện đổi
            bool result = SecurityDataService.ChangePassword(userName, hashedOldPassword, hashedNewPassword);

            // 6. Xử lý kết quả trả về
            if (!result)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác!");
                return View();
            }

            // Đổi mật khẩu thành công thì bắt đăng xuất để đăng nhập lại
            return RedirectToAction("Logout");
        }

        /// <summary>
        /// Truy cập bị từ chối
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}