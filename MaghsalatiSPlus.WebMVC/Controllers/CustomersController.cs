using MaghsalatiSPlus.WebMVC.Models;
using MaghsalatiSPlus.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MaghsalatiSPlus.WebMVC.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApiClientService _apiService;

        public CustomersController(ApiClientService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Customers
        public async Task<IActionResult> Index()
        {
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            var customers = await _apiService.GetCustomersByOwnerAsync(string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId);
            return View(customers ?? new List<CustomerViewModel>());
        }

        // GET: /Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _apiService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // GET: /Customers/Create
        public IActionResult Create()
        {
            var shopOwnerId = HttpContext.Session.GetString("CurrentUserId");
            var model = new CreateCustomerDto
            {
                ShopOwnerId = string.IsNullOrEmpty(shopOwnerId) ? null : shopOwnerId
            };
            return View(model);
        }

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _apiService.CreateCustomerAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "تم إضافة العميل بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "فشل إنشاء العميل عبر الـ API.");
            return View(model);
        }

        // GET: /Customers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _apiService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();

            // تحويل إلى DTO للتعديل إذا أردت استخدام CreateCustomerDto
            var dto = new CreateCustomerDto
            {
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                ShopOwnerId = customer.ShopOwnerId
            };

            ViewBag.CustomerId = id;
            return View(dto);
        }

        // POST: /Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCustomerDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _apiService.UpdateCustomerAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "تم تعديل بيانات العميل.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "فشل تعديل بيانات العميل عبر الـ API.");
            return View(model);
        }

        // GET: /Customers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _apiService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: /Customers/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _apiService.DeleteCustomerAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "تم حذف العميل.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "فشل حذف العميل عبر الـ API.");
            var customer = await _apiService.GetCustomerByIdAsync(id);
            return View("Delete", customer);
        }
    }
}
