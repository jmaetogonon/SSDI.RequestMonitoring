using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Request_PO_Slip_DetailVM : INotifyPropertyChanged
{
    private int _quantity;
    private decimal _unitPrice;
    private decimal _totalPrice;
    private bool _isManualTotal = false;

    public int Id { get; set; }
    public int Request_PO_SlipId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public decimal Unit_Price
    {
        get => _unitPrice;
        set
        {
            _unitPrice = value;
            OnPropertyChanged();
        }
    }

    public decimal Total_Price
    {
        get => _isManualTotal ? _totalPrice : Quantity * Unit_Price;
        set
        {
            _totalPrice = value;
            _isManualTotal = true;
            OnPropertyChanged();
        }
    }

    public bool IsManualTotal => _isManualTotal;
    public decimal AutoCalculatedTotal => Quantity * Unit_Price;

    public void ResetToAutoCalculate()
    {
        _isManualTotal = false;
        OnPropertyChanged(nameof(Total_Price));
        OnPropertyChanged(nameof(IsManualTotal));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
}
