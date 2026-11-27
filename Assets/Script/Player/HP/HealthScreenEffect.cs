using UnityEngine;

public class HealthScreenEffect
{
    private static readonly int HealthId = Shader.PropertyToID("_Health");

    public struct Settings
    {
        public float fadeSpeed;
    }

    private readonly Material material;
    private readonly Settings settings;

    private float targetHealth = 1f;
    private float shownHealth = 1f;

    public HealthScreenEffect(Material material, Settings settings)
    {
        this.material = material;
        this.settings = settings;
    }

    public void Apply(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0f)
            targetHealth = Mathf.Clamp01(currentHealth / maxHealth);
    }

    public void Tick(float deltaTime)
    {
        if (material == null || shownHealth == targetHealth)
            return;

        shownHealth = Mathf.MoveTowards(shownHealth, targetHealth, settings.fadeSpeed * deltaTime);
        material.SetFloat(HealthId, shownHealth);
    }

    public void Snap()
    {
        shownHealth = targetHealth;
        if (material != null)
            material.SetFloat(HealthId, shownHealth);
    }

    public void Restore()
    {
        targetHealth = 1f;
        Snap();
    }
}