using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace Notes.UI.Tests.Helpers;

public static class AppiumDriverFactory
{
    private static IConfiguration? _configuration;
    
    static AppiumDriverFactory()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
    }

    public static AndroidDriver CreateAndroidDriver()
    {
        var serverUrl = new Uri(_configuration!["AppiumSettings:ServerUrl"]!);
        var options = new AppiumOptions();
        
        // Basic Android configuration
        options.PlatformName = "Android";
        options.DeviceName = _configuration["Android:DeviceName"]!;
        options.PlatformVersion = _configuration["Android:PlatformVersion"]!;
        options.AutomationName = _configuration["Android:AutomationName"]!;
        
        // App configuration
        var appPath = Path.GetFullPath(_configuration["Android:AppPath"]!);
        if (File.Exists(appPath))
        {
            options.App = appPath;
        }
        else
        {
            // Fallback to installed app
            options.AddAdditionalAppiumOption("appPackage", _configuration["Android:AppPackage"]);
            options.AddAdditionalAppiumOption("appActivity", _configuration["Android:AppActivity"]);
        }
        
        // Additional settings for stability
        options.AddAdditionalAppiumOption("newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("fullReset", false);
        options.AddAdditionalAppiumOption("autoLaunch", true);
        options.AddAdditionalAppiumOption("skipServerInstallation", false);
        options.AddAdditionalAppiumOption("skipDeviceInitialization", false);
        
        return new AndroidDriver(serverUrl, options);
    }

    public static IOSDriver CreateiOSDriver()
    {
        var serverUrl = new Uri(_configuration!["AppiumSettings:ServerUrl"]!);
        var options = new AppiumOptions();
        
        // Basic iOS configuration
        options.PlatformName = "iOS";
        options.DeviceName = _configuration["iOS:DeviceName"]!;
        options.PlatformVersion = _configuration["iOS:PlatformVersion"]!;
        options.AutomationName = _configuration["iOS:AutomationName"]!;
        
        // App configuration
        var appPath = Path.GetFullPath(_configuration["iOS:AppPath"]!);
        if (Directory.Exists(appPath))
        {
            options.App = appPath;
        }
        else
        {
            // Fallback to bundle ID
            options.AddAdditionalAppiumOption("bundleId", _configuration["iOS:BundleId"]);
        }
        
        // Additional settings for stability and isolation
        options.AddAdditionalAppiumOption("newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("noReset", true);  // Keep app data between sessions for speed
        options.AddAdditionalAppiumOption("fullReset", false);
        options.AddAdditionalAppiumOption("autoLaunch", true);
        options.AddAdditionalAppiumOption("skipServerInstallation", false);
        options.AddAdditionalAppiumOption("skipDeviceInitialization", false);
        
        // Enhanced session management for test isolation
        options.AddAdditionalAppiumOption("resetOnSessionStartOnly", true);
        options.AddAdditionalAppiumOption("shouldTerminateApp", true);
        options.AddAdditionalAppiumOption("forceAppLaunch", true);
        
        return new IOSDriver(serverUrl, options);
    }

    public static TimeSpan GetImplicitWaitTimeout()
    {
        var seconds = int.Parse(_configuration!["AppiumSettings:ImplicitWaitTimeoutSeconds"]!);
        return TimeSpan.FromSeconds(seconds);
    }

    public static TimeSpan GetPageLoadTimeout()
    {
        var seconds = int.Parse(_configuration!["AppiumSettings:PageLoadTimeoutSeconds"]!);
        return TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Resets the app state by terminating and relaunching the app
    /// </summary>
    public static async Task ResetAppSession(AppiumDriver driver)
    {
        try
        {
            // Terminate the app
            driver.TerminateApp(GetBundleId(driver));
            await Task.Delay(2000); // Wait for app to fully terminate
            
            // Relaunch the app
            driver.ActivateApp(GetBundleId(driver));
            await Task.Delay(3000); // Wait for app to fully launch
        }
        catch (Exception ex)
        {
            // If reset fails, try to restart the session
            Console.WriteLine($"App reset failed: {ex.Message}. Attempting session restart...");
            await RestartSession(driver);
        }
    }

    /// <summary>
    /// Restarts the entire Appium session
    /// </summary>
    public static async Task RestartSession(AppiumDriver driver)
    {
        try
        {
            driver.Quit();
            await Task.Delay(2000);
            
            // Create new driver instance
            if (driver is IOSDriver)
            {
                driver = CreateiOSDriver();
            }
            else
            {
                driver = CreateAndroidDriver();
            }
            
            driver.Manage().Timeouts().ImplicitWait = GetImplicitWaitTimeout();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to restart Appium session: {ex.Message}", ex);
        }
    }

    private static string GetBundleId(AppiumDriver driver)
    {
        if (driver is IOSDriver)
        {
            return _configuration!["iOS:BundleId"]!;
        }
        else
        {
            return _configuration!["Android:AppPackage"]!;
        }
    }
} 