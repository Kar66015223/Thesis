using UnityEngine;
using UnityEngine.UI;

public class GameOverPresenter : IUIPresenter
{
    private readonly GameObject panel;
    private readonly Button restartButton;

    public GameOverPresenter(GameObject panel, Button restartButton)
    {
        this.panel = panel;
        this.restartButton = restartButton;
    }

    public void Bind()
    {
        PlayerEvents.Died += Show;
        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
    }

    public void Unbind()
    {
        PlayerEvents.Died -= Show;
        if (restartButton != null)
            restartButton.onClick.RemoveListener(Restart);

        InputModeService.RequestUI(this, false);
    }

    private void Show() => SetVisible(true);

    private void Restart()
    {
        SetVisible(false);

        // Respawn at the last checkpoint; with no checkpoint reached yet, start the scene over.
        CheckpointManager checkpoints = CheckpointManager.Instance;
        if (checkpoints == null || !checkpoints.Load())
            SceneLoader.RestartScene();
    }

    private void SetVisible(bool isVisible)
    {
        if (panel != null)
            panel.SetActive(isVisible);

        InputModeService.RequestUI(this, isVisible);
    }
}
