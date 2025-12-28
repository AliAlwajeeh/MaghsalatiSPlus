namespace MaghsalatiSPlus.Models
{
    public class CreateOrderItemDto
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public ServiceType Service { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}