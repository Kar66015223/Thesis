using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private const string ResourcePath = "LoadingScreen";
    private const float LoadShare = 0.8f;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressFill;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Wait For Render")]
    [SerializeField] private int minRenderedFrames = 3;
    [SerializeField] private int stableFramesRequired = 10;
    [SerializeField] private float stableFrameTime = 1f / 30f;
    [SerializeField] private float maxRenderWait = 5f;

    private static LoadingScreen instance;
    private float progress;

    public static bool IsLoading { get; private set; }
    public static event Action Finished;

    public static void Load(string sceneName)
    {
        if (IsLoading)
            return;

        LoadingScreen screen = GetInstance();
        if (screen == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        screen.StartCoroutine(screen.LoadRoutine(sceneName));
    }

    private static LoadingScreen GetInstance()
    {
        if (instance != null)
            return instance;

        LoadingScreen prefab = Resources.Load<LoadingScreen>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"No LoadingScreen at Resources/{ResourcePath}");
            return null;
        }

        instance = Instantiate(prefab);
        DontDestroyOnLoad(instance.gameObject);
        return instance;
    }

    void Awake() => SetAlpha(0f);

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        InputModeService.RequestUI(this, true);
        SetProgress(0f, force: true);

        yield return Fade(1f);

        Time.timeScale = 0f;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            SetProgress(op.progress / 0.9f * LoadShare);
            yield return null;
        }

        Time.timeScale = 1f;
        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        yield return WaitUntilRendered();

        SetProgress(1f);
        InputModeService.RequestUI(this, false);
        yield return Fade(0f);

        IsLoading = false;
        Finished?.Invoke();
    }

    private IEnumerator WaitUntilRendered()
    {
        WaitForEndOfFrame endOfFrame = new();
        for (int i = 0; i < minRenderedFrames; i++)
            yield return endOfFrame;

        int stableFrames = 0;
        float waited = 0f;

        while(stableFrames < stableFramesRequired && waited < maxRenderWait)
        {
            yield return null;

            float frameTime = Time.unscaledDeltaTime;
            waited += frameTime;
            stableFrames = frameTime <= stableFrameTime ? stableFrames + 1 : 0;

            SetProgress(Mathf.Lerp(LoadShare, 1f, (float)stableFrames / stableFramesRequired));
        }
    }

    private IEnumerator Fade(float target)
    {
        canvasGroup.blocksRaycasts = true;
        float start = canvasGroup.alpha;

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0f;
    }

    private void SetProgress(float value, bool force = false)
    {
        progress = force ? value : Mathf.Max(progress, value);
        if (progressFill != null)
            progressFill.fillAmount = progress;
    }
}
