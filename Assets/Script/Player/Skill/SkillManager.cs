using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class SkillManager : MonoBehaviour
{
    public List<Skill> AllPhysSkills { get; private set; } = new();
    public Skill EquippedPhysSkill { get; private set; }
    private SkillContext context;

    [Header("Skills Data")]
    [SerializeField] private TackleData tackleData;

    private PlayerInputHandler inputHandler;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        context = new SkillContext { User = gameObject };
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
        // AllPhysSkills.Add(new Tackle(tackleData, context));
        EquippedPhysSkill = AllPhysSkills.FirstOrDefault();
    }

    void Update()
    {
        EquippedPhysSkill?.Tick(Time.deltaTime);
    }

    public void UseEquippedPhysSkill(bool isHolding)
    {
        if (EquippedPhysSkill == null)
            return;

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