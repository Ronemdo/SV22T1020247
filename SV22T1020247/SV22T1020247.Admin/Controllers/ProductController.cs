using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Models.Catalog;
using SV22T1020247.Models.Common;

namespace SV22T1020247.Admin.Controllers
{
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH = "ProductSearchInput";
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH) ?? new ProductSearchInput()
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize,
                SearchValue = "",
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            await LoadViewBagData();
            return View(input);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var repo = new ProductRepository(Configuration.ConnectionString);
            var result = await repo.ListAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
            return PartialView(result);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            await LoadViewBagData();
            var model = new Product() { ProductID = 0, IsSelling = true };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            var repo = new ProductRepository(Configuration.ConnectionString);
            var model = await repo.GetAsync(id);
            if (model == null) return RedirectToAction("Index");

            ViewBag.ProductID = id;
            await LoadViewBagData();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Product data, string priceStr, IFormFile? uploadPhoto)
        {
            try
            {
                // 1. XỬ LÝ GIÁ (Chuyển đổi từ chuỗi sang decimal)
                if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any,
                    new System.Globalization.CultureInfo("vi-VN"), out decimal price))
                    data.Price = price;
                else
                    ModelState.AddModelError("priceStr", "Giá không hợp lệ");

                // 2. VALIDATION
                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được trống");
                if (data.CategoryID == 0)
                    ModelState.AddModelError("CategoryID", "Chưa chọn loại hàng");
                if (data.SupplierID == 0)
                    ModelState.AddModelError("SupplierID", "Chưa chọn nhà cung cấp");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật mặt hàng";
                    await LoadViewBagData();
                    return View("Edit", data);
                }

                // 3. XỬ LÝ ẢNH ĐẠI DIỆN
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                    string folder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                // 4. LƯU DATABASE
                var repo = new ProductRepository(Configuration.ConnectionString);
                if (data.ProductID == 0)
                {
                    int newId = await repo.AddAsync(data);
                    return RedirectToAction("Edit", new { id = newId });
                }
                else
                {
                    await repo.UpdateAsync(data);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                await LoadViewBagData();
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xử lý Xóa sản phẩm (Sửa lỗi 404)
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var repo = new ProductRepository(Configuration.ConnectionString);
            if (Request.Method == "POST")
            {
                await repo.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            var model = await repo.GetAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model); // Phải có file Delete.cshtml trong Views/Product/
        }

        // ==========================================
        // QUẢN LÝ ẢNH THƯ VIỆN
        // ==========================================

        public async Task<IActionResult> Photo(int id, string method, long photoId = 0)
        {
            var repo = new ProductRepository(Configuration.ConnectionString);
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";
                    return View(new ProductPhoto { ProductID = id, PhotoID = 0, DisplayOrder = 1 });
                case "edit":
                    ViewBag.Title = "Thay đổi ảnh";
                    return View(await repo.GetPhotoAsync(photoId));
                case "delete":
                    await repo.DeletePhotoAsync(photoId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Edit", new { id = id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images", "products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }

            var repo = new ProductRepository(Configuration.ConnectionString);
            if (data.PhotoID == 0) await repo.AddPhotoAsync(data);
            else await repo.UpdatePhotoAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        // ==========================================
        // QUẢN LÝ THUỘC TÍNH
        // ==========================================

        public async Task<IActionResult> Attribute(int id, string method, long attributeId = 0)
        {
            var repo = new ProductRepository(Configuration.ConnectionString);
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính";
                    return View(new ProductAttribute { ProductID = id, AttributeID = 0, DisplayOrder = 1 });
                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính";
                    return View(await repo.GetAttributeAsync(attributeId));
                case "delete":
                    await repo.DeleteAttributeAsync(attributeId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Edit", new { id = id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            var repo = new ProductRepository(Configuration.ConnectionString);
            if (data.AttributeID == 0) await repo.AddAttributeAsync(data);
            else await repo.UpdateAttributeAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }

        // --- HÀM HỖ TRỢ ---
        private async Task LoadViewBagData()
        {
            var categoryDB = new CategoryRepository(Configuration.ConnectionString);
            var supplierDB = new SupplierRepository(Configuration.ConnectionString);
            ViewBag.Categories = (await categoryDB.ListAsync(new PaginationSearchInput { Page = 1, PageSize = 100 })).DataItems;
            ViewBag.Suppliers = (await supplierDB.ListAsync(new PaginationSearchInput { Page = 1, PageSize = 100 })).DataItems;
        }
    }
}