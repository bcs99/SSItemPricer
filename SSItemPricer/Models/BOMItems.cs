namespace SSItemPricer.Models
{
    public class BOMItems : Item
    {
        public decimal ItemQuantity { get; set; }

        public decimal Total => BuyUnitPrice * ItemQuantity;

        public void Copy(Item source)
        {
            this.BuyUnitPrice = source.BuyUnitPrice;
            this.UseBOM = source.UseBOM;
            this.LaborCost = source.LaborCost;
            this.Discontinued = source.Discontinued;
            this.ItemNumber = source.ItemNumber;
            this.ItemName = source.ItemName;
            this.Calculated = source.Calculated;
            this.ECOStatusID = source.ECOStatusID;
            this.Status = source.Status;
            this.SetupCost = source.SetupCost;
            this.PieceCost = source.PieceCost;
            this.Notes = source.Notes;
        }
    }
}