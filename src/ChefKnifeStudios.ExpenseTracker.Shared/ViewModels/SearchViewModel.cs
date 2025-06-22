using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ISearchViewModel : IViewModel
{
    bool IsLoading { get; }
    IEnumerable<BudgetDTO> Budgets { get; }
    IEnumerable<ExpenseSearchResponseDTO> SearchedExpenses { get; }

    Task LoadPagedBudgetsAsync();
    Task ChangeSearchTextAsync(string searchText, int topN);
    Task ChangePageNumberAsync(int pageNumber);
}

public class SearchViewModel
    (IStorageService storageService, 
    IToastService toastService,
    ISemanticService semanticService) : BaseViewModel, ISearchViewModel
{
    readonly IStorageService _storageService = storageService;
    readonly ISemanticService _semanticService = semanticService;
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

    IEnumerable<ExpenseSearchResponseDTO> _searchedExpenses = [];
    public IEnumerable<ExpenseSearchResponseDTO> SearchedExpenses
    {
        get => _searchedExpenses;
        private set => SetValue(ref _searchedExpenses, value);
    }

    string _searchText = string.Empty;
    int _pageNumber = 0;

    public async Task LoadPagedBudgetsAsync()
    {
        IsLoading = true;
        try
        {
            var res = await _storageService.GetBudgetsAsync();
            if (res.IsSuccess) Budgets = res.Data;
            else _toastService.ShowError("Budgets failed to load");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ChangeSearchTextAsync(string searchText, int topN)
    {
        _searchText = searchText;
        ExpenseSearchDTO reqBody = new () { SearchText = searchText, TopN = topN, };
        var res = await _semanticService.SearchExpensesAsync(reqBody);
        if (res.IsSuccess) SearchedExpenses = res.Data;
        else _toastService.ShowError("Search results failed to load");
    }

    public async Task ChangePageNumberAsync(int pageNumber)
    {
        if (_pageNumber == pageNumber) return;
        _pageNumber = pageNumber;
        await LoadPagedBudgetsAsync();
    }
}
