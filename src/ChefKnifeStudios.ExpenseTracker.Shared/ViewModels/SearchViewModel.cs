using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ISearchViewModel : IViewModel
{
    bool IsLoading { get; }
    PagedResultDTO<BudgetDTO>? PagedBudgets { get; }

    Task LoadPagedBudgets();
    Task ChangeSearchText(string searchText);
    Task ChangePageNumber(int pageNumber);
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

    PagedResultDTO<BudgetDTO>? _pagedBudgets = null!;
    public PagedResultDTO<BudgetDTO>? PagedBudgets
    {
        get => _pagedBudgets;
        private set => SetValue(ref _pagedBudgets, value);
    }

    string _searchText = string.Empty;
    int _pageNumber = 0;

    public async Task LoadPagedBudgets()
    {
        IsLoading = true;
        PagedBudgets = await _storageService.SearchBudgetsAsync(
            _searchText,
            PAGE_SIZE,
            _pageNumber
        );
        IsLoading = false;
    }

    public async Task ChangeSearchText(string searchText)
    {
        if (_searchText == searchText) return;
        _searchText = searchText;
        await LoadPagedBudgets();
    }

    public async Task ChangePageNumber(int pageNumber)
    {
        if (_pageNumber == pageNumber) return;
        _pageNumber = pageNumber;
        await LoadPagedBudgets();
    }
}
