using static MaghsalatiSPlus.Controllers.OrdersController;

namespace MaghsalatiSPlus.Models
{
    public class UpdateOrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? Status { get; set; }
        public List<UpdateOrderItemDto> OrderItems { get; set; } = new();
    }
}
