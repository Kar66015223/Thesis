using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(() => ToggleSetting(true));
        quitButton.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        SceneLoader.LoadScene(SceneConstants.STARTCUTSCENE_NAME);
    }

    void ToggleSetting(bool isOpen)
    {

    }
    
    void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}