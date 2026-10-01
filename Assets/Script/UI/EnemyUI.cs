using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private GameObject struggleRootPanel;

    void OnEnable()
    {
        GameEvent.OnToggleStruggleUI += ToggleStruggleUI;
    }

    void OnDisable()
    {
        GameEvent.OnToggleStruggleUI -= ToggleStruggleUI;
    }

    private void ToggleStruggleUI(bool isOn)
    {
        struggleRootPanel.SetActive(isOn);
    }
}