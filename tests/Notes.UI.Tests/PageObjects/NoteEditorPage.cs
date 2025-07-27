using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

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
            // Strategy 1: Try AccessibilityId first (Android/some iOS)
            try
            {
                return _driver.FindElement(MobileBy.AccessibilityId("NoteEditor"));
            }
            catch
            {
                // Strategy 2: iOS - Look for TextEditor element
                try
                {
                    return _driver.FindElement(By.XPath("//XCUIElementTypeTextView"));
                }
                catch
                {
                    // Strategy 3: iOS - Look for any text input element
                    return _driver.FindElement(By.XPath("//XCUIElementTypeTextField | //XCUIElementTypeTextView"));
                }
            }
        }
    }
    
    // Multiple strategies for buttons
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
                return _driver.FindElement(By.XPath("//XCUIElementTypeButton[@name='Save' or @label='Save']"));
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
                return _driver.FindElement(By.XPath("//XCUIElementTypeButton[@name='Delete' or @label='Delete']"));
            }
        }
    }
    
    public AppiumElement BackButton => _driver.FindElement(MobileBy.AccessibilityId("BackButton"));

    // Page actions
    public async Task WaitForPageToLoad(int timeoutSeconds = 3)
    {
        await WaitForElement(() => NoteEditor, TimeSpan.FromSeconds(timeoutSeconds));
    }

    public async Task<NoteEditorPage> EnterText(string text)
    {
        // Clear existing text first
        NoteEditor.Clear();
        await Task.Delay(50);
        
        // Enter new text
        NoteEditor.SendKeys(text);
        await Task.Delay(50);
        
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
        return string.IsNullOrWhiteSpace(GetNoteText());
    }

    public bool IsSaveButtonEnabled()
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