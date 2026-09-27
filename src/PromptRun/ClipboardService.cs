using System.Runtime.InteropServices;
using System.Windows;

namespace PromptRun;

/// <summary>Clipboard abstraction so the copy path is testable without an STA thread.</summary>
public interface IClipboardService
{
    void SetText(string text);
}

public sealed class WpfClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException)
        {
            // Retry with the raw data object (handles transient clipboard lock / non-STA edge cases).
            Clipboard.SetDataObject(text, true);
        }
    }
}
