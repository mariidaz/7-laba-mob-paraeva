using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab7_MAUI.Models;
using Lab7_MAUI.Services;

namespace Lab7_MAUI.ViewModels;

public partial class ApartmentsViewModel : BaseViewModel
{
    private readonly IUtilityDataService _dataService;

    public ObservableCollection<Apartment> Apartments { get; } = new();
    public ObservableCollection<Tenant> AvailableTenants { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListView))]
    [NotifyPropertyChangedFor(nameof(IsCreateView))]
    [NotifyPropertyChangedFor(nameof(IsEditView))]
    [NotifyPropertyChangedFor(nameof(IsDetailsView))]
    [NotifyPropertyChangedFor(nameof(IsDeleteView))]
    private string _currentViewMode = "List"; // "List", "Create", "Edit", "Details", "Delete"

    public bool IsListView => CurrentViewMode == "List";
    public bool IsCreateView => CurrentViewMode == "Create";
    public bool IsEditView => CurrentViewMode == "Edit";
    public bool IsDetailsView => CurrentViewMode == "Details";
    public bool IsDeleteView => CurrentViewMode == "Delete";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Apartment? _selectedApartment;

    partial void OnSearchTextChanged(string value)
    {
        _ = RefreshApartmentsDataAsync();
    }

    // Form inputs
    [ObservableProperty]
    private Tenant? _selectedTenantForApartment;

    [ObservableProperty]
    private string _inputAddress = string.Empty;

    [ObservableProperty]
    private string _inputAccountNumber = string.Empty;

    [ObservableProperty]
    private string _inputResidentsCount = "1";

    [ObservableProperty]
    private string _inputArea = "50.0";

    public ApartmentsViewModel(IUtilityDataService dataService)
    {
        _dataService = dataService;
    }

    private async Task RefreshApartmentsDataAsync()
    {
        var list = await _dataService.GetApartmentsAsync(SearchText);
        Apartments.Clear();
        foreach (var item in list)
        {
            Apartments.Add(item);
        }

        var tenants = await _dataService.GetTenantsAsync();
        AvailableTenants.Clear();
        foreach (var t in tenants)
        {
            AvailableTenants.Add(t);
        }
    }

    [RelayCommand]
    public async Task LoadApartmentsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await RefreshApartmentsDataAsync();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Помилка завантаження: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ShowCreateAsync()
    {
        await RefreshApartmentsDataAsync();
        InputAddress = string.Empty;
        InputAccountNumber = string.Empty;
        InputResidentsCount = "1";
        InputArea = "50.0";
        SelectedTenantForApartment = AvailableTenants.FirstOrDefault();
        CurrentViewMode = "Create";
    }

    [RelayCommand]
    public async Task ShowEditAsync(Apartment apartment)
    {
        if (apartment == null) return;
        await RefreshApartmentsDataAsync();
        SelectedApartment = apartment;
        InputAddress = apartment.Address;
        InputAccountNumber = apartment.AccountNumber;
        InputResidentsCount = apartment.ResidentsCount.ToString();
        InputArea = apartment.Area.ToString("0.00");
        SelectedTenantForApartment = AvailableTenants.FirstOrDefault(t => t.TenantId == apartment.TenantId);
        CurrentViewMode = "Edit";
    }

    [RelayCommand]
    public void ShowDetails(Apartment apartment)
    {
        if (apartment == null) return;
        SelectedApartment = apartment;
        CurrentViewMode = "Details";
    }

    [RelayCommand]
    public void ShowDelete(Apartment apartment)
    {
        if (apartment == null) return;
        SelectedApartment = apartment;
        CurrentViewMode = "Delete";
    }

    [RelayCommand]
    public void BackToList()
    {
        CurrentViewMode = "List";
    }

    [RelayCommand]
    public async Task SaveCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(InputAddress) || string.IsNullOrWhiteSpace(InputAccountNumber))
        {
            await ShowAlertAsync("Validation", "Адреса та особовий рахунок є обов'язковими.");
            return;
        }

        if (SelectedTenantForApartment == null)
        {
            await ShowAlertAsync("Validation", "Оберіть власника квартири (TenantId).");
            return;
        }

        int.TryParse(InputResidentsCount, out int residents);
        if (residents <= 0) residents = 1;

        decimal.TryParse(InputArea.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal area);
        if (area <= 0) area = 50.0m;

        try
        {
            IsBusy = true;
            var newApt = new Apartment
            {
                TenantId = SelectedTenantForApartment.TenantId,
                Address = InputAddress.Trim(),
                AccountNumber = InputAccountNumber.Trim(),
                ResidentsCount = residents,
                Area = area
            };

            await _dataService.AddApartmentAsync(newApt);
            await RefreshApartmentsDataAsync();

            InputAddress = string.Empty;
            InputAccountNumber = string.Empty;
            CurrentViewMode = "List";
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Помилка збереження: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveEditAsync()
    {
        if (SelectedApartment == null) return;
        if (string.IsNullOrWhiteSpace(InputAddress) || string.IsNullOrWhiteSpace(InputAccountNumber))
        {
            await ShowAlertAsync("Validation", "Адреса та особовий рахунок є обов'язковими.");
            return;
        }

        int.TryParse(InputResidentsCount, out int residents);
        if (residents <= 0) residents = SelectedApartment.ResidentsCount;

        decimal.TryParse(InputArea.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal area);
        if (area <= 0) area = SelectedApartment.Area;

        try
        {
            IsBusy = true;
            if (SelectedTenantForApartment != null)
            {
                SelectedApartment.TenantId = SelectedTenantForApartment.TenantId;
            }
            SelectedApartment.Address = InputAddress.Trim();
            SelectedApartment.AccountNumber = InputAccountNumber.Trim();
            SelectedApartment.ResidentsCount = residents;
            SelectedApartment.Area = area;

            await _dataService.UpdateApartmentAsync(SelectedApartment);
            await RefreshApartmentsDataAsync();
            CurrentViewMode = "List";
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Помилка оновлення: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ConfirmDeleteAsync()
    {
        if (SelectedApartment == null) return;

        try
        {
            IsBusy = true;
            await _dataService.DeleteApartmentAsync(SelectedApartment.ApartmentId);
            await RefreshApartmentsDataAsync();
            CurrentViewMode = "List";
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Помилка видалення: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ViewBillAsync(Apartment apartment)
    {
        if (apartment == null) return;
        Preferences.Set("SelectedAccountNumber", apartment.AccountNumber);
        await Shell.Current.GoToAsync("//BillsPage");
    }
}
