using System.Collections.Generic;

namespace MaghsalatiSPlus.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public string ShopOwnerId { get; set; }
        public virtual ShopOwner ShopOwner { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}