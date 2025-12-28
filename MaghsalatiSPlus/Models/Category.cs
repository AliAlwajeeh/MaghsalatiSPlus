namespace MaghsalatiSPlus.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        
        public string ShopOwnerId { get; set; }
        public virtual ShopOwner ShopOwner { get; set; }
    }
}