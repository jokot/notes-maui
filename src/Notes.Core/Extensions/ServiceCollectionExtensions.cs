namespace Notes.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string? dataPath = null)
    {
        // Logging - Required for all services and MediatR
        services.AddLogging();

        // MediatR Registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Database Configuration
        var dbPath = Path.Combine(dataPath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "notes.db");
        services.AddDbContext<NotesDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
            options.EnableSensitiveDataLogging(); // Only for development
        });

        // Core Services - Keep FileDataService for potential migration needs
        services.AddSingleton<IFileDataService>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<FileDataService>>();
            return new FileDataService(logger, dataPath);
        });

        // Database Services
        services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
        services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();

        // Repositories - Use SQLite repository as the primary implementation
        services.AddScoped<IRepository<Note>, SqliteNoteRepository>();

        return services;
    }
}
