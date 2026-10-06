using UnityEngine;
using UnityEngine.UI;

public class PlayerStruggleUI : MonoBehaviour
{
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private Image struggleBar;

    void OnEnable()
    {
        GameEvent.OnToggleStruggleUI += ToggleUI;
        GameEvent.OnStruggleProgressChanged += UpdateStruggleBar;
    }

    void OnDisable()
    {
        GameEvent.OnToggleStruggleUI -= ToggleUI;
        GameEvent.OnStruggleProgressChanged -= UpdateStruggleBar;
    }

    private void ToggleUI(bool isOn)
    {
        UIPanel.SetActive(isOn);
        if (isOn)
            struggleBar.fillAmount = 0f;
    }

    private void UpdateStruggleBar(float progress) => struggleBar.fillAmount = progress;
}