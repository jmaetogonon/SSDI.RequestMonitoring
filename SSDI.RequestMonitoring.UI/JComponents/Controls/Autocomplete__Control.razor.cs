using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace SSDI.RequestMonitoring.UI.JComponents.Controls;

public partial class Autocomplete__Control<TItem, TValue> : ComponentBase, IAsyncDisposable
{ 
    [Parameter] public IEnumerable<TItem> Items { get; set; } = [];

    [Parameter] public TItem? SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem?> SelectedItemChanged { get; set; }

    [Parameter] public TValue? SelectedValue { get; set; }
    [Parameter] public EventCallback<TValue?> SelectedValueChanged { get; set; }

    [Parameter] public string? SelectedText { get; set; }
    [Parameter] public EventCallback<string?> SelectedTextChanged { get; set; }

    [Parameter] public string? SearchText { get; set; }
    [Parameter] public EventCallback<string?> SearchTextChanged { get; set; }

    [Parameter] public Expression<Func<TItem, string>>? TextSelector { get; set; }
    [Parameter] public Expression<Func<TItem, TValue>>? ValueSelector { get; set; }

    [Parameter] public Func<TItem, string>? DisplayTextSelector { get; set; }

    [Parameter] public string Placeholder { get; set; } = "Type to search...";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ShowClearButton { get; set; } = true;
    [Parameter] public bool ShowAddNewOption { get; set; }
    [Parameter] public bool ShowLoading { get; set; }
    [Parameter] public bool HasMoreItems { get; set; }
    [Parameter] public int MaxResults { get; set; } = 20;

    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
    [Parameter] public EventCallback OnAddNew { get; set; }
    [Parameter] public EventCallback OnLoadMore { get; set; }

    private ElementReference _searchInput;
    private DotNetObjectReference<Autocomplete__Control<TItem, TValue>>? _dotNetRef;

    private bool _isDropdownVisible;
    private TItem? _hoveredItem;
    private TItem? _internalSelectedItem;
    private string _displayText = string.Empty;
    private string? _previousSearchText;

    private IEnumerable<TItem> _filteredItems = [];
    private readonly Dictionary<TItem, string> _displayTextCache = [];

    private Func<TItem, string>? _textFunc;
    private Func<TItem, TValue>? _valueFunc;
    private int _itemsCount;
    private bool _disposed;

    protected override void OnParametersSet()
    {
        _itemsCount = Items is ICollection<TItem> c ? c.Count : Items.Count();

        if (TextSelector != null && _textFunc == null)
            _textFunc = TextSelector.Compile();

        if (ValueSelector != null && _valueFunc == null)
            _valueFunc = ValueSelector.Compile();

        UpdateInternalState();

        if (_previousSearchText != _displayText)
        {
            _previousSearchText = _displayText;
            FilterItems();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("autocomplete.initClickOutside", _dotNetRef, _searchInput);
        }
    }

    private void UpdateInternalState()
    {
        if (SelectedItem != null)
        {
            _internalSelectedItem = SelectedItem;
            _displayText = GetItemDisplayTextCached(SelectedItem);
        }
        else if (SelectedValue != null && _valueFunc != null)
        {
            _internalSelectedItem = Items.FirstOrDefault(i =>
                EqualityComparer<TValue>.Default.Equals(_valueFunc(i), SelectedValue));

            if (_internalSelectedItem != null)
                _displayText = GetItemDisplayTextCached(_internalSelectedItem);
        }
        else if (!string.IsNullOrEmpty(SelectedText))
        {
            _displayText = SelectedText!;
            _internalSelectedItem = default;
        }
    }

    private async Task HandleInput(ChangeEventArgs args)
    {
        _displayText = args.Value?.ToString() ?? string.Empty;

        if (SearchTextChanged.HasDelegate)
            await SearchTextChanged.InvokeAsync(_displayText);

        if (SelectedTextChanged.HasDelegate)
            await SelectedTextChanged.InvokeAsync(_displayText);

        FilterItems();
        _isDropdownVisible = true;
        StateHasChanged();
    }

    private void HandleFocus()
    {
        if (Disabled) return;
        _isDropdownVisible = true;
        FilterItems();
    }

    private void FilterItems()
    {
        if (_displayTextCache.Count > _itemsCount * 2)
            _displayTextCache.Clear();

        if (string.IsNullOrWhiteSpace(_displayText))
        {
            _filteredItems = Items.Take(MaxResults);
            return;
        }

        var terms = _displayText
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        _filteredItems = Items
            .Where(item =>
            {
                var text = GetItemDisplayTextCached(item).ToLowerInvariant();
                return terms.All(t => text.Contains(t));
            })
            .Take(MaxResults);
    }

    private async Task SelectItem(TItem item)
    {
        _internalSelectedItem = item;
        _displayText = GetItemDisplayTextCached(item);

        if (SelectedItemChanged.HasDelegate)
            await SelectedItemChanged.InvokeAsync(item);

        if (SelectedValueChanged.HasDelegate && _valueFunc != null)
            await SelectedValueChanged.InvokeAsync(_valueFunc(item));

        if (SelectedTextChanged.HasDelegate)
            await SelectedTextChanged.InvokeAsync(_displayText);

        _isDropdownVisible = false;
        StateHasChanged();
    }

    private async Task ClearSelection()
    {
        _internalSelectedItem = default;
        _displayText = string.Empty;

        await SelectedItemChanged.InvokeAsync(default);
        await SelectedValueChanged.InvokeAsync(default);
        await SelectedTextChanged.InvokeAsync(default);
        await SearchTextChanged.InvokeAsync(default);

        _isDropdownVisible = false;
        await _searchInput.FocusAsync();
    }

    private void ToggleDropdown()
    {
        if (Disabled) return;
        _isDropdownVisible = !_isDropdownVisible;
        if (_isDropdownVisible)
            FilterItems();
    }

    private async Task AddNewItem() => await OnAddNew.InvokeAsync();
    private async Task LoadMoreItems() => await OnLoadMore.InvokeAsync();

    private string GetItemDisplayTextCached(TItem item)
    {
        if (!_displayTextCache.TryGetValue(item, out var text))
        {
            text = DisplayTextSelector?.Invoke(item)
                   ?? _textFunc?.Invoke(item)
                   ?? item?.ToString()
                   ?? string.Empty;

            _displayTextCache[item!] = text;
        }
        return text;
    }

    private bool IsSelected(TItem item)
    {
        if (_internalSelectedItem == null) return false;

        if (_valueFunc != null)
            return EqualityComparer<TValue>.Default.Equals(
                _valueFunc(_internalSelectedItem),
                _valueFunc(item));

        return EqualityComparer<TItem>.Default.Equals(_internalSelectedItem, item);
    }

    [JSInvokable]
    public void HandleClickOutside()
    {
        if (_disposed) return;
        _isDropdownVisible = false;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        try
        {
            await jsRuntime.InvokeVoidAsync("autocomplete.removeClickOutside", _searchInput);
        }
        catch { }

        _dotNetRef?.Dispose();
        _displayTextCache.Clear();
        GC.SuppressFinalize(this);
    }
}
