using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MaghsalatiSPlus.Models
{
    public class RegisterDto
    {
        [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string ShopName { get; set; }
        [Required] public string PhoneNumber { get; set; }
        public string? Location { get; set; }
        public IFormFile? ProfileImageFile { get; set; } 
    }
}