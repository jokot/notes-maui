namespace Notes.Core.Constants;

public class AppConstants
{

    public static class SecureStorage
    {
        public const string DatabaseName = "notes.db";
        public const string SecureStorageKey = "notes_secure_storage_key";
    }

    public static class Navigation
    {
        public const string AllNotesPage = nameof(AllNotesPage);
        public const string NotePage = nameof(NotePage);
        public const string AboutPage = nameof(AboutPage);
    }
    public static class Cache
    {
        public const int DefaultTimeoutMinutes = 5;
    }
}
