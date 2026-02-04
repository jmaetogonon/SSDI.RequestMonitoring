using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls;

public partial class Autocomplete__Control<TItem, TValue> : ComponentBase, IAsyncDisposable
{
    [Parameter] public TItem? SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem?> SelectedItemChanged { get; set; }

    [Parameter] public TValue? SelectedValue { get; set; }
    [Parameter] public EventCallback<TValue?> SelectedValueChanged { get; set; }

    [Parameter] public string? SelectedText { get; set; }
    [Parameter] public EventCallback<string?> SelectedTextChanged { get; set; }

    [Parameter] public string? SearchText { get; set; }
    [Parameter] public EventCallback<string?> SearchTextChanged { get; set; }

    [Parameter] public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    [Parameter] public Expression<Func<TItem, string>>? TextSelector { get; set; }
    [Parameter] public Expression<Func<TItem, TValue>>? ValueSelector { get; set; }

    [Parameter] public Func<TItem, string>? DisplayTextSelector { get; set; }

    [Parameter] public string Placeholder { get; set; } = "Type to search...";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ShowClearButton { get; set; } = true;
    [Parameter] public bool ShowAddNewOption { get; set; }
    [Parameter] public bool ShowLoading { get; set; }
    [Parameter] public bool HasMoreItems { get; set; }

    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

    [Parameter] public EventCallback OnAddNew { get; set; }
    [Parameter] public EventCallback OnLoadMore { get; set; }

    [Parameter] public int MaxResults { get; set; } = 20;

    private ElementReference searchInput;
    private ElementReference dropdownList;
    private DotNetObjectReference<Autocomplete__Control<TItem, TValue>>? dotNetRef;

    private bool IsDropdownVisible { get; set; }
    private TItem? HoveredItem { get; set; }
    private IEnumerable<TItem> FilteredItems { get; set; } = [];
    private string? _previousSearchText;
    private string _displayText = string.Empty;
    private TItem? _internalSelectedItem;
    private Dictionary<TItem, string> _displayTextCache = [];


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            UpdateInternalState();
            await jsRuntime.InvokeVoidAsync("autocomplete.initClickOutside", dotNetRef, searchInput);
        }
    }

    protected override void OnParametersSet()
    {
        UpdateInternalState();

        // Only refilter if search text changed
        if (_previousSearchText != _displayText)
        {
            _previousSearchText = _displayText;
            FilterItems();
        }
    }

    private void UpdateInternalState()
    {
        // Determine the selected item
        if (SelectedItemChanged.HasDelegate && SelectedItem != null)
        {
            // Using SelectedItem binding
            _internalSelectedItem = SelectedItem;
            _displayText = GetItemDisplayText(SelectedItem);
        }
        else if (SelectedValueChanged.HasDelegate && SelectedValue != null && ValueSelector != null)
        {
            // Using SelectedValue binding - find matching item
            var valueFunc = ValueSelector.Compile();
            _internalSelectedItem = Items.FirstOrDefault(item =>
            {
                var itemValue = valueFunc(item);
                return itemValue != null && itemValue.Equals(SelectedValue);
            });

            if (_internalSelectedItem != null)
            {
                _displayText = GetItemDisplayText(_internalSelectedItem);
            }
            else
            {
                _displayText = SelectedText ?? string.Empty;
            }
        }
        else if (SelectedTextChanged.HasDelegate && !string.IsNullOrEmpty(SelectedText))
        {
            // Using SelectedText binding
            _displayText = SelectedText;
            _internalSelectedItem = default;
        }
        else if (SelectedValue != null && ValueSelector != null && Items.Any())
        {
            // Initial load with SelectedValue set
            var valueFunc = ValueSelector.Compile();
            _internalSelectedItem = Items.FirstOrDefault(item =>
            {
                var itemValue = valueFunc(item);
                return itemValue != null && itemValue.Equals(SelectedValue);
            });

            if (_internalSelectedItem != null)
            {
                _displayText = GetItemDisplayText(_internalSelectedItem);
            }
        }
        else
        {
            // No binding or clear state
            _displayText = string.Empty;
            _internalSelectedItem = default;
        }
    }

    private async Task HandleInput(ChangeEventArgs args)
    {
        _displayText = args.Value?.ToString() ?? string.Empty;

        await InvokeAsync(async () =>
        {
            var tasks = new List<Task>();

            if (SearchTextChanged.HasDelegate)
                tasks.Add(SearchTextChanged.InvokeAsync(_displayText));

            if (SelectedTextChanged.HasDelegate)
                tasks.Add(SelectedTextChanged.InvokeAsync(_displayText));

            if (tasks.Any())
                await Task.WhenAll(tasks);

            FilterItems();

            if (!string.IsNullOrEmpty(_displayText) && !IsDropdownVisible)
                IsDropdownVisible = true;

            StateHasChanged();
        });
    }

    private void HandleFocus()
    {
        if (Disabled) return;

        if (!IsDropdownVisible)
        {
            IsDropdownVisible = true;
            FilterItems();
            StateHasChanged();
        }
    }

    private void HandleBlur()
    {
        // Blur is handled by JavaScript click outside detection
    }

    [JSInvokable]
    public async Task HandleClickOutside()
    {
        await Task.Delay(100); // Small delay to allow click events to process

        // Only close if dropdown is visible
        if (IsDropdownVisible)
        {
            IsDropdownVisible = false;
            HoveredItem = default;

            // Restore selected item text if nothing was selected
            if (_internalSelectedItem != null && string.IsNullOrEmpty(_displayText))
            {
                _displayText = GetItemDisplayText(_internalSelectedItem);
            }

            await InvokeAsync(StateHasChanged);
        }
    }

    private void FilterItems()
    {
        // Clear cache if items changed significantly
        if (_displayTextCache.Count > Items.Count() * 2)
        {
            _displayTextCache.Clear();
        }

        if (string.IsNullOrEmpty(_displayText))
        {
            FilteredItems = Items.Take(MaxResults);
            return;
        }

        var searchTerms = _displayText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        FilteredItems = Items.Where(item =>
        {
            var itemText = GetItemDisplayText(item).ToLower();
            return searchTerms.All(term => itemText.Contains(term));
        }).Take(MaxResults);
    }

    private async Task SelectItem(TItem item)
    {
        _internalSelectedItem = item;
        _displayText = GetItemDisplayText(item);

        // Update all relevant bindings
        if (SelectedItemChanged.HasDelegate)
        {
            await SelectedItemChanged.InvokeAsync(item);
        }

        if (SelectedValueChanged.HasDelegate && ValueSelector != null)
        {
            var valueFunc = ValueSelector.Compile();
            await SelectedValueChanged.InvokeAsync(valueFunc(item));
        }

        if (SelectedTextChanged.HasDelegate)
        {
            await SelectedTextChanged.InvokeAsync(_displayText);
        }

        if (SearchTextChanged.HasDelegate)
        {
            await SearchTextChanged.InvokeAsync(_displayText);
        }

        IsDropdownVisible = false;
        HoveredItem = default;

        await InvokeAsync(StateHasChanged);
    }

    private async Task ClearSelection()
    {
        _internalSelectedItem = default;
        _displayText = string.Empty;

        // Clear all bindings
        if (SelectedItemChanged.HasDelegate)
        {
            await SelectedItemChanged.InvokeAsync(default);
        }

        if (SelectedValueChanged.HasDelegate)
        {
            await SelectedValueChanged.InvokeAsync(default);
        }

        if (SelectedTextChanged.HasDelegate)
        {
            await SelectedTextChanged.InvokeAsync(default);
        }

        if (SearchTextChanged.HasDelegate)
        {
            await SearchTextChanged.InvokeAsync(default);
        }

        // Close dropdown
        IsDropdownVisible = false;
        HoveredItem = default;

        // Focus back on input
        await searchInput.FocusAsync();

        await InvokeAsync(StateHasChanged);
    }

    private void ToggleDropdown()
    {
        if (Disabled) return;

        IsDropdownVisible = !IsDropdownVisible;
        if (IsDropdownVisible)
        {
            FilterItems();
            StateHasChanged();
        }
        else
        {
            StateHasChanged();
        }
    }

    private void SetHoveredItem(TItem item)
    {
        HoveredItem = item;
    }

    private async Task AddNewItem()
    {
        await OnAddNew.InvokeAsync();
        IsDropdownVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadMoreItems()
    {
        await OnLoadMore.InvokeAsync();
    }

    private string GetItemDisplayText(TItem item)
    {
        if (DisplayTextSelector != null)
        {
            return DisplayTextSelector(item);
        }

        if (TextSelector != null)
        {
            var func = TextSelector.Compile();
            return func(item) ?? string.Empty;
        }

        return item?.ToString() ?? string.Empty;
    }

    private bool IsSelected(TItem item)
    {
        if (_internalSelectedItem == null || item == null) return false;

        if (ValueSelector != null)
        {
            var valueFunc = ValueSelector.Compile();
            var selectedValue = valueFunc(_internalSelectedItem);
            var itemValue = valueFunc(item);
            return selectedValue != null && selectedValue.Equals(itemValue);
        }

        return Equals(_internalSelectedItem, item);
    }
    private string GetItemDisplayTextCached(TItem item)
    {
        if (!_displayTextCache.TryGetValue(item, out var text))
        {
            text = GetItemDisplayText(item);
            _displayTextCache[item] = text;
        }
        return text;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(
                "autocomplete.removeClickOutside",
                searchInput
            );

            // Also clear .NET references
            dotNetRef?.Dispose();
            dotNetRef = null;

            // Clear caches 
            _displayTextCache?.Clear();
        }
        catch (TaskCanceledException)
        {
            // Ignore if JS runtime disposed
        }
        catch (JSDisconnectedException)
        {
            // Ignore if circuit disconnected
        }
    }
}