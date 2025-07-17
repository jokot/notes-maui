namespace Notes.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Core Serivces
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDataService, LocalDataService>();
        services.AddSingleton<IFileDataService, FileDataService>();

        // Repositories
        services.AddSingleton<IRepository<Note>, NoteRepository>();

        // ViewModels
        services.AddTransient<AllNotesViewModel>();
        services.AddTransient<NoteViewModel>();
        services.AddTransient<AboutViewModel>();

        return services;
    }
}
