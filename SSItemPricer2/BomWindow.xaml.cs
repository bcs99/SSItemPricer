using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SSItemPricer2
{
    public partial class BomWindow : Window
    {
        public BomWindow(string? itemNumber)
        {
            InitializeComponent();

            var sql = App.GetEmbeddedResourceFile("BomQuery.SQL");
            
            ViewModel. DataView = App.ExecuteQuery(sql, new []{new SqlParameter("@BOMID", itemNumber)});

            Title = itemNumber;
            
            DataGrid.Focus();
        }

        private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
            => App.AutoGeneratingColumn(sender, e);

        private void Window_OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}