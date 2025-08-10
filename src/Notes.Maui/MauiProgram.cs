using Notes.Core.Services.Data;

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

		// Register Core Services with platform-specific data directory
		builder.Services.AddCoreServices(FileSystem.AppDataDirectory);
		
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

		var app = builder.Build();
		
		// Initialize database and run migration
		Task.Run(async () =>
		{
			using var scope = app.Services.CreateScope();
			
			// Initialize database first
			var dbInitService = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
			await dbInitService.InitializeDatabaseAsync();
			
			// Run migration from files to database
			var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
			var migratedCount = await migrationService.MigrateFromFilesToDatabaseAsync();
			
			// Log migration result (only visible in debug output)
			Console.WriteLine($"Database migration completed. Migrated {migratedCount} notes from files to database.");
		});

		return app;
	}
}
