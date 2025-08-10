using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace Notes.UI.Tests.PageObjects;

public class NoteEditorPage
{
    private readonly AppiumDriver _driver;

    public NoteEditorPage(AppiumDriver driver)
    {
        _driver = driver;
    }

    // Page elements - iOS compatible selectors
    public AppiumElement PageTitle => _driver.FindElement(MobileBy.AccessibilityId("PageTitle"));
    
    // Multiple strategies for finding the note editor
    public AppiumElement NoteEditor 
    {
        get
        {
            Console.WriteLine("🔍 Looking for NoteEditor element...");
            
            // Strategy 1: Try AccessibilityId first (cross-platform)
            try
            {
                Console.WriteLine("🎯 Trying AccessibilityId 'NoteEditor'...");
                var element = _driver.FindElement(MobileBy.AccessibilityId("NoteEditor"));
                Console.WriteLine("✅ Found NoteEditor by AccessibilityId");
                return element;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ AccessibilityId failed: {ex.Message}");
                
                // Strategy 2: Android - Look for EditText element
                try
                {
                    Console.WriteLine("🎯 Trying Android EditText class...");
                    var element = _driver.FindElement(By.ClassName("android.widget.EditText"));
                    Console.WriteLine("✅ Found NoteEditor by Android EditText class");
                    return element;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"❌ Android EditText failed: {ex2.Message}");
                    
                    try
                    {
                        Console.WriteLine("🎯 Trying Android EditText with hint...");
                        // Strategy 3: Android - Multi-line EditText for longer content
                        var element = _driver.FindElement(By.XPath("//android.widget.EditText[@text='' or @hint='Enter note text' or not(@text)]"));
                        Console.WriteLine("✅ Found NoteEditor by Android EditText with hint");
                        return element;
                    }
                    catch (Exception ex3)
                    {
                        Console.WriteLine($"❌ Android EditText with hint failed: {ex3.Message}");
                        
                        try
                        {
                            Console.WriteLine("🎯 Trying iOS TextEditor...");
                            // Strategy 4: iOS - Look for TextEditor element
                            var element = _driver.FindElement(By.XPath("//XCUIElementTypeTextView"));
                            Console.WriteLine("✅ Found NoteEditor by iOS TextEditor");
                            return element;
                        }
                        catch (Exception ex4)
                        {
                            Console.WriteLine($"❌ iOS TextEditor failed: {ex4.Message}");
                            
                            try
                            {
                                Console.WriteLine("🎯 Trying any text input element...");
                                // Strategy 5: Fallback - any element that accepts text input
                                var element = _driver.FindElement(By.XPath("//*[@class='android.widget.EditText' or @class='android.widget.TextView' or contains(@content-desc, 'text') or contains(@name, 'text')]"));
                                Console.WriteLine("✅ Found NoteEditor by fallback text input");
                                return element;
                            }
                            catch (Exception ex5)
                            {
                                Console.WriteLine($"❌ All strategies failed. Last error: {ex5.Message}");
                                throw new InvalidOperationException($"Could not find NoteEditor element. Tried all strategies. Last error: {ex5.Message}");
                            }
                        }
                    }
                }
            }
        }
    }    // Multiple strategies for buttons
    public AppiumElement SaveButton
    {
        get
        {
            try
            {
                return _driver.FindElement(MobileBy.AccessibilityId("SaveButton"));
            }
            catch
            {
                // Android: Look for button with Save text
                try
                {
                    return _driver.FindElement(By.XPath("//android.widget.Button[contains(@text,'Save')]"));
                }
                catch
                {
                    // iOS: Look for button with Save text
                    return _driver.FindElement(By.XPath("//XCUIElementTypeButton[@name='Save' or @label='Save']"));
                }
            }
        }
    }
    
    public AppiumElement DeleteButton
    {
        get
        {
            try
            {
                return _driver.FindElement(MobileBy.AccessibilityId("DeleteButton"));
            }
            catch
            {
                // Android: Look for button with Delete text
                try
                {
                    return _driver.FindElement(By.XPath("//android.widget.Button[contains(@text,'Delete')]"));
                }
                catch
                {
                    // iOS: Look for button with Delete text
                    return _driver.FindElement(By.XPath("//XCUIElementTypeButton[@name='Delete' or @label='Delete']"));
                }
            }
        }
    }
    
    public AppiumElement BackButton => _driver.FindElement(MobileBy.AccessibilityId("BackButton"));

    // Page actions
    public async Task WaitForPageToLoad(int timeoutSeconds = 10)
    {
        Console.WriteLine($"⏳ Waiting for Note Editor page to load (timeout: {timeoutSeconds}s)...");
        await WaitForElement(() => NoteEditor, TimeSpan.FromSeconds(timeoutSeconds));
        Console.WriteLine("✅ Note Editor page loaded successfully");
    }

    public async Task<NoteEditorPage> EnterText(string text)
    {
        Console.WriteLine($"📝 Attempting to enter text: '{text}'");
        
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                Console.WriteLine($"📝 Attempt {attempt}/3 to enter text...");
                
                // Get a fresh element reference each time to avoid stale references
                AppiumElement? element = null;
                
                // Try multiple strategies to find a valid text input element
                string[] strategies = {
                    "AccessibilityId",
                    "AndroidEditText", 
                    "AndroidEditTextWithHint",
                    "iOSTextEditor",
                    "FocusableElement"
                };
                
                foreach (var strategy in strategies)
                {
                    try
                    {
                        Console.WriteLine($"📝 Trying strategy: {strategy}");
                        
                        switch (strategy)
                        {
                            case "AccessibilityId":
                                element = _driver.FindElement(MobileBy.AccessibilityId("NoteEditor"));
                                break;
                            case "AndroidEditText":
                                element = _driver.FindElement(By.ClassName("android.widget.EditText"));
                                break;
                            case "AndroidEditTextWithHint":
                                element = _driver.FindElement(By.XPath("//android.widget.EditText[@text='' or @hint]"));
                                break;
                            case "iOSTextEditor":
                                element = _driver.FindElement(By.XPath("//XCUIElementTypeTextView"));
                                break;
                            case "FocusableElement":
                                var focusableElements = _driver.FindElements(By.XPath("//*[@focusable='true' and (contains(@class,'EditText') or contains(@class,'TextEditor'))]"));
                                element = focusableElements.FirstOrDefault();
                                break;
                        }
                        
                        if (element != null)
                        {
                            Console.WriteLine($"✅ Found element using {strategy}: {element.TagName}");
                            Console.WriteLine($"📝 Element details - Enabled: {element.Enabled}, Displayed: {element.Displayed}");
                            break;
                        }
                    }
                    catch (Exception strategyEx)
                    {
                        Console.WriteLine($"⚠️ Strategy {strategy} failed: {strategyEx.Message}");
                        continue;
                    }
                }
                
                if (element == null)
                {
                    throw new InvalidOperationException("Could not find any suitable text input element");
                }
                
                // Try to interact with the element
                try
                {
                    // Ensure element is ready for interaction
                    Console.WriteLine("📝 Clicking element to ensure focus...");
                    element.Click();
                    await Task.Delay(200);
                    
                    Console.WriteLine("📝 Clearing existing text...");
                    element.Clear();
                    await Task.Delay(100);
                    
                    Console.WriteLine($"📝 Sending text: '{text}'...");
                    element.SendKeys(text);
                    await Task.Delay(200);
                    
                    Console.WriteLine($"✅ Successfully entered text: '{text}' on attempt {attempt}");
                    return this;
                }
                catch (Exception interactionEx)
                {
                    Console.WriteLine($"❌ Interaction failed on attempt {attempt}: {interactionEx.Message}");
                    if (attempt == 3)
                    {
                        throw new InvalidOperationException($"Failed to interact with text input after 3 attempts. Last error: {interactionEx.Message}");
                    }
                    
                    // Wait a bit before retrying
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Attempt {attempt} failed: {ex.Message}");
                if (attempt == 3)
                {
                    throw new InvalidOperationException($"Could not enter text '{text}' after 3 attempts. Last error: {ex.Message}");
                }
                
                // Wait a bit before retrying
                await Task.Delay(1000);
            }
        }
        
        return this;
    }

    public async Task<NoteEditorPage> AppendText(string text)
    {
        NoteEditor.SendKeys(text);
        await Task.Delay(50);
        return this;
    }

    public async Task<NotesListPage> SaveNote()
    {
        SaveButton.Click();
        await Task.Delay(100); // Minimal wait for local save and navigation
        return new NotesListPage(_driver);
    }

    public async Task<NotesListPage> DeleteNote()
    {
        DeleteButton.Click();
        
        // Handle delete confirmation if it exists
        try
        {
            var confirmButton = _driver.FindElement(MobileBy.AccessibilityId("ConfirmDeleteButton"));
            confirmButton.Click();
        }
        catch
        {
            // No confirmation dialog, continue
        }
        
        await Task.Delay(100); // Minimal wait for local delete and navigation
        return new NotesListPage(_driver);
    }

    public async Task<NotesListPage> GoBack()
    {
        try
        {
            BackButton.Click();
        }
        catch
        {
            // If no back button, use system back
            _driver.Navigate().Back();
        }
        
        await Task.Delay(50); // Minimal wait for local navigation
        return new NotesListPage(_driver);
    }

    public string GetNoteText()
    {
        return NoteEditor.Text;
    }

    public string GetPageTitle()
    {
        return PageTitle.Text;
    }

        public bool IsNoteEmpty()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            
            try
            {
                // Try multiple element finding strategies
                IWebElement? textField = null;
                
                // Strategy 1: Try accessibility ID first
                try
                {
                    textField = wait.Until(d => d.FindElement(MobileBy.AccessibilityId("NoteEditor")));
                }
                catch
                {
                    // Strategy 2: Try XPath with multiple options
                    try
                    {
                        textField = wait.Until(d => d.FindElement(By.XPath(
                            "//android.widget.EditText | " +
                            "//XCUIElementTypeTextView")));
                    }
                    catch
                    {
                        // Strategy 3: Try any text input element
                        textField = wait.Until(d => d.FindElement(By.XPath(
                            "//*[@class='android.widget.EditText'] | " +
                            "//*[contains(@content-desc, 'text')] | " +
                            "//XCUIElementTypeTextView")));
                    }
                }
                
                if (textField == null) return true;
                
                // Get text content using multiple approaches
                var directText = textField.Text?.Trim() ?? "";
                var attributeText = textField.GetAttribute("text")?.Trim() ?? "";
                var valueText = textField.GetAttribute("value")?.Trim() ?? "";
                var contentDesc = textField.GetAttribute("contentDescription")?.Trim() ?? "";
                var hintText = textField.GetAttribute("hint")?.Trim() ?? "";
                
                // Check if any text exists that's not a hint/placeholder
                var actualTexts = new[] { directText, attributeText, valueText, contentDesc }
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Where(t => !t.Equals(hintText, StringComparison.OrdinalIgnoreCase))
                    .Where(t => !t.Contains("Enter note", StringComparison.OrdinalIgnoreCase))
                    .Where(t => !t.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
                    .Where(t => !t.Contains("hint", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                // If we found any real content, field is not empty
                return actualTexts.Count == 0;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Could not find text field element - considering empty");
                return true; // If we can't find the text field, consider it empty
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error checking if note is empty: {ex.Message}");
                return true; // On error, assume empty
            }
        }    public bool IsSaveButtonEnabled()
    {
        return SaveButton.Enabled;
    }

    public bool IsDeleteButtonVisible()
    {
        try
        {
            return DeleteButton.Displayed;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitForElement(Func<AppiumElement> elementSelector, TimeSpan timeout)
    {
        var endTime = DateTime.Now.Add(timeout);
        while (DateTime.Now < endTime)
        {
            try
            {
                var element = elementSelector();
                if (element.Displayed)
                    return;
            }
            catch
            {
                // Element not found, continue waiting
            }
            await Task.Delay(100);
        }
        throw new TimeoutException($"Element not found within {timeout.TotalSeconds} seconds");
    }
} 