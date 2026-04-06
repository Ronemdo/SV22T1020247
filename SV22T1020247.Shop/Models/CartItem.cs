namespace SV22T1020247.Shop.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public int Quantity { get; set; } = 0;
        public decimal SalePrice { get; set; } = 0;
        public decimal TotalPrice => Quantity * SalePrice;
    }
}