using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ISearchViewModel : IViewModel
{
    bool IsLoading { get; }
    IEnumerable<BudgetDTO> Budgets { get; }
    ExpenseSearchResponseDTO SearchResult { get; }

    Task LoadPagedBudgetsAsync();
    Task<bool> ChangeSearchTextAsync(string searchText, int topN);
    Task ChangePageNumberAsync(int pageNumber);
}

public class SearchViewModel
     : BaseViewModel, ISearchViewModel, IDisposable
{
    readonly IStorageService _storageService;
    readonly ISemanticService _semanticService;
    readonly IToastService _toastService;
    readonly IEventNotificationService _eventNotificationService;

    public SearchViewModel(
        IStorageService storageService,
        IToastService toastService,
        ISemanticService semanticService,
        IEventNotificationService eventNotificationService)
    {
        _storageService = storageService;
        _semanticService = semanticService;
        _toastService = toastService;
        _eventNotificationService = eventNotificationService;

        _eventNotificationService.EventReceived += HandleEventReceived;
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BudgetEventArgs budgetEvent:
            case ExpenseEventArgs expenseEvent:
                await LoadPagedBudgetsAsync();
                break;
            default:
                break;
        }
    }

    public void Dispose()
    {
        _eventNotificationService.EventReceived -= HandleEventReceived;
    }

    const int PAGE_SIZE = 10;

    bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetValue(ref _isLoading, value);
    }

    IEnumerable<BudgetDTO> _budgets = [];
    public IEnumerable<BudgetDTO> Budgets
    {
        get => _budgets;
        private set => SetValue(ref _budgets, value);
    }

    ExpenseSearchResponseDTO _searchResult = new() { RagMessage = string.Empty, Expenses = [], };
    public ExpenseSearchResponseDTO SearchResult
    {
        get => _searchResult;
        private set => SetValue(ref _searchResult, value);
    }

    string _searchText = string.Empty;
    int _pageNumber = 0;

    public async Task LoadPagedBudgetsAsync()
    {
        IsLoading = true;
        try
        {
            var res = await _storageService.GetBudgetsAsync();
            if (res.IsSuccess && res.Data is not null) Budgets = res.Data;
            else _toastService.ShowError("Budgets failed to load");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> ChangeSearchTextAsync(string searchText, int topN)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            _toastService.ShowWarning("Search text is required");
            return false;
        }

        _searchText = searchText;
        ExpenseSearchDTO reqBody = new () { SearchText = searchText, TopN = topN, };
        var res = await _semanticService.SearchExpensesAsync(reqBody);
        if (res.IsSuccess) SearchResult = res.Data ?? new() { RagMessage = string.Empty, Expenses = [], };
        else _toastService.ShowError("Search results failed to load");
        return res.IsSuccess;
    }

    public async Task ChangePageNumberAsync(int pageNumber)
    {
        if (_pageNumber == pageNumber) return;
        _pageNumber = pageNumber;
        await LoadPagedBudgetsAsync();
    }
}
