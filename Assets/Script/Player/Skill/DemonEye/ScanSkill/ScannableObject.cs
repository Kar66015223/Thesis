using UnityEngine;

public class ScannableObject : MonoBehaviour, IScannable
{
    [Header("Highlight Settings")]
    [Tooltip("ชื่อ Layer ที่ตั้งไว้ใน URP Render Objects (ต้องพิมพ์ให้ตรงกัน) - must match the URP Render Objects layer")]
    [SerializeField] private string highlightLayerName = "RenderOnTop";
    private LayerHighlighter highlighter;
    private LayerHighlighter Highlighter => highlighter ??= new LayerHighlighter(gameObject, highlightLayerName);

    [SerializeField] private LayerMask sightBlockMask;
    [SerializeField] private Vector3 sightTargetOffset = new(0f, 1f, 0f);

    [SerializeField] private SkillUpgradeTrigger trigger = SkillUpgradeTrigger.None;
    [Tooltip("Seconds after giving upgrade progress before this object can give it again.")]
    [SerializeField, Min(0f)] private float progressCooldown = 5f;

    private float nextProgressTime;

    public void OnScanEnter()
    {
        Highlighter.Apply();

        if (trigger == SkillUpgradeTrigger.None || Time.time < nextProgressTime)
            return;

        if (!IsHiddenFromCamera())
        {
            Debug.Log($"Enemy wasn't blocked by anything");
            return;
        }

        nextProgressTime = Time.time + progressCooldown;
        SkillUpgradeProgress.Report(trigger);
    }

    public void OnScanExit() => Highlighter.Remove();

    private bool IsHiddenFromCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Vector3 target = transform.position + sightTargetOffset;
        return Physics.Linecast(cam.transform.position, target, sightBlockMask, QueryTriggerInteraction.Ignore);
    }
}
