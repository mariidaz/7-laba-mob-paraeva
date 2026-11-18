using Lab7_MAUI.ViewModels;

namespace Lab7_MAUI.Views;

public partial class ApartmentsPage : ContentPage
{
    private readonly ApartmentsViewModel _viewModel;

    public ApartmentsPage(ApartmentsViewModel viewModel)
    {
        BindingContext = _viewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadApartmentsAsync();
    }
}
