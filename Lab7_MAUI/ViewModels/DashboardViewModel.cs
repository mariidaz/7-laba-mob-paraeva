using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab7_MAUI.Models;
using Lab7_MAUI.Services;

namespace Lab7_MAUI.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IUtilityDataService _dataService;

    [ObservableProperty]
    private int _totalTenants;

    [ObservableProperty]
    private int _totalApartments;

    [ObservableProperty]
    private int _totalServices;

    [ObservableProperty]
    private decimal _totalMonthlyRevenue;

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(IUtilityDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await _dataService.InitializeAsync();

            var stats = await _dataService.GetStatisticsAsync();
            TotalTenants = stats.TotalTenants;
            TotalApartments = stats.TotalApartments;
            TotalServices = stats.TotalServices;
            TotalMonthlyRevenue = stats.TotalEstimatedMonthlyRevenue;
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Помилка", $"Не вдалося завантажити дані: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
