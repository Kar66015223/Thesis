using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class SkillManager : MonoBehaviour
{
    [Header("Skills Data")]
    [SerializeField] private TackleData tackleData;

    private readonly List<Skill> physicalSkills = new();

    private PlayerInputHandler inputHandler;
    private PlayerController controller;
    private ScannerSkill scanner;
    private SkillContext context;

    public IReadOnlyList<Skill> AllPhysSkills => physicalSkills;
    public Skill EquippedPhysSkill { get; private set; }
    public float ScannerCooldownRemaining => scanner != null ? scanner.CooldownRemaining : 0f;

    public event Action SkillsChanged;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        controller = GetComponent<PlayerController>();
        scanner = GetComponentInChildren<ScannerSkill>();
        context = new SkillContext(gameObject);
    }

    void OnEnable()
    {
        inputHandler.OnPhysicalSkillInput += HandlePhysicalSkillInput;
        inputHandler.OnDemonEyeSkillPressed += ToggleDemonEye;
    }

    void OnDisable()
    {
        inputHandler.OnPhysicalSkillInput -= HandlePhysicalSkillInput;
        inputHandler.OnDemonEyeSkillPressed -= ToggleDemonEye;
    }

    void Start()
    {
        // if (tackleData != null)
            // AddPhysicalSkill(new Tackle(tackleData, context));
    }

    void Update() => EquippedPhysSkill?.Tick(Time.deltaTime);

    public void AddPhysicalSkill(Skill skill)
    {
        physicalSkills.Add(skill);

        if (EquippedPhysSkill == null)
            EquippedPhysSkill = skill;

        SkillsChanged?.Invoke();
    }

    public void RestoreScannerCooldown(float remaining)
    {
        if (scanner == null)
            return;

        scanner.Restore(remaining);
        if (controller != null)
            controller.SetUsingSkill(false);
    }

    private void HandlePhysicalSkillInput(bool isPressed)
    {
        if (EquippedPhysSkill == null)
            return;

        if (isPressed)
            EquippedPhysSkill.OnInputPressed();
        else
            EquippedPhysSkill.OnInputReleased();
    }

    private void ToggleDemonEye()
    {
        if (scanner == null || controller == null)
            return;

        if (scanner.IsActive)
        {
            scanner.Deactivate();
            controller.SetUsingSkill(false);
            return;
        }

        if (controller.CurrentState == PlayerState.Running || !scanner.CanActivate)
            return;

        scanner.Activate();
        controller.SetUsingSkill(true);
    }
}
