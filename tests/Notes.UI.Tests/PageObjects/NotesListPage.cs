using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using System.Linq;

namespace Notes.UI.Tests.PageObjects;

public class NotesListPage
{
    private readonly AppiumDriver _driver;

    public NotesListPage(AppiumDriver driver)
    {
        _driver = driver;
    }

    // Page elements - iOS compatible selectors
    public AppiumElement PageTitle => _driver.FindElement(MobileBy.AccessibilityId("PageTitle"));
    
    // Multiple strategies for finding the Add button (toolbar item)
    public AppiumElement AddNoteButton
    {
        get
        {
            // Strategy 1: Try AccessibilityId first (Android/some iOS)
            try
            {
                var element = _driver.FindElement(MobileBy.AccessibilityId("AddNoteButton"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 2: iOS - Find button within navigation bar (toolbar buttons appear here)
            try
            {
                var navBar = _driver.FindElement(By.XPath("//XCUIElementTypeNavigationBar"));
                var element = (AppiumElement)navBar.FindElement(By.XPath(".//XCUIElementTypeButton"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 3: iOS - Find button by position (usually last element in nav bar)
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeNavigationBar/XCUIElementTypeButton[last()]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 4: iOS - Any button in the top part of screen (toolbar area)
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeButton[@y<100]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 5: Try any button that might be the add button
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeButton[contains(@name, 'Add') or contains(@label, 'Add') or contains(@value, '+')]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 6: Last resort - any clickable button element
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeButton[@enabled='true']"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 7: Wait for any button to appear with explicit wait
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                var element = wait.Until(driver => 
                {
                    try
                    {
                        return driver.FindElement(By.XPath("//XCUIElementTypeButton"));
                    }
                    catch
                    {
                        return null;
                    }
                });
                if (element != null) return (AppiumElement)element;
            }
            catch { /* Continue to next strategy */ }

            // If all strategies fail, throw a detailed error
            throw new NoSuchElementException("AddNoteButton could not be found using any strategy. " +
                                            "Tried: AccessibilityId, NavigationBar buttons, position-based selectors, " +
                                            "attribute-based selectors, and explicit wait strategies.");
        }
    }
    
    // Alternative element selectors for iOS
    public AppiumElement? AnyElement => _driver.FindElements(MobileBy.ClassName("XCUIElementTypeOther")).FirstOrDefault() as AppiumElement;
    public AppiumElement NavigationBar => _driver.FindElement(MobileBy.ClassName("XCUIElementTypeNavigationBar"));
    public IReadOnlyCollection<AppiumElement> NoteItems => _driver.FindElements(MobileBy.AccessibilityId("NoteItem"));
    public AppiumElement RefreshButton => _driver.FindElement(MobileBy.AccessibilityId("RefreshButton"));

    // Page actions
        public async Task WaitForPageToLoad(int timeoutSeconds = 5)
    {
        // Use optimized timeouts for local app performance
        var pageLoadTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        
        // Extra delay for session stability in full test suite
        await Task.Delay(100);
        
        // Try multiple strategies to find page elements
        try 
        {
            await WaitForElement(() => PageTitle, pageLoadTimeout);
        }
        catch
        {
            try
            {
                // Try to find navigation bar or any main element with longer timeout
                await WaitForElement(() => NavigationBar, pageLoadTimeout);
            }
            catch
            {
                try
                {
                    // Try to find add button with longer timeout
                    await WaitForElement(() => AddNoteButton, pageLoadTimeout);
                }
                catch
                {
                    // Last resort: wait for any visible UI element on the page
                    Console.WriteLine("⚠️ Standard elements not found, trying fallback element detection...");
                    await Task.Delay(250); // Minimal time for local app to load
                    
                    // Try to find any button or text element
                    try
                    {
                        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                        wait.Until(driver => 
                        {
                            try
                            {
                                var elements = driver.FindElements(By.XPath("//*[@class='UILabel' or @class='UIButton' or contains(@name, 'button')]"));
                                return elements.Count > 0;
                            }
                            catch
                            {
                                return false;
                            }
                        });
                        Console.WriteLine("✅ Fallback element detection successful");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ All page load strategies failed: {ex.Message}");
                        throw new TimeoutException($"Page failed to load after {pageLoadTimeout.TotalSeconds} seconds using all available strategies");
                    }
                }
            }
        }
    }

    public async Task<NoteEditorPage> TapAddNote()
    {
        AddNoteButton.Click();
        await Task.Delay(100); // Minimal wait for local navigation
        return new NoteEditorPage(_driver);
    }

    public async Task<NoteEditorPage> TapNoteItem(int index = 0)
    {
        var notes = NoteItems.ToList();
        if (notes.Count <= index)
        {
            throw new InvalidOperationException($"Note at index {index} not found. Only {notes.Count} notes available.");
        }
        
        notes[index].Click();
        await Task.Delay(100); // Minimal wait for local navigation
        return new NoteEditorPage(_driver);
    }

    public async Task RefreshNotes()
    {
        RefreshButton.Click();
        await Task.Delay(250); // Minimal wait for local refresh
    }

    public int GetNotesCount()
    {
        return NoteItems.Count;
    }

    public string GetPageTitle()
    {
        return PageTitle.Text;
    }

    public bool IsNoteDisplayed(string noteText)
    {
        try
        {
            // Try multiple strategies for cross-platform compatibility
            
            // Strategy 1: Android - using @text attribute
            try
            {
                var noteElement = _driver.FindElement(MobileBy.XPath($"//*[contains(@text, '{noteText}')]"));
                return noteElement.Displayed;
            }
            catch
            {
                // Strategy 2: iOS - using @label attribute
                try
                {
                    var noteElement = _driver.FindElement(MobileBy.XPath($"//*[contains(@label, '{noteText}')]"));
                    return noteElement.Displayed;
                }
                catch
                {
                    // Strategy 3: iOS - using @value attribute
                    try
                    {
                        var noteElement = _driver.FindElement(MobileBy.XPath($"//*[contains(@value, '{noteText}')]"));
                        return noteElement.Displayed;
                    }
                    catch
                    {
                        // Strategy 4: Search through all note items
                        return NoteItems.Any(item => item.Text?.Contains(noteText) == true);
                    }
                }
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<NotesListPage> DeleteAllNotes()
    {
        await WaitForPageToLoad();
        
        // Keep deleting notes until none are left
        while (GetNotesCount() > 0)
        {
            try
            {
                // Tap the first note
                var editorPage = await TapNoteItem(0);
                await editorPage.WaitForPageToLoad();
                
                // Try to delete it if delete button is available
                if (editorPage.IsDeleteButtonVisible())
                {
                    var returnedPage = await editorPage.DeleteNote();
                    await returnedPage.WaitForPageToLoad();
                    await Task.Delay(200); // Minimal wait for local deletion
                }
                else
                {
                    // If no delete button, just go back
                    await editorPage.GoBack();
                    break; // Exit if we can't delete
                }
            }
            catch
            {
                // If something goes wrong, break to avoid infinite loop
                break;
            }
        }
        
        return this;
    }

    private async Task WaitForElement(Func<AppiumElement> elementSelector, TimeSpan timeout)
    {
        var endTime = DateTime.Now.Add(timeout);
        Exception? lastException = null;
        var attempts = 0;
        
        Console.WriteLine($"⏳ Waiting for element with timeout: {timeout.TotalSeconds}s");
        
        while (DateTime.Now < endTime)
        {
            attempts++;
            try
            {
                var element = elementSelector();
                if (element != null && element.Displayed)
                {
                    Console.WriteLine($"✅ Element found after {attempts} attempts");
                    // Element found and visible, wait a bit more for stability
                    await Task.Delay(200);
                    return;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                // Log every 10th attempt to avoid spam
                if (attempts % 10 == 0)
                {
                    Console.WriteLine($"⏳ Attempt {attempts}: {ex.GetType().Name}");
                }
            }
            
            // Longer delays during full test suite runs
            await Task.Delay(100); // Minimal wait for local app stability
        }
        
        var elementName = elementSelector.Method.Name ?? "Unknown element";
        var errorMessage = $"Element '{elementName}' not found within {timeout.TotalSeconds} seconds after {attempts} attempts. Last error: {lastException?.Message}";
        Console.WriteLine($"❌ {errorMessage}");
        throw new TimeoutException(errorMessage);
    }
} 