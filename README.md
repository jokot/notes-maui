# Notes MAUI App

A cross-platform note-taking application built with .NET MAUI, demonstrating modern mobile development patterns and MVVM architecture with smart caching.

## 📱 Project Overview

This project serves as a learning foundation for building enterprise-grade mobile applications, specifically preparing for POS (Point of Sale) system development. It showcases best practices in mobile app architecture, data management, and testing strategies.

## 🏗️ Project Structure

```
Notes/
├── src/                           # Main application source code
│   ├── Core/                      # Business logic and architecture
│   │   ├── Interfaces/            # Contracts and abstractions
│   │   ├── Models/                # Data models
│   │   ├── Services/              # Business services
│   │   └── ViewModels/            # MVVM ViewModels
│   ├── Views/                     # XAML pages and UI
│   ├── Platforms/                 # Platform-specific code
│   ├── Resources/                 # Images, fonts, styles
│   └── Shared/                    # Shared utilities and constants
├── tests/                         # Unit tests project
│   ├── ViewModels/                # ViewModel tests
│   └── (Test infrastructure)      # Test setup and utilities
├── README.md                      # Project documentation
├── LICENSE                        # MIT license
└── Notes.sln                      # Solution file
```

## ✨ Key Features

### Smart Caching System
- **Performance Optimization**: Intelligent caching with 5-minute timeout reduces unnecessary I/O operations
- **Real-time Updates**: Cache automatically refreshes on save/delete operations
- **Force Refresh**: Manual cache bypass option for guaranteed fresh data

### Modern Architecture
- **MVVM Pattern**: Clean separation of concerns with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for loose coupling
- **Shell Navigation**: Efficient page navigation with parameter passing
- **Async/Await**: Proper asynchronous programming throughout

### Cross-Platform Support
- **Android**: API 35.0 (Android 15)
- **iOS**: iOS 18.5
- **macOS**: macCatalyst 18.5
- **Responsive UI**: Adaptive layouts for different screen sizes

## 🛠️ Technical Stack

- **.NET 9**: Latest framework version
- **.NET MAUI**: Cross-platform UI framework
- **CommunityToolkit.Mvvm**: MVVM framework with source generators
- **System.Text.Json**: High-performance JSON serialization
- **Xunit**: Unit testing framework
- **FluentAssertions**: Expressive test assertions
- **Moq**: Mocking framework for unit tests

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 (17.5+) or Visual Studio Code with C# extension
- .NET 9 SDK
- Platform-specific SDKs:
  - Android SDK (API 35)
  - Xcode 15+ (for iOS/macOS development)

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
   dotnet build src/Notes.csproj -t:Run -f net9.0-android
   
   # iOS Simulator
   dotnet build src/Notes.csproj -t:Run -f net9.0-ios
   
   # macOS
   dotnet build src/Notes.csproj -t:Run -f net9.0-maccatalyst
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Notes.Tests.csproj

# Run with detailed output
dotnet test --verbosity normal
```

## 📋 Core Functionality

### Note Management
- **Create**: Add new notes with automatic filename generation
- **Read**: List all notes with smart caching
- **Update**: Edit existing note content
- **Delete**: Remove notes with immediate cache cleanup

### Smart Caching Logic
```csharp
// Cache expires after 5 minutes of inactivity
private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

// Automatic cache invalidation on modifications
public async Task SaveNoteAsync(Note note)
{
    await _fileDataService.SaveNoteAsync(note);
    InvalidateCache(); // Immediate cache refresh
}
```

### MVVM Implementation
- **ObservableProperty**: Auto-generated property change notifications
- **RelayCommand**: Command pattern with async support
- **QueryProperty**: Navigation parameter binding
- **Dependency Injection**: Constructor-based service injection

## 🧪 Testing Strategy

The project includes a comprehensive testing framework setup:

### Test Structure
- **Unit Tests**: Business logic validation
- **ViewModel Tests**: MVVM behavior verification
- **Service Tests**: Data layer functionality
- **Mock Integration**: Isolated component testing

### Current Testing Notes
- Test project targets `net9.0` for compatibility
- MAUI-specific testing requires special consideration
- Future enhancement: Extract business logic to separate libraries for easier testing

## 🎯 Learning Objectives

This project addresses key concepts for enterprise mobile development:

1. **Architecture Patterns**: MVVM, Dependency Injection, Repository Pattern
2. **Performance Optimization**: Smart caching, async programming, memory management
3. **Cross-Platform Development**: Platform abstractions, responsive design
4. **Testing Practices**: Unit testing, mocking, test-driven development
5. **Data Management**: File I/O, JSON serialization, data persistence

## 🔄 Smart Cache Implementation

The `NoteService` implements intelligent caching to optimize performance:

### Cache Features
- **Timeout-based**: 5-minute cache expiration
- **Event-driven**: Immediate invalidation on data changes
- **Memory efficient**: Lazy loading with cleanup
- **Thread-safe**: Concurrent access protection

### Performance Benefits
- Reduced file system access
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
1. **Enterprise Architecture**: Clean Architecture implementation
2. **Advanced Testing**: Integration and UI tests
3. **Offline Sync**: Cloud synchronization with conflict resolution
4. **Security**: Encryption and authentication
5. **POS Integration**: Payment processing and inventory management

### Technical Improvements
- **Separate Class Libraries**: Extract business logic for better testing
- **CQRS Pattern**: Command Query Responsibility Segregation
- **Event Sourcing**: Audit trail and data history
- **Microservices**: Distributed architecture preparation

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
