// ›Ì: Controllers/HomeController.cs

using MaghsalatiSPlus.WebMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic; // „Â„
using MaghsalatiSPlus.WebMVC.Services; // „Â„

namespace MaghsalatiSPlus.WebMVC.Controllers
{
    // ·« ÌÊÃœ [Authorize] Â‰«
    public class HomeController : Controller
    {
        // ‰Õ «Ã ≈·Ï ApiClientService ·Ã·» «·ÿ·»«  Ê«·»Ì«‰«  «·√Œ—Ï
        private readonly ApiClientService _apiService;

        public HomeController(ApiClientService apiService)
        {
            _apiService = apiService;
        }

        // --- «·’›Õ… «·—∆Ì”Ì… (Dashboard) ---
        public async Task<IActionResult> Index()
        {
            // ⁄—÷ «”„ «·„Õ· „‰ «·Ã·”… (Session) ≈–« ﬂ«‰ «·„” Œœ„ ﬁœ ”Ã· œŒÊ·Â "ÊÂ„Ì«"
            ViewBag.ShopName = HttpContext.Session.GetString("CurrentShopName");
            var userId = HttpContext.Session.GetString("CurrentUserId");

            List<OrderViewModel> ordersToShow;

            if (!string.IsNullOrEmpty(userId))
            {
                // ≈–« ﬂ«‰ «·„” Œœ„ „”Ã·« œŒÊ·Â° «Ã·» ÿ·»« Â
                ordersToShow = await _apiService.GetOrdersByOwnerAsync(userId);
            }
            else
            {
                // ≈–« ·„ Ìﬂ‰ „”Ã·« œŒÊ·Â° «⁄—÷ ﬁ«∆„… ›«—€…
                ordersToShow = new List<OrderViewModel>();
            }

            return View(ordersToShow ?? new List<OrderViewModel>());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}