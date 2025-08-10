# Notes App - Enterprise Edition

A sophisticated note-taking application built with .NET MAUI, featuring enterprise-level MVVM architecture with clean separation of concerns, dependency injection, and scalable patterns.

## Features

- ✅ **Create, Edit, and Delete Notes** - Full CRUD operations for managing your notes
- ✅ **Enterprise Architecture** - Clean separation of concerns with proper layering
- ✅ **Dependency Injection** - Proper service registration and lifecycle management
- ✅ **Repository Pattern** - Abstracted data access with caching strategies
- ✅ **Service Layer** - Dedicated services for file operations and navigation
- ✅ **Error Handling** - Centralized error management with proper logging
- ✅ **Cross-Platform** - Runs on Android, iOS, Windows, and macOS

## Architecture

This project follows enterprise .NET MAUI best practices:

- **MVVM Pattern** with CommunityToolkit.Mvvm
- **Clean Architecture** with proper layer separation
- **Dependency Injection** for service management
- **Repository Pattern** for data access abstraction
- **Service Layer** for business logic separation
- **Interface Segregation** for testability and flexibility
- **Error Handling** with centralized exception management

## Project Structure

```
Notes/
├── Core/
│   ├── Interfaces/
│   │   ├── IRepository.cs           # Generic repository interface
│   │   ├── IDataService.cs          # Generic data service interface
│   │   ├── IFileDataService.cs      # File operations interface
│   │   └── INavigationService.cs    # Navigation service interface
│   ├── Models/
│   │   ├── BaseEntity.cs            # Base entity with common properties
│   │   └── Note.cs                  # Note data model
│   ├── Services/
│   │   ├── Repository/
│   │   │   ├── BaseRepository.cs    # Base repository implementation
│   │   │   └── NoteRepository.cs    # Note-specific repository
│   │   ├── Data/
│   │   │   ├── LocalDataService.cs  # Secure storage service
│   │   │   └── FileDataService.cs   # File operations service
│   │   └── Navigation/
│   │       └── NavigationService.cs # Navigation abstraction
│   └── ViewModels/
│       ├── Base/
│       │   └── BaseViewModel.cs     # Base ViewModel with common patterns
│       └── Features/
│           ├── Notes/
│           │   ├── AllNotesViewModel.cs
│           │   └── NoteViewModel.cs
│           └── About/
│               └── AboutViewModel.cs
├── Shared/
│   ├── Constants/
│   │   └── AppConstants.cs          # Application constants
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs # DI configuration
├── Views/
│   └── Features/
│       ├── Notes/
│       │   ├── AllNotesPage.xaml
│       │   └── NotePage.xaml
│       └── About/
│           └── AboutPage.xaml
└── App.xaml                         # Application configuration
```

## Key Components

### Core Architecture
- **Interfaces**: Define contracts for all services and repositories
- **Models**: Domain entities with proper inheritance
- **Services**: Business logic and data access abstraction
- **ViewModels**: UI logic with proper dependency injection

### Repository Pattern
The `NoteRepository` provides:
- **Data Abstraction**: Hides file system implementation details
- **Caching Strategy**: Intelligent caching with timeout management
- **CRUD Operations**: Standard repository pattern implementation
- **Error Handling**: Proper exception management and logging

### Service Layer
- **FileDataService**: Handles raw file operations with error handling
- **LocalDataService**: Manages secure storage for app settings
- **NavigationService**: Abstracts Shell navigation logic

### ViewModels
- **BaseViewModel**: Common patterns for loading states and error handling
- **Feature ViewModels**: Domain-specific UI logic with proper DI
- **Command Pattern**: Uses `[RelayCommand]` for clean command implementation

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

- **CommunityToolkit.Mvvm** - MVVM helpers and source generators
- **Microsoft.Extensions.Logging.Debug** - Debug logging support
- **Microsoft.Extensions.DependencyInjection** - Dependency injection container

## Usage

1. **View Notes**: Launch the app to see your existing notes
2. **Add Note**: Tap the "+" button to create a new note
3. **Edit Note**: Tap any existing note to edit it
4. **Delete Note**: Use the delete button within a note
5. **Refresh**: Pull down on the notes list to manually refresh

## Enterprise Features

### Dependency Injection
- **Service Registration**: Centralized DI configuration
- **Lifecycle Management**: Proper singleton and transient registrations
- **Interface Segregation**: Services depend on interfaces, not implementations

### Repository Pattern
- **Data Abstraction**: Repository hides data source implementation
- **Caching Strategy**: Intelligent caching with configurable timeouts
- **Error Handling**: Centralized exception management

### Service Layer
- **Separation of Concerns**: Each service has a single responsibility
- **Testability**: Services can be easily mocked for unit testing
- **Flexibility**: Easy to swap implementations (file → database)

### Error Handling
- **Centralized Management**: Base ViewModel handles common errors
- **User-Friendly Messages**: Proper error messages for different scenarios
- **Logging**: Structured logging for debugging and monitoring

## Performance Features

### Smart Caching System
- **Configurable Timeout**: Cache expires after configurable time
- **Lazy Loading**: Data loaded only when needed
- **Cache Invalidation**: Automatic cache refresh on data changes
- **Memory Management**: Efficient memory usage with proper cleanup

### I/O Optimization
The app minimizes disk operations by:
- **Caching Strategy**: Reduces file system access
- **Batch Operations**: Efficient file operations
- **Error Recovery**: Graceful handling of file system errors

## Data Storage

Notes are stored as individual text files in the application's local data directory:
- **Format**: `.notes.txt` files
- **Location**: `FileSystem.AppDataDirectory`
- **Naming**: Random filename with timestamp-based sorting
- **Content**: Plain text with metadata (filename, content, date)

## Architecture Benefits

### For Learning
- **Clean Code**: Easy to understand and maintain
- **Best Practices**: Follows enterprise patterns
- **Scalability**: Easy to add new features
- **Testability**: Designed for unit testing

### For Enterprise
- **Maintainability**: Clear separation of concerns
- **Extensibility**: Easy to add new services and repositories
- **Reliability**: Proper error handling and logging
- **Performance**: Optimized caching and I/O operations

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
- Follows enterprise architecture patterns for scalable applications
