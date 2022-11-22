using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SSItemPricer2
{
    public partial class App : Application
    {
        private const string ConnectionString = "Server=10.0.0.25,1400; Database=MIS; Uid=web123; Pwd=web123";
        
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
    }
}