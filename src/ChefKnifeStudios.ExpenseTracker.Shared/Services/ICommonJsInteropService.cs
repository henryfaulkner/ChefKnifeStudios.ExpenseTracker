using System.Numerics;

namespace ChefKnifeStudios.ExpenseTracker.Shared.Services;

public interface ICommonJsInteropService
{

    Task RegisterClickOutside(string elementId);
    void AddClickOusideCallback(Action callback, Guid? key = null);
    void RemoveClickOutsideCallback(Guid key);
}
