namespace PromptRun;

/// <summary>Implemented by the GitHub sync coordinator (wired in Init once settings exist).</summary>
internal interface ISyncCoordinator
{
    void Push();

    void Pull();
}
