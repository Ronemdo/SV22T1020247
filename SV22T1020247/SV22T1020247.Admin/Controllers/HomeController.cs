using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SV22T1020247.Admin.Controllers
{
    /// <summary>
    /// Controller trang chủ của hệ thống quản trị.
    /// Cung cấp các trang chính như Dashboard, Privacy và Error.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Khởi tạo HomeController và inject dịch vụ ghi log.
        /// </summary>
        /// <param name="logger">Đối tượng logger dùng để ghi log hệ thống</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang chủ của hệ thống (Dashboard).
        /// </summary>
        /// <returns>Trang Index</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị trang chính sách bảo mật.
        /// </summary>
        /// <returns>Trang Privacy</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị trang lỗi khi hệ thống xảy ra exception.
        /// </summary>
        /// <returns>Trang Error cùng với RequestId</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}