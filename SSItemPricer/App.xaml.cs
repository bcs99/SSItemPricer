using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using SSItemPricer.Lib.Db;
using SSItemPricer.Views;
using UglyToad.PdfPig.Core;

namespace SSItemPricer
{
    public partial class App : Application
    {
        private static DataTable _gpQuantities = new();

        public static string? AssemblyVersion =>
            // If you want the full four-part version number:
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(4);
        // You can also reference Assembly.GetName().Version to get Major, Minor, MajorRevision, MinorRevision
        // components individually and do with them as you please.

        public static async Task GpGetQuantities()
        {
            await Task.Run(() =>
            {
                _gpQuantities = Db.Read<Gp>(
                    """
                    SELECT ITEMNMBR               AS [Item Number], 
                           ATYALLOC               AS [Allocated], 
                           (QTYONHND - ATYALLOC)  AS [In Stock] 
                    FROM IV00102 IV
                    WHERE RCRDTYPE = 1 AND (QTYONHND > 0 OR ATYALLOC > 0)
                    """);

                _gpQuantities.PrimaryKey = new[] { _gpQuantities.Columns[0] };

            });
        }

        public static void UpdateQuantities(DataView dataView)
        {
            foreach (DataRowView row in dataView)
            {
                var itemNumber = row["Item Number"];
                var gp = _gpQuantities.Rows.Find(itemNumber);

                if (gp == null) continue;

                row["Allocated"] = gp["Allocated"];
                row["In Stock"] = gp["In Stock"];
            }
        }

        public static DataView ExecuteQuery<T>(string sql, params SqlParameter[] args) where T : IConnectionString
        {
            using var conn = new SqlConnection(Activator.CreateInstance<T>().Text);

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

        /// <summary>
        /// Exports the specified DataTable table to an Excel file.
        /// </summary>
        /// <param name="table">The DataTable to be exported.</param>
        /// <param name="window">The window used to display the Save file dialog.</param>
        /// <returns>The path of the exported Excel file.</returns>
        public static async Task<string> ExportTable(DataTable table, Window window)
        {
            var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                DefaultExt = ".xlsx",
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"SS Item Pricing ({DateTime.Now:yyyy-MM-dd}).xlsx"
            };

            if (dialog.ShowDialog(window) != true)
                return string.Empty;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                var headings = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

                for (var i = 0; i < headings.Length; i++)
                {
                    var xlCell = worksheet.Cell(1, i + 1);

                    xlCell.Value = headings[i];
                    xlCell.Style.Font.Bold = true;
                }

                worksheet.SheetView.FreezeRows(1);

                await Task.Run(() => { worksheet.Cell(2, 1).InsertData(table); });

                var columnHeadings = new[] { "Parts", "Labor", "Price", "Setup Cost", "Piece Cost" };

                foreach (var xlColumn in worksheet.Columns())
                {
                    if (columnHeadings.Contains(xlColumn.Cell(1).Value.ToString()))
                        xlColumn.Cells().Style.NumberFormat.SetFormat(
                            @"_(""$""* #,##0.00_);_(""$""* \(#,##0.00\);_(""$""* ""-""??_);_(@_)");
                    xlColumn.AdjustToContents();
                }

                workbook.SaveAs(dialog.FileName);
            }

            return dialog.FileName;
        }
    }
}