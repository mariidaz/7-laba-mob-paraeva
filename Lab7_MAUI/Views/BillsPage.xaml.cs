using Lab7_MAUI.ViewModels;

namespace Lab7_MAUI.Views;

public partial class BillsPage : ContentPage
{
    private readonly BillsViewModel _viewModel;

    public BillsPage(BillsViewModel viewModel)
    {
        BindingContext = _viewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeBillsAsync();
    }
}
