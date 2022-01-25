namespace SSItemPricer.Models
{
    public class Item : Mis
    {
        private decimal _buyUnitPrice;
        private decimal _laborCost;
        private bool _calculated;
        private bool _isBom;
        private string _notes;

        public int ItemNumber { get; set; }
        public string ItemName { get; set; }
        public bool Discontinued { get; set; }
        public int ECOStatusID { get; set; }
        public string Status { get; set; }
        public decimal PartsCost => BuyUnitPrice - LaborCost;

        public bool UseBOM
        {
            get => _isBom;
            set
            {
                if (value == _isBom) return;
                _isBom = value;
                OnPropertyChanged();
            }
        }

        public decimal BuyUnitPrice
        {
            get => _buyUnitPrice;
            set
            {
                if (value == _buyUnitPrice) return;
                _buyUnitPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PartsCost));
            }
        }

        public decimal LaborCost
        {
            get => _laborCost;
            set
            {
                if (value == _laborCost) return;
                _laborCost = value;
                OnPropertyChanged();
            }
        }

        public bool Calculated
        {
            get => _calculated;
            set
            {
                if (value == _calculated) return;
                _calculated = value;
                OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (value == _notes) return;
                _notes = value;
                OnPropertyChanged();
            }
        }
    }
}