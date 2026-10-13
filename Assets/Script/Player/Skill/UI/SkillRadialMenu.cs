using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillRadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject slotParent;
    
    [SerializeField] private float radius = 300f;
    [SerializeField] private float startAngle = 90f;
    [SerializeField] private GameObject slotPrefab;

    public List<RectTransform> slots = new();
    // [SerializeField] private RectTransform selectedSlot;
    // [SerializeField] private float shortestDistance = 400f;
    private Dictionary<Skill, GameObject> skillSlotPair = new();

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
    }

    void OnDisable()
    {
        inputHandler.OnPhysSkillMenuInput -= ToggleMenu;
    }

    void Update()
    {
        UpdateLayout();
    }

    void ToggleMenu(bool isOn)
    {
        slotParent.SetActive(isOn);
        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void UpdateLayout()
    {
        foreach (var skill in manager.AllPhysSkills)
        {
            if (!skillSlotPair.ContainsKey(skill))
            {
                GameObject slot = Instantiate(slotPrefab, slotParent.transform);
                if (slot.TryGetComponent(out Image img))
                {
                    img.sprite = skill.icon;
                }

                RectTransform slotRT = slot.GetComponent<RectTransform>();

                skillSlotPair.Add(skill, slot);
                slots.Add(slotRT);
            }
        }

        int count = slots.Count;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle - step * i;
            float rad = angle * Mathf.Deg2Rad;

            slots[i].anchoredPosition = new(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
        }
    }
    
    // RectTransform GetClosestSlotToCursor()
    // {
    //     Vector3 mouseScreenPos = Input.mousePosition;

    //     RectTransform closest = null;
    //     float minDist = float.MaxValue;

    //     foreach (var slot in slots)
    //     {
    //         Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, slot.position);
    //         float distance = Vector2.Distance(mouseScreenPos, slotScreenPos);

    //         if (slot.TryGetComponent(out SkillSlot ss))
    //             ss.isSelected = false;

    //         if (distance < minDist)
    //         {
    //             minDist = distance;
    //             closest = slot;
    //         }
    //     }

    //     if(closest != null && minDist <= shortestDistance)
    //     {
    //         if (closest.TryGetComponent(out SkillSlot ss))
    //             ss.isSelected = true;

    //         return closest;
    //     }

    //     return null;
    // }
}