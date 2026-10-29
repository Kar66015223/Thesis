using UnityEngine;

public class ObjectiveHighlight : MonoBehaviour, IScannable
{
    public const string layerName = "ObjectiveHighlight";

    private LayerHighlighter highlighter;
    private bool isCompleted;

    private bool isInScan;

    public bool IsCurrent { get; private set; }
    public bool IsShow { get; private set; }

    private LayerHighlighter Highlighter => highlighter ??= new LayerHighlighter(gameObject, layerName);

    public void SetCurrent(bool isCurrent)
    {
        IsCurrent = isCurrent;
        isCompleted = false;
        RefreshHighlight();
    }

    public void SetShow(bool isShow)
    {
        IsShow = isShow;
        RefreshHighlight();
    }

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
        if (IsCurrent && !isCompleted && isInScan && IsShow)
            Highlighter.Apply();
        else
            Highlighter.Remove();
    }
}
