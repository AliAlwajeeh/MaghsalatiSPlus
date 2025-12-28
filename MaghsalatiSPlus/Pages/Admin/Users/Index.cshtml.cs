using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaghsalatiSPlus.Pages.Admin.Users
{
    // TODO: √÷› [Authorize] Â‰« ·«Õﬁ« · √„Ì‰ «·’›Õ…
    public class IndexModel : PageModel
    {
        private readonly UserManager<ShopOwner> _userManager;
        public IndexModel(UserManager<ShopOwner> userManager) { _userManager = userManager; }
        public IList<ShopOwner> Users { get; set; } = new List<ShopOwner>();

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.ToListAsync();
        }
    }
}