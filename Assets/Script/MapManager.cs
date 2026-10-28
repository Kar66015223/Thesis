using UnityEngine;
using UnityEngine.SceneManagement;

// Runs before other scripts, so anything that asks for DefaultActionMapName in its Awake/OnEnable already gets an answer.
[DefaultExecutionOrder(-100)]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    [SerializeField] private string defaultActionMapName;
    public string DefaultActionMapName => defaultActionMapName;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        bool isBase = SceneManager.GetActiveScene().name == SceneConstants.HIDEOUT_NAME;
        defaultActionMapName = isBase ?
            PlayerConstants.ACTIONMAP_PLAYER_BASE : PlayerConstants.ACTIONMAP_PLAYER_MISSION;
    }
}