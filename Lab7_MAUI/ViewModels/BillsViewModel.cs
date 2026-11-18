using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lab7_MAUI.Models;
using Lab7_MAUI.Services;

namespace Lab7_MAUI.ViewModels;

public partial class BillsViewModel : BaseViewModel
{
    private readonly IUtilityDataService _dataService;

    [ObservableProperty]
    private string _searchAccount = string.Empty;

    [ObservableProperty]
    private BillViewModel? _currentBill;

    [ObservableProperty]
    private bool _hasBill;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _emailInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotification))]
    private string? _notificationMessage;

    public bool HasNotification => !string.IsNullOrEmpty(NotificationMessage);

    [ObservableProperty]
    private bool _isNotificationSuccess;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<string> AvailableAccounts { get; } = new();

    public BillsViewModel(IUtilityDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    public async Task InitializeBillsAsync()
    {
        try
        {
            var accounts = await _dataService.GetAvailableAccountNumbersAsync();
            AvailableAccounts.Clear();
            foreach (var acc in accounts)
            {
                AvailableAccounts.Add(acc);
            }

            string? navAcc = Preferences.Get("SelectedAccountNumber", null);
            if (!string.IsNullOrEmpty(navAcc))
            {
                Preferences.Remove("SelectedAccountNumber");
                SearchAccount = navAcc;
                await CalculateBillAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Помилка завантаження рахунків: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task CalculateBillAsync()
    {
        ErrorMessage = null;
        NotificationMessage = null;

        if (string.IsNullOrWhiteSpace(SearchAccount))
        {
            ErrorMessage = "Введіть номер особового рахунку для пошуку.";
            return;
        }

        try
        {
            IsBusy = true;
            var bill = await _dataService.GetBillByAccountNumberAsync(SearchAccount.Trim());
            if (bill == null)
            {
                HasBill = false;
                CurrentBill = null;
                ErrorMessage = $"Квартиру за особовим рахунком '{SearchAccount}' не знайдено.";
                return;
            }

            CurrentBill = bill;
            HasBill = true;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Помилка формування нарахувань: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelectSampleAccountAsync(string acc)
    {
        if (string.IsNullOrEmpty(acc)) return;
        SearchAccount = acc;
        await CalculateBillAsync();
    }

    [RelayCommand]
    public void BackToSearch()
    {
        HasBill = false;
        NotificationMessage = null;
        ErrorMessage = null;
    }

    [RelayCommand]
    public async Task SendNotificationAsync()
    {
        if (CurrentBill == null) return;

        var email = EmailInput?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            NotificationMessage = "Вкажіть адресу електронної пошти для відправки.";
            IsNotificationSuccess = false;
            return;
        }

        NotificationMessage = $"Рахунок за особовим рахунком {CurrentBill.AccountNumber} успішно надіслано на {email}.";
        IsNotificationSuccess = true;
    }

    [RelayCommand]
    public async Task PrintReceiptAsync()
    {
        if (CurrentBill == null) return;

        await ShowAlertAsync(
            "Друк квитанції",
            $"Повідомлення на оплату житлово-комунальних послуг\n" +
            $"Особовий рахунок: {CurrentBill.AccountNumber}\n" +
            $"ПІБ: {CurrentBill.TenantFullName}\n" +
            $"Адреса: {CurrentBill.Address}\n" +
            $"Разом до сплати: {CurrentBill.FormattedGrandTotal}");
    }
}
