using System.ComponentModel.DataAnnotations;
namespace MaghsalatiSPlus.Models
{
    public class CreateCustomerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}