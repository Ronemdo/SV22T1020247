namespace SV22T1020247.Admin.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public string Photo { get; set; } = ""; // Ảnh đại diện
        public bool IsSelling { get; set; } = true;
    }

    public class ProductPhoto
    {
        public int PhotoID { get; set; }
        public string Photo { get; set; } = "";
        public string Description { get; set; } = "";
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }
    }

    public class ProductAttribute
    {
        public int AttributeID { get; set; }
        public string AttributeName { get; set; } = ""; // Ví dụ: Màu sắc
        public string AttributeValue { get; set; } = ""; // Ví dụ: Đỏ
        public int DisplayOrder { get; set; }
    }
}