namespace Notes.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string? dataPath = null)
    {
        // Core Services - Configure FileDataService with provided data path
        services.AddSingleton<IFileDataService>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<FileDataService>>();
            return new FileDataService(logger, dataPath);
        });

        // Repositories
        services.AddSingleton<IRepository<Note>, NoteRepository>();

        // CQRS Handlers
        services.AddSingleton<SaveNoteHandler>();
        services.AddSingleton<DeleteNoteHandler>();
        services.AddSingleton<GetAllNotesHandler>();
        services.AddSingleton<RefreshNotesHandler>();

        return services;
    }
}
