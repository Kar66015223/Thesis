using UnityEngine;

public class LightsOutPresenter : IUIPresenter
{
    private readonly GameObject effect;

    public LightsOutPresenter(GameObject effect) => this.effect = effect;

    public void Bind() => WorldEvents.LightsOutChanged += Toggle;
    public void Unbind() => WorldEvents.LightsOutChanged -= Toggle;

    private void Toggle(bool isLightsOut)
    {
        if (effect != null)
            effect.SetActive(isLightsOut);
    }
}
