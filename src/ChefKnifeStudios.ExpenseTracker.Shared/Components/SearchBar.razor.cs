using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class SearchBar : ComponentBase
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;

    string? _searchText = string.Empty;

    async Task HandleSearch()
    {
        if (string.IsNullOrWhiteSpace(_searchText)) _searchText = string.Empty;
        _searchText = _searchText.Trim();
        await SearchViewModel.ChangeSearchTextAsync(_searchText);
    }
}
