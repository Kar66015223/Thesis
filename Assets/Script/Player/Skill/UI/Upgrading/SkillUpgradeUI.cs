using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button closeButton;

    [SerializeField] private Transform contentParent;
    [SerializeField] private SkillUpgradeButton skillUpgradePrefab;

    [SerializeField] private GameObject upgradeDetailPanel;
    [SerializeField] private TMP_Text upgradeName;
    [SerializeField] private TMP_Text upgradeDesc;
    [SerializeField] private Image upgradeUsageImg;

    [Header("Claim")]
    [Tooltip("Button in the detail panel. Only clickable when the selected upgrade's progress is full and it isn't learned yet.")]
    [SerializeField] private Button learnButton;
    [SerializeField] private TMP_Text learnButtonText;
    [SerializeField] private string learnLabel = "Learn";
    [SerializeField] private string learnedLabel = "Learned";
    [SerializeField] private string lockedLabel = "Can't Learn";

    private SkillUpgradeManager manager;
    private SkillUpgradeData selected;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => ToggleRootPanel(false));
        if (learnButton != null)
            learnButton.onClick.AddListener(LearnSelected);

        manager = GetComponent<SkillUpgradeManager>();
    }

    void OnEnable()
    {
        UIEvents.SkillUpgradeToggled += ToggleRootPanel;
        UIEvents.SkillUpgradeDetailToggled += ToggleUpgradeDetail;
    }

    void OnDisable()
    {
        UIEvents.SkillUpgradeToggled -= ToggleRootPanel;
        UIEvents.SkillUpgradeDetailToggled -= ToggleUpgradeDetail;
    }

    private void ToggleRootPanel(bool isOpen)
    {
        rootPanel.SetActive(isOpen);
        InputModeService.RequestUI(this, isOpen);

        RefreshUpgradeButtons();
    }

    public void RefreshUpgradeButtons()
    {
        foreach (Transform child in contentParent)
        {
            if (child.TryGetComponent(out SkillUpgradeButton _))
                Destroy(child.gameObject);
        }

        if (manager == null || skillUpgradePrefab == null)
            return;

        for(int i = 0; i < manager.AvailableUpgrades.Count; i++)
        {
            SkillUpgradeData data = manager.AvailableUpgrades[i];
            SkillUpgradeButton button = Instantiate(skillUpgradePrefab, contentParent);
            button.Bind(data);
        }
    }
    
    private void ToggleUpgradeDetail(bool isOpen, SkillUpgradeData data)
    {
        if (!isOpen && selected != null && data != selected)
            return;

        upgradeDetailPanel.SetActive(isOpen);
        if (data == null || !isOpen)
        {
            selected = null;
            upgradeName.text = "";
            upgradeDesc.text = "";
            upgradeUsageImg.sprite = null;
            RefreshLearnButton();

            return;
        }

        selected = data;
        upgradeName.text = data.upgradeName;
        upgradeDesc.text = data.upgradeDesc;
        upgradeUsageImg.sprite = data.upgradeUsageImg;
        RefreshLearnButton();
    }

    private void LearnSelected()
    {
        if (SkillUpgradeProgress.Learn(selected))
            UIEvents.ShowTip($"Learned: {selected.upgradeName}");

        RefreshLearnButton();
    }

    private void RefreshLearnButton()
    {
        if (learnButton == null)
            return;

        learnButton.gameObject.SetActive(selected != null);
        learnButton.interactable = SkillUpgradeProgress.CanLearn(selected);

        if (learnButtonText == null || selected == null)
            return;

        if (SkillUpgradeProgress.IsUnlocked(selected))
            learnButtonText.text = learnedLabel;
        else if (SkillUpgradeProgress.IsComplete(selected))
            learnButtonText.text = learnLabel;
        else
            learnButtonText.text = lockedLabel;
    }
}