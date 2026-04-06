using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.HR;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SV22T1020247.Admin.Controllers
{
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("EmployeeSearchInput");
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
        /// Tìm kiếm và trả về kết quả tìm kiếm Nhân viên
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData("EmployeeSearchInput", input);
            return View(result);
        }

        /// <summary>
        /// Thêm nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model); // Dùng chung View Edit
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần xóa</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            // Nếu method là POST thì thực hiện xóa
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }

            // GET: hiển thị thông tin nhân viên cần xóa
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Thay đổi vai trò nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần thay đổi</param>
        /// <returns></returns>
        public IActionResult ChangeRole(int id)
        {
            return View();
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản của nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns></returns>
        public IActionResult ChangePassword(int id)
        {
            ViewData["Title"] = "Đổi mật khẩu nhân viên";
            ViewData["EmployeeId"] = id;

            return View("~/Views/Account/ChangePassword.cshtml");
        }

        /// <summary>
        /// Lưu dữ liệu từ Form Create/Edit (POST)
        /// </summary>
        /// <param name="data">Thông tin nhân viên</param>
        /// <param name="uploadPhoto">File ảnh upload</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                // Kiểm tra dữ liệu đầu vào: FullName và Email là bắt buộc, Email chưa được sử dụng
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                // Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var folder = Path.Combine(ApplicationContext.WWWRootPath, "images/employees");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                // Tiền xử lý dữ liệu trước khi lưu vào database
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                // Lưu dữ liệu vào database (bổ sung hoặc cập nhật)
                if (data.EmployeeID == 0)
                    await HRDataService.AddEmployeeAsync(data); // Thêm mới
                else
                    await HRDataService.UpdateEmployeeAsync(data); // Cập nhật

                return RedirectToAction("Index");
            }
            catch // (Exception ex)
            {
                // TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }
    }
}