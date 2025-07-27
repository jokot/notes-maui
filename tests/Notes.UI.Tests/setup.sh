#!/bin/bash

echo "🚀 Setting up Notes UI Tests with Appium"
echo "========================================="

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js first."
    echo "   Download from: https://nodejs.org/"
    exit 1
fi

echo "✅ Node.js found: $(node --version)"

# Check if npm is available
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not available."
    exit 1
fi

echo "✅ npm found: $(npm --version)"

# Install Appium globally
echo "📦 Installing Appium..."
npm install -g appium

# Install Appium drivers
echo "📱 Installing Appium drivers..."
appium driver install uiautomator2
appium driver install xcuitest

# Check Android setup
echo "🤖 Checking Android setup..."
if command -v adb &> /dev/null; then
    echo "✅ Android Debug Bridge (adb) found"
    adb devices
else
    echo "⚠️  Android Debug Bridge (adb) not found"
    echo "   Install Android Studio and add platform-tools to PATH"
fi

# Check iOS setup (macOS only)
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "🍎 Checking iOS setup..."
    if command -v xcrun &> /dev/null; then
        echo "✅ Xcode tools found"
        echo "Available iOS simulators:"
        xcrun simctl list devices available | grep iPhone | head -5
    else
        echo "⚠️  Xcode tools not found"
        echo "   Install Xcode from the App Store"
    fi
fi

# Run Appium Doctor
echo "🏥 Running Appium Doctor..."
echo "For Android:"
appium-doctor --android
echo ""
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "For iOS:"
    appium-doctor --ios
fi

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Next steps:"
echo "1. Build your Notes app for the target platform:"
echo "   Android: dotnet build src/Notes.Maui/Notes.csproj -f net9.0-android -c Release"
echo "   iOS:     dotnet build src/Notes.Maui/Notes.csproj -f net9.0-ios -c Release"
echo ""
echo "2. Start an emulator/simulator:"
echo "   Android: emulator -avd YourDeviceName"
echo "   iOS:     Simulator launches automatically"
echo ""
echo "3. Start Appium server:"
echo "   appium server --port 4723"
echo ""
echo "4. Run UI tests (category-based execution recommended):"
echo "   # Smoke tests (critical path - run on every commit):"
echo "   dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter 'Category=Smoke'"
echo ""
echo "   # Regression tests (edge cases - run nightly):"
echo "   dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter 'Category=Regression'"
echo ""
echo "   # Diagnostic tests (debugging - run when needed):"
echo "   dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj --filter 'Category=Diagnostic'"
echo ""
echo "   # All tests (not recommended - use only for debugging):"
echo "   dotnet test tests/Notes.UI.Tests/Notes.UI.Tests.csproj"
echo ""
echo "📖 See tests/Notes.UI.Tests/README.md for detailed instructions" 