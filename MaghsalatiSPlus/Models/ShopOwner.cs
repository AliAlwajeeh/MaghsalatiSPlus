using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MaghsalatiSPlus.Models
{
   
    public class ShopOwner : IdentityUser
    {
        public string ShopName { get; set; }
        public string? Location { get; set; }

       
        public byte[]? ProfileImageData { get; set; }

        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
       
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}