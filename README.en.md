# PromptRUN

A PowerToys Run plugin for prompt snippets: press Alt+Space, search your hand-maintained prompt library, and copy the full text with one keystroke.

Fully standalone — the plugin owns its data file and depends on nothing else.

## Features

- **Search + copy**: `pp <keywords>` multi-token AND matching (title / tags / content, case-insensitive); Enter copies the full prompt to the clipboard and shows a notification
- **Smart ranking**: favorites first → use count → match weight (title > tags > content) → recently updated
- **Placeholder mark**: entries whose content contains `{{variables}}` get a "contains unfilled variables" subtitle; placeholders are copied as-is
- **Hot reload**: edit the data file in any editor — changes apply within ~300ms without restarting
- **Usage count**: a successful copy bumps `useCount` so ranking improves over time
- **Management panel**: typing only `pp` shows three actions — open data folder / push to GitHub / pull from GitHub
- **Private-repo sync**: single-file sync of `prompts.json` via the GitHub Contents API; push overwrites the remote, pull overwrites the local file after backing it up as `prompts.json.bak`

## Install

1. Close PowerToys (tray icon → exit, or kill `PowerToys.exe`)
2. Download `PromptRun.zip` from the latest release and extract it to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PromptRun\`
3. Reopen PowerToys

Type `pp ` in PowerToys Run — if the management panel appears, you are done.

## Changing the action keyword

The default keyword is `pp`. Change it in PowerToys settings → PowerToys Run → PromptRUN → Action Keyword (native PowerToys support).

## Data file

Location (a template with examples is generated on first run):

```
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\PromptRun\prompts.json
```

Format (schema v1 — edit it directly with any editor):

```json
{
  "version": 1,
  "prompts": [
    {
      "id": "any unique string",
      "title": "display title",
      "content": "the full text copied to the clipboard",
      "tags": ["search tags"],
      "favorite": false,
      "useCount": 0,
      "createdAt": 1758888888888,
      "updatedAt": 1758888888888
    }
  ]
}
```

- Entries missing `title` or `content` are skipped; a corrupt file degrades to an empty library with a hint instead of crashing
- Timestamps are Unix milliseconds; `id` should be a GUID

## GitHub sync setup (3 steps)

1. Create a **private** GitHub repository
2. Create a **fine-grained personal access token** (Settings → Developer settings → Fine-grained tokens):
   - Repository access: **only the repository above**
   - Permissions: **Contents → Read and write** (nothing else)
3. PowerToys settings → PowerToys Run → PromptRUN — fill in `GitHub repo (owner/name)` and the token

Then type `pp` in PowerToys Run and use Push / Pull. Multi-machine sync: install the plugin on each machine, configure the same repo and token, and move data with Push / Pull.

> The PAT is stored in plain text inside PowerToys settings — always use a minimal-scope fine-grained token.

## Building from source

```powershell
dotnet build -c Release   # requires the .NET 10 SDK (PowerToys 0.97+ runs on .NET 8/9/10)
dotnet test               # run the unit tests
```

The output lives in `src/PromptRun/bin/Release/net10.0-windows10.0.26100.0/` (distribution needs only `PromptRun.dll` + `plugin.json` + `Images/`).

## Troubleshooting

- **`pp` shows nothing**: make sure PromptRUN is not disabled in PowerToys Run settings; make sure `plugin.json` sits at the root of the `PromptRun` folder
- **"prompts.json is corrupt"**: check the JSON syntax, or delete the file to regenerate the template
- **Sync fails**: check that the token has not expired, the repo is spelled correctly, and the token has Contents read/write on that repo

## License

MIT
