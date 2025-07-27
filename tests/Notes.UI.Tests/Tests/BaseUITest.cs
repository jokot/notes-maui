using Notes.UI.Tests.Helpers;
using Notes.UI.Tests.PageObjects;
using OpenQA.Selenium.Appium;
using Xunit;

namespace Notes.UI.Tests.Tests;

/// <summary>
/// Base class for UI tests with proper session management
/// </summary>
public abstract class BaseUITest : IDisposable
{
    protected AppiumDriver? _driver;
    protected NotesListPage? _notesListPage;
    private bool _isDisposed = false;

    /// <summary>
    /// Initialize the driver with proper error handling and session management
    /// </summary>
    protected async Task InitializeDriver()
    {
        if (_driver == null)
        {
            // Check for platform preference from environment variable
            var targetPlatform = Environment.GetEnvironmentVariable("TEST_PLATFORM")?.ToLower();
            var preferAndroid = targetPlatform == "android";
            var preferIOS = targetPlatform == "ios";
            
            Exception? firstException = null;
            Exception? secondException = null;
            
            try
            {
                if (preferAndroid)
                {
                    // Try Android first if explicitly requested
                    _driver = AppiumDriverFactory.CreateAndroidDriver();
                    Console.WriteLine("✅ Android driver created successfully (preferred platform)");
                }
                else if (preferIOS)
                {
                    // Try iOS first if explicitly requested
                    _driver = AppiumDriverFactory.CreateiOSDriver();
                    Console.WriteLine("✅ iOS driver created successfully (preferred platform)");
                }
                else
                {
                    // Default behavior: try iOS first
                    _driver = AppiumDriverFactory.CreateiOSDriver();
                    Console.WriteLine("✅ iOS driver created successfully (default)");
                }
            }
            catch (Exception ex)
            {
                firstException = ex;
                Console.WriteLine($"❌ {(preferAndroid ? "Android" : "iOS")} driver creation failed: {ex.Message}");
                
                try
                {
                    if (preferAndroid || (!preferIOS && firstException != null))
                    {
                        // Fall back to iOS if Android was preferred or default failed
                        _driver = AppiumDriverFactory.CreateiOSDriver();
                        Console.WriteLine("✅ iOS driver created successfully (fallback)");
                    }
                    else
                    {
                        // Fall back to Android if iOS was preferred
                        _driver = AppiumDriverFactory.CreateAndroidDriver();
                        Console.WriteLine("✅ Android driver created successfully (fallback)");
                    }
                }
                catch (Exception fallbackEx)
                {
                    secondException = fallbackEx;
                    throw new InvalidOperationException(
                        "Could not initialize Appium driver for either platform. " +
                        "Ensure Appium server is running and simulator/emulator is available. " +
                        $"First attempt error: {firstException.Message}, " +
                        $"Fallback attempt error: {fallbackEx.Message}", 
                        new AggregateException(firstException, fallbackEx));
                }
            }

            _driver.Manage().Timeouts().ImplicitWait = AppiumDriverFactory.GetImplicitWaitTimeout();
            _notesListPage = new NotesListPage(_driver);
            
            // Wait a moment for the driver to stabilize (minimal for local app)
            await Task.Delay(100);
            
            // Additional stability check
            try
            {
                // Verify the driver is responsive by checking window handles
                var windowHandles = _driver.WindowHandles;
                Console.WriteLine($"📱 Driver responsive, windows: {windowHandles.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Driver stability check failed: {ex.Message}");
                                // Give app a moment to reinitialize (reduced for local app)
                await Task.Delay(200);
            }
        }
    }

    /// <summary>
    /// Reset the app session to ensure clean state between tests
    /// </summary>
    protected async Task ResetAppSession()
    {
        if (_driver != null)
        {
            try
            {
                Console.WriteLine("🔄 Resetting app session...");
                await AppiumDriverFactory.ResetAppSession(_driver);
                Console.WriteLine("✅ App session reset successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ App session reset failed: {ex.Message}");
                // Continue with the test - the reset failure shouldn't break the test
            }
        }
    }

    /// <summary>
    /// Restart the entire Appium session if needed
    /// </summary>
    protected async Task RestartSession()
    {
        if (_driver != null)
        {
            try
            {
                Console.WriteLine("🔄 Restarting Appium session...");
                await AppiumDriverFactory.RestartSession(_driver);
                _notesListPage = new NotesListPage(_driver);
                Console.WriteLine("✅ Appium session restarted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Session restart failed: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Wait for the app to be in a stable state
    /// </summary>
    protected async Task WaitForAppStability()
    {
        if (_driver != null)
        {
            try
            {
                // Wait for any ongoing animations or loading states (minimal for local app)
                await Task.Delay(100);
                
                // Try to find a basic element to ensure app is responsive
                var elements = _driver.FindElements(OpenQA.Selenium.By.XPath("//*"));
                Console.WriteLine($"📱 App is responsive - found {elements.Count} elements");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ App stability check failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Ensure the app is in a known state before starting a test
    /// </summary>
    protected async Task EnsureCleanAppState()
    {
        await InitializeDriver();
        
        // Add minimal wait for stability (local app)
        await Task.Delay(100);
        
        await ResetAppSession();
        await WaitForAppStability();
        
        // Clean up any existing notes to ensure test isolation
        if (_notesListPage != null)
        {
            await _notesListPage.WaitForPageToLoad();
            Console.WriteLine("🧹 Cleaning up existing notes...");
            await _notesListPage.DeleteAllNotes();
            Console.WriteLine($"✅ Cleanup complete. Notes count: {_notesListPage.GetNotesCount()}");
        }
        
        // Minimal stability wait after cleanup (local app)
        await Task.Delay(50);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
        {
            try
            {
                _driver?.Quit();
                Console.WriteLine("✅ Driver session closed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during driver cleanup: {ex.Message}");
            }
            finally
            {
                _driver?.Dispose();
                _isDisposed = true;
            }
        }
    }
} 