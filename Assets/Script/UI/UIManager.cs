using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Read UI")]
    [SerializeField] private GameObject readUIPanel;
    [SerializeField] private TMP_Text readUIText;
    [SerializeField] private Button closeButton;

    [Header("Tips")]
    [SerializeField] private TMP_Text tipsText;
    private Coroutine fadeCo;

    [Header("Password Input")]
    [SerializeField] private GameObject passwordInputPanel;

    [SerializeField] private GameObject lightsOutEffect;

    void Awake()
    {
        if (readUIText != null)
            readUIText.text = "";
            
        if (closeButton != null)
            closeButton.onClick.AddListener(() => GameEvent.OnToggleReadUI?.Invoke(false, ""));
    }

    void OnEnable()
    {
        GameEvent.OnToggleReadUI += ToggleReadUI;
        GameEvent.OnShowTips += ShowTipsText;
        GameEvent.OnShowPasswordInputUI += TogglePasswordInputUI;
        GameEvent.OnLightsOut += ToggleLightsOutEffect;
    }

    void OnDisable()
    {
        GameEvent.OnToggleReadUI -= ToggleReadUI;
        GameEvent.OnShowTips -= ShowTipsText;
        GameEvent.OnShowPasswordInputUI -= TogglePasswordInputUI;
        GameEvent.OnLightsOut -= ToggleLightsOutEffect;
    }

    public void ToggleReadUI(bool isOn, string text)
    {
        readUIPanel.SetActive(isOn);
        readUIText.text = text;

        Time.timeScale = isOn ? 0f : 1f;
        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ShowTipsText(string text)
    {
        tipsText.text = text;

        if (fadeCo != null)
            StopCoroutine(fadeCo);

        if (tipsText.TryGetComponent(out FadeOutText fade))
        {
            fade.SetAlphaToFull();
            fadeCo = StartCoroutine(fade.FadeOut());
        }
    }

    public void TogglePasswordInputUI(bool isOn, PasswordLock curLock)
    {
        passwordInputPanel.SetActive(isOn);
        if (passwordInputPanel.TryGetComponent(out PasswordInputUI ui))
            ui.SetCurrentLock(curLock);

        Time.timeScale = isOn ? 0f : 1f;
        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
    public void ToggleLightsOutEffect(bool isOn)
    {
        lightsOutEffect.SetActive(isOn);
    }
}