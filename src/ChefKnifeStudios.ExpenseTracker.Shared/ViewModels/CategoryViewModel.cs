using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface ICategoryViewModel : IViewModel
{
    IEnumerable<CategoryDTO> Categories { get; }
    bool IsLoading { get; }
    Task LoadCategoriesAsync();
}

public class CategoryViewModel : BaseViewModel, ICategoryViewModel
{
    readonly IStorageService _storageService;
    readonly IToastService _toastService;

    public CategoryViewModel(
        IStorageService storageService,
        IToastService toastService)
    {
        _storageService = storageService;
        _toastService = toastService;
    }

    IEnumerable<CategoryDTO> _category = [];
    public IEnumerable<CategoryDTO> Categories
    {
        get => _category;
        private set => SetValue(ref _category, value);
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
}
