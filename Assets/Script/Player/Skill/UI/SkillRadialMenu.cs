using System.Collections.Generic;
using UnityEngine;

public class SkillRadialMenu : MonoBehaviour
{
    [SerializeField] private float radius = 300f;
    [SerializeField] private float startAngle = 90f;
    // [SerializeField] private GameObject slotPrefab;

    public List<RectTransform> slots = new();
    // private Dictionary<Skill, GameObject> skillSlotPair = new();

    private SkillManager manager;

    void Awake()
    {
        manager = GetComponentInParent<SkillManager>();
    }

    void Update()
    {
        UpdateLayout();
    }

    void UpdateLayout()
    {
        // foreach(var skill in manager.AllPhysSkills)
        // {
        //     if(!skillSlotPair.ContainsKey(skill))
        //     {
        //         GameObject slot = Instantiate(slotPrefab, transform);
        //         skillSlotPair.Add(skill, slot);
        //     }
        // }
        int count = slots.Count;
        float step = 360f / count;

        for(int i = 0; i < count; i++)
        {
            float angle = startAngle - step * i;
            float rad = angle * Mathf.Deg2Rad;

            slots[i].anchoredPosition = new(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
        }
    }
}