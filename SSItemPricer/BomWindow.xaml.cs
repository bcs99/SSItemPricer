using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SSItemPricer.Models;

namespace SSItemPricer
{
    /// <summary>
    /// Interaction logic for BomWindow.xaml
    /// </summary>
    public partial class BomWindow : Window
    {
        private BomViewModel _viewModel;

        public BomWindow(Item item, ObservableCollection<Item> source)
        {
            InitializeComponent();

            Title = $"{item.ItemNumber} {item.ItemName}";

            _viewModel = (BomViewModel)DataContext;

            _viewModel.Item = item;
            _viewModel.Source = source;

            var bomItems =
                Mis.FindMany<BOMItems>(
                    $@"SELECT * FROM tblBOMItems AS B JOIN tblItem AS I ON (B.ItemID = I.ItemNumber) JOIN tblItemVendor AS V ON (V.ItemID=I.ItemNumber AND V.VendorPriority=1) WHERE B.BOMID={item.ItemNumber}");


            foreach (var bomItem in bomItems)
            {
                bomItem.Copy(source.FirstOrDefault(i => i.ItemNumber == bomItem.ItemNumber));

                _viewModel.Materials.Add(bomItem);
            }

            StatusBarItem.Content = "$" + _viewModel.Materials.Sum(i => i.Total).ToString("0.0000");
        }

        private void DataGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;

            ShowBomWindow(sender);
        }

        private void DataGridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowBomWindow(sender);
        }

        private void ShowBomWindow(object sender)
        {
            if (sender is not DataGrid dataGrid) return;

            if (dataGrid.SelectedItem is not Item item) return;

            if (item.UseBOM == false) return;

            new BomWindow(item, _viewModel.Source){ Owner = this }.Show();
        }
    }
}
