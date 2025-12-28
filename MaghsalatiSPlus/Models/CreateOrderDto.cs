using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace MaghsalatiSPlus.Models
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }


        public string OrderItemsJson { get; set; }


        public List<IFormFile>? ItemImages { get; set; }
    }
}