using UnityEngine;

public class ScanSkillUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    private ScannerSkill scanSkill;

    void Awake() => scanSkill = GetComponentInParent<ScannerSkill>();
    void Update() => ToggleRootPanel(scanSkill.IsActive);
    public void ToggleRootPanel(bool isShow) => rootPanel.SetActive(isShow);
}