using Microsoft.Extensions.DependencyInjection;

namespace Lab7_MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell())
		{
			Title = "Комунальні послуги",
			Width = 430,
			Height = 860
		};
	}
}