using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab7_MAUI.Models;
using Lab7_MAUI.Services;

namespace Lab7_MAUI.ViewModels;

public partial class ServicesViewModel : BaseViewModel
{
    private readonly IUtilityDataService _dataService;

    public ObservableCollection<Service> Services { get; } = new();

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
    private Service? _selectedService;

    partial void OnSearchTextChanged(string value)
    {
        _ = RefreshServicesDataAsync();
    }

    // Form inputs
    [ObservableProperty]
    private string _inputServiceName = string.Empty;

    [ObservableProperty]
    private string _inputPricePerSquareMeter = string.Empty;

    [ObservableProperty]
    private string _inputPricePerPerson = string.Empty;

    [ObservableProperty]
    private string _inputCalculationType = "за площею";

    public ServicesViewModel(IUtilityDataService dataService)
    {
        _dataService = dataService;
    }

    private async Task RefreshServicesDataAsync()
    {
        var list = await _dataService.GetServicesAsync(SearchText);
        Services.Clear();
        foreach (var item in list)
        {
            Services.Add(item);
        }
    }

    [RelayCommand]
    public async Task LoadServicesAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await RefreshServicesDataAsync();
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
        InputServiceName = string.Empty;
        InputPricePerSquareMeter = "10.00";
        InputPricePerPerson = string.Empty;
        InputCalculationType = "за площею";
        CurrentViewMode = "Create";
    }

    [RelayCommand]
    public void ShowEdit(Service service)
    {
        if (service == null) return;
        SelectedService = service;
        InputServiceName = service.ServiceName;
        InputPricePerSquareMeter = service.PricePerSquareMeter?.ToString("0.00") ?? string.Empty;
        InputPricePerPerson = service.PricePerPerson?.ToString("0.00") ?? string.Empty;
        InputCalculationType = service.CalculationType;
        CurrentViewMode = "Edit";
    }

    [RelayCommand]
    public void ShowDetails(Service service)
    {
        if (service == null) return;
        SelectedService = service;
        CurrentViewMode = "Details";
    }

    [RelayCommand]
    public void ShowDelete(Service service)
    {
        if (service == null) return;
        SelectedService = service;
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
        if (string.IsNullOrWhiteSpace(InputServiceName))
        {
            await ShowAlertAsync("Validation", "Назва послуги є обов'язковою.");
            return;
        }

        decimal? priceSqM = null;
        if (decimal.TryParse(InputPricePerSquareMeter.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal psm) && psm > 0)
        {
            priceSqM = psm;
        }

        decimal? pricePerson = null;
        if (decimal.TryParse(InputPricePerPerson.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal pp) && pp > 0)
        {
            pricePerson = pp;
        }

        try
        {
            IsBusy = true;
            var newService = new Service
            {
                ServiceName = InputServiceName.Trim(),
                PricePerSquareMeter = priceSqM,
                PricePerPerson = pricePerson,
                CalculationType = !string.IsNullOrWhiteSpace(InputCalculationType) ? InputCalculationType : (priceSqM.HasValue ? "за площею" : "за кількістю мешканців")
            };

            await _dataService.AddServiceAsync(newService);
            await RefreshServicesDataAsync();

            InputServiceName = string.Empty;
            InputPricePerSquareMeter = string.Empty;
            InputPricePerPerson = string.Empty;
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
        if (SelectedService == null) return;
        if (string.IsNullOrWhiteSpace(InputServiceName))
        {
            await ShowAlertAsync("Validation", "Назва послуги є обов'язковою.");
            return;
        }

        decimal? priceSqM = null;
        if (decimal.TryParse(InputPricePerSquareMeter.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal psm) && psm > 0)
        {
            priceSqM = psm;
        }

        decimal? pricePerson = null;
        if (decimal.TryParse(InputPricePerPerson.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out decimal pp) && pp > 0)
        {
            pricePerson = pp;
        }

        try
        {
            IsBusy = true;
            SelectedService.ServiceName = InputServiceName.Trim();
            SelectedService.PricePerSquareMeter = priceSqM;
            SelectedService.PricePerPerson = pricePerson;
            SelectedService.CalculationType = !string.IsNullOrWhiteSpace(InputCalculationType) ? InputCalculationType : (priceSqM.HasValue ? "за площею" : "за кількістю мешканців");

            await _dataService.UpdateServiceAsync(SelectedService);
            await RefreshServicesDataAsync();
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
        if (SelectedService == null) return;

        try
        {
            IsBusy = true;
            await _dataService.DeleteServiceAsync(SelectedService.ServiceId);
            await RefreshServicesDataAsync();
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
}
