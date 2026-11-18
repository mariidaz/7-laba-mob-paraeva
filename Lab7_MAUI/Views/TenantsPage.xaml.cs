using Lab7_MAUI.ViewModels;

namespace Lab7_MAUI.Views;

public partial class TenantsPage : ContentPage
{
    private readonly TenantsViewModel _viewModel;

    public TenantsPage(TenantsViewModel viewModel)
    {
        BindingContext = _viewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTenantsAsync();
    }
}
