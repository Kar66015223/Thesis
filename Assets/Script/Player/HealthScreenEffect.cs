using UnityEngine;

/// <summary>
/// Drives the blood/vignette full-screen material from a health ratio.
/// Split out of PlayerHealth so health rules and visuals can change independently.
/// </summary>
public class HealthScreenEffect
{
    private static readonly int VignettePowerId = Shader.PropertyToID("_VignettePower");
    private static readonly int BreathFrequencyId = Shader.PropertyToID("_BreathFrequency");

    public struct Settings
    {
        public float NormalVignettePower;
        public float HurtVignettePower;
        public float LowVignettePower;
        public float NormalBreathFrequency;
        public float LowBreathFrequency;
        public float LowHealthRatio;
    }

    private readonly Material material;
    private readonly Settings settings;

    public HealthScreenEffect(Material material, Settings settings)
    {
        this.material = material;
        this.settings = settings;
    }

    public void Apply(float currentHealth, float maxHealth)
    {
        if (material == null || maxHealth <= 0f)
            return;

        float ratio = currentHealth / maxHealth;

        if (ratio >= 1f)
            Set(settings.NormalVignettePower, settings.NormalBreathFrequency);
        else if (ratio > settings.LowHealthRatio)
            Set(settings.HurtVignettePower, settings.NormalBreathFrequency);
        else
            Set(settings.LowVignettePower, settings.LowBreathFrequency);
    }

    /// <summary>The material is a shared asset, so restore it or the Editor keeps the "hurt" look.</summary>
    public void Restore()
    {
        if (material != null)
            Set(settings.NormalVignettePower, settings.NormalBreathFrequency);
    }

    private void Set(float vignettePower, float breathFrequency)
    {
        material.SetFloat(VignettePowerId, vignettePower);
        material.SetFloat(BreathFrequencyId, breathFrequency);
    }
}
