namespace SSDI.RequestMonitoring.UI.Models.Requests;

public class Request_PO_SlipVM
{
    private decimal _totalAmount;
    private bool _isManualTotal = false;

    public int Id { get; set; }
    public string PO_Number { get; set; } = string.Empty;
    public int? Job_OrderId { get; set; } = null;
    public int? Purchase_RequestId { get; set; } = null;
    public string Supplier { get; set; } = string.Empty;
    public DateTime Date_Issued { get; set; }
    public string Terms { get; set; } = string.Empty;

    public decimal Total_Amount
    {
        get => _isManualTotal ? _totalAmount : Details.Sum(d => d.Total_Price);
        set
        {
            _totalAmount = value;
            _isManualTotal = true;
        }
    }

    public bool IsManualTotal => _isManualTotal;
    public decimal AutoCalculatedTotal => Details.Sum(d => d.Total_Price);

    public int PreparedById { get; set; }

    public ApprovalAction Approval { get; set; } = ApprovalAction.Pending;
    public string SlipApproverName { get; set; } = string.Empty;
    public DateTime? SlipApprovalDate { get; set; } = null;

    public ICollection<Request_PO_Slip_DetailVM> Details { get; set; } = [];

    public void ResetToAutoCalculate()
    {
        _isManualTotal = false;
    }

    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
}
