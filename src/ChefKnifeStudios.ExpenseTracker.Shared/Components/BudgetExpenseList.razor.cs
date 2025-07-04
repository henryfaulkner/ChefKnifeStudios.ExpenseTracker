using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components;

public partial class BudgetExpenseList : ComponentBase, IDisposable
{
    [Inject] ISearchViewModel SearchViewModel { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;

    PaginationState _pagination = new PaginationState { ItemsPerPage = 10 };

    readonly string[] _subscriptions =
    [
        nameof(SearchViewModel.Budgets),
        nameof(SearchViewModel.IsLoading),
    ];

    protected override void OnInitialized()
    {
        SearchViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
    }

    public void Dispose()
    {
        SearchViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
    }

    void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscriptions.Contains(e.PropertyName) is false) return;
        Task.Run(async () => await InvokeAsync(StateHasChanged));
    }

    void HandleEditBudgetPressed(BudgetDTO budget)
    {

    }
}
