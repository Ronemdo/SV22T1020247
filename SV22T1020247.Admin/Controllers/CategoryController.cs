using Microsoft.AspNetCore.Mvc;
using SV22T1020247.Models.Catalog;
using SV22T1020247.Admin;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Common;

using System;
using System.Threading.Tasks;

namespace SV22T1020247.Admin.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("CategorySearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData("CategorySearchInput", input);
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var model = new Category() { CategoryID = 0 };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin loại hàng";
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được trống");

                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.CategoryID == 0 ? "Bổ sung loại hàng" : "Cập nhật loại hàng";
                    return View("Edit", data);
                }

                if (data.CategoryID == 0)
                    await CatalogDataService.AddCategoryAsync(data);
                else
                    await CatalogDataService.UpdateCategoryAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Lỗi hệ thống, vui lòng thử lại sau.");
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteCategoryAsync(id);
                return RedirectToAction("Index");
            }
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null) return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await CatalogDataService.IsUsedCategoryAsync(id));
            return View(model);
        }
    }
}