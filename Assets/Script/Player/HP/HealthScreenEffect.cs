using UnityEngine;

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
        if (material == null || maxHealth <= 0)
            return;

        float ratio = currentHealth / maxHealth;

        if (ratio >= 1f)
            Set(settings.NormalVignettePower, settings.NormalBreathFrequency);
        else if (ratio > settings.LowHealthRatio)
            Set(settings.HurtVignettePower, settings.NormalBreathFrequency);
        else
            Set(settings.LowVignettePower, settings.LowBreathFrequency);
    }

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