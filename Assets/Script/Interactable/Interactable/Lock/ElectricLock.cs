using UnityEngine;

public class ElectricLock : Lock
{
    [SerializeField] private bool canUnlock = false;
    [SerializeField] private Transform unlockableSide;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    [SerializeField] private Renderer lockLight;
    private Material emissionMat;
    private Color baseEmission;

    protected override void Awake()
    {
        base.Awake();
        emissionMat = lockLight.material;
        baseEmission = emissionMat.GetColor(EmissionColorId);
    }

    void OnEnable() => WorldEvents.LightsOutChanged += SetCanUnlock;
    void OnDisable() => WorldEvents.LightsOutChanged -= SetCanUnlock;

    void OnDestroy()
    {
        if (emissionMat != null)
            Destroy(emissionMat);
    }

    public override bool TryUnlock(GameObject interactor)
    {
        if (IsOnSide(interactor, unlockableSide) || canUnlock)
        {
            SetLocked(false);
            return true;
        }
        
        // UIEvents.ShowTip("The morse code must be here, I need to find a way to get inside.");
        return false;
    }

    private void SetCanUnlock(bool isLightOut)
    {
        canUnlock = isLightOut;
        SetLockLight(!isLightOut);
    } 

    private void SetLockLight(bool isOn) => emissionMat.SetColor(EmissionColorId, isOn ? baseEmission : Color.black);
}