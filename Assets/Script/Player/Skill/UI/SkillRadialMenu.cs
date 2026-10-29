using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillRadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject slotParent;
    [SerializeField] private float radius = 300f;
    [SerializeField] private float startAngle = 90f;
    [SerializeField] private GameObject slotPrefab;

    private readonly Dictionary<Skill, RectTransform> skillSlots = new();
    private readonly List<RectTransform> slots = new();

    private SkillManager manager;
    private PlayerInputHandler inputHandler;

    void Awake()
    {
        manager = GetComponentInParent<SkillManager>();
        inputHandler = GetComponentInParent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        inputHandler.OnPhysSkillMenuInput += ToggleMenu;
        manager.SkillsChanged += RebuildSlots;
        RebuildSlots();
    }

    void OnDisable()
    {
        inputHandler.OnPhysSkillMenuInput -= ToggleMenu;
        manager.SkillsChanged -= RebuildSlots;
        InputModeService.RequestCursor(this, false);
    }

    private void ToggleMenu(bool isOn)
    {
        slotParent.SetActive(isOn);
        InputModeService.RequestCursor(this, isOn);
    }

    private void RebuildSlots()
    {
        foreach (Skill skill in manager.AllPhysSkills)
        {
            if (skillSlots.ContainsKey(skill))
                continue;

            GameObject slot = Instantiate(slotPrefab, slotParent.transform);
            if (slot.TryGetComponent(out Image image))
                image.sprite = skill.Icon;

            RectTransform slotRect = slot.GetComponent<RectTransform>();
            skillSlots.Add(skill, slotRect);
            slots.Add(slotRect);
        }

        LayoutSlots();
    }

    private void LayoutSlots()
    {
        if (slots.Count == 0)
            return;

        float step = 360f / slots.Count;
        for (int i = 0; i < slots.Count; i++)
        {
            float angle = (startAngle - step * i) * Mathf.Deg2Rad;
            slots[i].anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }
    }
}
