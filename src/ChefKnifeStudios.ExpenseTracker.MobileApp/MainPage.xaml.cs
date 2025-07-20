using CommunityToolkit.Maui.Core;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public void SetCameraActive(bool isActive)
    {
        if (StatusBarBehavior is not null)
        {
            StatusBarBehavior.StatusBarStyle = isActive ? StatusBarStyle.DarkContent : StatusBarStyle.LightContent;
            StatusBarBehavior.StatusBarColor = isActive ? Colors.Black : Color.FromArgb("#e6e1e3");
        }
    }
}
