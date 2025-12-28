using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
namespace MaghsalatiSPlus.WebMVC.Models
{
    public class ApiErrorResponse
    {
        public string? Message { get; set; }
        public IEnumerable<IdentityError>? Errors { get; set; }
    }
}