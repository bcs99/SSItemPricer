﻿using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace SSItemPricer2
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
            if (e.Key != Key.Enter || DataGrid.SelectedItem is not DataRowView dataRowView) return;

            e.Handled = true;
            
            new BomWindow(dataRowView["Item Number"].ToString())
            {
                Owner = this,
                Title = $"{dataRowView["Item Number"]} {dataRowView["Item Description"]}"
            }.Show();
        }
    }
}