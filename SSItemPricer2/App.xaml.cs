using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SSItemPricer2
{
    public partial class App : Application
    {
        private const string ConnectionString = "Server=10.0.0.25,1400; Database=MIS; Uid=web123; Pwd=web123";

        public static string? AssemblyVersion =>
            //If you want the full four-part version number:
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(4);
        //You can also reference asm.GetName().Version to get Major, Minor, MajorRevision, MinorRevision
        //components individually and do with them as you please.

        public static DataView ExecuteQuery(string sql, params SqlParameter[] args)
        {
            using var conn = new SqlConnection(App.ConnectionString);

            conn.Open();

            var command = new SqlCommand(sql, conn);

            foreach (var arg in args)
                command.Parameters.Add(arg);

            var dataAdapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable.DefaultView;
        }

        public static string GetEmbeddedResourceFile(string filename)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var manifestResourceNames = assembly.GetManifestResourceNames();

            if (manifestResourceNames.FirstOrDefault(r => r.EndsWith(filename)) is { } resource)
            {
                using var resourceStream = assembly.GetManifestResourceStream(resource);

                if (resourceStream != null)
                {
                    using var reader = new System.IO.StreamReader(resourceStream);

                    return reader.ReadToEnd();
                }
            }

            return "";
        }

        public static void AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyType)
            {
                case not null when e.PropertyType == typeof(string):

                    e.Column.Width = new DataGridLength(
                        e.Column.Header.ToString() == "Item Description" ? 4.0 : 1.0,
                        DataGridLengthUnitType.Star);
                    break;

                case not null when e.PropertyType == typeof(decimal):
                    ((DataGridTextColumn)e.Column).ElementStyle = Current.FindResource("RightCell") as Style;
                    break;

                case not null when e.PropertyType == typeof(int):
                    ((DataGridTextColumn)e.Column).ElementStyle = Current.FindResource("CenterCell") as Style;
                    break;
            }
        }

        public static bool ShowBomWindow(DataGrid dataGrid, Window window)
        {
            if (dataGrid.SelectedItem is not DataRowView dataRowView ||
                !bool.TryParse(dataRowView["Is BOM"].ToString(), out var isBom) ||
                isBom == false) return false;

            new BomWindow(dataRowView["Item Number"].ToString())
            {
                Owner = window,
                Title = $"{dataRowView["Item Number"]} {dataRowView["Item Description"]}"
            }.Show();

            return true;
        }

        public static void ExportTable(DataTable table, Window window)
        {
            var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                DefaultExt = ".csv",
                Filter = "Comma Separated Values (*.csv)|*.csv",
                FileName = "Untitled.csv"
            };

            if (dialog.ShowDialog(window) != true) 
                return;

            var streamWriter = new StreamWriter(dialog.FileName, false);

            streamWriter.Write(string.Join(',', table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
            streamWriter.Write(streamWriter.NewLine);

            foreach (DataRow row in table.Rows)
            {
                streamWriter.Write(string.Join(',', row.ItemArray.Select(GetCellValue)));
                streamWriter.Write(streamWriter.NewLine);
            }

            streamWriter.Close();
        }

        private static string GetCellValue(object? cell)
        {
            if (Convert.IsDBNull(cell) || cell?.ToString() is not {} value)
                return string.Empty;

            return value.Contains(',') ? $"\"{value}\"" : value;
        }
    }
}