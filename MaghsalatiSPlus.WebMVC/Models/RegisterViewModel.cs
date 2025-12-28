
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MaghsalatiSPlus.WebMVC.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password), Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }

        [Required, Display(Name = "اسم المحل")]
        public string ShopName { get; set; }

        [Required, Phone, Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "صورة البروفايل")]
        public IFormFile? ProfileImageFile { get; set; } 
        public string Location { get; set; }
    }
}