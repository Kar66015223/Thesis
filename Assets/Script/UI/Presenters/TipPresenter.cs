using TMPro;

/// <summary>Shows short hint messages that fade out.</summary>
public class TipPresenter : IUIPresenter
{
    private readonly TMP_Text tipsText;
    private readonly FadeOutText fade;

    public TipPresenter(TMP_Text tipsText)
    {
        this.tipsText = tipsText;
        if (tipsText != null)
            tipsText.TryGetComponent(out fade);
    }

    public void Bind() => UIEvents.TipRequested += Show;
    public void Unbind() => UIEvents.TipRequested -= Show;

    private void Show(string message)
    {
        if (fade != null)
            fade.Show(message);
        else if (tipsText != null)
            tipsText.text = message;
    }
}
