using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ISearchViewModel : IViewModel
{
    bool IsLoading { get; }
    IEnumerable<BudgetDTO> Budgets { get; }

    Task LoadPagedBudgetsAsync();
    Task ChangeSearchTextAsync(string searchText);
    Task ChangePageNumberAsync(int pageNumber);
}

public class SearchViewModel(IStorageService storageService) : BaseViewModel, ISearchViewModel
{
    readonly IStorageService _storageService = storageService;

    const int PAGE_SIZE = 10;

    bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetValue(ref _isLoading, value);
    }

    IEnumerable<BudgetDTO>? _budgets = null!;
    public IEnumerable<BudgetDTO>? Budgets
    {
        get => _budgets;
        private set => SetValue(ref _budgets, value);
    }

    string _searchText = string.Empty;
    int _pageNumber = 0;

    public async Task LoadPagedBudgetsAsync()
    {
        IsLoading = true;
        Budgets = await _storageService.GetBudgetsAsync();
        IsLoading = false;
    }

    public async Task ChangeSearchTextAsync(string searchText)
    {
        if (_searchText == searchText) return;
        _searchText = searchText;
        await LoadPagedBudgetsAsync();
    }

    public async Task ChangePageNumberAsync(int pageNumber)
    {
        if (_pageNumber == pageNumber) return;
        _pageNumber = pageNumber;
        await LoadPagedBudgetsAsync();
    }
}
