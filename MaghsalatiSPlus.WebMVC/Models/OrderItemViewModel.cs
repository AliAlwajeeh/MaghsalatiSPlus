namespace MaghsalatiSPlus.WebMVC.Models
{
    public class OrderItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public ServiceType Service { get; set; }

    }
}
