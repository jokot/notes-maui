#!/bin/bash

# Script to run UI tests on Android platform
# Usage: ./run-android-tests.sh [test-filter]

echo "🤖 Running UI tests on Android platform..."

# Run setup to ensure Appium environment is ready
echo "🔧 Running setup to ensure Appium environment is ready..."
if [ -f "./setup.sh" ]; then
    ./setup.sh
    if [ $? -ne 0 ]; then
        echo "❌ Setup failed. Exiting."
        exit 1
    fi
else
    echo "⚠️ setup.sh not found. Make sure Appium is installed and configured."
fi

# Set the platform environment variable
export TEST_PLATFORM=android

# Default test filter (smoke tests)
TEST_FILTER="${1:-Category=Smoke}"

echo "📱 Target Platform: Android"
echo "🧪 Test Filter: $TEST_FILTER"
echo "⏳ Starting tests..."

# Run the tests with Android platform preference
dotnet test Notes.UI.Tests.csproj \
    --filter "$TEST_FILTER" \
    --verbosity normal \
    --logger "console;verbosity=detailed"

echo "✅ Test execution completed"
