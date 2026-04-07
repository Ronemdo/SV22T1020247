using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.Partner;
using SV22T1020247.Admin;

namespace SV22T1020247.Admin.Controllers
{
    [Authorize(Roles = "customer,admin")]
    public class CustomerController : Controller
    {
        /// <summary>
        /// tên của biến dùng để lưu điều kiện tìm kiếm khách hàng trong session
        /// </summary>
        private const string CUSTOMER_SEARCH = "CustomerSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm, hiển thị kết quả tìm kiếm khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
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
        /// Tìm kiếm và trả về kết quả tìm kiếm khách hàng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListCustomersAsync(input);
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
            return View(result);
        }

        /// <summary>
        /// Thêm khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewData["Title"] = "Bổ sung khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Cập nhật khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập tên khách hàng");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (!(await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID)))
                    ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành");

                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }

                if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                if (data.CustomerID == 0)
                {
                    await PartnerDataService.AddCustomerAsync(data);
                }
                else
                {
                    await PartnerDataService.UpdateCustomerAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận, Vui lòng thử lại sau");
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xóa khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteCustomerAsync(id);
                return RedirectToAction("Index");
            }
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.CanDelete = !(await PartnerDataService.IsUsedCustomerAsync(id));

            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật mật khẩu khách hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int customerId, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không trùng khớp");

            if (!ModelState.IsValid)
            {
                var model = await PartnerDataService.GetCustomerAsync(customerId);
                return View(model);
            }

            try
            {
                var customer = await PartnerDataService.GetCustomerAsync(customerId);
                if (customer == null)
                    return RedirectToAction("Index");

                customer.Password = CryptHelper.HashMD5(newPassword);

                await PartnerDataService.UpdateCustomerAsync(customer);

                TempData["Message"] = "Đổi mật khẩu khách hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận, Vui lòng thử lại sau");
                var model = await PartnerDataService.GetCustomerAsync(customerId);
                return View(model);
            }
        }
    }
}