﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SSItemPricer2;

public class MainWindowVm : INotifyPropertyChanged
{
    private string _message = string.Empty;
    public string? Version { get; set; }

    public DataView DataView { get; set; }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public MainWindowVm()
    {
        Version = App.AssemblyVersion;
        DataView = App.ExecuteQuery(App.GetEmbeddedResourceFile("MainQuery.SQL"));
        DataView.Sort = "Item Number";
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}