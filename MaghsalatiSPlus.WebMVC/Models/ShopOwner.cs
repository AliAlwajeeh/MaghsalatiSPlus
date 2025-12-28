

using Microsoft.AspNetCore.Identity; 

namespace MaghsalatiSPlus.WebMVC.Models
{
 
    public class ShopOwner : IdentityUser
    {
      
        public string ShopName { get; set; }
    }
}