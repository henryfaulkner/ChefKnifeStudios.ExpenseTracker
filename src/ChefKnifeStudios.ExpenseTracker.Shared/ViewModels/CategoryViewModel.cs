using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ICategoryViewModel : IViewModel
{
    IEnumerable<CategoryDTO> Categories { get; }
    List<CategoryDTO> SelectedCategoryList { get; set; }
    bool IsLoading { get; }
    Task LoadCategoriesAsync();
}

public class CategoryViewModel : BaseViewModel, ICategoryViewModel, IDisposable
{
    readonly IStorageService _storageService;
    readonly IToastService _toastService;
    readonly IEventNotificationService _eventNotificationService;

    public CategoryViewModel(
        IStorageService storageService,
        IToastService toastService,
        IEventNotificationService eventNotificationService)
    {
        _storageService = storageService;
        _toastService = toastService;
        _eventNotificationService = eventNotificationService;
        _eventNotificationService.EventReceived += HandleEventReceived;
    }

    public void Dispose()
    {
        _eventNotificationService.EventReceived -= HandleEventReceived;
        GC.SuppressFinalize(this);
    }

    IEnumerable<CategoryDTO> _category = [];
    public IEnumerable<CategoryDTO> Categories
    {
        get => _category;
        private set => SetValue(ref _category, value);
    }

    List<CategoryDTO> _selectedCategoryList = [];
    public List<CategoryDTO> SelectedCategoryList
    {
        get => _selectedCategoryList;
        set => SetValue(ref _selectedCategoryList, value);
    }

    bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetValue(ref _isLoading, value);
    }

    public async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        try
        {
            var res = await _storageService.GetCategoriesAsync();
            if (res.IsSuccess && res.Data is not null) Categories = res.Data;
            else _toastService.ShowError("Categories failed to load");
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task HandleEventReceived(object sender, IEventArgs args)
    {
        switch (args)
        {
            case CategoryEventArgs { Type: CategoryEventArgs.EventTypes.AddExpenseCategories, Data: { Categories: IEnumerable<CategoryDTO> newCategories } }:
                var selectedIds = SelectedCategoryList.Select(x => x.Id);
                foreach (var category in newCategories)
                {
                    if (!selectedIds.Contains(category.Id))
                        SelectedCategoryList.Add(category);
                }
                SelectedCategoryList = SelectedCategoryList.ToList(); // create copy to force state change
                break;
        }
        await Task.CompletedTask;
    }
}
