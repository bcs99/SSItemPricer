using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SSItemPricer;

public class BomWindowVm : INotifyPropertyChanged
{
    private DataView _dataView = new DataView();
    private decimal _total;

    public decimal Total
    {
        get => _total;
        set
        {
            if (value == _total) return;
            _total = value;
            OnPropertyChanged();
        }
    }

    public DataView DataView
    {
        get => _dataView;
        set
        {
            if (value.Equals(_dataView)) return;
            _dataView = value;
            OnPropertyChanged();
            _total = GetTotal();
            OnPropertyChanged(nameof(Total));
        }
    }

    private decimal GetTotal()
    {
        return _dataView.Cast<DataRowView>()
            .Sum(rowView => decimal.Parse(rowView.Row["Price"].ToString()!));
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