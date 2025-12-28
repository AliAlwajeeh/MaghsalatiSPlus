using System.ComponentModel.DataAnnotations;

namespace MaghsalatiSPlus.WebMVC.Models
{
    public class CreateOrderItemDto
    {
        [Required]
        public string ItemName { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public ServiceType Service { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

      
        public int CategoryId { get; set; }
    }
}