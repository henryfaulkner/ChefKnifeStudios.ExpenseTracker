using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChefKnifeStudios.ExpenseTracker.Shared.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
}

public class BaseViewModel : IViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
 
    protected void SetValue<T>(ref T backingFiled, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingFiled, value)) return;
        backingFiled = value;
        OnPropertyChanged(propertyName);
    }
}
