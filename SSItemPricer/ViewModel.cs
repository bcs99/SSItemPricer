using System.Collections.ObjectModel;
using SSItemPricer.Models;

namespace SSItemPricer
{
    public sealed class ViewModel : ObservableObject
    {
        private string _version;

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Item> Items { get; set; } = new();
    }
}