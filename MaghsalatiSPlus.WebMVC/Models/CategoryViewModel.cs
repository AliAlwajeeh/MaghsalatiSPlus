using System.ComponentModel.DataAnnotations;

namespace MaghsalatiSPlus.WebMVC.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم يجب ألا يتجاوز 50 حرفًا")]
        public string Name { get; set; } = string.Empty;

       
        public string? ShopOwnerId { get; set; }
    }

    public class CategoryCreateUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? ShopOwnerId { get; set; }
    }
}
