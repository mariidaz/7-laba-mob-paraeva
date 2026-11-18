using Lab7_MAUI.ViewModels;

namespace Lab7_MAUI.Views;

public partial class ServicesPage : ContentPage
{
    private readonly ServicesViewModel _viewModel;

    public ServicesPage(ServicesViewModel viewModel)
    {
        BindingContext = _viewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadServicesAsync();
    }
}
