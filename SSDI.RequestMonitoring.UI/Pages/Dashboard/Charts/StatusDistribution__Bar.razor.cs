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

public partial class StatusDistribution__Bar : ComponentBase
{
    [Parameter] public List<Purchase_RequestVM> PurchaseRequests { get; set; } = [];
    [Parameter] public List<Job_OrderVM> JobOrders { get; set; } = [];
    [Parameter] public RequestType RequestType { get; set; } = RequestType.All;

    private Chart? _chartRef;
    protected List<StatusCountItem> _chartData = [];
    private bool _jsReady = false;

    private readonly BarConfig _chartConfig = new(horizontal: true)
    {
        Options = new BarOptions
        {
            Responsive = true,
            MaintainAspectRatio = false,

            Legend = new Legend
            {
                Display = false
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
                    new LinearCartesianAxis
                    {
                        Ticks = new LinearCartesianTicks
                        {
                            BeginAtZero = true,
                            Precision = 0
                        },
                        GridLines = new GridLines { DrawBorder = false}
                    }
                ],
                YAxes =
                [
                    new BarCategoryAxis
                    {
                        Ticks = new CategoryTicks
                        {
                            FontSize = 11,
                            MaxRotation = 0,
                        },
                        GridLines = new GridLines { DrawBorder = false }
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
    private string ChartTitle =>
    RequestType switch
    {
        RequestType.Purchase => "Purchase Requests by Status",
        RequestType.JobOrder => "Job Orders by Status",
        _ => "All Requests by Status"
    };

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
        await Task.Delay(20);

        var prList = PurchaseRequests;
        var joList = JobOrders;

        IEnumerable<RequestStatus> statuses;

        if (RequestType == RequestType.Purchase)
        {
            // Only purchase requests
            statuses = PurchaseRequests.Select(p => p.Status);
        }
        else if (RequestType == RequestType.JobOrder)
        {
            // Only job orders
            statuses = JobOrders.Select(p => p.Status);
        }
        else
        {
            // All requests: purchase + job order
            statuses = PurchaseRequests.Select(p => p.Status)
                        .Concat(JobOrders.Select(j => j.Status));
        }

        _chartData = [.. statuses
            .GroupBy(s => s)
            .Select(g => new StatusCountItem
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)];

        StateHasChanged();
    }

    private void UpdateChart()
    {
        _chartConfig.Data.Datasets.Clear();
        _chartConfig.Data.Labels.Clear();

        foreach (var item in _chartData)
        {
            _chartConfig.Data.Labels.Add(Utils.GetStatusDisplay(item.Status));
        }

        var colors = new IndexableOption<string>(
                        [.. _chartData.Select(x => Utils.GetStatusColor(x.Status))]
                    );

        var dataset = new BarDataset<int>([.. _chartData.Select(x => x.Count)], horizontal: true)
        {
            BackgroundColor = colors,
            BorderWidth = 1,
            BarPercentage = 0.6,
            CategoryPercentage = 0.8
        };

        _chartConfig.Data.Datasets.Add(dataset);

        StateHasChanged();
    }

    public class StatusCountItem
    {
        public RequestStatus Status { get; set; }
        public int Count { get; set; }
    }
}