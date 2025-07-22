using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class SearchBar : ComponentBase
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    string? _searchText = string.Empty;
    string _topNStr = "10";
    int _topN => int.Parse(_topNStr);

    async Task HandleSearchKeyPressed(KeyboardEventArgs e)
    {
        if (e.Code.ToLower() == "enter")
        {
            await Task.Delay(50);
            await HandleSearch();
            return;
        }
    }

    async Task HandleSearch()
    {
        if (string.IsNullOrWhiteSpace(_searchText)) _searchText = string.Empty;
        _searchText = _searchText.Trim();
        bool isSuccess = await SearchViewModel.ChangeSearchTextAsync(_searchText, _topN);
        if (isSuccess && SearchViewModel.SearchResult.Expenses.Any())
        {
            EventNotificationService.PostEvent(
                this,
                new BladeEventArgs()
                {
                    Type = BladeEventArgs.Types.Search,
                }
            );
        }
        else if (isSuccess)
        {
            ToastService.ShowWarning("Your search returned zero results");
        }
    }
}
