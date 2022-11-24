using System.Data;
using System.Windows.Controls;

namespace SSItemPricer2;

public class MainWindowVm
{
    public string? Version { get; set; }

    public DataView DataView { get; set; }

    public MainWindowVm()
    {
        // Version = $" v{typeof(MainWindow).Assembly.GetName().Version}";
        Version = App.AssemblyVersion;

        DataView = App.ExecuteQuery(App.GetEmbeddedResourceFile("MainQuery.SQL"));
    }
}