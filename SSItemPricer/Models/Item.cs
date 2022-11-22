using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SSItemPricer.Annotations;

namespace SSItemPricer.Models
{
    public class Item : Mis
    {
        private static readonly List<BomPrice> BomPrices =
            new (Mis.FindMany<BomPrice>(File.ReadAllText("PricingQuery.SQL")));

        private decimal _buyUnitPrice;
        private decimal _laborCost;
        private bool _calculated;
        private bool _useBOM;
        private string _notes;
        private decimal _price;
        
        [CanBeNull] private BomPrice _bomPrice;

        [CanBeNull]
        public BomPrice BomPrice
        {
            get => _bomPrice ??= BomPrices.FirstOrDefault(bp=>bp.RootId == ItemNumber);
            set
            {
                if (Equals(value, _bomPrice)) return;
                _bomPrice = value;
                OnPropertyChanged();
            }
        }

        public bool Diff
        {
            get
            {
                var bp = BomPrice;

                try
                {
                    if (Price != bp.Price) throw new Exception("Price is different");
                    if (LaborCost != bp.Labor) throw new Exception("Labor is different");
                    if (SetupCost != bp.SetupCost) throw new Exception("Setup Cost is different");
                    if (PieceCost != bp.PieceCost) throw new Exception("Piece Cost is different");
                    
                    return true;
                }
                catch (Exception e)
                {
                    Notes = e.Message;
                    return false;
                }
            }
        }

        public int ItemNumber { get; set; }
        public string ItemName { get; set; }
        public bool Discontinued { get; set; }
        public int ECOStatusID { get; set; }
        public string Status { get; set; }
        public decimal SetupCost { get; set; }
        public decimal PieceCost { get; set; }
        public decimal PartsCost => BuyUnitPrice - LaborCost;

        public decimal Price
        {
            get => _price;
            set
            {
                if (value == _price) return;
                _price = value;
                OnPropertyChanged();
            }
        }

        public bool UseBOM
        {
            get => _useBOM;
            set
            {
                if (value == _useBOM) return;
                _useBOM = value;
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