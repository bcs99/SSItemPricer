namespace SSItemPricer.Models
{
    public class BomPrice : Mis
    {
        public int RootId { get; set; }
        public decimal Labor { get; set; }
        public decimal Price { get; set; }
        public decimal SetupCost { get; set; }
        public decimal PieceCost { get; set; }
        public bool UseBOM { get; set; }
        public bool Discontinued { get; set; }
        public string Status { get; set; }
    }
}