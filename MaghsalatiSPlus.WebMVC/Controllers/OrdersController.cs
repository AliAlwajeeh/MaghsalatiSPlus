using MaghsalatiSPlus.WebMVC.Models;
using MaghsalatiSPlus.WebMVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MaghsalatiSPlus.WebMVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApiClientService _apiService;

        public OrdersController(ApiClientService apiService)
        {
            _apiService = apiService;
        }

        // ---------- قائمة الطلبات ----------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // استخدم الـ Session إن وجد، وإلا دع الخدمة تستخدم ShopOwnerId الافتراضي من الإعدادات
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            var orders = await _apiService.GetOrdersByOwnerAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId);
            return View(orders ?? new List<OrderViewModel>());
        }

        // ---------- إنشاء طلب (عرض النموذج) ----------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            await PopulateDropdownsAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId);

            // عنصر ابتدائي لضمان الأسماء المفهرسة في الـ View
            var model = new CreateOrderViewModel();
            if (model.OrderItems == null)
                model.OrderItems = new List<CreateOrderItemViewModel>();
            model.OrderItems.Add(new CreateOrderItemViewModel()); // لكل عنصر صورة واحدة عبر OrderItems[i].ImageFile

            return View(model);
        }

        // ---------- إنشاء طلب (حفظ) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");

            // نظّف العناصر الفارغة
            model.OrderItems = (model.OrderItems ?? new List<CreateOrderItemViewModel>())
                .Where(i => i != null && !string.IsNullOrWhiteSpace(i.ItemName))
                .ToList();

            // اربط ملفات الصور لكل عنصر (صورة واحدة لكل عنصر)
            BindItemFilesFromRequest(model);

            // تحققات أساسية
            if (model.CustomerId <= 0)
                ModelState.AddModelError(nameof(model.CustomerId), "اختيار العميل مطلوب.");

            if (model.OrderItems.Count == 0)
                ModelState.AddModelError(nameof(model.OrderItems), "أضف عنصرًا واحدًا على الأقل.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId, model.CustomerId);
                if (model.OrderItems.Count == 0)
                    model.OrderItems.Add(new CreateOrderItemViewModel());
                return View(model);
            }

            var (success, errorMessage) = await _apiService.CreateOrderAsync(model);

            if (success)
                return RedirectToAction(nameof(Index));

            // فشل من API: أظهر الرسالة وأعد تعبئة القوائم
            ModelState.AddModelError(string.Empty, $"فشل إنشاء الطلب: {errorMessage}");

            await PopulateDropdownsAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId, model.CustomerId);
            if (model.OrderItems.Count == 0)
                model.OrderItems.Add(new CreateOrderItemViewModel());

            return View(model);
        }

        // ---------- تعبئة القوائم (عميل/قسم) ----------
        /*private async Task PopulateDropdownsAsync(string? shopOwnerId, int? selectedCustomerId = null)
        {
            // دع الخدمة تتكفل بـ ShopOwnerId الافتراضي عند تمرير null
            var customers = await _apiService.GetCustomersByOwnerAsync(shopOwnerId) ?? new List<CustomerViewModel>();
            var categories = (await _apiService.GetCategoriesByOwnerAsync(shopOwnerId))?.ToList() ?? new List<CategoryViewModel>();

            // ابنِ القوائم مباشرة من الموديلات لضمان تطابق أسماء الخصائص
            ViewBag.CustomerId = new SelectList(customers, nameof(CustomerViewModel.Id), nameof(CustomerViewModel.Name), selectedCustomerId);
            ViewBag.CategoryId = new SelectList(categories, nameof(CategoryViewModel.Id), nameof(CategoryViewModel.Name));

            // لإظهار رسائل إرشادية في الواجهة عند عدم وجود بيانات
            ViewBag.NoCustomers = customers.Count == 0;
            ViewBag.NoCategories = categories.Count == 0;
        }*/
        private async Task PopulateDropdownsAsync(string? shopOwnerId, int? selectedCustomerId = null)
        {
            // استخدم ShopOwnerId من الجلسة إذا كان المستخدم مسجلاً دخوله
            if (string.IsNullOrEmpty(shopOwnerId))
                shopOwnerId = HttpContext.Session.GetString("CurrentUserId");

            var customers = await _apiService.GetCustomersByOwnerAsync(shopOwnerId) ?? new List<CustomerViewModel>();
            var categories = (await _apiService.GetCategoriesByOwnerAsync(shopOwnerId))?.ToList() ?? new List<CategoryViewModel>();

            ViewBag.CustomerId = new SelectList(customers, nameof(CustomerViewModel.Id), nameof(CustomerViewModel.Name), selectedCustomerId);
            ViewBag.CategoryId = new SelectList(categories, nameof(CategoryViewModel.Id), nameof(CategoryViewModel.Name));
            ViewBag.NoCustomers = customers.Count == 0;
            ViewBag.NoCategories = categories.Count == 0;
        }

        // ---------- ربط الملفات المفهرسة من الطلب ----------
        // يلتقط الملفات المسماة بالطريقة: OrderItems[i].ImageFile
        private void BindItemFilesFromRequest(CreateOrderViewModel model)
        {
            if (model?.OrderItems == null || model.OrderItems.Count == 0)
                return;

            var files = HttpContext.Request.Form.Files;

            for (int i = 0; i < model.OrderItems.Count; i++)
            {
                var item = model.OrderItems[i];
                if (item == null) continue;

                // اسم الحقل المتوقع في الـ View: <input type="file" name="OrderItems[0].ImageFile" />
                var key = $"OrderItems[{i}].ImageFile";

                if (item.ImageFile == null)
                {
                    var file = files.GetFile(key);
                    if (file != null && file.Length > 0)
                        item.ImageFile = file;
                }
            }
        }

        // ---------- تفاصيل الطلب ----------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var order = await _apiService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return View(order); // يتطلب Views/Orders/Details.cshtml | @model OrderViewModel
        }

        // ---------- تعديل الطلب (عرض النموذج) ----------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();

            var order = await _apiService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            var model = new UpdateOrderViewModel
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status,
                OrderItems = order.OrderItems?.Select(item => new CreateOrderItemViewModel
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    CategoryId = item.CategoryId,
                    Service = item.Service
                }).ToList() ?? new List<CreateOrderItemViewModel>()
            };

            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            await PopulateDropdownsAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId, order.CustomerId);

            return View(model);
        }

        // ---------- تعديل الطلب (حفظ) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateOrderViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                // إعادة تحميل عناصر الطلب عند وجود أخطاء في النموذج
                var order = await _apiService.GetOrderByIdAsync(id);
                if (order != null)
                {
                    model.OrderItems = order.OrderItems?.Select(item => new CreateOrderItemViewModel
                    {
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        CategoryId = item.CategoryId,
                        Service = item.Service,
                    }).ToList() ?? new List<CreateOrderItemViewModel>();
                }

                var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
                await PopulateDropdownsAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId, model.CustomerId);
                return View(model);
            }

            // تنفيذ التحديث عند صلاحية النموذج
            var (success, errorMessage) = await _apiService.UpdateOrderAsync(model);
            if (success)
                return RedirectToAction(nameof(Index));

            // إعادة تحميل عناصر الطلب عند فشل التحديث
            ModelState.AddModelError(string.Empty, $"فشل التحديث: {errorMessage}");

            var orderAfterFail = await _apiService.GetOrderByIdAsync(id);
            if (orderAfterFail != null)
            {
                model.OrderItems = orderAfterFail.OrderItems?.Select(item => new CreateOrderItemViewModel
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    CategoryId = item.CategoryId,
                    Service = item.Service
                }).ToList() ?? new List<CreateOrderItemViewModel>();
            }

            var ownerId = HttpContext.Session.GetString("CurrentUserId");
            await PopulateDropdownsAsync(string.IsNullOrEmpty(ownerId) ? null : ownerId, model.CustomerId);
            return View(model);
        }



        // ---------- حذف الطلب (عرض التأكيد) ----------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            var order = await _apiService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return View(order); // يتطلب Views/Orders/Delete.cshtml | @model OrderViewModel
        }

        // ---------- حذف الطلب (تنفيذ) ----------
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
