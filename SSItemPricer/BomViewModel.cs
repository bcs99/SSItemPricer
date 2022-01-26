using System.Collections.ObjectModel;
using SSItemPricer.Models;

namespace SSItemPricer
{
    public sealed class BomViewModel : ObservableObject
    {
        private Item _item;

        public ObservableCollection<Item> Source { get; set; }

        public ObservableCollection<BOMItems> Materials { get; set; } = new();

        public Item Item
        {
            get => _item;
            set
            {
                if (Equals(value, _item)) return;
                _item = value;
                OnPropertyChanged();
            }
        }
    }
}
