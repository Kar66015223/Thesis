using UnityEngine;
using UnityEngine.UI;

public class UIQuestNPC : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject questButtonPrefab;
    [SerializeField] private Button closeButton;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => ToggleRootPanel(false));
    }

    void OnEnable()
    {
        GameEvent.OnToggleUIQuestNPC += ToggleRootPanel;
    }

    void OnDisable()
    {
        GameEvent.OnToggleUIQuestNPC -= ToggleRootPanel;
    }

    private void ToggleRootPanel(bool isOn)
    {
        rootPanel.SetActive(isOn);
        if (isOn)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}