using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class SearchBar : ComponentBase, IDisposable
{
    [Inject] ISemanticService SemanticService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] ICommonJsInteropService CommonJsInteropService { get; set; } = null!;

    Guid _uid = Guid.NewGuid();
    string _elementId => $"search-bar-{_uid.ToString()}";
    DateTime _lastOpenedUtc;
    const int MinOpenDurationMs = 300;

    List<ExpenseSearchResponseDTO> _expenseList = [];
    string? _searchText = string.Empty;
    string _topNStr = "10";
    int _topN => int.Parse(_topNStr);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        await CommonJsInteropService.RegisterClickOutside(_elementId);
        CommonJsInteropService.AddClickOutsideCallback(HandleClose, _uid);
    }

    public void Dispose()
    {
        CommonJsInteropService.RemoveClickOutsideCallback(_uid);
    }

    async Task HandleSearchKeyPressed(KeyboardEventArgs e)
    {
        if (e.Code.ToLower() == "enter")
        {
            await HandleSearch();
            return;
        }
    }

    async Task HandleSearch()
    {
        if (string.IsNullOrWhiteSpace(_searchText)) _searchText = string.Empty;
        _searchText = _searchText.Trim();

        ExpenseSearchDTO reqBody = new() { SearchText = _searchText, TopN = _topN, };
        var searchRes = await SemanticService.SearchExpensesAsync(reqBody);
        if (searchRes?.Data == null)
        {
            ToastService.ShowError("Search failed");
            _expenseList = [];
            return;
        }
        _expenseList = searchRes.Data.ToList();
        if (_expenseList.Count == 0) 
            ToastService.ShowWarning("No expenses were found");

        _lastOpenedUtc = DateTime.UtcNow;
        _ = InvokeAsync(StateHasChanged);
    }

    void HandleClose()
    {
        // Prevent close if not enough time has passed since open
        if ((DateTime.UtcNow - _lastOpenedUtc).TotalMilliseconds < MinOpenDurationMs)
            return;

        _expenseList = [];
        _ = InvokeAsync(StateHasChanged);
    }
}
