using Microsoft.AspNetCore.Mvc;

namespace SV22T1020247.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý các chức năng liên quan đến tài khoản người dùng 
    /// như đăng nhập, đăng xuất và thay đổi mật khẩu.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Hiển thị giao diện đăng nhập cho người dùng.
        /// </summary>
        /// <returns>Trang Login</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Thực hiện đăng xuất khỏi hệ thống và chuyển về trang đăng nhập.
        /// </summary>
        /// <returns>Chuyển hướng về trang Login</returns>
        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Hiển thị giao diện cho phép người dùng thay đổi mật khẩu.
        /// </summary>
        /// <returns>Trang ChangePassword</returns>
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}