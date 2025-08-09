using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class CategoryPicker : ComponentBase, IDisposable
{
    [Inject] ICategoryViewModel CategoryViewModel { get; set; } = null!;

    readonly string[] _subscriptions = [
        nameof(ICategoryViewModel.SelectedCategoryList),
    ];

    bool _isOpen;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        CategoryViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
    }

    public void Dispose()
    {
        CategoryViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        GC.SuppressFinalize(this);
    }

    public IEnumerable<CategoryDTO> GetSelections() => CategoryViewModel.SelectedCategoryList;

    public void Clear()
    {
        _isOpen = false;
        CategoryViewModel.SelectedCategoryList = [];
    }

    void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscriptions.Contains(e.PropertyName) is false || string.IsNullOrWhiteSpace(e.PropertyName)) return;
        if (e.PropertyName.Equals(nameof(ICategoryViewModel.SelectedCategoryList)))
        {
            if (CategoryViewModel.SelectedCategoryList.Any())
            {
                _isOpen = true;
            }
        }
        Task.Run(async () => await InvokeAsync(StateHasChanged));
    }

    void HandleOpenOptionsPressed()
    {
        _isOpen = true;
    }

    void HandleCloseOptionsPressed()
    {
        _isOpen = false;
    }

    void ToggleSelectedCategory(CategoryDTO category)
    {
        var selectedIds = CategoryViewModel.SelectedCategoryList.Select(x => x.Id);
        if (selectedIds.Contains(category.Id))
            CategoryViewModel.SelectedCategoryList.RemoveAll(x => x.Id == category.Id);
        else CategoryViewModel.SelectedCategoryList.Add(category);
        // copy to force state change
        CategoryViewModel.SelectedCategoryList = CategoryViewModel.SelectedCategoryList.ToList();
    }
}
