
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace MaghsalatiSPlus.WebMVC.Models
{
   
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public string? RecipientName { get; set; }
        public DateTime PickupDate { get; set; }
        public string OrderItemsJson { get; set; }
        public List<IFormFile>? ItemImages { get; set; }
    }
}