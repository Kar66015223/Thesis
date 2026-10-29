using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadPanelPresenter : IUIPresenter
{
    private readonly GameObject panel;
    private readonly TMP_Text text;
    private readonly Button closeButton;

    public ReadPanelPresenter(GameObject panel, TMP_Text text, Button closeButton)
    {
        this.panel = panel;
        this.text = text;
        this.closeButton = closeButton;

        if (text != null)
            text.text = "";
    }

    public void Bind()
    {
        UIEvents.ReadPanelToggled += Toggle;
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Unbind()
    {
        UIEvents.ReadPanelToggled -= Toggle;
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        InputModeService.RequestUI(this, false);
    }

    private void Close() => UIEvents.ToggleReadPanel(false);

    private void Toggle(bool isOpen, string content)
    {
        if (panel != null)
            panel.SetActive(isOpen);
        if (text != null)
            text.text = content;

        InputModeService.RequestUI(this, isOpen);
    }
}
