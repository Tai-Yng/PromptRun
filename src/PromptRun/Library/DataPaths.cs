using System.IO;

namespace PromptRun.Library;

/// <summary>
/// Resolves the plugin data directory. Non-portable default mirrors what Wox's
/// PluginJsonStorage computes: %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\PromptRun.
/// The host adapter may override the base path with the Wox-resolved directory (portable mode).
/// </summary>
public static class DataPaths
{
    public const string FolderName = "PromptRun";
    public const string DataFileName = "prompts.json";

    public static string DefaultBasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Microsoft", "PowerToys", "PowerToys Run", "Settings", "Plugins", FolderName);
    }
}
