using UnityEngine;

/// <summary>Opens/closes the password pad and tells it which lock it is for.</summary>
public class PasswordPanelPresenter : IUIPresenter
{
    private readonly GameObject panel;
    private readonly PasswordInputUI input;

    public PasswordPanelPresenter(GameObject panel)
    {
        this.panel = panel;
        if (panel != null)
            panel.TryGetComponent(out input);
    }

    public void Bind() => UIEvents.PasswordPanelToggled += Toggle;

    public void Unbind()
    {
        UIEvents.PasswordPanelToggled -= Toggle;
        InputModeService.RequestUI(this, false);
    }

    private void Toggle(bool isOpen, IPasswordLock target)
    {
        if (panel != null)
            panel.SetActive(isOpen);
        if (input != null)
            input.SetCurrentLock(target);

        InputModeService.RequestUI(this, isOpen);
    }
}
