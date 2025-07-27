#!/bin/bash

# Script to run UI tests on iOS platform
# Usage: ./run-ios-tests.sh [test-filter]

echo "🍎 Running UI tests on iOS platform..."

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
export TEST_PLATFORM=ios

# Default test filter (smoke tests)
TEST_FILTER="${1:-Category=Smoke}"

echo "📱 Target Platform: iOS"
echo "🧪 Test Filter: $TEST_FILTER"
echo "⏳ Starting tests..."

# Run the tests with iOS platform preference
dotnet test Notes.UI.Tests.csproj \
    --filter "$TEST_FILTER" \
    --verbosity normal \
    --logger "console;verbosity=detailed"

echo "✅ Test execution completed"
