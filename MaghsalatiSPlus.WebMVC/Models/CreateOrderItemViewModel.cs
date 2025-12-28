
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace MaghsalatiSPlus.WebMVC.Models
{
    public class CreateOrderItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public ServiceType Service { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

}