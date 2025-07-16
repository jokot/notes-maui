# Notes App

A simple and efficient note-taking application built with .NET MAUI, featuring smart caching and modern MVVM architecture.

## Features

- ✅ **Create, Edit, and Delete Notes** - Full CRUD operations for managing your notes
- ✅ **Smart Caching** - Optimized performance with intelligent file system caching
- ✅ **Pull-to-Refresh** - Manually refresh your notes list when needed
- ✅ **Auto-Save** - Notes are automatically saved to local storage
- ✅ **Newest First** - Notes are sorted by date with newest entries at the top
- ✅ **Cross-Platform** - Runs on Android, iOS, Windows, and macOS

## Architecture

This project follows modern .NET MAUI best practices:

- **MVVM Pattern** with CommunityToolkit.Mvvm
- **Dependency Injection** for service management
- **Smart Caching Service** to minimize file I/O operations
- **Shell Navigation** for seamless page transitions
- **ObservableProperties** for automatic property change notifications

## Project Structure

```
Notes/
├── Models/
│   └── Note.cs                 # Note data model
├── Services/
│   └── NoteService.cs          # File operations and caching logic
├── ViewModels/
│   ├── BaseViewModel.cs        # Base class for ViewModels
│   ├── AllNotesViewModel.cs    # Main notes list ViewModel
│   └── NoteViewModel.cs        # Individual note editing ViewModel
├── Views/
│   ├── AllNotesPage.xaml       # Main notes list page
│   └── NotePage.xaml           # Note editing page
└── App.xaml                    # Application configuration
```

## Key Components

### NoteService
The heart of the application, providing:
- **Smart Caching**: Loads notes from disk only when necessary
- **Cache Timeout**: Automatically refreshes cache after 5 minutes
- **Instant Updates**: Cache is updated immediately on save/delete operations
- **Performance Optimization**: Avoids unnecessary file I/O operations

### ViewModels
- **AllNotesViewModel**: Manages the notes list, refresh operations, and navigation
- **NoteViewModel**: Handles individual note creation and editing
- Uses `[RelayCommand]` and `[ObservableProperty]` attributes for clean, maintainable code

### Navigation
- Utilizes Shell navigation with query parameters
- Passes note objects between pages using `QueryProperty` attributes
- Supports both new note creation and existing note editing workflows

## Getting Started

### Prerequisites
- .NET 8.0 or later
- Visual Studio 2022 or Visual Studio Code
- Appropriate platform SDKs (Android SDK, Xcode for iOS, etc.)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/jokot/notes-maui.git
cd notes-maui
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build and run:
```bash
dotnet build
dotnet run
```

### Dependencies

- **Microsoft.Extensions.Logging.Debug** - Debug logging support
- **CommunityToolkit.Mvvm** - MVVM helpers and source generators

## Usage

1. **View Notes**: Launch the app to see your existing notes
2. **Add Note**: Tap the "+" button to create a new note
3. **Edit Note**: Tap any existing note to edit it
4. **Delete Note**: Use the delete button within a note
5. **Refresh**: Pull down on the notes list to manually refresh

## Performance Features

### Smart Caching System
- **First Load**: Reads all notes from disk and caches them
- **Subsequent Loads**: Returns cached data instantly
- **Save Operations**: Updates both file and cache simultaneously
- **Delete Operations**: Removes from both file system and cache
- **Cache Expiry**: Automatically refreshes after 5 minutes

### I/O Optimization
The app minimizes disk operations by:
- Loading files only on first access or cache expiry
- Updating cache immediately on modifications
- Using efficient file naming with timestamps
- Avoiding unnecessary re-reads of unchanged data

## Data Storage

Notes are stored as individual text files in the application's local data directory:
- **Format**: `.notes.txt` files
- **Location**: `FileSystem.AppDataDirectory`
- **Naming**: Random filename with timestamp-based sorting
- **Content**: Plain text with metadata (filename, content, date)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [.NET MAUI](https://dotnet.microsoft.com/apps/maui)
- Uses [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM implementation
- Inspired by modern mobile app development best practices
