using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab7_MAUI.Models;
using Lab7_MAUI.Services;

namespace Lab7_MAUI.ViewModels;

public partial class TenantsViewModel : BaseViewModel
{
    private readonly IUtilityDataService _dataService;

    public ObservableCollection<Tenant> Tenants { get; } = new();

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
    private Tenant? _selectedTenant;

    partial void OnSearchTextChanged(string value)
    {
        _ = RefreshTenantsDataAsync();
    }

    // Form inputs
    [ObservableProperty]
    private string _inputLastName = string.Empty;

    [ObservableProperty]
    private string _inputFirstName = string.Empty;

    [ObservableProperty]
    private string _inputMiddleName = string.Empty;

    public TenantsViewModel(IUtilityDataService dataService)
    {
        _dataService = dataService;
    }

    private async Task RefreshTenantsDataAsync()
    {
        var list = await _dataService.GetTenantsAsync(SearchText);
        Tenants.Clear();
        foreach (var item in list)
        {
            Tenants.Add(item);
        }
    }

    [RelayCommand]
    public async Task LoadTenantsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await RefreshTenantsDataAsync();
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
    public void ShowCreate()
    {
        InputLastName = string.Empty;
        InputFirstName = string.Empty;
        InputMiddleName = string.Empty;
        CurrentViewMode = "Create";
    }

    [RelayCommand]
    public void ShowEdit(Tenant tenant)
    {
        if (tenant == null) return;
        SelectedTenant = tenant;
        InputLastName = tenant.LastName;
        InputFirstName = tenant.FirstName;
        InputMiddleName = tenant.MiddleName;
        CurrentViewMode = "Edit";
    }

    [RelayCommand]
    public void ShowDetails(Tenant tenant)
    {
        if (tenant == null) return;
        SelectedTenant = tenant;
        CurrentViewMode = "Details";
    }

    [RelayCommand]
    public void ShowDelete(Tenant tenant)
    {
        if (tenant == null) return;
        SelectedTenant = tenant;
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
        if (string.IsNullOrWhiteSpace(InputLastName) || string.IsNullOrWhiteSpace(InputFirstName))
        {
            await ShowAlertAsync("Validation", "Прізвище та ім'я є обов'язковими для заповнення.");
            return;
        }

        try
        {
            IsBusy = true;
            var newTenant = new Tenant
            {
                LastName = InputLastName.Trim(),
                FirstName = InputFirstName.Trim(),
                MiddleName = InputMiddleName.Trim()
            };

            await _dataService.AddTenantAsync(newTenant);
            await RefreshTenantsDataAsync();

            InputLastName = string.Empty;
            InputFirstName = string.Empty;
            InputMiddleName = string.Empty;
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
        if (SelectedTenant == null) return;
        if (string.IsNullOrWhiteSpace(InputLastName) || string.IsNullOrWhiteSpace(InputFirstName))
        {
            await ShowAlertAsync("Validation", "Прізвище та ім'я є обов'язковими для заповнення.");
            return;
        }

        try
        {
            IsBusy = true;
            SelectedTenant.LastName = InputLastName.Trim();
            SelectedTenant.FirstName = InputFirstName.Trim();
            SelectedTenant.MiddleName = InputMiddleName.Trim();

            await _dataService.UpdateTenantAsync(SelectedTenant);
            await RefreshTenantsDataAsync();
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
        if (SelectedTenant == null) return;

        try
        {
            IsBusy = true;
            await _dataService.DeleteTenantAsync(SelectedTenant.TenantId);
            await RefreshTenantsDataAsync();
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
    public async Task ManageServicesAsync(Tenant tenant)
    {
        if (tenant == null) return;
        var page = GetCurrentPage();
        if (page == null) return;

        var allServices = await _dataService.GetServicesAsync();
        var selectedIds = await _dataService.GetSelectedServiceIdsForTenantAsync(tenant.TenantId);

        var serviceNames = allServices.Select(s => 
            $"{(selectedIds.Contains(s.ServiceId) ? "✓ [Підключено] " : "○ [Відключено] ")}{s.ServiceName} ({s.TariffDisplay})"
        ).ToArray();

        string action = await page.DisplayActionSheet(
            $"Послуги для {tenant.FullName}",
            "Закрити",
            null,
            serviceNames);

        if (action == null || action == "Закрити") return;

        for (int i = 0; i < serviceNames.Length; i++)
        {
            if (serviceNames[i] == action)
            {
                var s = allServices[i];
                if (selectedIds.Contains(s.ServiceId))
                {
                    selectedIds.Remove(s.ServiceId);
                }
                else
                {
                    selectedIds.Add(s.ServiceId);
                }

                await _dataService.SaveTenantServicesAsync(tenant.TenantId, selectedIds);
                await ShowAlertAsync("Успіх", $"Статус послуги '{s.ServiceName}' змінено.");
                break;
            }
        }
    }
}
