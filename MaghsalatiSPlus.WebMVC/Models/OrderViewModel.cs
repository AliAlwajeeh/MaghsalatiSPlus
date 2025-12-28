

using System;

namespace MaghsalatiSPlus.WebMVC.Models
{
  
    public class OrderViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }
        public int CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        
         public List<OrderItemViewModel> OrderItems { get; set; }
    }
}