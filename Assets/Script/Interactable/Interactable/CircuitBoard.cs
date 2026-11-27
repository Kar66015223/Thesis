using UnityEngine;

public class CircuitBoard : Interactable
{
    [SerializeField] private ObjectiveData objTiveData;
    private bool hasChangedPath = false;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Renderer lightRend;
    private Material emissionMat;
    // private Color baseEmission;
    [SerializeField] private GameObject pointLight;

    [SerializeField] private ParticleSystem sparkVFX;

    void Awake()
    {
        emissionMat = lightRend.material;
        // baseEmission = emissionMat.GetColor(EmissionColorId);
    }

    void OnDestroy()
    {
        if (emissionMat != null)
            Destroy(emissionMat);
    }

    protected override void OnInteract(GameObject interactor)
    {
        WorldEvents.SetLightsOut(true);
        WorldEvents.RaiseObjectiveShow(true);
        UIEvents.ShowTip("Someone will come to check, I need to hide.");
        UIEvents.RequestChangeObjective(objTiveData);

        pointLight.SetActive(false);
        emissionMat.SetColor(EmissionColorId, Color.black);

        sparkVFX.Play();

        if (!hasChangedPath)
        {
            EnemyEvents.RequestAdvanceAllRoutes();
            hasChangedPath = true;
        }
    }
}
