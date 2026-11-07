using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SavePlayerSession();
        SceneManager.LoadScene($"{sceneName}");
    }

    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void SavePlayerSession()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
            PlayerSession.SaveInventory(player.Inventory);
    }
}