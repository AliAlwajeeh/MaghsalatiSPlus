using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class EditModel : PageModel
{
    private readonly UserManager<ShopOwner> _userManager;
    public EditModel(UserManager<ShopOwner> userManager) { _userManager = userManager; }

    [BindProperty]
    public ShopOwner UserToEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null) return NotFound();
        UserToEdit = await _userManager.FindByIdAsync(id);
        if (UserToEdit == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _userManager.FindByIdAsync(UserToEdit.Id);
        if (user == null) return NotFound();
        user.ShopName = UserToEdit.ShopName;
        user.PhoneNumber = UserToEdit.PhoneNumber;
        await _userManager.UpdateAsync(user);
        return RedirectToPage("./Index");
    }
}