using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using SSDI.RequestMonitoring.UI.Models.Requests.JobOrder;
using SSDI.RequestMonitoring.UI.Models.Requests.Purchase;

namespace SSDI.RequestMonitoring.UI.Pages.Dashboard.Charts;

public partial class VolumeComparison__Bar : ComponentBase
{
    [Parameter] public List<Purchase_RequestVM> PurchaseRequests { get; set; } = [];
    [Parameter] public List<Job_OrderVM> JobOrders { get; set; } = [];
    [Parameter] public string EmptyMessage { get; set; } = "No data available for the selected period";
    [Parameter] public RequestType RequestType { get; set; } = RequestType.All;

    private string _chartPeriod = "monthly";
    protected List<VolumeChartItem> _chartData = [];
    private bool _jsReady = false;

    private string ChartTitle =>
    $"{Utils.GetRequestTypeName(RequestType)} Requests Comparison ({Utils.FirstCharToUpper(_chartPeriod)})";

    private readonly BarConfig _chartConfig = new()
    {
        Options = new BarOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,
            Legend = new Legend
            {
                Display = true,
                Position = Position.Top,
                Labels = new LegendLabels
                {
                    UsePointStyle = true,
                    Padding = 20,
                    FontSize = 12
                }
            },
            Tooltips = new Tooltips
            {
                Mode = InteractionMode.Index,
                Intersect = false,
                BackgroundColor = ColorUtil.ColorHexString(255, 255, 255),
                TitleFontColor = ColorUtil.ColorHexString(55, 65, 81),
                BodyFontColor = ColorUtil.ColorHexString(55, 65, 81),
                BorderColor = ColorUtil.ColorHexString(229, 231, 235),
                BorderWidth = 1
            },
            Scales = new BarScales
            {
                XAxes =
                    [
                        new BarCategoryAxis
                        {
                            GridLines = new GridLines { Display = false },
                            Ticks = new CategoryTicks
                            {
                                FontSize = 11,
                                MaxRotation = 0
                            }
                        }
                    ],
                YAxes =
                    [
                        new LinearCartesianAxis
                        {
                            GridLines = new GridLines { DrawBorder = false },
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true,
                                FontSize = 11,
                                Precision = 0
                            }
                        }
                    ]
            },
            Animation = new Animation
            {
                Duration = 800,
                Easing = ChartJs.Blazor.Common.Enums.Easing.EaseOutQuart
            }
        }
    };

    private Chart? _chartRef;

    protected override async Task OnParametersSetAsync()
    {
        await RefreshChart();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(150);

            _jsReady = true;
            await RefreshChart();
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task RefreshChart()
    {
        if (!_jsReady)
            return;

        await LoadChartData();
        UpdateChart();

        if (_chartRef != null)
        {
            await _chartRef.Update();
        }
    }

    private async Task LoadChartData()
    {
        _chartData = _chartPeriod switch
        {
            "daily" => await GetDailyData(),
            "weekly" => await GetWeeklyData(),
            _ => await GetMonthlyData()
        };

        // Apply RequestType filter
        if (RequestType == RequestType.Purchase)
        {
            _chartData.ForEach(x => x.JobOrderCount = 0);
        }
        else if (RequestType == RequestType.JobOrder)
        {
            _chartData.ForEach(x => x.PurchaseCount = 0);
        }
        StateHasChanged();
    }

    private async Task<List<VolumeChartItem>> GetMonthlyData()
    {
        await Task.Delay(20);
        //var (pr, jo) = await LoadPRandJO();
        var pr = PurchaseRequests;
        var jo = JobOrders;

        var months = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddMonths(-i))
            .Reverse();

        return [.. months.Select(m => new VolumeChartItem
        {
            Label = m.ToString("MMM yyyy"),
            PurchaseCount = pr.Count(r => r.DateRequested?.Month == m.Month && r.DateRequested?.Year == m.Year),
            JobOrderCount = jo.Count(r => r.DateRequested?.Month == m.Month && r.DateRequested?.Year == m.Year)
        })];
    }

    private async Task<List<VolumeChartItem>> GetWeeklyData()
    {
        await Task.Delay(20);
        //var (pr, jo) = await LoadPRandJO();
        var pr = PurchaseRequests;
        var jo = JobOrders;

        var weeks = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddDays(-i * 7))
            .Reverse();

        return [.. weeks.Select(w => new VolumeChartItem
        {
            Label = $"W{GetWeekNumber(w)}",
            PurchaseCount = pr.Count(r =>
                GetWeekNumber(r.DateRequested ?? DateTime.MinValue) == GetWeekNumber(w) &&
                r.DateRequested?.Year == w.Year),
            JobOrderCount = jo.Count(r =>
                GetWeekNumber(r.DateRequested ?? DateTime.MinValue) == GetWeekNumber(w) &&
                r.DateRequested?.Year == w.Year)
        })];
    }

    private async Task<List<VolumeChartItem>> GetDailyData()
    {
        await Task.Delay(20);
        //var (pr, jo) = await LoadPRandJO();
        var pr = PurchaseRequests;
        var jo = JobOrders;

        var days = Enumerable.Range(0, 7)
            .Select(i => DateTime.Now.AddDays(-i))
            .Reverse();

        return [.. days.Select(d => new VolumeChartItem
        {
            Label = d.ToString("ddd dd"),
            PurchaseCount = pr.Count(r => r.DateRequested?.Date == d.Date),
            JobOrderCount = jo.Count(r => r.DateRequested?.Date == d.Date)
        })];
    }

    //private async Task<(List<Purchase_RequestVM>, List<Job_OrderVM>)> LoadPRandJO()
    //{
    //    var prTask = Utils.IsSupervisor()
    //        ? purchaseRequestSvc.GetAllPurchaseReqBySupervisor(currentUser.UserId, true, true)
    //        : purchaseRequestSvc.GetAllPurchaseRequestsByUser(currentUser.UserId);

    //    var joTask = Utils.IsSupervisor()
    //        ? jobOrderSvc.GetAllJobOrderBySupervisor(currentUser.UserId, true, true)
    //        : jobOrderSvc.GetAllJobOrdersByUser(currentUser.UserId);

    //    await Task.WhenAll(prTask, joTask);
    //    return (prTask.Result, joTask.Result);
    //}

    private static int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }

    private void UpdateChart()
    {
        _chartConfig.Data.Datasets.Clear();
        _chartConfig.Data.Labels.Clear();

        // Add labels
        foreach (var item in _chartData)
            _chartConfig.Data.Labels.Add(item.Label);

        // Always add Purchase dataset when All OR Purchase selected
        if (RequestType == RequestType.All || RequestType == RequestType.Purchase)
        {
            _chartConfig.Data.Datasets.Add(new BarDataset<int>([.. _chartData.Select(x => x.PurchaseCount)])
            {
                Label = "Purchase Requests",
                BackgroundColor = ColorUtil.ColorHexString(59, 130, 246),
                BorderColor = ColorUtil.ColorHexString(37, 99, 235),
                BorderWidth = 1,
                BarPercentage = 0.6,
                CategoryPercentage = 0.8
            });
        }

        // Add Job Order dataset when All OR JobOrder selected
        if (RequestType == RequestType.All || RequestType == RequestType.JobOrder)
        {
            _chartConfig.Data.Datasets.Add(new BarDataset<int>([.. _chartData.Select(x => x.JobOrderCount)])
            {
                Label = "Job Orders",
                BackgroundColor = ColorUtil.ColorHexString(245, 158, 11),
                BorderColor = ColorUtil.ColorHexString(217, 119, 6),
                BorderWidth = 1,
                BarPercentage = 0.6,
                CategoryPercentage = 0.8
            });
        }

        StateHasChanged();
    }
}

public class VolumeChartItem
{
    public string Label { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public int JobOrderCount { get; set; }
}