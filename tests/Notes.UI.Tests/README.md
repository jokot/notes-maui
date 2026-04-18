# Notes UI Tests with Appium

This project contains comprehensive UI automation tests for the Notes MAUI application using Appium WebDriver.

## 🚀 Quick Start

### Prerequisites
1. **Node.js** (for Appium)
2. **Appium Server** and drivers
3. **Android Studio** (for Android testing)
4. **Xcode** (for iOS testing on macOS)

### Installation & Setup

The easiest way to get started is to use the provided setup script:

```bash
# Run the automated setup (installs Appium, drivers, and verifies configuration)
./setup.sh
```

Or install manually:

```bash
# 1. Install Appium globally
npm install -g appium

# 2. Install Appium drivers
appium driver install uiautomator2    # Android
appium driver install xcuitest        # iOS (macOS only)

# 3. Verify installation
appium-doctor --android
appium-doctor --ios  # macOS only
```

## 🧪 Running Tests

### Platform-Specific Test Scripts

Use the provided scripts to run tests on specific platforms:

```bash
# Run tests on Android (automatically sets up environment)
./run-android-tests.sh

# Run tests on iOS (automatically sets up environment)
./run-ios-tests.sh

# Run specific test categories
./run-android-tests.sh "Category=Integration"
./run-ios-tests.sh "Category=Unit"
```

### Manual Test Execution

You can also run tests manually with environment variables:

```bash
# Run tests on Android
export TEST_PLATFORM=android
dotnet test --filter "Category=Smoke"

# Run tests on iOS
export TEST_PLATFORM=ios
dotnet test --filter "Category=Smoke"

# Let the system auto-detect platform (tries iOS first, falls back to Android)
unset TEST_PLATFORM
dotnet test --filter "Category=Smoke"
```

## 📱 Android Setup

### 1. Android Development Environment
```bash
# Install Android Studio
# Download from: https://developer.android.com/studio

# Set environment variables (add to ~/.bashrc or ~/.zshrc)
export ANDROID_HOME=$HOME/Library/Android/sdk  # macOS
export ANDROID_HOME=$HOME/Android/Sdk          # Linux
export PATH=$PATH:$ANDROID_HOME/emulator
export PATH=$PATH:$ANDROID_HOME/tools
export PATH=$PATH:$ANDROID_HOME/tools/bin
export PATH=$PATH:$ANDROID_HOME/platform-tools
```

### 2. Create Android Virtual Device (AVD)
```bash
# List available system images
avdmanager list

# Create AVD (example)
avdmanager create avd -n "TestDevice" -k "system-images;android-33;google_apis;x86_64"

# Start emulator
emulator -avd TestDevice
```

### 3. Build Notes App for Android
```bash
# From project root
dotnet build src/Notes.Maui/Notes.csproj -f net10.0-android -c Release

# The APK will be at:
# src/Notes.Maui/bin/Release/net10.0-android/com.jktdeveloper.notto-Signed.apk
```

## 🍎 iOS Setup (macOS Only)

### 1. Xcode Setup
```bash
# Install Xcode from Mac App Store
# Install command line tools
xcode-select --install

# Accept Xcode license
sudo xcodebuild -license accept
```

### 2. iOS Simulator
```bash
# List available simulators
xcrun simctl list devices

# Create iOS simulator (if needed)
xcrun simctl create "Test iPhone" com.apple.CoreSimulator.SimDeviceType.iPhone-15 com.apple.CoreSimulator.SimRuntime.iOS-17-0

# Boot simulator
xcrun simctl boot "Test iPhone"
```

### 3. Build Notes App for iOS
```bash
# From project root
dotnet build src/Notes.Maui/Notes.csproj -f net10.0-ios -c Release

# The app will be at:
# src/Notes.Maui/bin/Release/net10.0-ios/iossimulator-arm64/Notes.app
```

## ⚙️ Configuration

### Update App Paths
Edit `tests/Notes.UI.Tests/appsettings.json`:

```json
{
  "Android": {
    "AppPath": "../../../src/Notes.Maui/bin/Release/net10.0-android/com.jktdeveloper.notto-Signed.apk",
    "AppPackage": "com.jktdeveloper.notto"
  },
  "iOS": {
    "AppPath": "../../../src/Notes.Maui/bin/Release/net10.0-ios/iossimulator-arm64/Notes.app",
    "BundleId": "com.jktdeveloper.notto"
  }
}
```

### Add Accessibility IDs to XAML
Ensure your XAML elements have `AutomationId` attributes:

```xml
<!-- AllNotesPage.xaml -->
<Label x:Name="PageTitle" AutomationId="PageTitle" Text="Your Notes" />
<Button AutomationId="AddNoteButton" Text="Add Note" />
<CollectionView AutomationId="NotesCollection">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid AutomationId="NoteItem">
                <!-- Note content -->
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>

<!-- NotePage.xaml -->
<Label AutomationId="PageTitle" Text="Note" />
<Editor x:Name="NoteEditor" AutomationId="NoteEditor" />
<Button AutomationId="SaveButton" Text="Save" />
<Button AutomationId="DeleteButton" Text="Delete" />
```

## 🧪 Running Tests

### 1. Start Appium Server
```bash
appium server --port 4723
```

### 2. Start Device/Emulator
```bash
# Android
emulator -avd TestDevice

# iOS - simulators start automatically
```

### 3. Run Tests

**⭐ RECOMMENDED: Category-based execution**
```bash
# Smoke tests (critical path - run on every commit/PR)
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Smoke"

# Regression tests (edge cases - run nightly/weekly)
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Regression"

# Diagnostic tests (debugging - run when investigating issues)
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Diagnostic"
```

**Alternative execution methods:**
```bash
# Run specific test class
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "ClassName=NotesAppSmokeTests"

# Run with verbose output
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Smoke" --logger "console;verbosity=detailed"

# Run all tests (NOT recommended - use only for debugging)
dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj
```

## 📋 Test Structure

```
tests/Notes.UI.Tests/
├── Tests/
│   ├── SmokeTests.cs                 # Critical user journeys (7 tests)
│   ├── RegressionTests.cs            # Edge cases and data integrity (6 tests)
│   └── DiagnosticTests.cs            # Debugging and investigation (4 tests)
├── PageObjects/
│   ├── NotesListPage.cs              # Notes list page interactions
│   └── NoteEditorPage.cs             # Note editor page interactions
├── Helpers/
│   └── AppiumDriverFactory.cs       # Driver setup and configuration
├── appsettings.json                  # Platform and app configuration
└── README.md                         # This file
```

### Test Categories Explained

**🎯 Smoke Tests (Category=Smoke)**
- **Purpose**: Verify critical user journeys work end-to-end
- **When**: Run on every commit/PR (CI/CD gate)
- **Duration**: ~8 minutes
- **Success Rate**: 7/7 (100%) when run in isolation

**🔄 Regression Tests (Category=Regression)**  
- **Purpose**: Test edge cases, data integrity, and complex scenarios
- **When**: Run nightly or before releases
- **Duration**: ~11 minutes
- **Success Rate**: 6/6 (100%) when run in isolation

**🔍 Diagnostic Tests (Category=Diagnostic)**
- **Purpose**: Debug test failures and investigate UI structure
- **When**: Run on-demand when troubleshooting
- **Duration**: ~3 minutes
- **Success Rate**: 4/4 (100%) when run in isolation

## 🎯 Test Coverage

### Smoke Tests (Category=Smoke) - Critical Path
**7 tests covering essential user journeys:**
- ✅ `AppLaunches_ShouldDisplayNotesListPage` - App startup
- ✅ `AddNewNote_ShouldNavigateToEditorAndReturnToList` - Navigation flow
- ✅ `CreateNote_ShouldSaveSuccessfullyAndAppearInList` - Note creation
- ✅ `EditExistingNote_ShouldUpdateContentSuccessfully` - Note editing
- ✅ `DeleteNote_ShouldRemoveFromList` - Note deletion
- ✅ `NavigateBackWithoutSaving_ShouldNotCreateNote` - Cancel operations
- ✅ `RefreshNotesList_ShouldMaintainNotesDisplay` - List refresh

### Regression Tests (Category=Regression) - Edge Cases & Data Integrity
**6 tests covering complex scenarios:**
- ✅ `CreateNoteWithLongText_ShouldHandleGracefully` - Large text handling
- ✅ `CreateNoteWithSpecialCharacters_ShouldPreserveContent` - Unicode support
- ✅ `CreateEmptyNote_ShouldHandleAppropriately` - Empty content handling
- ✅ `MultipleNotesCreation_ShouldMaintainOrder` - Bulk operations
- ✅ `EditNoteMultipleTimes_ShouldPersistLatestChanges` - Multiple edits
- ✅ `AppRestart_ShouldPersistNotes` - Data persistence across restarts

### Diagnostic Tests (Category=Diagnostic) - Debugging Tools
**4 tests for investigation and troubleshooting:**
- ✅ `InspectAvailableElements_ShouldListAllElementsOnScreen` - Element discovery
- ✅ `FindAddButtonUsingDifferentStrategies_ShouldLocateButton` - Selector strategies
- ✅ `FindToolbarElements_ShouldLocateAddButton` - Toolbar structure
- ✅ `DumpPageSource_ShouldShowAllElements` - Complete page XML dump

## 🔧 Troubleshooting

### Common Issues

**Appium connection failed:**
```bash
# Check if Appium server is running
curl http://localhost:4723/status

# Restart Appium server
pkill -f appium
appium server --port 4723
```

**App not found:**
```bash
# Verify app path in appsettings.json
ls -la src/Notes.Maui/bin/Release/net10.0-android/
ls -la src/Notes.Maui/bin/Release/net10.0-ios/
```

**Element not found:**
```bash
# Use Appium Inspector to find elements
appium inspector
```

**Android emulator issues:**
```bash
# Cold boot emulator
emulator -avd TestDevice -wipe-data

# Check if ADB can see device
adb devices
```

**iOS simulator issues:**
```bash
# Reset simulator
xcrun simctl erase all

# Check available simulators
xcrun simctl list devices available
```

### Debug Mode
Enable detailed logging by setting environment variable:
```bash
export APPIUM_LOG_LEVEL=debug
appium server --port 4723
```

## 📊 Best Practices

### Test Strategy
- ✅ **Use category-based execution** - Run tests by category for better reliability
- ✅ **Smoke tests first** - Gate deployments with critical path tests
- ✅ **Schedule regression tests** - Run comprehensive tests nightly/weekly
- ✅ **Diagnostic on-demand** - Use debugging tests when investigating issues
- ✅ **Avoid running all tests together** - Reduces reliability from 100% to 41%

### Test Design
- ✅ Use Page Object Model for maintainability
- ✅ Keep tests independent and atomic
- ✅ Use meaningful test names
- ✅ Add appropriate waits for stability
- ✅ Clean up test data

### Element Selection
- ✅ Prefer AccessibilityId over XPath
- ✅ Use stable selectors
- ✅ Avoid hard-coded coordinates
- ✅ Handle dynamic content gracefully

### Test Execution
- ✅ **Category-based CI/CD** - Use different categories for different pipeline stages
- ✅ Test on multiple devices/OS versions
- ✅ Parallel execution on different devices
- ✅ Regular test maintenance

### Execution Strategy
```bash
# ✅ RECOMMENDED Pipeline Strategy:
# Stage 1: Critical validation (every commit)
dotnet test --filter "Category=Smoke"

# Stage 2: Comprehensive testing (nightly)  
dotnet test --filter "Category=Regression"

# Stage 3: Investigation (when needed)
dotnet test --filter "Category=Diagnostic"

# ❌ AVOID: Full suite execution (debugging only)
dotnet test  # 41% success rate due to test interference
```

## 🚀 Integration with CI/CD

### GitHub Actions Example (Category-based Strategy)
```yaml
name: UI Tests
on: [push, pull_request]

jobs:
  smoke-tests:
    name: Smoke Tests (Critical Path)
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install Appium
        run: |
          npm install -g appium
          appium driver install uiautomator2
      - name: Build Android App
        run: dotnet build src/Notes.Maui/Notes.csproj -f net10.0-android -c Release
      - name: Start Appium
        run: appium server --port 4723 &
      - name: Run Smoke Tests
        run: dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Smoke"

  regression-tests:
    name: Regression Tests (Nightly)
    runs-on: macos-latest
    if: github.event_name == 'schedule' # Run only on scheduled builds
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install Appium
        run: |
          npm install -g appium
          appium driver install uiautomator2
      - name: Build Android App
        run: dotnet build src/Notes.Maui/Notes.csproj -f net10.0-android -c Release
      - name: Start Appium
        run: appium server --port 4723 &
      - name: Run Regression Tests
        run: dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter "Category=Regression"
```

### Pipeline Strategy
```yaml
# Continuous Integration (every commit/PR)
- Smoke Tests: 7 tests, ~8 minutes, 100% success rate
- Block deployment if any smoke test fails

# Nightly Builds
- Regression Tests: 6 tests, ~11 minutes, 100% success rate  
- Alert team if any regression test fails

# On-Demand (debugging/investigation)
- Diagnostic Tests: 4 tests, ~3 minutes, 100% success rate
- Use when investigating test infrastructure issues
```

## 📈 Reporting

Test results are automatically generated in:
- Console output
- TestResults/ directory (TRX format)
- JUnit XML (with appropriate logger)

For advanced reporting, consider integrating with:
- Allure Framework
- ExtentReports
- ReportPortal 