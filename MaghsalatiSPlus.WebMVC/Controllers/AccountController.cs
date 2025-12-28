using MaghsalatiSPlus.WebMVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MaghsalatiSPlus.WebMVC.Models;

namespace MaghsalatiSPlus.WebMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClientService _apiService;

        public AccountController(ApiClientService apiService)
        {
            _apiService = apiService;
        }

      
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // --- معالجة طلب إنشاء الحساب ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errorMessage) = await _apiService.RegisterAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "✅ تم إنشاء حسابك بنجاح! يمكنك الآن تسجيل الدخول.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, $"❌ فشل إنشاء الحساب: {errorMessage}");
            return View(model);
        }

        // --- عرض صفحة تسجيل الدخول ---
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        // --- معالجة طلب تسجيل الدخول ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _apiService.LoginAsync(model);

            if (result != null && !string.IsNullOrWhiteSpace(result.ShopOwnerId))
            {
                // تخزين البيانات الأساسية في الجلسة
                HttpContext.Session.SetString("CurrentUserId", result.ShopOwnerId);
                HttpContext.Session.SetString("CurrentShopName", result.ShopName ?? "بدون اسم");

                // يمكنك تخزين التوكن أيضًا إذا احتجته لاحقًا
                _apiService.SetAuthToken(result.Token);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "⚠️ فشل تسجيل الدخول. تحقق من البريد أو كلمة المرور.");
            return View(model);
        }

        // --- تسجيل الخروج ---
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
