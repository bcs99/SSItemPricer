using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SSItemPricer2;

public class MainWindowVm
{
    private const string ConnectionString = "Server=10.0.0.25,1400; Database=MIS; Uid=web123; Pwd=web123";

    private static readonly string PricingQuery = GetEmbeddedResourceFile("PricingQuery.SQL");
    private static readonly string BomPricingQuery = GetEmbeddedResourceFile("BomPricingQuery.SQL");
    
    public DataView DataView { get; set; }

    public MainWindowVm()
    {
        using var conn = new SqlConnection(ConnectionString);

        conn.Open();

        var dataAdapter = new SqlDataAdapter(PricingQuery, conn);
        var dataTable = new DataTable();

        dataAdapter.Fill(dataTable);

        DataView = dataTable.DefaultView;
    }

    private static string GetEmbeddedResourceFile(string filename)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();

        if (assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(filename)) is { } resource)
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
}