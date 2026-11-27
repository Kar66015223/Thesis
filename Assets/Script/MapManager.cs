using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    private string defaultActionMapName;
    public string DefaultActionMapName => defaultActionMapName;

    private bool isBase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        isBase = SceneManager.GetActiveScene().name == SceneConstants.HIDEOUT_NAME;

        defaultActionMapName = isBase ?
            PlayerConstants.ACTIONMAP_PLAYER_BASE : PlayerConstants.ACTIONMAP_PLAYER_MISSION;
    }

    void Start()
    {
        if (LoadingScreen.IsLoading)
            LoadingScreen.Finished += ShowFirstObjective;
        else
            ShowFirstObjective();
    }

    void OnDestroy() => LoadingScreen.Finished -= ShowFirstObjective;
    
    private void ShowFirstObjective()
    {
        LoadingScreen.Finished -= ShowFirstObjective;

        if (ObjectiveUIManager.Instance != null && ObjectiveUIManager.Instance.allObjectives.Count > 0)
            UIEvents.RequestChangeObjective(ObjectiveUIManager.Instance.allObjectives[0]);
    }
}