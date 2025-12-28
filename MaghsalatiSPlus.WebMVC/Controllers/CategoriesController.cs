using MaghsalatiSPlus.WebMVC.Models;
using MaghsalatiSPlus.WebMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MaghsalatiSPlus.WebMVC.Controllers
{
    // [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApiClientService _apiService;

        public CategoriesController(ApiClientService apiService)
        {
            _apiService = apiService;
        }

        // عرض جميع الأقسام
        public async Task<IActionResult> Index()
        {
            // إذا كان المستخدم مسجلاً دخوله، اجلب الأقسام الخاصة به فقط
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            IEnumerable<CategoryViewModel> categories;
            if (string.IsNullOrEmpty(shopOwnerId))
            {
                // إذا لم يكن هناك مستخدم، اجلب كل الأقسام (جاهزة أو عامة)
                categories = await _apiService.GetCategoriesByOwnerAsync(null);
            }
            else
            {
                // إذا كان هناك مستخدم، اجلب الأقسام الخاصة به فقط
                categories = await _apiService.GetCategoriesByOwnerAsync(shopOwnerId);
            }
            return View(categories);
        }

        // عرض تفاصيل قسم
        public async Task<IActionResult> Details(int id)
        {
            var category = await _apiService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "تعذر العثور على القسم أو أنك لا تملك صلاحية الوصول إليه.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // عرض نموذج إنشاء قسم
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // تنفيذ إنشاء قسم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            // احصل على ShopOwnerId من الجلسة
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            if (!string.IsNullOrEmpty(shopOwnerId))
                model.ShopOwnerId = shopOwnerId;

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.ShopOwnerId))
                model.ShopOwnerId = null;

            var (success, errorMessage) = await _apiService.CreateCategoryAsync(model);
            if (success)
            {
                TempData["Success"] = "تم إنشاء القسم بنجاح ✅";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "حدث خطأ أثناء إنشاء القسم");
            return View(model);
        }

        // عرض نموذج تعديل قسم
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _apiService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "تعذر العثور على القسم أو أنك لا تملك صلاحية تعديله.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // تنفيذ تعديل قسم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.ShopOwnerId))
                model.ShopOwnerId = null;

            var (success, errorMessage) = await _apiService.UpdateCategoryAsync(model);
            if (success)
            {
                TempData["Success"] = "تم تعديل القسم بنجاح ✅";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "حدث خطأ أثناء تعديل القسم");
            return View(model);
        }

        // عرض تأكيد حذف قسم
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _apiService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "تعذر العثور على القسم أو أنك لا تملك صلاحية حذفه.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // تنفيذ حذف قسم
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, errorMessage) = await _apiService.DeleteCategoryAsync(id);
            if (errorMessage?.Contains("cannot be deleted") == true)
            {
                errorMessage = "لا يمكن حذف هذا القسم لأنه مستخدم في طلبات حالية.";
            }

            if (success)
            {
                TempData["Success"] = "تم حذف القسم بنجاح 🗑️";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = errorMessage ?? "حدث خطأ أثناء حذف القسم";
            return RedirectToAction(nameof(Index));
        }
    }
}
