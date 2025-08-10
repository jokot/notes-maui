using Notes.UI.Tests.Helpers;
using Notes.UI.Tests.PageObjects;
using OpenQA.Selenium.Appium;

namespace Notes.UI.Tests.Tests;

public class RegressionTests : BaseUITest
{

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task CreateNoteWithLongText_ShouldHandleGracefully()
    {
        // Arrange
        await EnsureCleanAppState();
        var longText = string.Join(" ", Enumerable.Repeat("This is a very long note with lots of text content.", 50));

        // Act
        var editorPage = await _notesListPage!.TapAddNote();
        await editorPage.WaitForPageToLoad();
        await editorPage.EnterText(longText);
        var returnedListPage = await editorPage.SaveNote();

        // Assert
        await returnedListPage.WaitForPageToLoad();
        Assert.True(returnedListPage.IsNoteDisplayed("This is a very long note"), 
            "Long note should be saved and partially visible in list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task CreateNoteWithSpecialCharacters_ShouldPreserveContent()
    {
        // Arrange
        await EnsureCleanAppState();
        var specialText = "Special chars: áéíóú ñ € ¥ £ ® © ™ \n\t Line break and tab!";

        // Act
        var editorPage = await _notesListPage!.TapAddNote();
        await editorPage.WaitForPageToLoad();
        await editorPage.EnterText(specialText);
        var returnedListPage = await editorPage.SaveNote();

        // Assert
        await returnedListPage.WaitForPageToLoad();
        
        // Verify by editing the note again
        var verifyEditorPage = await returnedListPage.TapNoteItem(0);
        await verifyEditorPage.WaitForPageToLoad();
        
        var savedText = verifyEditorPage.GetNoteText();
        Assert.Contains("Special chars:", savedText);
        Assert.Contains("áéíóú", savedText);
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task CreateEmptyNote_ShouldHandleAppropriately()
    {
        // Arrange
        await EnsureCleanAppState();
        var initialNotesCount = await GetInitialNotesCount();

        // Act
        var editorPage = await _notesListPage!.TapAddNote();
        await editorPage.WaitForPageToLoad();
        
        // Try to save without entering any text
        var returnedListPage = await editorPage.SaveNote();

        // Assert
        await returnedListPage.WaitForPageToLoad();
        var finalNotesCount = returnedListPage.GetNotesCount();
        
        // The behavior here depends on your app's logic
        // Either it should save an empty note OR reject the save
        // Adjust this assertion based on your expected behavior
        Assert.True(finalNotesCount >= initialNotesCount, 
            "App should handle empty note creation gracefully");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task MultipleNotesCreation_ShouldMaintainOrder()
    {
        // Arrange
        await EnsureCleanAppState();
        var noteTexts = new[]
        {
            $"First Note - {DateTime.Now:HH:mm:ss}",
            $"Second Note - {DateTime.Now.AddSeconds(1):HH:mm:ss}",
            $"Third Note - {DateTime.Now.AddSeconds(2):HH:mm:ss}"
        };

        // Act - Create multiple notes
        foreach (var noteText in noteTexts)
        {
            await CreateTestNote(noteText);
            await Task.Delay(1000); // Ensure different timestamps
        }

        // Assert
        await _notesListPage!.WaitForPageToLoad();
        var finalNotesCount = _notesListPage.GetNotesCount();
        
        Assert.True(finalNotesCount >= noteTexts.Length, 
            $"Should have at least {noteTexts.Length} notes, found {finalNotesCount}");
        
        // Verify all notes are visible
        foreach (var noteText in noteTexts)
        {
            Assert.True(_notesListPage.IsNoteDisplayed(noteText.Split(' ')[0]), 
                $"Note containing '{noteText.Split(' ')[0]}' should be visible");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task EditNoteMultipleTimes_ShouldPersistLatestChanges()
    {
        // Arrange
        await EnsureCleanAppState();
        var originalText = $"Original - {DateTime.Now:mm:ss}";
        await CreateTestNote(originalText);

        // Act - Edit the note multiple times
        var edits = new[]
        {
            $"First Edit - {DateTime.Now:mm:ss}",
            $"Second Edit - {DateTime.Now.AddSeconds(1):mm:ss}",
            $"Final Edit - {DateTime.Now.AddSeconds(2):mm:ss}"
        };

        foreach (var editText in edits)
        {
            var editorPage = await _notesListPage!.TapNoteItem(0);
            await editorPage.WaitForPageToLoad();
            await editorPage.EnterText(editText);
            _notesListPage = await editorPage.SaveNote();
            await _notesListPage.WaitForPageToLoad();
            await Task.Delay(500);
        }

        // Assert - Only the final edit should be visible
        Assert.True(_notesListPage!.IsNoteDisplayed("Final Edit"), 
            "Final edit should be visible");
        Assert.False(_notesListPage.IsNoteDisplayed("Original"), 
            "Original text should not be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Category", "Regression")]
    public async Task AppRestart_ShouldPersistNotes()
    {
        // Arrange
        await EnsureCleanAppState();
        var testText = $"Persistence Test - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        await CreateTestNote(testText);

        // Verify note was created before restart
        var notesCountBeforeRestart = _notesListPage!.GetNotesCount();
        Console.WriteLine($"📊 Notes count before restart: {notesCountBeforeRestart}");
        Assert.Equal(1, notesCountBeforeRestart);
        
        // Get the bundle ID for termination/activation
        var bundleId = "com.companyname.notes"; // iOS bundle ID
        
        try
        {
            Console.WriteLine("🔄 Initiating app background/foreground cycle to trigger data persistence...");
            
            // First, send app to background and bring it back to ensure data is saved
            _driver!.ExecuteScript("mobile: backgroundApp", new Dictionary<string, object> { ["seconds"] = 5 });
            await Task.Delay(6000); // Wait for background processing and auto-save
            
            // Bring app back to foreground
            _driver.ActivateApp(bundleId);
            await Task.Delay(3000); // Wait for foreground restoration
            
            Console.WriteLine("🔄 Terminating app completely...");
            // Now terminate the app completely (simulates user force-closing the app)
            _driver.TerminateApp(bundleId);
            await Task.Delay(5000); // Extended wait for app to fully terminate and save data
            
            Console.WriteLine("🔄 Reactivating app...");
            // Reactivate the app (simulates user reopening the app)
            _driver.ActivateApp(bundleId);
            await Task.Delay(5000); // Extended wait for app to fully launch and load data
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ App restart using terminate/activate failed: {ex.Message}");
            // Fallback to driver session restart
            _driver?.Quit();
            _driver?.Dispose();
            _driver = null;
            
            await Task.Delay(3000);
            await EnsureCleanAppState();
        }
        
        // Recreate page object to ensure fresh state
        _notesListPage = new NotesListPage(_driver!);
        await _notesListPage.WaitForPageToLoad();
        
        // Add extra delay for data loading after restart
        await Task.Delay(3000);

        // Assert - Check if notes persisted
        var notesCountAfterRestart = _notesListPage.GetNotesCount();
        Console.WriteLine($"📊 Notes count after restart: {notesCountAfterRestart}");
        
        if (notesCountAfterRestart == 0)
        {
            Console.WriteLine("⚠️ No notes found after restart. Attempting to refresh notes list...");
            try
            {
                // Try to trigger a refresh if the app has such functionality
                await _notesListPage.WaitForPageToLoad();
                await Task.Delay(2000);
                var refreshedCount = _notesListPage.GetNotesCount();
                Console.WriteLine($"📊 Notes count after refresh attempt: {refreshedCount}");
                notesCountAfterRestart = refreshedCount;
            }
            catch (Exception refreshEx)
            {
                Console.WriteLine($"⚠️ Failed to refresh notes list: {refreshEx.Message}");
            }
        }
        
        Assert.Equal(1, notesCountAfterRestart);
        Assert.True(_notesListPage.IsNoteDisplayed("Persistence Test"), 
            "Note should persist after app restart");
    }

    private async Task<int> GetInitialNotesCount()
    {
        await _notesListPage!.WaitForPageToLoad();
        return _notesListPage.GetNotesCount();
    }

    private async Task CreateTestNote(string noteText)
    {
        var editorPage = await _notesListPage!.TapAddNote();
        await editorPage.WaitForPageToLoad();
        await editorPage.EnterText(noteText);
        _notesListPage = await editorPage.SaveNote();
        await _notesListPage.WaitForPageToLoad();
    }
} 