using PromptRun;
using PromptRun.Library;
using Wox.Plugin;

namespace PromptRun.Tests;

public sealed class MainTests : IDisposable
{
    private readonly string _dir;
    private readonly PromptLibrary _library;
    private readonly List<string> _notified = new();
    private readonly List<string> _clipboardTexts = new();
    private bool _folderOpened;

    public MainTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "promptrun-main-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _library = new PromptLibrary(_dir);
        File.WriteAllText(_library.FilePath,
            """{"version":1,"prompts":[{"id":"1","title":"Translate","content":"translate {{lang}} now","tags":["demo"]}]}""");
        _library.Initialize();
    }

    public void Dispose()
    {
        _library.Dispose();
        try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
    }

    private Main CreateMain() => new(
        new FakeClipboard(text => _clipboardTexts.Add(text)),
        _library,
        msg => _notified.Add(msg),
        () => _folderOpened = true);

    private Result SingleResult(string search)
    {
        var main = CreateMain();
        return Assert.Single(main.Query(new Query(search)));
    }

    [Fact]
    public void Query_EmptySearch_ListsPromptsFirstThenManagementEntries()
    {
        var main = CreateMain();
        var results = main.Query(new Query(""));

        // 1 prompt in the fixture + 3 management actions
        Assert.Equal(4, results.Count);
        Assert.Equal("Translate", results[0].Title); // prompts come first
        Assert.Equal("Open data folder", results[1].Title);
        Assert.Contains(results, r => r.Title == "Push to GitHub");
        Assert.Contains(results, r => r.Title == "Pull from GitHub");
    }

    [Fact]
    public void Query_EmptySearch_ShowsTemplateNoticeOnFirstRun()
    {
        var dir = Path.Combine(Path.GetTempPath(), "promptrun-tpl-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            using var library = new PromptLibrary(dir);
            library.Initialize();
            Assert.True(library.TemplateCreated);

            var main = new Main(new FakeClipboard(_ => { }), library, _ => { }, () => { });
            var results = main.Query(new Query(""));

            // 2 template prompts + 3 management + 1 template notice
            Assert.Equal(6, results.Count);
            Assert.Contains(results, r => r.Title.Contains("Template created"));
            Assert.Equal("专业翻译助手", results[0].Title); // prompts first, notice last
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void Query_Keyword_MapsSearchResultWithPlaceholderMark()
    {
        var result = SingleResult("translate");

        Assert.Equal("Translate", result.Title);
        Assert.Contains("demo", result.SubTitle);
        Assert.Contains("contains unfilled variables", result.SubTitle);
    }

    [Fact]
    public void EnterAction_CopiesContentNotifiesAndBumpsUseCount()
    {
        var result = SingleResult("translate");

        Assert.True(result.Action!(null!));

        Assert.Equal("translate {{lang}} now", Assert.Single(_clipboardTexts));
        Assert.Equal("Copied: Translate", Assert.Single(_notified));
        Assert.Equal(1, _library.Entries[0].UseCount);
    }

    [Fact]
    public void ContextMenu_OpenDataFolder_Works()
    {
        var main = CreateMain();
        var result = Assert.Single(main.Query(new Query("translate")));

        var menus = main.LoadContextMenus(result);

        var menu = Assert.Single(menus, m => m.Title == "Open data folder");
        Assert.True(menu.Action!(null!));
        Assert.True(_folderOpened);
    }

    private sealed class FakeClipboard(Action<string> setter) : IClipboardService
    {
        public void SetText(string text) => setter(text);
    }
}
