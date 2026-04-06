using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Common;


namespace SV22T1020247.Admin.Controllers
{
    public class ShipperController : Controller
    {
        private const int PAGESIZE = 5;

        /// <summary>
        /// hiển thị danh sách dữ liệu
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("ShipperSearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả tìm kiếm người giao hàng 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListShippersAsync(input);
            ApplicationContext.SetSessionData("ShipperSearchInput", input);
            return View(result);
        }

        /// <summary>
        /// Tạo shipper
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewData["Title"] = "Bổ sung shipper";
            return View("Edit");
        }
        /// <summary>
        /// Cập nhật thông tin shipper
        /// </summary>
        /// <param name="id">Mã shipper cần cập nhật</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Sửa shipper";
            return View();
        }

        /// <summary>
        /// Xóa shipper
        /// </summary>
        /// <param name="id">Mã shipper cần xóa</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}
