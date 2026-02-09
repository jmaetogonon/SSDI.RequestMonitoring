using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using SSDI.RequestMonitoring.UI.JComponents.Filters;
using SSDI.RequestMonitoring.UI.JComponents.Modals;
using SSDI.RequestMonitoring.UI.Models.DTO;
using SSDI.RequestMonitoring.UI.Models.MasterData;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using System.Web;

namespace SSDI.RequestMonitoring.UI.Pages.Requests.JobOrder;

public partial class JobOrder_Index : ComponentBase, IAsyncDisposable
{
    private IQueryable<Job_OrderVM>? _allItems;
    private IQueryable<Job_OrderVM>? _filteredItems;
    private List<StatusSummary> _statusSummaries = [];

    private List<DivisionVM> _divisions = [];
    private List<DepartmentVM> _departments = [];

    private string _searchValue = "";
    private Status__Filter? _statusFilter;
    private Priority__Filter? _priorityFilter;
    private HashSet<RequestStatus> _selectedStatuses = [];
    private HashSet<RequestPriority> _selectedPriorities = [];
    private RequestStatus? _lastClickedStatus = null;

    private readonly PaginationState _pagination = new() { ItemsPerPage = 10 };
    private readonly GridSort<Job_OrderVM> _sortStatus = GridSort<Job_OrderVM>.ByAscending(x => x.Status).ThenAscending(x => x.Status);

    private Confirmation__Modal? _confirmModal;
    private bool _isNewRequestModalVisible = false;
    private bool _isLoading = false;
    private bool IsShowNewBtn => CheckNewBtnPermission();

    //carousel fields
    private int _currentSlide = 0;

    private ElementReference _carouselContainer;
    private DotNetObjectReference<JobOrder_Index>? _dotNetRef;
    private IJSObjectReference? _jsModule;

    private DateRange__Filter? _dateRangeFilter;
    private DateTime? _startDateFilter;
    private DateTime? _endDateFilter;

    protected override async Task OnInitializedAsync()
    {
        await currentUser.InitializeAsync();
        currentUser.OnUserChanged += Refresh;
        _divisions = await divisionSvc.GetAllDivisions();
        _departments = await departmentSvc.GetAllDepartments();
        await LoadDataAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _dotNetRef = DotNetObjectReference.Create(this);

        _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/requestCarousel.js");

        await _jsModule.InvokeVoidAsync(
            "initializeCarousel",
            _carouselContainer,
            _dotNetRef
        );
    }

    private async Task LoadDataAsync()
    {
        if (_isLoading) return;

        _isLoading = true;
        try
        {
            HashSet<RequestStatus> preset = [];
            if (currentUser.IsUser)
            {
                var requests = await jobOrderSvc.GetAllJobOrdersByUser(currentUser.UserId);
                _allItems = requests.AsQueryable();
            }
            else
            {
                var requests = await jobOrderSvc.GetAllJobOrderBySupervisor(currentUser.UserId, true, true);
                _allItems = requests.AsQueryable();
                if (requests.Any(e => e.ReportType == "Division"))
                {
                    _allItems = requests.Where(e => e.Status == RequestStatus.ForEndorsement).AsQueryable();
                    preset.Add(RequestStatus.ForEndorsement);
                }

                if (currentUser.IsCEO)
                {
                    var ceoRequests = await jobOrderSvc.GetAllJobOrderByCeo();
                    _allItems = _allItems.Concat(ceoRequests).DistinctBy(r => r.Id).AsQueryable();
                    preset.Add(RequestStatus.ForCeoApproval);
                    preset.Add(RequestStatus.ForRequisition);
                }

                if (currentUser.IsAdmin)
                {
                    var adminRequests = await jobOrderSvc.GetAllJobOrdersByAdmin();
                    _allItems = _allItems.Concat(adminRequests).DistinctBy(r => r.Id).AsQueryable();
                    preset.Add(RequestStatus.ForAdminVerification);
                    preset.Add(RequestStatus.ForRequisition);
                }
                OnStatusFilterChanged(preset);
                _statusFilter?.SetSelectedStatus(_selectedStatuses);
            }

            PreLoadFilterFromDB();
            ApplyFilters();
            BuildStatusSummaries();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void Refresh() => InvokeAsync(StateHasChanged);

    private void PreLoadFilterFromDB()
    {
        var uri = new Uri(navigationManager.Uri);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        var statusValue = queryParams["status"];
        var actionValue = queryParams["action"];

        if (Enum.TryParse<RequestStatus>(statusValue, out var parsedStatus))
        {
            _selectedStatuses = [parsedStatus];
            _statusFilter?.SetSelectedStatus(_selectedStatuses);
        }

        if (actionValue == "new")
        {
            _isNewRequestModalVisible = true;
        }
    }

    private void OnViewDetails(FluentDataGridRow<Job_OrderVM>? row)
    {
        if (row?.Item is null) return;
        navigationManager.NavigateTo($"/requests/job-orders/{row?.Item?.Id}/{row?.Item?.ReportType}");
    }

    private void OnCloseNewReqModal() => _isNewRequestModalVisible = false;

    private async Task OnSaveNewReqModal()
    {
        _isNewRequestModalVisible = false;
        _allItems = null;
        await LoadDataAsync();
        toastSvc.ShowSuccess("The request has been added successfully.");
    }

    private void OnAddNewRequest() => _isNewRequestModalVisible = true;

    private void HandleSearch()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_allItems == null) return;

        var query = _allItems.AsQueryable();

        // Apply status filter
        if (_selectedStatuses.Count > 0)
        {
            query = query?.Where(r => _selectedStatuses.Contains(r.Status));
        }
        // Apply priority filter
        if (_selectedPriorities.Count > 0)
        {
            query = query?.Where(r => _selectedPriorities.Contains(r.Priority));
        }

        if (_startDateFilter.HasValue)
        {
            query = query?.Where(r => r.DateRequested!.Value.Date >= _startDateFilter.Value.Date);
        }

        if (_endDateFilter.HasValue)
        {
            query = query?.Where(r => r.DateRequested!.Value.Date <= _endDateFilter.Value.Date);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchValue))
        {
            query = query?.Where(r =>
                (r.Name != null && r.Name.Contains(_searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Nature_Of_Request != null && r.Nature_Of_Request.ToLower().Contains(_searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Justification != null && r.Justification.ToLower().Contains(_searchValue, StringComparison.OrdinalIgnoreCase)) ||
                (r.Division_Department != null && r.Division_Department.ToLower().Contains(_searchValue, StringComparison.OrdinalIgnoreCase))
            );
        }

        _filteredItems = query;
    }

    private void OnStatusFilterChanged(HashSet<RequestStatus> _selectedStatuses)
    {
        this._selectedStatuses = _selectedStatuses;
        ApplyFilters();
    }

    private void OnPriorityFilterChanged(HashSet<RequestPriority> _selectedPriorities)
    {
        this._selectedPriorities = _selectedPriorities;
        ApplyFilters();
    }

    private void OnStatusCardClick(RequestStatus status)
    {
        if (_lastClickedStatus == status && _selectedStatuses.Count == 1 && _selectedStatuses.Contains(status))
        {
            ClearAllFilters();
            _lastClickedStatus = null;
        }
        else
        {
            ClearAllFilters();
            _selectedStatuses = [status];
            _statusFilter?.SetSelectedStatus(_selectedStatuses);
            ApplyFilters();
            _lastClickedStatus = status;
        }

        StateHasChanged();
    }

    private void OnStartDateChanged(DateTime? startDate)
    {
        _startDateFilter = startDate;
        ApplyFilters();
    }

    private void OnEndDateChanged(DateTime? endDate)
    {
        _endDateFilter = endDate;
        ApplyFilters();
    }

    private void OnDateRangeChanged()
    {
        ApplyFilters();
    }

    private void BuildStatusSummaries()
    {
        if (_allItems == null) return;

        var totalCount = _allItems.Count();

        // Single pass to get all counts
        var statusCounts = _allItems
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        if (currentUser.IsAdmin)
        {
            _statusSummaries =
            [
                new(RequestStatus.ForAdminVerification,TokenCons.Status__ForAdminVerification, statusCounts.GetValueOrDefault(RequestStatus.ForAdminVerification, 0), totalCount , Utils.GetStatusIcon(RequestStatus.ForAdminVerification)),
                new(RequestStatus.ForRequisition,TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, Utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.PendingRequesterClosure,TokenCons.Status__PendingClose, statusCounts.GetValueOrDefault(RequestStatus.PendingRequesterClosure, 0), totalCount, Utils.GetStatusIcon(RequestStatus.PendingRequesterClosure)),
                new(RequestStatus.Closed,TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, Utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else if (currentUser.IsCEO)
        {
            _statusSummaries =
            [
                new(RequestStatus.ForEndorsement, TokenCons.Status__ForEndorsement, statusCounts.GetValueOrDefault(RequestStatus.ForEndorsement, 0), totalCount , Utils.GetStatusIcon(RequestStatus.ForEndorsement)),
                new(RequestStatus.ForCeoApproval, TokenCons.Status__ForCeoApproval, statusCounts.GetValueOrDefault(RequestStatus.ForCeoApproval, 0), totalCount , Utils.GetStatusIcon(RequestStatus.ForCeoApproval)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, Utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, Utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else if (currentUser.IsSupervisor && !currentUser.IsCEO)
        {
            _statusSummaries =
            [
                new(RequestStatus.Draft, TokenCons.Status__Draft, statusCounts.GetValueOrDefault(RequestStatus.Draft, 0), totalCount , Utils.GetStatusIcon(RequestStatus.Draft)),
                new(RequestStatus.Rejected, TokenCons.Status__Rejected, statusCounts.GetValueOrDefault(RequestStatus.Rejected, 0), totalCount , Utils.GetStatusIcon(RequestStatus.Rejected)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, Utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.PendingRequesterClosure, TokenCons.Status__PendingClose, statusCounts.GetValueOrDefault(RequestStatus.PendingRequesterClosure, 0), totalCount, Utils.GetStatusIcon(RequestStatus.PendingRequesterClosure)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, Utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
        else
        {
            _statusSummaries =
            [
                new(RequestStatus.Draft, TokenCons.Status__Draft, statusCounts.GetValueOrDefault(RequestStatus.Draft, 0), totalCount , Utils.GetStatusIcon(RequestStatus.Draft)),
                new(RequestStatus.Rejected, TokenCons.Status__Rejected, statusCounts.GetValueOrDefault(RequestStatus.Rejected, 0), totalCount , Utils.GetStatusIcon(RequestStatus.Rejected)),
                new(RequestStatus.ForRequisition, TokenCons.Status__ForRequisition, statusCounts.GetValueOrDefault(RequestStatus.ForRequisition, 0), totalCount, Utils.GetStatusIcon(RequestStatus.ForRequisition)),
                new(RequestStatus.Closed, TokenCons.Status__Closed, statusCounts.GetValueOrDefault(RequestStatus.Closed, 0), totalCount, Utils.GetStatusIcon(RequestStatus.Closed)),
            ];
        }
    }

    private async Task OnPageSizeChanged()
    {
        _pagination.ItemsPerPage = uiStateSvc.PageSize;
        await _pagination.SetCurrentPageIndexAsync(0);
    }

    private void ClearSearch()
    {
        _searchValue = "";
        ApplyFilters();
    }

    private void ClearAllFilters()
    {
        _searchValue = "";
        _statusFilter?.Reset();
        _priorityFilter?.Reset();
        _selectedStatuses.Clear();
        _selectedPriorities.Clear();
        _dateRangeFilter?.Clear();
        _startDateFilter = null;
        _endDateFilter = null;
        ApplyFilters();
        StateHasChanged();
    }

    public bool CheckNewBtnPermission()
    {
        return !currentUser.IsAdmin && !currentUser.IsCEO;
    }

    private async Task ExportToExcel()
    {
        if (_filteredItems == null || !_filteredItems.Any())
            return;

        string dateRangeText = "";
        if (_startDateFilter.HasValue && _endDateFilter.HasValue)
        {
            dateRangeText = $" from {_startDateFilter.Value:MMM dd, yyyy} to {_endDateFilter.Value:MMM dd, yyyy}";
        }
        else if (_startDateFilter.HasValue)
        {
            dateRangeText = $" from {_startDateFilter.Value:MMM dd, yyyy}";
        }
        else if (_endDateFilter.HasValue)
        {
            dateRangeText = $" up to {_endDateFilter.Value:MMM dd, yyyy}";
        }

        var options = new ConfirmationModalOptions
        {
            Message = $"This will export the job order requests based on your recent filter selected{dateRangeText}. Do you wish to proceed?",
            Title = "Export Requests",
            Variant = ConfirmationModalVariant.info,
            ConfirmText = "Proceed",
            CancelText = "Cancel",
            Icon = "bi bi-download",
        };

        var result = await _confirmModal!.ShowAsync(options);
        if (!result) return;

        var progress = new Progress<int>(value =>
        {
            InvokeAsync(StateHasChanged);
        });

        var rows = _filteredItems.Select(r => new RequestExportRow
        {
            SeriesNo = r.SeriesNumber,
            RequestedBy = r.Name,
            NatureOfRequest = r.Nature_Of_Request,
            DivisionDepartment = r.Division_Department,
            BusinessUnit = r.BusinessUnitCode,
            Priority = Utils.GetPriorityDisplay(r.Priority, r.OtherPriority, false),
            Status = Utils.GetStatusDisplay(r.Status),
            DateRequested = r.DateRequested,
            TotalAmount = r.Attachments.Sum(e => e.ReceiptAmount),
        }).ToList();

        var banner = await Http.GetByteArrayAsync("images/logo/banner.png");

        // UPDATED: Pass the date range parameters
        var fileBytes = await Helpers.Export.ExportRequest.Export(
            rows,
            statusFilter: _selectedStatuses.Count == 0 ? "All" : string.Join(",", _selectedStatuses),
            priorityFilter: _selectedPriorities.Count == 0 ? "All" : string.Join(",", _selectedPriorities),
            startDate: _startDateFilter,
            endDate: _endDateFilter,
            type: "Job Order",
            bannerBytes: banner
        );

        // Create filename with date range if applicable
        string fileName = $"Job Order Request List";

        if (_startDateFilter.HasValue && _endDateFilter.HasValue)
        {
            if (_startDateFilter.Value.Date == _endDateFilter.Value.Date)
            {
                fileName += $" {_startDateFilter.Value:yyyy-MM-dd}";
            }
            else
            {
                fileName += $" {_startDateFilter.Value:yyyy-MM-dd} to {_endDateFilter.Value:yyyy-MM-dd}";
            }
        }
        else if (_startDateFilter.HasValue)
        {
            fileName += $" from {_startDateFilter.Value:yyyy-MM-dd}";
        }
        else if (_endDateFilter.HasValue)
        {
            fileName += $" up to {_endDateFilter.Value:yyyy-MM-dd}";
        }
        else
        {
            fileName += $" {DateTime.Now:yyyy-MM-dd}";
        }

        fileName += ".xlsx";

        await jsRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
        toastSvc.ShowSuccess("Exported Successfully.");
    }

    #region Carousel

    // Carousel methods
    private void NextSlide()
    {
        if (_statusSummaries.Count > 0 && _currentSlide < _statusSummaries.Count - 1)
        {
            _currentSlide++;
            StateHasChanged();
        }
    }

    private void PrevSlide()
    {
        if (_currentSlide > 0)
        {
            _currentSlide--;
            StateHasChanged();
        }
    }

    private void GoToSlide(int slideIndex)
    {
        if (slideIndex >= 0 && slideIndex < _statusSummaries.Count)
        {
            _currentSlide = slideIndex;
            StateHasChanged();
        }
    }

    // These methods will be called from JavaScript
    [JSInvokable]
    public async Task NextSlideJS()
    {
        NextSlide();
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task PrevSlideJS()
    {
        PrevSlide();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleWheel(WheelEventArgs e)
    {
        try
        {
            // Check if mobile view using a simpler approach
            var isMobile = await jsRuntime.InvokeAsync<bool>("eval",
                "window.innerWidth <= 768");

            if (isMobile)
            {
                if (e.DeltaY > 0)
                {
                    NextSlide();
                }
                else
                {
                    PrevSlide();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling wheel: {ex.Message}");
        }
    }

    #endregion Carousel

    public async ValueTask DisposeAsync()
    {
        currentUser.OnUserChanged -= Refresh;

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync(
                "disposeCarousel",
                _carouselContainer
            );

            await _jsModule.DisposeAsync();
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    private record StatusSummary(RequestStatus Status, string Label, int Count, int TotalCount, string Icon);
}