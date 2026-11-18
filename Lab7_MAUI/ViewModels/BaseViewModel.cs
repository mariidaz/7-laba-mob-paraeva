using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lab7_MAUI.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [RelayCommand]
    public async Task NavigateToSectionAsync(string section)
    {
        switch (section)
        {
            case "Home":
            case "Dashboard":
                await Shell.Current.GoToAsync("//DashboardPage");
                break;
            case "Tenants":
                await Shell.Current.GoToAsync("//TenantsPage");
                break;
            case "Apartments":
                await Shell.Current.GoToAsync("//ApartmentsPage");
                break;
            case "Services":
                await Shell.Current.GoToAsync("//ServicesPage");
                break;
            case "Bills":
                await Shell.Current.GoToAsync("//BillsPage");
                break;
        }
    }

    protected Page? GetCurrentPage()
    {
        return Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
    }

    protected async Task ShowAlertAsync(string title, string message)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert(title, message, "OK");
            return;
        }

        var page = GetCurrentPage();
        if (page != null)
        {
            await page.DisplayAlert(title, message, "OK");
        }
    }
}
