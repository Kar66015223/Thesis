using UnityEngine;

/// <summary>
/// One step of the level's objective chain. It owns its own state (is it the current objective,
/// has it been completed) instead of every interactable keeping a "hasInteracted" copy.
/// </summary>
public class ObjectiveHighlight : MonoBehaviour, IScannable
{
    public const string layerName = "ObjectiveHighlight";

    private LayerHighlighter highlighter;
    private bool isCompleted;

    // Kept even while not current: the scan is a wide toggle, so the next objective is usually
    // already inside it when it becomes current and gets no new OnScanEnter.
    private bool isInScan;

    public bool IsCurrent { get; private set; }

    // Lazy, so it also works when ObjectiveManager reaches an object that is still inactive.
    private LayerHighlighter Highlighter => highlighter ??= new LayerHighlighter(gameObject, layerName);

    /// <summary>Called by ObjectiveManager. Becoming current again resets completion.</summary>
    public void SetCurrent(bool isCurrent)
    {
        IsCurrent = isCurrent;
        isCompleted = false;
        RefreshHighlight();
    }

    /// <summary>Called by the interactable linked to this objective.</summary>
    public void ReportInteraction()
    {
        if (!IsCurrent || isCompleted)
            return;

        isCompleted = true;
        RefreshHighlight();
        WorldEvents.RaiseObjectiveCompleted(this);
    }

    public void OnScanEnter()
    {
        isInScan = true;
        RefreshHighlight();
    }

    public void OnScanExit()
    {
        isInScan = false;
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (IsCurrent && !isCompleted && isInScan)
            Highlighter.Apply();
        else
            Highlighter.Remove();
    }
}
