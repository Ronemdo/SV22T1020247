using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.HR;

namespace SV22T1020247.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        private const string EMPLOYEE_SEARCH = "EmployeeSearchInput";
        public IActionResult Index()
        {   
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH);
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
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH, input);
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
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.AllowDelete = !(await HRDataService.IsUsedEmployeeAsync(id));

            return View(model);
        }

        /// <summary>
        /// Thay đổi vai trò nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần thay đổi</param>
        /// <returns></returns>
        /// <summary>
        /// GET: Hiển thị giao diện thay đổi vai trò nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần thay đổi</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangeRole(int id)
        {
            // 1. Lấy thông tin nhân viên từ Database
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.AvailableRoles = new List<dynamic>
{
    new { RoleName = "customer", Description = "Quản lý khách hàng" },
    new { RoleName = "product", Description = "Quản lý mặt hàng" },
    new { RoleName = "sale", Description = "Quản lý đơn hàng" },
    new { RoleName = "admin", Description = "Quản trị hệ thống" }
};

            // Truyền dữ liệu nhân viên sang View
            return View(model);
        }

        /// <summary>
        /// POST: Xử lý lưu các quyền được chọn vào Database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, List<string> selectedRoles)
        {
            // Lấy lại thông tin nhân viên cũ
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            // selectedRoles tự động nhận danh sách các checkbox được tích (ví dụ: ["Quản lý khách hàng", "Quản lý mặt hàng"])
            // Ta nối chúng lại thành 1 chuỗi cách nhau bởi dấu phẩy
            string roleNames = selectedRoles != null && selectedRoles.Count > 0
                               ? string.Join(",", selectedRoles)
                               : "";

            // Cập nhật thuộc tính RoleNames của nhân viên
            employee.RoleNames = roleNames;

            // Gọi hàm cập nhật lại thông tin nhân viên xuống Database
            await HRDataService.UpdateEmployeeAsync(employee);

            // Xong xuôi thì đá về trang danh sách
            TempData["Message"] = "Đã cập nhật phân quyền thành công!";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản của nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên</param>
        /// <returns></returns>
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null) return RedirectToAction("Index");

            // 1. Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ mật khẩu mới và xác nhận!");
                return View(employee);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu xác nhận không khớp!");
                return View(employee);
            }

            // 2. Mã hóa mật khẩu mới (Gọi hàm HashMD5 từ class CryptHelper của bạn)
            string hashedNewPassword = CryptHelper.HashMD5(newPassword);

            // 3. Gọi Service cập nhật mật khẩu (Hàm này bạn đã tạo ở SecurityDataService)
            bool result = SecurityDataService.ResetPassword(id.ToString(), hashedNewPassword);

            if (!result)
            {
                ModelState.AddModelError("Error", "Cập nhật mật khẩu thất bại!");
                return View(employee);
            }

            // Đổi thành công thì báo thông điệp và quay về danh sách
            TempData["Message"] = "Đã cấp lại mật khẩu thành công!";
            return RedirectToAction("Index");
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