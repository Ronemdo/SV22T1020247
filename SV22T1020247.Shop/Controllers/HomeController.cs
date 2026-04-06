using Microsoft.AspNetCore.Mvc;
using SV22T1020247.BusinessLayers;
using SV22T1020247.Models.Catalog;
using SV22T1020247.Models.Common;

namespace SV22T1020247.Shop.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(int page = 1, string searchValue = "", int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0, string sortPrice = "")
        {
            int pageSize = 12;
            var searchCondition = new ProductSearchInput()
            {
                Page = 1,
                PageSize = 999999,
                SearchValue = searchValue ?? "",
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var result = await CatalogDataService.ListProductsAsync(searchCondition);
            var allProducts = result.DataItems as IEnumerable<SV22T1020247.Models.Catalog.Product>;

            if (allProducts != null)
            {
                if (sortPrice == "asc")
                {
                    allProducts = allProducts.OrderBy(p => p.Price); 
                }
                else if (sortPrice == "desc")
                {
                    allProducts = allProducts.OrderByDescending(p => p.Price);
                }
            }
            int rowCount = allProducts?.Count() ?? 0;
            int pageCount = rowCount / pageSize;
            if (rowCount % pageSize > 0)
            {
                pageCount += 1;
            }
            var pagedProducts = allProducts?.Skip((page - 1) * pageSize).Take(pageSize);

            try
            {
                var categorySearchInput = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 999,
                    SearchValue = ""
                };
                var categoryResult = await CatalogDataService.ListCategoriesAsync(categorySearchInput);
                ViewBag.Categories = categoryResult.DataItems;
            }
            catch
            {
                ViewBag.Categories = new List<dynamic>();
            }
            ViewBag.SearchValue = searchCondition.SearchValue;
            ViewBag.CurrentPage = page;
            ViewBag.PageCount = pageCount; 

            ViewBag.CategoryID = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortPrice = sortPrice;
            return View(pagedProducts);
        }
        public async Task<IActionResult> Details(int id = 0)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index");
            }
            var product = await CatalogDataService.GetProductAsync(id);

            if (product == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            return View(product);
        }
    }
}