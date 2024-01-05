using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SSItemPricer;

public sealed class MainWindowVm : INotifyPropertyChanged
{
    private string _message = string.Empty;
    private DataView _dataView = new();
    public string? Version { get; set; } = App.AssemblyVersion;

    public DataView DataView
    {
        get => _dataView;
        set => SetField(ref _dataView, value);
    }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public async Task LoadSql()
    {
        await Task.Run(() =>
        {
            DataView = App.ExecuteQuery(App.GetEmbeddedResourceFile("MainQuery.SQL"));
            DataView.Sort = "Item Number";
        });
    }

    public void ItemIsInCatalog(int itemNumber)
    {
        var index = DataView.Find(itemNumber);

        if (index == -1)
            Debug.WriteLine($"Item not found in BOM pricing ({itemNumber}).");
        else
            DataView[index]["Catalog"] = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}