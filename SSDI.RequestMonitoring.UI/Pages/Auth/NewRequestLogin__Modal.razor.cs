using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Enums;
using SSDI.RequestMonitoring.UI.Models.Requests;

namespace SSDI.RequestMonitoring.UI.Pages.Auth;

public partial class NewRequestLogin__Modal : ComponentBase
{
    [Parameter] public bool IsNewRequestModalFormVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }

    private Purchase_RequestVM RequestModel { get; set; } = new();

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

        RequestModel.Status = RequestStatus.Draft;
        RequestModel.DateRequested = DateTime.Now;

        var response = await purchaseRequestSvc.CreatePurchaseRequest(RequestModel);
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
}