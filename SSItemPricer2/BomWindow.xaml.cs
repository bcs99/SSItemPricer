using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SSItemPricer2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataGrid.Focus();
        }

        private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyType)
            {
                case not null when e.PropertyType == typeof(string):

                    e.Column.Width = new DataGridLength(
                        e.Column.Header.ToString() == "Item Description" ? 4.0 : 1.0, 
                        DataGridLengthUnitType.Star);
                    break;

                case not null when e.PropertyType == typeof(decimal):
                    ((DataGridTextColumn)e.Column).ElementStyle = FindResource("RightCell") as Style;
                    break;

                case not null when e.PropertyType == typeof(int):
                    ((DataGridTextColumn)e.Column).ElementStyle = FindResource("CenterCell") as Style;
                    break;
            }
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

        private void DataGrid_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out var itemNumber) == false) return;

            SearchBox.Text = e.Text;
            SearchBox.CaretIndex = 1;
            SearchBox.Focus();

            e.Handled = true;
        }
    }
}