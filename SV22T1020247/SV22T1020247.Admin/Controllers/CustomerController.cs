using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.Partner;


namespace SV22T1020247.Admin.Controllers
{
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
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("CustomerSearchInput");
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
            ApplicationContext.SetSessionData("CustomerSearchInput", input);
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
                ViewBag.Title = data.CustomerID == 0 ? "Bo sung khach hang" : "Cap nhat thong tin khach hang";
                //TODO:kiem tra tinh hop le cua du lieu va thong bao loi neu du lieu khong hop le
                //Su dung ModelSate để lưu các tình huống lỗi và thông báo lỗi cho người dùng trên view 
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui long nhap ten khach hang");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email khong duoc de trong");
                else if (!(await PartnerDataService.ValidateCustomerEmailAsync(data.Email, data.CustomerID)))
                    ModelState.AddModelError(nameof(data.Email), "Email nay bị trung");
                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui long chon tinh/thanh");
                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }

                //Hieu chinh du lieu theo qui dinh cua he thong
                if (string.IsNullOrEmpty(data.ContactName)) data.ContactName = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                
                //Yeu cau luu du lieu vao CSDL 
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
            catch (Exception ex)
            {
                //ghi log loi dua vao thong tin trong Exception(ex.Message,ex.StackTrace
                ModelState.AddModelError("Error", "He thong dang ban,vui long thu lai sau");
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
            // Neu method la POST thi xoa
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteCustomerAsync(id);
                return RedirectToAction("Index");
            }

            //GET: hien thi thong tin khach hang can xoa
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.CanDelete = !(await PartnerDataService.IsUsedCustomerAsync(id));

            return View(model);
        }
        /// <summary>
        /// Đổi mật khẩu tài khoảng khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns></returns>
        public IActionResult ChangePassword(int id)
        {
            ViewData["Title"] = "Đổi mật khẩu khách hàng";
            ViewData["CustomerId"] = id;

            return View("~/Views/Account/ChangePassword.cshtml");
        }
    }
}
