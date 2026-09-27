using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.PowerToys.Settings.UI.Library;
using PromptRun.Library;
using PromptRun.Sync;
using Wox.Plugin;

namespace PromptRun;

public sealed class Main : IPlugin, IContextMenu, ISettingProvider
{
    private const string IconPath = "Images/icon.png";

    private readonly PromptRunSettings _settings = new();
    private IClipboardService _clipboard = new WpfClipboardService();
    private PromptLibrary? _library;
    private ISyncCoordinator? _sync;
    private Action<string> _notify = static _ => { };
    private Action _openDataFolder = static () => { };

    public string Name => "PromptRUN";
    public string Description => "Search and copy your hand-maintained prompt snippets.";

    /// <summary>Host-side anti-GPO-bypass check: must equal the ID in plugin.json.</summary>
    public static string PluginID => "41cb6646-ea9f-44b3-bb4b-509555f24f44";

    public Main() { }

    internal Main(
        IClipboardService clipboard,
        PromptLibrary library,
        Action<string> notify,
        Action openDataFolder,
        ISyncCoordinator? sync = null)
    {
        _clipboard = clipboard;
        _library = library;
        _notify = notify;
        _openDataFolder = openDataFolder;
        _sync = sync;
    }

    public void Init(PluginInitContext context)
    {
        var basePath = ResolveBasePath();
        var library = new PromptLibrary(basePath);
        library.Initialize();

        _library = library;
        _notify = message => context.API.ShowNotification("PromptRUN", message);
        _openDataFolder = () => Process.Start(new ProcessStartInfo
        {
            FileName = basePath,
            UseShellExecute = true,
        });
        _sync = new GitHubSyncCoordinator(library, () => _settings.GetConfig(), _notify);
    }

    internal void AttachSyncCoordinator(ISyncCoordinator sync) => _sync = sync;

    /// <summary>Mirrors Wox's PluginJsonStorage location (portable-mode aware).</summary>
    private static string ResolveBasePath()
    {
        try
        {
            var dataDirectory = Wox.Plugin.Constant.DataDirectory;
            return Path.Combine(dataDirectory, "Settings", "Plugins", DataPaths.FolderName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or TypeInitializationException)
        {
            return DataPaths.DefaultBasePath();
        }
    }

    public List<Result> Query(Query query)
    {
        var library = _library;
        if (library is null) return new List<Result>();

        var search = query.Search;
        var results = new List<Result>();

        if (string.IsNullOrWhiteSpace(search))
        {
            // Browsing mode: the whole library first, management actions at the end.
            results.AddRange(PromptSearch.All(library.Entries).Select(ToResult));
            results.AddRange(ManagementResults());

            if (library.IsCorrupt)
                results.Add(Notice(
                    "prompts.json is corrupt or unreadable — library is empty",
                    "Click to open the data folder and inspect the file"));
            else if (library.TemplateCreated)
                results.Add(Notice(
                    $"Template created at {library.FilePath}",
                    "Edit the file to add your own prompts — click to open the folder"));
            return results;
        }

        results.AddRange(PromptSearch.Search(library.Entries, search).Select(ToResult));
        return results;
    }

    private Result ToResult(PromptEntry entry)
    {
        var hasPlaceholder = PromptSearch.HasPlaceholder(entry);
        return new Result
        {
            Title = entry.Title,
            SubTitle = BuildSubTitle(entry, hasPlaceholder),
            IcoPath = IconPath,
            ContextData = entry,
            Action = _ =>
            {
                _clipboard.SetText(entry.Content);
                _notify($"Copied: {entry.Title}");
                _library?.IncrementUseCount(entry.Id);
                return true;
            },
        };
    }

    private static string BuildSubTitle(PromptEntry entry, bool hasPlaceholder)
    {
        var summary = entry.Content.Length > 60 ? entry.Content[..60] + "…" : entry.Content;
        var parts = new List<string>();
        if (entry.Tags.Count > 0) parts.Add(string.Join(", ", entry.Tags));
        parts.Add(summary);
        if (hasPlaceholder) parts.Add("contains unfilled variables");
        return string.Join(" · ", parts);
    }

    private List<Result> ManagementResults() => new()
    {
        new Result
        {
            Title = "Open data folder",
            SubTitle = "Open the folder that contains prompts.json",
            IcoPath = IconPath,
            Action = _ =>
            {
                _openDataFolder();
                return true;
            },
        },
        new Result
        {
            Title = "Push to GitHub",
            SubTitle = "Upload local prompts.json to the configured private repo",
            IcoPath = IconPath,
            Action = _ =>
            {
                var sync = _sync;
                if (sync is null) _notify("Sync is not configured yet");
                else Task.Run(sync.Push);
                return true;
            },
        },
        new Result
        {
            Title = "Pull from GitHub",
            SubTitle = "Overwrite local prompts.json (a .bak backup is made first)",
            IcoPath = IconPath,
            Action = _ =>
            {
                var sync = _sync;
                if (sync is null) _notify("Sync is not configured yet");
                else Task.Run(sync.Pull);
                return true;
            },
        },
    };

    private Result Notice(string title, string subTitle) => new()
    {
        Title = title,
        SubTitle = subTitle,
        IcoPath = IconPath,
        Action = _ =>
        {
            _openDataFolder();
            return true;
        },
    };

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is not PromptEntry) return new List<ContextMenuResult>();

        return new List<ContextMenuResult>
        {
            new()
            {
                PluginName = Name,
                Title = "Open data folder",
                Glyph = "\xE838", // Segoe MDL2 Assets: Folder
                AcceleratorKey = Key.O,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ =>
                {
                    _openDataFolder();
                    return true;
                },
            },
        };
    }

    // ISettingProvider: declarative Textbox options; the host renders the panel.
    public Control CreateSettingPanel() => null!;

    public void UpdateSettings(PowerLauncherPluginSettings settings) => _settings.Apply(settings);

    public IEnumerable<PluginAdditionalOption> AdditionalOptions => _settings.Options;
}
