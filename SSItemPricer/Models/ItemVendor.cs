namespace SSItemPricer.Models
{
    public class ItemVendor : Mis
    {
        public int ItemID { get; set; }
        public int VendorPriority { get; set; }
        public bool UseBOM { get; set; }
        public decimal BuyUnitPrice { get; set; }
        public string VendorName { get; set; }
    }
}