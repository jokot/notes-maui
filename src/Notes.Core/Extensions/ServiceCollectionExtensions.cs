namespace Notes.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string? dataPath = null)
    {
        // Logging - Required for all services and MediatR
        services.AddLogging();

        // MediatR Registration
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Core Services - Configure FileDataService with provided data path
        services.AddSingleton<IFileDataService>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<FileDataService>>();
            return new FileDataService(logger, dataPath);
        });

        // Repositories
        services.AddSingleton<IRepository<Note>, NoteRepository>();

        return services;
    }
}
