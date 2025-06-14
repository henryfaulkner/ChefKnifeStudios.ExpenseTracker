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

public class SearchViewModel(IStorageService storageService, IToastService toastService) : BaseViewModel, ISearchViewModel
{
    readonly IStorageService _storageService = storageService;
    readonly IToastService _toastService = toastService;

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
        var res = await _storageService.GetBudgetsAsync();
        if (res.IsSuccess) Budgets = res.Data;
        else _toastService.ShowError("Budgets failed to load");
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
