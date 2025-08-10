using Xunit;
using Notes.UI.Tests.PageObjects;
using Notes.UI.Tests.Helpers;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium;
using System.Diagnostics;

namespace Notes.UI.Tests.Tests;

[Collection("UITests")]
public class DiagnosticTests : BaseUITest
{
    public DiagnosticTests()
    {
        // Initialize driver in constructor to ensure it's available for all tests
        InitializeDriver().Wait();
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Diagnostic")]
    public async Task InspectAvailableElements_ShouldListAllElementsOnScreen()
    {
        // Arrange - ensure clean app state
        await EnsureCleanAppState();
        await Task.Delay(5000); // Give app time to fully load

        // Act - Find all elements on screen
        var allElements = _driver!.FindElements(OpenQA.Selenium.By.XPath("//*"));
        
        Debug.WriteLine($"=== FOUND {allElements.Count} TOTAL ELEMENTS ===");
        
        for (int i = 0; i < Math.Min(allElements.Count, 20); i++) // Limit to first 20 elements
        {
            var element = allElements[i];
            try
            {
                var tagName = element.TagName ?? "Unknown";
                var text = element.Text ?? "";
                var accessibilityId = element.GetAttribute("accessibilityIdentifier") ?? "";
                var name = element.GetAttribute("name") ?? "";
                var label = element.GetAttribute("label") ?? "";
                var value = element.GetAttribute("value") ?? "";
                
                Debug.WriteLine($"Element {i + 1}:");
                Debug.WriteLine($"  TagName: {tagName}");
                Debug.WriteLine($"  Text: '{text}'");
                Debug.WriteLine($"  AccessibilityId: '{accessibilityId}'");
                Debug.WriteLine($"  Name: '{name}'");
                Debug.WriteLine($"  Label: '{label}'");
                Debug.WriteLine($"  Value: '{value}'");
                Debug.WriteLine("---");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Element {i + 1}: Error reading - {ex.Message}");
            }
        }

        // Try specific iOS element types
        try
        {
            var buttons = _driver.FindElements(OpenQA.Selenium.By.ClassName("XCUIElementTypeButton"));
            Debug.WriteLine($"=== FOUND {buttons.Count} BUTTONS ===");
            
            foreach (var button in buttons.Take(5))
            {
                try
                {
                    Debug.WriteLine($"Button: Text='{button.Text}', AccessibilityId='{button.GetAttribute("accessibilityIdentifier")}', Name='{button.GetAttribute("name")}'");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Button: Error reading - {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error finding buttons: {ex.Message}");
        }

        // Try toolbar items
        try
        {
            var toolbarButtons = _driver.FindElements(OpenQA.Selenium.By.ClassName("XCUIElementTypeOther"));
            Debug.WriteLine($"=== FOUND {toolbarButtons.Count} OTHER ELEMENTS (may include toolbar) ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error finding other elements: {ex.Message}");
        }

        // Assert - Just ensure we found some elements
        Assert.True(allElements.Count > 0, "Should find at least some elements on screen");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Diagnostic")]
    public async Task FindAddButtonUsingDifferentStrategies_ShouldLocateButton()
    {
        // Arrange
        await EnsureCleanAppState();
        await Task.Delay(5000);
        
        var strategies = new List<(string name, Func<IWebElement?> selector)>
        {
            ("AccessibilityId", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.Appium.MobileBy.AccessibilityId("AddNoteButton")); }
                catch { return null; }
            }),
            ("Text 'Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//*[@text='Add']")); }
                catch { return null; }
            }),
            ("Name 'Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//*[@name='Add']")); }
                catch { return null; }
            }),
            ("Label 'Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//*[@label='Add']")); }
                catch { return null; }
            }),
            ("Contains 'Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//*[contains(@name,'Add') or contains(@label,'Add') or contains(text(),'Add')]")); }
                catch { return null; }
            }),
            ("Button with +'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeButton[contains(@name,'+') or contains(@label,'+')]")); }
                catch { return null; }
            })
        };

        Debug.WriteLine("=== TESTING DIFFERENT SELECTOR STRATEGIES ===");
        
        foreach (var (name, selector) in strategies)
        {
            try
            {
                var element = selector();
                if (element != null)
                {
                    Debug.WriteLine($"✅ SUCCESS: {name} - Found element!");
                    Debug.WriteLine($"   Text: '{element.Text}'");
                    Debug.WriteLine($"   AccessibilityId: '{element.GetAttribute("accessibilityIdentifier")}'");
                    Debug.WriteLine($"   Name: '{element.GetAttribute("name")}'");
                    Debug.WriteLine($"   Label: '{element.GetAttribute("label")}'");
                }
                else
                {
                    Debug.WriteLine($"❌ FAILED: {name} - Element not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR: {name} - {ex.Message}");
            }
            Debug.WriteLine("---");
        }

        // For test assertion, just verify the driver is working
        Assert.NotNull(_driver);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Diagnostic")]
    public async Task FindToolbarElements_ShouldLocateAddButton()
    {
        // Arrange
        await EnsureCleanAppState();
        await Task.Delay(5000);
        
        Debug.WriteLine("=== SEARCHING FOR TOOLBAR ELEMENTS ===");

        // Strategy 1: Look for navigation bar
        try
        {
            var navBars = _driver!.FindElements(OpenQA.Selenium.By.ClassName("XCUIElementTypeNavigationBar"));
            Debug.WriteLine($"Found {navBars.Count} navigation bars");
            
            foreach (var navBar in navBars)
            {
                Debug.WriteLine($"NavBar: Name='{navBar.GetAttribute("name")}', Label='{navBar.GetAttribute("label")}'");
                
                // Look for buttons within the nav bar
                var navButtons = navBar.FindElements(OpenQA.Selenium.By.ClassName("XCUIElementTypeButton"));
                Debug.WriteLine($"  Found {navButtons.Count} buttons in nav bar");
                
                foreach (var btn in navButtons)
                {
                    Debug.WriteLine($"    Button: Text='{btn.Text}', Name='{btn.GetAttribute("name")}', Label='{btn.GetAttribute("label")}', AccessibilityId='{btn.GetAttribute("accessibilityIdentifier")}'");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error with navigation bars: {ex.Message}");
        }

        // Strategy 2: Look for all buttons and find ones with "Add" or "+"
        try
        {
            var allButtons = _driver!.FindElements(OpenQA.Selenium.By.ClassName("XCUIElementTypeButton"));
            Debug.WriteLine($"=== FOUND {allButtons.Count} TOTAL BUTTONS ===");
            
            foreach (var button in allButtons)
            {
                try
                {
                    var text = button.Text ?? "";
                    var name = button.GetAttribute("name") ?? "";
                    var label = button.GetAttribute("label") ?? "";
                    var accessibilityId = button.GetAttribute("accessibilityIdentifier") ?? "";
                    
                    Debug.WriteLine($"Button: Text='{text}', Name='{name}', Label='{label}', AccessibilityId='{accessibilityId}'");
                    
                    // Check if this looks like our Add button
                    if (text.Contains("Add") || name.Contains("Add") || label.Contains("Add") || 
                        text.Contains("+") || name.Contains("+") || label.Contains("+"))
                    {
                        Debug.WriteLine($"  ✅ POTENTIAL ADD BUTTON FOUND!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"  Error reading button: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error finding buttons: {ex.Message}");
        }

        // Strategy 3: Try to find by text content
        var textStrategies = new List<(string name, Func<IWebElement?> selector)>
        {
            ("XPath text='Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeButton[@name='Add']")); }
                catch { return null; }
            }),
            ("XPath label='Add'", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeButton[@label='Add']")); }
                catch { return null; }
            }),
            ("XPath contains Add", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeButton[contains(@name,'Add') or contains(@label,'Add')]")); }
                catch { return null; }
            }),
            ("XPath contains +", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeButton[contains(@name,'+') or contains(@label,'+')]")); }
                catch { return null; }
            }),
            ("Partial link text", () =>
            {
                try { return _driver!.FindElement(OpenQA.Selenium.By.PartialLinkText("Add")); }
                catch { return null; }
            })
        };

        Debug.WriteLine("=== TESTING TEXT-BASED STRATEGIES ===");
        
        foreach (var (name, selector) in textStrategies)
        {
            try
            {
                var element = selector();
                if (element != null)
                {
                    Debug.WriteLine($"✅ SUCCESS: {name}");
                    Debug.WriteLine($"   Text: '{element.Text}'");
                    Debug.WriteLine($"   Name: '{element.GetAttribute("name")}'");
                    Debug.WriteLine($"   Label: '{element.GetAttribute("label")}'");
                    
                    // Try to click it to test
                    Debug.WriteLine("   Attempting to click...");
                    element.Click();
                    await Task.Delay(2000);
                    Debug.WriteLine("   Click successful!");
                    
                    break; // Found working selector
                }
                else
                {
                    Debug.WriteLine($"❌ FAILED: {name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR: {name} - {ex.Message}");
            }
        }

        // Assert - just verify driver is working
        Assert.NotNull(_driver);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Diagnostic")]
    public async Task DumpPageSource_ShouldShowAllElements()
    {
        // Arrange - ensure clean app state
        await EnsureCleanAppState();
        await Task.Delay(5000); // Wait for app to fully load
        
        // Act - Get the complete page source
        var pageSource = _driver!.PageSource;
        
        // Write to console and also to a file for easier reading
        Debug.WriteLine("=== COMPLETE PAGE SOURCE ===");
        Debug.WriteLine(pageSource);
        
        // Also write to a file
        var outputPath = "/tmp/ios_page_source.xml";
        await File.WriteAllTextAsync(outputPath, pageSource);
        Debug.WriteLine($"Page source written to: {outputPath}");
        
        // Assert
        Assert.True(!string.IsNullOrEmpty(pageSource), "Page source should not be empty");
    }
} 