using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MaghsalatiSPlus.WebMVC.Models
{
    public class CreateOrderViewModel
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemViewModel> OrderItems { get; set; } = new();
        public List<IFormFile> ItemImages { get; set; } = new();
    }

}