using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPresenter : IUIPresenter
{
    private readonly GameObject panel;
    private readonly TMP_Text headerText;
    private readonly TMP_Text bodyText;
    private readonly Image img;

    private readonly Button nextButton;

    public TutorialPresenter(GameObject panel, TMP_Text headerText, TMP_Text bodyText, Button nextButton, Image img)
    {
        this.panel = panel;
        this.headerText = headerText;
        this.bodyText = bodyText;
        this.img = img;

        this.nextButton = nextButton;
    }

    public void Bind()
    {
        UIEvents.TutorialToggled += Toggle;
        if (nextButton != null)
            nextButton.onClick.AddListener(Hide);
    }

    public void Unbind()
    {
        UIEvents.TutorialToggled -= Toggle;
        if (nextButton != null)
            nextButton.onClick.RemoveListener(Hide);

        InputModeService.RequestUI(this, false);
    }

    private void Toggle(bool isOpen, TutorialData data)
    {
        if (panel != null)
            panel.SetActive(isOpen);

        if (data != null)
        {
            headerText.text = data.headerText;
            bodyText.text = data.bodyText;
            img.sprite = data.sprite;
        }
        else
        {
            headerText.text = "";
            bodyText.text = "";
            img.sprite = null;
        }

        InputModeService.RequestUI(this, isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
    }

    private void Hide() => UIEvents.ToggleTutorialPanel(false);
}