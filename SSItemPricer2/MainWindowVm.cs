using System.Data;
using System.Windows.Controls;

namespace SSItemPricer2;

public class MainWindowVm
{
    public DataView DataView { get; set; }

    public MainWindowVm()
    {
        DataView = App.ExecuteQuery(App.GetEmbeddedResourceFile("MainQuery.SQL"));
    }
}