# Notes MAUI App

A cross-platform note-taking application built with .NET MAUI, demonstrating modern mobile development patterns and MVVM architecture with smart caching.

## 📱 Project Overview

This project serves as a learning foundation for building enterprise-grade mobile applications, specifically preparing for POS (Point of Sale) system development. It showcases best practices in mobile app architecture, data management, and testing strategies.

## 🏗️ Project Structure

```
Notes/
├── src/                           # Main application source code
│   ├── Notes.Core/                # Business logic and core functionality
│   │   ├── Commands/              # CQRS command definitions
│   │   ├── Constants/             # Application constants
│   │   ├── Data/                  # Entity Framework DbContext and data layer
│   │   ├── Extensions/            # Service collection extensions
│   │   ├── Handlers/              # MediatR command and query handlers
│   │   ├── Interfaces/            # Contracts and abstractions
│   │   ├── Models/                # Data models and entities
│   │   ├── Queries/               # CQRS query definitions
│   │   └── Services/              # Business services and data access
│   └── Notes.Maui/                # MAUI application (UI layer)
│       ├── App.xaml               # Application entry point
│       ├── AppShell.xaml          # Shell navigation structure
│       ├── Platforms/             # Platform-specific implementations
│       ├── Resources/             # Images, fonts, styles
│       ├── Services/              # UI-specific services
│       ├── ViewModels/            # MVVM ViewModels
│       └── Views/                 # XAML pages and UI components
├── tests/                         # Comprehensive testing suite
│   ├── Notes.Core.Tests/          # Unit tests for business logic
│   ├── Notes.Integration.Tests/   # Integration tests for services
│   ├── Notes.UI.Tests/            # UI automation tests with Appium
│   └── README.md                  # Testing documentation
├── README.md                      # This file
├── LICENSE                        # MIT license
└── Notes.sln                      # Solution file
```

## ✨ Key Features

### Smart Caching System
- **Performance Optimization**: Intelligent caching with 5-minute timeout reduces unnecessary I/O operations
- **Real-time Updates**: Cache automatically refreshes on save/delete operations
- **Force Refresh**: Manual cache bypass option for guaranteed fresh data

### Modern Architecture
- **Clean Architecture**: Separation of concerns with Notes.Core and Notes.Maui projects
- **CQRS Pattern**: Command Query Responsibility Segregation for better organization
- **MVVM Pattern**: Clean separation of concerns with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for loose coupling
- **Shell Navigation**: Efficient page navigation with parameter passing
- **Async/Await**: Proper asynchronous programming throughout

### Cross-Platform Support
- **Android**: API 21+ (Android 5.0+)
- **iOS**: iOS 15.0+
- **macOS**: macCatalyst 15.0+
- **Windows**: Windows 10 version 1903 (build 10.0.17763.0)+
- **Responsive UI**: Adaptive layouts for different screen sizes

## 🛠️ Technical Stack

### Core Framework
- **.NET 9**: Latest framework version
- **.NET MAUI**: Cross-platform UI framework
- **CommunityToolkit.Mvvm**: MVVM framework with source generators

### Data & Persistence
- **Entity Framework Core**: Object-relational mapping (ORM)
- **SQLite**: Lightweight, embedded database
- **System.Text.Json**: High-performance JSON serialization

### Architecture & Patterns
- **Clean Architecture**: Separation of business logic and UI concerns
- **CQRS Pattern**: Command Query Responsibility Segregation with MediatR
- **MediatR**: Mediator pattern implementation for CQRS
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

### Testing
- **xUnit**: Unit testing framework with comprehensive test coverage
- **FluentAssertions**: Expressive test assertions
- **Moq**: Mocking framework for unit tests
- **Appium**: UI automation testing for cross-platform scenarios

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 (17.8+) or Visual Studio Code with C# extension
- .NET 9 SDK
- Platform-specific SDKs:
  - Android SDK (API 21+)
  - Xcode 15+ (for iOS/macOS development)
  - Windows SDK 10.0.17763.0+ (for Windows development)

### Building the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Notes
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run on specific platform**
   ```bash
   # Android
   dotnet build src/Notes.Maui/Notes.csproj -t:Run -f net9.0-android
   
   # iOS Simulator
   dotnet build src/Notes.Maui/Notes.csproj -t:Run -f net9.0-ios
   
   # macOS
   dotnet build src/Notes.Maui/Notes.csproj -t:Run -f net9.0-maccatalyst
   
   # Windows
   dotnet build src/Notes.Maui/Notes.csproj -t:Run -f net9.0-windows10.0.19041.0
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter Category=Unit          # Unit tests only
dotnet test --filter Category=Integration   # Integration tests only
dotnet test --filter Category=UI            # UI automation tests only

# Run by speed
dotnet test --filter Category=Fast          # Quick tests
dotnet test --filter Category=Slow          # Longer-running tests

# Run specific test project
dotnet test tests/Notes.Core.Tests
dotnet test tests/Notes.Integration.Tests
dotnet test tests/Notes.UI.Tests

# Run with detailed output
dotnet test --verbosity normal
```

## 📋 Core Functionality

### Note Management
- **Create**: Add new notes with automatic filename generation
- **Read**: List all notes with smart caching
- **Update**: Edit existing note content
- **Delete**: Remove notes with immediate cache cleanup

### Clean Architecture Implementation
```csharp
// CQRS Command pattern with MediatR
public class SaveNoteCommand : IRequest
{
    public Note Note { get; set; }
}

public class SaveNoteHandler : IRequestHandler<SaveNoteCommand>
{
    private readonly NotesDbContext _context;
    
    public SaveNoteHandler(NotesDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(SaveNoteCommand request, CancellationToken cancellationToken)
    {
        // Business logic separated from UI concerns
        // Entity Framework operations
    }
}
```

### Data Access with Entity Framework
```csharp
// Entity Framework DbContext configuration
public class NotesDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Filename).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Filename).IsUnique();
            // Additional configurations...
        });
    }
}

// Service layer with caching
public async Task SaveNoteAsync(Note note)
{
    await _context.Notes.AddAsync(note);
    await _context.SaveChangesAsync();
    InvalidateCache(); // Immediate cache refresh
}
```

### MVVM Implementation
- **ObservableProperty**: Auto-generated property change notifications
- **RelayCommand**: Command pattern with async support
- **QueryProperty**: Navigation parameter binding
- **Dependency Injection**: Constructor-based service injection

## 🧪 Testing Strategy

The project includes a comprehensive three-layer testing framework:

### Test Architecture
- **Notes.Core.Tests**: Unit tests for business logic (23 tests)
  - Commands, Handlers, Models validation
  - Fast execution (< 30 seconds)
  - Categories: `Unit`, `Fast`, `Commands`, `Handlers`

- **Notes.Integration.Tests**: Service integration tests (9 tests)
  - Cross-service interactions and data flow
  - Moderate execution (1-3 minutes)
  - Categories: `Integration`, `Slow`, `Services`, `Data`

- **Notes.UI.Tests**: End-to-end UI automation (17 tests)
  - Complete user workflows with Appium
  - Estimated execution (~5-7 minutes)
  - Categories: `UI`, `Smoke`, `Regression`, `Diagnostic`

### Test Organization
- **Trait-based categorization**: Multiple categories per test for flexible filtering
- **Speed-based filtering**: Fast/Slow categories for CI/CD optimization
- **Component-specific filtering**: Target specific areas of the application
- **Comprehensive coverage**: From unit tests to full UI automation

### Testing Best Practices
- Clean test architecture with base classes
- Proper test isolation and cleanup
- Mock-based unit testing for fast feedback
- Real integration testing for confidence

## 🎯 Learning Objectives

This project demonstrates key concepts for enterprise mobile development:

1. **Clean Architecture**: Clear separation between business logic (Notes.Core) and UI (Notes.Maui)
2. **CQRS Pattern**: Command handlers for organized business operations
3. **Testing Strategies**: Comprehensive test pyramid from unit to UI automation
4. **Cross-Platform Development**: Platform abstractions and responsive design
5. **Performance Optimization**: Smart caching, async programming, memory management
6. **Modern .NET Patterns**: Dependency injection, MVVM, and latest C# features

## 🔄 Data Architecture & Caching

The application implements a layered data architecture with Entity Framework Core and intelligent caching:

### Data Layer Features
- **Entity Framework Core**: Object-relational mapping with SQLite
- **Database Migrations**: Automatic schema management
- **Indexed Queries**: Optimized database performance
- **ACID Transactions**: Data consistency and integrity

### Caching Strategy
- **Timeout-based**: 5-minute cache expiration
- **Event-driven**: Immediate invalidation on data changes
- **Memory efficient**: Lazy loading with cleanup
- **Thread-safe**: Concurrent access protection

### Performance Benefits
- Reduced database queries
- Faster note list loading
- Improved user experience
- Lower battery consumption

## 📱 User Experience

### Navigation Flow
1. **All Notes Page**: List view with pull-to-refresh
2. **Note Detail Page**: Create/edit individual notes
3. **Seamless Navigation**: Shell-based routing with parameters

### UI Features
- **Pull-to-Refresh**: Manual cache refresh capability
- **Responsive Design**: Adaptive layouts for different devices
- **Touch Interactions**: Tap-to-edit, swipe gestures
- **Visual Feedback**: Loading states and animations

## 🔮 Future Enhancements

### Planned Features
1. **Enhanced Testing**: Expanded UI automation and performance testing
2. **Advanced Caching**: Distributed caching and sync mechanisms
3. **Offline Sync**: Cloud synchronization with conflict resolution
4. **Security**: Encryption and authentication layers
5. **POS Integration**: Payment processing and inventory management features

### Technical Improvements
- **Event Sourcing**: Audit trail and data history
- **Microservices**: Distributed architecture preparation
- **Advanced CQRS**: Event-driven architecture patterns
- **Performance Monitoring**: Application insights and telemetry

## 🤝 Contributing

This is a learning project, but contributions and suggestions are welcome:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft**: .NET MAUI framework and excellent documentation
- **Community Toolkit**: MVVM implementation and best practices
- **Open Source Community**: Libraries and tools that make development efficient

---

*This project serves as a stepping stone toward building professional POS applications and demonstrates modern mobile development practices suitable for enterprise environments.*
