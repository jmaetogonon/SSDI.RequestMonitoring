using Microsoft.AspNetCore.Components;

namespace SSDI.RequestMonitoring.UI.JComponents.Modals;

public partial class Confirmation__Modal : ComponentBase
{
    private bool _isVisible;
    private TaskCompletionSource<bool>? _tcs;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    // Content
    [Parameter] public string Title { get; set; } = "Confirm Action";

    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string Message { get; set; } = "Are you sure you want to proceed with this action?";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // Variants
    [Parameter] public ConfirmationModalVariant Variant { get; set; } = ConfirmationModalVariant.confirmation;

    // Icons
    [Parameter] public string Icon { get; set; } = string.Empty;

    [Parameter] public string ConfirmIcon { get; set; } = string.Empty;
    [Parameter] public string CancelIcon { get; set; } = "bi bi-x-circle";

    // Texts
    [Parameter] public string ConfirmText { get; set; } = "Confirm";

    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public string LoadingText { get; set; } = "Processing...";

    // Visibility Controls
    [Parameter] public bool ShowIcon { get; set; } = true;

    [Parameter] public bool ShowCloseButton { get; set; } = true;
    [Parameter] public bool ShowCancelButton { get; set; } = true;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public bool ShowLoadingOnConfirm { get; set; } = true;

    // Behavior
    [Parameter] public bool IsLoading { get; set; }

    [Parameter] public bool CloseOnOverlayClick { get; set; } = true;
    [Parameter] public bool CloseOnEscape { get; set; } = true;

    // Remarks
    [Parameter] public bool ShowRemarksField { get; set; }

    [Parameter] public bool RemarksRequired { get; set; }
    [Parameter] public string Remarks { get; set; } = string.Empty;
    //[Parameter] public EventCallback<string> RemarksChanged { get; set; }
    [Parameter] public string RemarksPlaceholder { get; set; } = "Enter your remarks here...";
    [Parameter] public string RemarksLabel { get; set; } = "Remarks";
    

    // Number Field
    [Parameter] public bool ShowNumberField { get; set; }

    [Parameter] public bool NumberRequired { get; set; }
    [Parameter] public decimal Number { get; set; } = 0;
    //[Parameter] public EventCallback<string> NumberChanged { get; set; }
    [Parameter] public string NumberPlaceholder { get; set; } = "Enter amount here...";
    [Parameter] public string NumberLabel { get; set; } = "Amount";

    // Events
    [Parameter] public EventCallback OnConfirm { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    protected override void OnInitialized()
    {
        SetDefaultIcons();
    }

    protected override void OnParametersSet()
    {
        _isVisible = IsVisible;

        if (string.IsNullOrEmpty(Icon) || string.IsNullOrEmpty(ConfirmIcon))
        {
            SetDefaultIcons();
        }
    }

    private void SetDefaultIcons()
    {
        if (string.IsNullOrEmpty(Icon))
        {
            Icon = Variant switch
            {
                ConfirmationModalVariant.info => "bi bi-info-circle",
                ConfirmationModalVariant.success => "bi bi-check-circle",
                ConfirmationModalVariant.error => "bi bi-x-circle",
                ConfirmationModalVariant.warning => "bi bi-exclamation-triangle",
                ConfirmationModalVariant.confirmation => "bi bi-question-circle",
                ConfirmationModalVariant.delete => "bi bi-trash",
                _ => "bi bi-question-circle"
            };
        }

        if (string.IsNullOrEmpty(ConfirmIcon))
        {
            ConfirmIcon = Variant switch
            {
                ConfirmationModalVariant.delete => "bi bi-trash",
                ConfirmationModalVariant.success => "bi bi-check-lg",
                _ => "bi bi-check-lg"
            };
        }
    }

    private async Task Close()
    {
        if (IsLoading) return;

        _isVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnOverlayClick()
    {
        if (CloseOnOverlayClick && !IsLoading)
        {
            await Close();
        }
    }

    private async Task Confirm()
    {
        if (ShowRemarksField && RemarksRequired && string.IsNullOrWhiteSpace(Remarks?.Trim()))
        {
            return;
        }
        if (ShowNumberField && NumberRequired && Number == 0)
        {
            return;
        }

        if (_tcs != null)
        {
            _tcs.TrySetResult(true);
            _tcs = null;
        }

        if (OnConfirm.HasDelegate)
        {
            await OnConfirm.InvokeAsync();
        }
        else
        {
            await Close();
        }
    }

    private async Task Cancel()
    {
        if (_tcs != null)
        {
            _tcs.TrySetResult(false);
            _tcs = null;
        }

        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
        await Close();
    }

    // Public methods for programmatic usage
    public async Task<bool> ShowAsync(ConfirmationModalOptions options)
    {
        Remarks = string.Empty;
        Number = 0;
        Message = options.Message;
        Title = options.Title ?? "Confirm Action";
        Variant = options.Variant ?? ConfirmationModalVariant.confirmation;
        ConfirmText = options.ConfirmText ?? "Confirm";
        CancelText = options.CancelText ?? "Cancel";
        Icon = options.Icon ?? "";
        ConfirmIcon = options.ConfirmIcon ?? "";
        Subtitle = options.Subtitle;
        ShowCancelButton = options.ShowCancelButton ?? true;
        ShowIcon = options.ShowIcon ?? true;
        ShowRemarksField = options.ShowRemarksField ?? false;
        RemarksRequired = options.RemarksRequired ?? false;
        RemarksPlaceholder = options.RemarksPlaceholder ?? "Enter your remarks here...";
        RemarksLabel = options.RemarksLabel ?? "Remarks";
        ShowNumberField = options.ShowNumberField ?? false;
        NumberRequired = options.NumberRequired ?? false;
        NumberPlaceholder = options.NumberPlaceholder ?? "Enter amount here...";
        NumberLabel = options.NumberLabel ?? "Amount";

        SetDefaultIcons();

        _tcs = new TaskCompletionSource<bool>();
        _isVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        StateHasChanged();

        return await _tcs.Task;
    }

    public async Task<bool> ShowRejectAsync(int reqId)
    {
        Message = $"Are you sure you want to reject the purchase request '#{reqId}'?";
        Title = "Reject Purchase Request";
        Variant = ConfirmationModalVariant.error;
        ConfirmText = "Yes, Reject";
        CancelText = "No, Cancel";
        ConfirmIcon = "bi bi-x-lg";
        ShowRemarksField = true;
        RemarksRequired = true;
        RemarksLabel = "Rejection Reason";
        RemarksPlaceholder = "Please provide a reason for rejecting this request...";

        _tcs = new TaskCompletionSource<bool>();
        _isVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        StateHasChanged();

        return await _tcs.Task;
    }

    public async Task<bool> ShowApproveAsync(int reqId)
    {
        Message = $"Are you sure you want to approve the purchase request '#{reqId}'? Once approve, it will be forwarded for the next level of approval.";
        Title = "Approve Purchase Request";
        Variant = ConfirmationModalVariant.confirmation;
        ConfirmText = "Yes, Approve";
        CancelText = "No, Cancel";
        ConfirmIcon = "bi bi-check-lg";
        ShowRemarksField = false;

        _tcs = new TaskCompletionSource<bool>();
        _isVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        StateHasChanged();

        return await _tcs.Task;
    }

    public async Task<bool> ShowSubmitAsync(int reqId)
    {
        Message = $"Are you sure you want to submit purchase request '#{reqId}'?<br>Once submitted, it will be forwarded for endorsement.";
        Title = "Submit Purchase Request";
        Variant = ConfirmationModalVariant.confirmation;
        ConfirmText = "Yes, Submit";
        CancelText = "No, Cancel";
        ConfirmIcon = "bi bi-send-check";
        ShowRemarksField = false;

        _tcs = new TaskCompletionSource<bool>();
        _isVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        StateHasChanged();

        return await _tcs.Task;
    }

    public async Task HideAsync()
    {
        await Close();
    }

    public async Task SetLoadingAsync(bool loading)
    {
        IsLoading = loading;
        StateHasChanged();
        await Task.CompletedTask;
    }
}

public class ConfirmationModalOptions
{
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public ConfirmationModalVariant? Variant { get; set; }
    public string? ConfirmText { get; set; }
    public string? CancelText { get; set; }
    public string? Icon { get; set; }
    public string? ConfirmIcon { get; set; }
    public bool? ShowCancelButton { get; set; }
    public bool? ShowIcon { get; set; }
    public bool? ShowRemarksField { get; set; }
    public bool? RemarksRequired { get; set; }
    public string? RemarksPlaceholder { get; set; }
    public string? RemarksLabel { get; set; }
    public bool? ShowNumberField { get; set; }
    public bool? NumberRequired { get; set; }
    public string? NumberPlaceholder { get; set; }
    public string? NumberLabel { get; set; }
}

public enum ConfirmationModalVariant
{
    info,
    success,
    error,
    warning,
    confirmation,
    delete
}