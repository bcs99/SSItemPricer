using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SSItemPricer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataGrid.Focus();
        }

        private void SearchBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            StatusBarItem.Content = "";

            if (e.Key != Key.Enter || int.TryParse(SearchBox.Text, out var itemNumber) == false) return;

            SearchBox.Text = "";
            ViewModel.DataView.Sort = "Item Number";
            DataGrid.SelectedIndex = ViewModel.DataView.Find(itemNumber.ToString());

            if (DataGrid.SelectedIndex == -1)
            {
                StatusBarItem.Content = $"{itemNumber} not found";
                return;
            }

            DataGrid.ScrollIntoView(DataGrid.SelectedItem);
            DataGrid.Focus();
        }

        private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
            => App.AutoGeneratingColumn(sender, e);

        private void DataGrid_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out var itemNumber) == false) return;

            SearchBox.Text = e.Text;
            SearchBox.CaretIndex = 1;
            SearchBox.Focus();

            e.Handled = true;
        }

        private void DataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
                e.Handled = App.ShowBomWindow(DataGrid, this);
        }

        private void DataGrid_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = App.ShowBomWindow(DataGrid, this);
        }

        private async void Export_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Message = "Exporting pricing...";

            var fileName = await App.ExportTable(ViewModel.DataView.Table!, this);

            ViewModel.Message = "Export completed. Opening workbook...";

            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
            });

            ViewModel.Message = string.Empty;
        }

        private async void Import_CatalogItems(object sender, RoutedEventArgs e)
        {
            ViewModel.Message = "Importing catalog items...";

            var count = await SetCatalogItemsAsync();

            ViewModel.Message = $"Imported {count} catalog items.";
        }

        private async Task<int> SetCatalogItemsAsync()
        {
            CatalogItems? catalogItems = null;

            try
            {
                await Task.Run(() => { catalogItems = new CatalogItems(); });

                foreach (var itemNumber in catalogItems!.ItemNumbers)
                    ViewModel.ItemIsInCatalog(itemNumber);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Import Catalog Items", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return catalogItems!.ItemNumbers.Count;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Message = "Creating item pricing...";

            await ViewModel.LoadSql();

            ViewModel.Message = $"Found {ViewModel.DataView.Count} items.";
        }
    }
}