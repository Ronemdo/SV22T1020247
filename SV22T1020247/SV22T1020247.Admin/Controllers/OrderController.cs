using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Catalog;
using SV22T1020247.Models.Sales;

using System.Threading.Tasks;

namespace SV22T1020247.Admin.Controllers
{
    /// <summary>
    /// Quản lý đơn hàng (Order)
    /// </summary>
    public class OrderController : Controller
    {
        private const int PAGESIZE = 10;
        private const string ORDER_SEARCH_INPUT = "OrderSearchInput";
        private const string PRODUCT_SEARCH_INPUT = "ProductSearchOrder";

        /// <summary>
        /// Trang danh sách đơn hàng
        /// </summary>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_INPUT);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);

            ApplicationContext.SetSessionData(ORDER_SEARCH_INPUT, input);

            return PartialView(result);
        }

        /// <summary>
        /// Tìm sản phẩm khi tạo đơn
        /// </summary>
        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_INPUT, input);

            var result = await CatalogDataService.ListProductsAsync(input);

            return PartialView("_SearchProduct", result);
        }

        /// <summary>
        /// Chi tiết đơn hàng
        /// </summary>
        public IActionResult Detail(int id)
        {
            return View();
        }

        /// <summary>
        /// Tạo đơn hàng
        /// </summary>
        public IActionResult Create()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_INPUT);

            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 3,
                    SearchValue = ""
                };
            }

            return View(input);
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public IActionResult Delete(int id)
        {
            return View();
        }

        /// <summary>
        /// Xóa sản phẩm trong giỏ
        /// </summary>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            return View();
        }

        /// <summary>
        /// Sửa sản phẩm trong giỏ
        /// </summary>
        public IActionResult EditCartItem(int id, int productId)
        {
            return View();
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public IActionResult ClearCart(int id)
        {
            return View();
        }

        /// <summary>
        /// Hoàn tất đơn
        /// </summary>
        public IActionResult Finish(int id)
        {
            return View();
        }

        /// <summary>
        /// Chấp nhận đơn
        /// </summary>
        public IActionResult Accept(int id)
        {
            return View();
        }

        /// <summary>
        /// Từ chối đơn
        /// </summary>
        public IActionResult Reject(int id)
        {
            return View();
        }

        /// <summary>
        /// Hủy đơn
        /// </summary>
        public IActionResult Cancel(int id)
        {
            return View();
        }

        /// <summary>
        /// Giao hàng
        /// </summary>
        public IActionResult Shipping(int id)
        {
            return View();
        }
    }
}
