namespace Notes.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<IFileDataService, FileDataService>();

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
