using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Linq;

namespace Notes.UI.Tests.PageObjects;

public class NotesListPage
{
    private readonly AppiumDriver _driver;

    public NotesListPage(AppiumDriver driver)
    {
        _driver = driver;
    }

    // Page elements - Cross-platform compatible selectors
    public AppiumElement PageTitle 
    {
        get
        {
            try
            {
                return _driver.FindElement(MobileBy.AccessibilityId("PageTitle"));
            }
            catch
            {
                // Android: Look for TextView with "Notes" text or in ActionBar
                try
                {
                    return _driver.FindElement(By.XPath("//android.widget.TextView[contains(@text,'Notes')] | //android.widget.TextView[@content-desc='Notes']"));
                }
                catch
                {
                    // Android: Try ActionBar title
                    try
                    {
                        return _driver.FindElement(By.XPath("//android.widget.TextView[contains(@resource-id,'action_bar_title')]"));
                    }
                    catch
                    {
                        // Fallback: Any TextView that might be a title
                        return _driver.FindElement(By.XPath("//android.widget.TextView | //XCUIElementTypeNavigationBar//XCUIElementTypeStaticText"));
                    }
                }
            }
        }
    }
    
    // Multiple strategies for finding the Add button (toolbar item)
    public AppiumElement AddNoteButton
    {
        get
        {
            // Strategy 1: Try AccessibilityId first (cross-platform)
            try
            {
                var element = _driver.FindElement(MobileBy.AccessibilityId("AddNoteButton"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 2: Android - Find FAB (Floating Action Button) or toolbar button
            try
            {
                // Try finding Floating Action Button first
                var element = _driver.FindElement(By.XPath("//android.widget.ImageButton | //android.support.design.widget.FloatingActionButton | //com.google.android.material.floatingactionbutton.FloatingActionButton"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 3: Android - Find button with plus or add content
            try
            {
                var element = _driver.FindElement(By.XPath("//android.widget.Button[contains(@text,'+') or contains(@content-desc,'Add')] | //android.widget.ImageButton[contains(@content-desc,'Add')]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 4: iOS - Find button within navigation bar (toolbar buttons appear here)
            try
            {
                var navBar = _driver.FindElement(By.XPath("//XCUIElementTypeNavigationBar"));
                var element = (AppiumElement)navBar.FindElement(By.XPath(".//XCUIElementTypeButton"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 5: Android - Find any clickable element in top area (likely toolbar)
            try
            {
                var element = _driver.FindElement(By.XPath("//android.widget.Toolbar//android.widget.ImageButton | //android.widget.Toolbar//android.widget.Button"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 5: iOS - Find button by position (usually last element in nav bar)
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeNavigationBar/XCUIElementTypeButton[last()]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 6: iOS - Any button in the top part of screen (toolbar area)
            try
            {
                var element = _driver.FindElement(By.XPath("//XCUIElementTypeButton[@y<100]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 7: Cross-platform - Try any button that might be the add button
            try
            {
                var element = _driver.FindElement(By.XPath("//android.widget.Button[contains(@text, 'Add') or contains(@content-desc, 'Add')] | //XCUIElementTypeButton[contains(@name, 'Add') or contains(@label, 'Add') or contains(@value, '+')]"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 8: Last resort - any clickable button element
            try
            {
                var element = _driver.FindElement(By.XPath("//android.widget.Button[@enabled='true'] | //XCUIElementTypeButton[@enabled='true']"));
                if (element != null && element.Displayed) return element;
            }
            catch { /* Continue to next strategy */ }

            // Strategy 9: Wait for any button to appear with explicit wait (cross-platform)
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                var element = wait.Until(driver => 
                {
                    try
                    {
                        // Try Android first, then iOS
                        return driver.FindElement(By.XPath("//android.widget.Button | //XCUIElementTypeButton"));
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
    
    // Cross-platform element selectors
    public AppiumElement? AnyElement 
    {
        get
        {
            try
            {
                return _driver.FindElements(MobileBy.ClassName("android.view.View")).FirstOrDefault() as AppiumElement;
            }
            catch
            {
                return _driver.FindElements(MobileBy.ClassName("XCUIElementTypeOther")).FirstOrDefault() as AppiumElement;
            }
        }
    }
    
    public AppiumElement NavigationBar 
    {
        get
        {
            try
            {
                return _driver.FindElement(MobileBy.ClassName("android.widget.Toolbar"));
            }
            catch
            {
                return _driver.FindElement(MobileBy.ClassName("XCUIElementTypeNavigationBar"));
            }
        }
    }
    
    public IReadOnlyCollection<AppiumElement> NoteItems 
    {
        get
        {
            try
            {
                // Try AccessibilityId first
                var items = _driver.FindElements(MobileBy.AccessibilityId("NoteItem"));
                if (items.Count > 0) return items;
            }
            catch { /* Continue to next strategy */ }

            try
            {
                // Android: Look for list items or text views that contain note content
                var items = _driver.FindElements(By.XPath("//android.widget.ListView//android.widget.TextView | //androidx.recyclerview.widget.RecyclerView//android.widget.TextView"));
                if (items.Count > 0) return items;
            }
            catch { /* Continue to next strategy */ }

            try
            {
                // Android: Look for any clickable text views (likely notes)
                var items = _driver.FindElements(By.XPath("//android.widget.TextView[@clickable='true']"));
                if (items.Count > 0) return items;
            }
            catch { /* Continue to next strategy */ }

            // Fallback: return empty collection
            return new List<AppiumElement>();
        }
    }
    
    public AppiumElement RefreshButton 
    {
        get
        {
            try
            {
                return _driver.FindElement(MobileBy.AccessibilityId("RefreshButton"));
            }
            catch
            {
                // Android: Look for refresh button - try common patterns
                try
                {
                    // Try overflow menu or action bar
                    return _driver.FindElement(By.XPath("//android.widget.ImageButton[@content-desc='More options'] | //android.widget.TextView[@text='Refresh']"));
                }
                catch
                {
                    try
                    {
                        // Try SwipeRefreshLayout or any refresh-related element
                        return _driver.FindElement(By.XPath("//*[contains(@resource-id,'refresh') or contains(@content-desc,'refresh') or contains(@text,'Refresh')]"));
                    }
                    catch
                    {
                        try
                        {
                            // Try any button with refresh semantics
                            return _driver.FindElement(By.XPath("//android.widget.ImageButton[contains(@content-desc,'refresh')] | //android.widget.Button[contains(@text,'Refresh')]"));
                        }
                        catch
                        {
                            // iOS fallback
                            return _driver.FindElement(By.XPath("//XCUIElementTypeButton[contains(@name,'Refresh')]"));
                        }
                    }
                }
            }
        }
    }

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
                    
                    // Try to find any button or text element (cross-platform)
                    try
                    {
                        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                        wait.Until(driver => 
                        {
                            try
                            {
                                // Android first, then iOS
                                var elements = driver.FindElements(By.XPath("//android.widget.TextView | //android.widget.Button | //*[@class='UILabel' or @class='UIButton' or contains(@name, 'button')]"));
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
        Console.WriteLine("🖱️ Clicking Add Note button...");
        AddNoteButton.Click();
        
        // Wait for navigation on Android
        if (_driver.PlatformName.ToLower() == "android")
        {
            Console.WriteLine("⏳ Waiting 500ms for Android navigation...");
            await Task.Delay(500);
        }
        else
        {
            await Task.Delay(100);
        }
        
        // Debug: Print current page state
        try
        {
            var elements = _driver.FindElements(By.XPath("//*"));
            Console.WriteLine($"📱 After navigation - found {elements.Count} elements on page");
            
            // Try to find any EditText or TextEditor elements
            var editTexts = _driver.FindElements(By.ClassName("android.widget.EditText"));
            var textViews = _driver.FindElements(By.XPath("//XCUIElementTypeTextView"));
            Console.WriteLine($"📝 Found {editTexts.Count} EditText elements and {textViews.Count} TextEditor elements");
            
            // Check if we can find elements with "note" in their accessibility ID
            var noteElements = _driver.FindElements(By.XPath("//*[contains(@content-desc, 'note') or contains(@name, 'note') or contains(@resource-id, 'note')]"));
            Console.WriteLine($"📋 Found {noteElements.Count} elements with 'note' in their attributes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Debug error: {ex.Message}");
        }
        
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
        try
        {
            // Try to find and tap a refresh button
            RefreshButton.Click();
        }
        catch (NoSuchElementException)
        {
            // If no refresh button found, simulate pull-to-refresh on Android
            try
            {
                // Get the main list/scroll area for pull-to-refresh
                var scrollableArea = _driver.FindElement(By.XPath("//android.widget.ScrollView | //android.widget.ListView | //androidx.recyclerview.widget.RecyclerView"));
                
                // Perform pull-to-refresh gesture using W3C Actions
                var actions = new Actions(_driver);
                actions.MoveToElement(scrollableArea, scrollableArea.Size.Width / 2, 100)
                       .ClickAndHold()
                       .MoveByOffset(0, 300)
                       .Release()
                       .Perform();
                
                await Task.Delay(1000); // Wait for refresh to complete
            }
            catch
            {
                // If pull-to-refresh also fails, just wait a moment
                // Some refresh might happen automatically
                await Task.Delay(500);
            }
        }
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
        
        var maxAttempts = 10; // Prevent infinite loops
        var attempts = 0;
        
        // Keep deleting notes until none are left
        while (GetNotesCount() > 0 && attempts < maxAttempts)
        {
            attempts++;
            var initialCount = GetNotesCount();
            
            try
            {
                Console.WriteLine($"🗑️ Deleting note {attempts}/{maxAttempts} (found {initialCount} notes)");
                
                // Tap the first note
                var editorPage = await TapNoteItem(0);
                await editorPage.WaitForPageToLoad();
                
                // Try to delete it if delete button is available
                if (editorPage.IsDeleteButtonVisible())
                {
                    var returnedPage = await editorPage.DeleteNote();
                    await returnedPage.WaitForPageToLoad();
                    
                    // Wait for deletion to complete and verify count decreased
                    await Task.Delay(500); // Longer wait for Android
                    
                    var newCount = GetNotesCount();
                    if (newCount >= initialCount)
                    {
                        Console.WriteLine($"⚠️ Note count didn't decrease ({initialCount} -> {newCount}), breaking");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ No delete button visible, going back");
                    await editorPage.GoBack();
                    break; // Exit if we can't delete
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during deletion attempt {attempts}: {ex.Message}");
                // If something goes wrong, break to avoid infinite loop
                break;
            }
        }
        
        var finalCount = GetNotesCount();
        Console.WriteLine($"🧹 Cleanup completed. Final count: {finalCount} notes");
        
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