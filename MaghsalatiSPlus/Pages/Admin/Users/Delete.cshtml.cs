using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class DeleteModel : PageModel
{
    private readonly UserManager<ShopOwner> _userManager;
    public DeleteModel(UserManager<ShopOwner> um) { _userManager = um; }

    [BindProperty]
    public ShopOwner UserToDelete { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null) return NotFound();
        UserToDelete = await _userManager.FindByIdAsync(id);
        if (UserToDelete == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (id == null) return NotFound();
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        return RedirectToPage("./Index");
    }
}