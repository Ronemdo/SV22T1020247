using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Sales;

namespace SV22T1020247.Web.Controllers
{
    [Authorize] 
    public class OrderController : Controller
    {
        public async Task<IActionResult> Index(int page = 1)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            int currentCustomerID = int.Parse(userIdClaim);

            var input = new OrderSearchInput()
            {
                Page = page,
                PageSize = 20,
                SearchValue = "",
                Status = 0
            };

            var result = await SalesDataService.ListOrdersAsync(input);
            var myOrders = result.DataItems.Where(o => o.CustomerID == currentCustomerID).ToList();
            result.DataItems = myOrders;

            return View(result);
        }
        public async Task<IActionResult> Details(int id = 0)
        {
            if (id <= 0) return RedirectToAction("Index");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentCustomerID = int.Parse(userIdClaim ?? "0");

            if (order.CustomerID != currentCustomerID)
            {
                return RedirectToAction("Index");
            }

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View(order);
        }
        public async Task<IActionResult> Cancel(int id = 0)
        {
            if (id <= 0) return RedirectToAction("Index");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentCustomerID = int.Parse(userIdClaim ?? "0");
            if (order.CustomerID != currentCustomerID)
            {
                return RedirectToAction("Index");
            }

            if ((int)order.Status != 1)
            {
                TempData["Message"] = "Lỗi: Không thể hủy đơn hàng đã được xử lý!";
                return RedirectToAction("Details", new { id = order.OrderID });
            }

            bool isSuccess = await SalesDataService.CancelOrderAsync(id);

            if (isSuccess)
            {
                TempData["Message"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["Message"] = "Có lỗi xảy ra, không thể hủy đơn hàng lúc này.";
            }

            return RedirectToAction("Details", new { id = order.OrderID });
        }
    }
}