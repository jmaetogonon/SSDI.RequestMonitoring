using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.PurchaseRequest.Modals;

public partial class EditRequest__Modal : ComponentBase
{
    [Parameter] public Purchase_RequestVM RequestModel { get; set; } = new();
    [Parameter] public bool IsModalVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }


    private bool _isDisabledBtns = false;
    private bool IsShowAlert { get; set; }
    private string AlertMessage { get; set; } = string.Empty;

    private async void CloseModal()
    {
        await OnClose.InvokeAsync(null);
        ResetForm();
    }

    private async Task HandleSave()
    {
        _isDisabledBtns = true;
        IsShowAlert = false;
        RequestModel.Status = TokenCons.Status__PreparedBy;
        RequestModel.DateRequested = DateTime.Now;

        var response = await purchaseRequestSvc.UpdatePurchaseRequest(RequestModel.Id,RequestModel);
        if (response.Success)
        {
            ResetForm();
            await OnSave.InvokeAsync(null);
            return;
        }

        IsShowAlert = true;
        AlertMessage = response.Message;
        _isDisabledBtns = false;
    }

    private void ResetForm()
    {
        RequestModel = new();
        _isDisabledBtns = false;
    }

    private string GetLastModifiedDisplay(DateTime? dateModified)
    {
        if (dateModified is null) return "Never modified";

        var now = DateTime.Now;
        var timeSpan = now - dateModified.Value;

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 1 => $"Updated {timeSpan.Minutes}m ago",
            < 1 => $"Updated {timeSpan.Hours}h ago",
            < 7 => $"Updated {timeSpan.Days}d ago",
            < 30 => $"Updated {timeSpan.Days / 7}w ago",
            _ => $"Updated {dateModified.Value:MMM dd, yyyy}"
        };
    }
}
