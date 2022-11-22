using System.Data;
using System.Windows.Controls;

namespace SSItemPricer2;

public class MainWindowVm
{
    public string Version { get; private set; }
    public DataView DataView { get; set; }

    public MainWindowVm()
    {
        Version = $" v{typeof(MainWindow).Assembly.GetName().Version}";

        DataView = App.ExecuteQuery(App.GetEmbeddedResourceFile("MainQuery.SQL"));
    }
}