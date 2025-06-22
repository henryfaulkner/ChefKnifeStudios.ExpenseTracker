using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Numerics;
using System.Text.Json;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

internal class CommonJsInteropService(
    IJSRuntime jsRuntime, 
    ILogger<CommonJsInteropService> logger) : ICommonJsInteropService
{
    readonly IJSRuntime _jsRuntime = jsRuntime;
    readonly ILogger<CommonJsInteropService> _logger = logger;
    readonly Dictionary<Guid, Action> _clickOutsideCallbackDict = new();

    public async Task RegisterClickOutside(string elementId)
    {
        await _jsRuntime!.InvokeVoidAsync(
            "commonJsInterop.registerClickOutsideListener",
            elementId,
            DotNetObjectReference.Create(this)
        );
    }

    [JSInvokable]
    public void HandleClickOutside()
    {
        var callbacks = _clickOutsideCallbackDict.Values.ToArray();
        foreach (var callback in callbacks)
        {
            callback.Invoke();
        }
    }

    public void AddClickOutsideCallback(Action callback, Guid? key = null)
    {
        if (!key.HasValue) key = Guid.NewGuid();
        _clickOutsideCallbackDict.Add(key.Value, callback);
    }

    public void RemoveClickOutsideCallback(Guid key)
    {
        _clickOutsideCallbackDict.Remove(key);
    }
}
