using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using MatBlazor;
using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class CategoryPicker : ComponentBase
{
    [Inject] ICategoryViewModel CategoryViewModel { get; set; } = null!;

    bool _isOpen;
    MatChip[] _selectedChips = [];

    void HandleOpenOptionsPressed()
    {
        _isOpen = true;
    }

    void HandleCloseOptionsPressed()
    {
        _isOpen = false;
    }
}
