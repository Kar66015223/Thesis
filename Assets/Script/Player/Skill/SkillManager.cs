using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class SkillManager : MonoBehaviour
{
    public List<Skill> AllPhysSkills { get; private set; } = new();
    public Skill EquippedPhysSkill { get; private set; }

    private PlayerInputHandler inputHandler;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        inputHandler.OnPhysicalSkillInput += UseEquippedPhysSkill;
    }

    void OnDisable()
    {
        inputHandler.OnPhysicalSkillInput -= UseEquippedPhysSkill;
    }

    void Start()
    {
        AllPhysSkills.Add(new Tackle());
        EquippedPhysSkill = AllPhysSkills.FirstOrDefault();

        Debug.Log(EquippedPhysSkill.skillName);
    }

    public void UseEquippedPhysSkill(bool isHolding)
    {
        switch(EquippedPhysSkill.usage)
        {
            case SkillUsage.Press:
                if(isHolding)
                    EquippedPhysSkill.Use();
                break;

            case SkillUsage.Hold:
                EquippedPhysSkill.HoldUse(isHolding);
                break;

            case SkillUsage.Both:
                EquippedPhysSkill.HandleUse();
                break;
        }
    }
}