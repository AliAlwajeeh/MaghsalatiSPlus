namespace MaghsalatiSPlus.Models
{
    public class UpdateOrderItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public ServiceType Service { get; set; }   
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
