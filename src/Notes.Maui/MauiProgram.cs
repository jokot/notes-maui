namespace Notes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Core Services
		builder.Services.AddCoreServices();
		
		// Register MAUI-specific Services
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		
		// Register Views
		builder.Services.AddTransient<AllNotesPage>();
		builder.Services.AddTransient<NotePage>();
		builder.Services.AddTransient<AboutPage>();
		
		// Register ViewModels
		builder.Services.AddTransient<AboutViewModel>();
		builder.Services.AddTransient<AllNotesViewModel>();
		builder.Services.AddTransient<NoteViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
