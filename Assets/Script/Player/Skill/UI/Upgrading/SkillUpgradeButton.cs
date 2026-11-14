using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Toggle))]
public class SkillUpgradeButton : MonoBehaviour
{
    [SerializeField] private SkillUpgradeData data;
    
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.black;

    [SerializeField] private TMP_Text upgradeName;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text progressText;

    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ToggleUpgradeDetail);
        toggle.group = GetComponentInParent<ToggleGroup>();
    }

    void Update()
    {
        toggle.GetComponent<Image>().color = toggle.isOn ? selectedColor : normalColor;
        upgradeName.color = toggle.isOn ? normalColor : selectedColor;
        progressBar.color = toggle.isOn ? normalColor : selectedColor;
        progressText.color = toggle.isOn ? selectedColor : normalColor;
    }

    public void Bind(SkillUpgradeData data)
    {
        this.data = data;
        upgradeName.text = data.upgradeName;

        Refresh();
    }

    void OnEnable()
    {
        SkillUpgradeProgress.Changed += HandleProgressChanged;
        SkillUpgradeProgress.Unlocked += HandleProgressChanged;
    }

    void OnDisable()
    {
        SkillUpgradeProgress.Changed -= HandleProgressChanged;
        SkillUpgradeProgress.Unlocked -= HandleProgressChanged;
    }

    private void HandleProgressChanged(SkillUpgradeData changed)
    {
        if (changed == data)
            Refresh();
    }

    private void Refresh()
    {
        if (data == null)
            return;

        int value = SkillUpgradeProgress.Get(data);
        progressBar.fillAmount = (float)value / data.maxProgress;

        if (SkillUpgradeProgress.IsUnlocked(data))
            progressText.text = "Learned";
        else
            progressText.text = $"{value} / {data.maxProgress}";
    }

    private void ToggleUpgradeDetail(bool isOpen)
    {
        UIEvents.ToggleSkillUpgradeDetail(isOpen, data);
    }
}