using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene($"{sceneName}");
    }

    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}