using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaghsalatiSPlus.WebMVC.Models
{
    public class UpdateOrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اختيار العميل مطلوب.")]
        public int CustomerId { get; set; }

        public string? Status { get; set; }

        [MinLength(1, ErrorMessage = "يجب إضافة عنصر واحد على الأقل.")]
        public List<CreateOrderItemViewModel> OrderItems { get; set; } = new();
    }
}
