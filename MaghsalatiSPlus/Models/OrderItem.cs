
namespace MaghsalatiSPlus.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } 

       
        public byte[]? ImageData { get; set; }

      
        public ServiceType Service { get; set; }

        // لربط الصنف بالطلب الرئيسي
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        // لربط الصنف بالقسم
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}