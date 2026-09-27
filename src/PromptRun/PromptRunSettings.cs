using Microsoft.PowerToys.Settings.UI.Library;

namespace PromptRun;

/// <summary>
/// Declares the two Textbox settings (repo, PAT) for the PowerToys Run settings UI
/// and keeps the latest user-supplied values for the sync coordinator.
/// </summary>
internal sealed class PromptRunSettings
{
    private const string RepoKey = "GithubRepo";
    private const string TokenKey = "GithubPat";

    private string _repo = string.Empty;
    private string _token = string.Empty;

    public (string Repo, string Token) GetConfig() => (_repo, _token);

    public List<PluginAdditionalOption> Options { get; } = new()
    {
        new()
        {
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            Key = RepoKey,
            DisplayLabel = "GitHub repo (owner/name)",
            DisplayDescription = "Private repo where prompts.json is synced",
            PlaceholderText = "your-name/your-private-repo",
        },
        new()
        {
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            Key = TokenKey,
            DisplayLabel = "Fine-grained personal access token",
            DisplayDescription = "Limit the token to the repo above with Contents read/write only",
            PlaceholderText = "github_pat_...",
            TextBoxMaxLength = 255,
        },
    };

    public void Apply(PowerLauncherPluginSettings settings)
    {
        var options = settings.AdditionalOptions;
        if (options is null) return;

        _repo = options.FirstOrDefault(o => o.Key == RepoKey)?.TextValue ?? string.Empty;
        _token = options.FirstOrDefault(o => o.Key == TokenKey)?.TextValue ?? string.Empty;
    }
}
