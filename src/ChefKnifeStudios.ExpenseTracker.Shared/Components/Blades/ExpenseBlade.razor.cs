using ChefKnifeStudios.ExpenseTracker.Shared.DTOs;
using ChefKnifeStudios.ExpenseTracker.Shared.Models.EventArgs;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Components.Blades;

public partial class ExpenseBlade : ComponentBase, IDisposable
{
    [Inject] ILogger<BudgetBlade> Logger { get; set; } = null!;
    [Inject] IEventNotificationService EventNotificationService { get; set; } = null!;
    [Inject] IStorageService StorageService { get; set; } = null!;
    [Inject] ISemanticService SemanticService { get; set; } = null!;
    [Inject] IToastService ToastService { get; set; } = null!;
    [Inject] IExpenseViewModel ExpenseViewModel { get; set; } = null!;
    [Inject] ICommonJsInteropService CommonJsInteropService { get; set; } = null!;

    readonly string _recurringHelpUid = Guid.NewGuid().ToString();
    readonly Guid _recurringHelpRegistrationUid = Guid.NewGuid();

    BladeContainer? _bladeContainer;
    CategoryPicker? _categoryPicker;

    string? _name;
    decimal? _cost;
    bool _isRecurring = false;
    List<string>? _labels;
    bool _isLoading = false;
    bool _isRecurringTooltipOpen = false;

    readonly string[] _subscriptions =
    [
        nameof(IExpenseViewModel.IsListening),
    ];

    protected override void OnInitialized()
    {
        EventNotificationService.EventReceived += HandleEventReceived;
        ExpenseViewModel.PropertyChanged += ViewModel_OnPropertyChanged;

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        await CommonJsInteropService.RegisterClickOutside(_recurringHelpUid);
        CommonJsInteropService.AddClickOutsideCallback(HandleRecurringHelpClickedOutside, _recurringHelpRegistrationUid);
    }

    public void Dispose()
    {
        EventNotificationService.EventReceived -= HandleEventReceived;
        ExpenseViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        CommonJsInteropService.RemoveClickOutsideCallback(_recurringHelpRegistrationUid);
    }

    void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscriptions.Contains(e.PropertyName) is false) return;
        Task.Run(async () => await InvokeAsync(StateHasChanged));
    }

    async Task HandleEventReceived(object sender, IEventArgs e)
    {
        switch (e)
        {
            case BladeEventArgs { Type: BladeEventArgs.Types.Expense }:
                _bladeContainer?.Open();
                break;
            case BladeEventArgs { Type: not BladeEventArgs.Types.Expense }:
                _bladeContainer?.Close();
                break;
            default:
                Logger.LogWarning("Event handler's switch statement fell through.");
                break;
        }
        await Task.CompletedTask;
    }

    async Task HandleSubmitPressedAsync(MouseEventArgs e)
    {
        if (_name is null || !_cost.HasValue)
            return;

        _isLoading = true;
        try
        {
            var selectedCategories = _categoryPicker?.GetSelections() ?? [];
            ReceiptLabelsDTO receiptLabels = new()
            {
                Name = _name,
                CreatedOn = DateTime.Now,
                Labels = _labels,
                Categories = selectedCategories,
            };

            ExpenseDTO expense = new()
            {
                Name = _name,
                Cost = _cost.Value,
                Labels = _labels ?? [],
                IsRecurring = _isRecurring,
                Categories = selectedCategories,
            };

            var res1 = await StorageService.AddExpenseAsync(
                new() { Expense = expense, ReceiptLabels = receiptLabels, }
            );
            if (!res1.IsSuccess)
            {
                ToastService.ShowError("Your expense could not be created");
            }
            else
            {
                if (_isRecurring)
                {
                    RecurringExpenseConfigDTO recurringExpense = new()
                    {
                        Name = expense.Name,
                        Cost = expense.Cost,
                        Labels = expense.Labels ?? [],
                        CategoryIds = expense.Categories.Select(x => x.Id),
                    };
                    var res2 = await StorageService.AddRecurringExpenseAsync(recurringExpense);
                    if (!res1.IsSuccess)
                    {
                        ToastService.ShowError("Your recurring expense could not be created");
                        _isLoading = false;
                        return;
                    }
                    else
                    {
                        ToastService.ShowSuccess("Your recurring expense was created");
                    }
                }
                else
                {
                    ToastService.ShowSuccess("Your expense was created");
                }
            }

            EventNotificationService.PostEvent(
                this,
                new ExpenseEventArgs()
                {
                    Type = ExpenseEventArgs.Types.Added,
                }
            );
            EventNotificationService.PostEvent(
                this,
                new BladeEventArgs()
                {
                    Type = BladeEventArgs.Types.Close,
                }
            );
            Clear();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while creating expense.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    void Clear()
    {
        _name = null;
        _cost = null;
        _isRecurring = false;
        _labels = null;

        if (_categoryPicker is not null)
        {
            _categoryPicker.Clear();
        }
    }

    async Task HandlePickPictureAsync()
    {
        _isLoading = true;
        try
        { 
            var receipt = await ExpenseViewModel.PickPhotoForReceiptAsync();
            if (receipt == null)
            {
                ToastService.ShowWarning("The receipt could not be processed");
                _isLoading = false;
                return;
            }
            if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

            _name = receipt.Name;
            _cost = receipt.Total;
            _labels = receipt.Labels;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while picking a photo.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    async Task HandleTakePictureAsync()
    {
        try
        { 
            _isLoading = true;
            var receipt = await ExpenseViewModel.CapturePhotoForReceiptAsync();
            if (receipt == null)
            {
                ToastService.ShowWarning("The receipt could not be processed");
                _isLoading = false;
                return;
            }
            if (!string.IsNullOrWhiteSpace(receipt.MerchantName)) receipt.Labels.Add(receipt.MerchantName);

            _name = receipt.Name;
            _cost = receipt.Total;
            _labels = receipt.Labels;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while taking a picture.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    async Task HandleStartListeningAsync()
    {
        try
        {
            await ExpenseViewModel.StartListeningForExpenseAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while starting to listen.");
        }
    }

    async Task HandleStopListeningAsync()
    {
        _isLoading = true;
        try
        {
            var data = await ExpenseViewModel.StopListeningForExpenseAsync();
            if (data is null)
            {
                ToastService.ShowWarning("Expense could not be created from description.");
                _isLoading = false;
                return;
            }

            _name = data.Name;
            _cost = data.Price;
            _labels = data.Labels?.ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while processing speech-to-text.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    void HandleRecurringHelpPressed()
    {
        _isRecurringTooltipOpen = !_isRecurringTooltipOpen;
    }

    void HandleRecurringHelpClickedOutside()
    {
        if (_isRecurringTooltipOpen)
        {
            _isRecurringTooltipOpen = false;
            InvokeAsync(StateHasChanged);
        }
    }
}
