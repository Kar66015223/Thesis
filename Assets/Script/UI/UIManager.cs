using System.Collections.Generic;
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

    [Header("Enemy State")]
    [SerializeField] private Transform enemyStateUIParent;
    [SerializeField] private GameObject enemyStateUIPrefab;

    private Dictionary<Transform, GameObject> activeEnemyUIs = new();

    public Sprite seeSprite;
    public Sprite hearSprite;
    public Sprite walkSprite;
    public Sprite chaseSprite;
    public Sprite stunnedSprite;

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

        GameEvent.OnAlertEnemyState += ToggleEnemyStateUI;
        GameEvent.OnUpdateConfirmTimer += UpdateConfirmUI;
    }

    void OnDisable()
    {
        GameEvent.OnToggleReadUI -= ToggleReadUI;
        GameEvent.OnShowTips -= ShowTipsText;
        GameEvent.OnShowPasswordInputUI -= TogglePasswordInputUI;
        GameEvent.OnLightsOut -= ToggleLightsOutEffect;

        GameEvent.OnAlertEnemyState -= ToggleEnemyStateUI;
        GameEvent.OnUpdateConfirmTimer -= UpdateConfirmUI;
    }

    public void ToggleReadUI(bool isOn, string text)
    {
        readUIPanel.SetActive(isOn);
        readUIText.text = text;

        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
        GameEvent.OnSwitchActionMap?.Invoke(
            isOn ? PlayerConstants.ACTIONMAP_UI : PlayerConstants.ACTIONMAP_PLAYER);
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

        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
        GameEvent.OnSwitchActionMap?.Invoke(
            isOn ? PlayerConstants.ACTIONMAP_UI : PlayerConstants.ACTIONMAP_PLAYER);
    }

    public void ToggleLightsOutEffect(bool isOn)
    {
        lightsOutEffect.SetActive(isOn);
    }

    public void ToggleEnemyStateUI(Transform target, EnemyState state)
    {
        switch (state)
        {
            case EnemyState.None:
                RemoveEnemyStateUI(target);
                break;

            case EnemyState.DistractedLook:
                CreateOrUpdateEnemyStateUI(target, hearSprite, seeSprite);
                break;

            case EnemyState.DistractedMove:
                CreateOrUpdateEnemyStateUI(target, hearSprite, walkSprite);
                break;

            case EnemyState.Confirming:
                CreateOrUpdateEnemyStateUI(target, seeSprite, null);
                break;

            case EnemyState.Chase:
                CreateOrUpdateEnemyStateUI(target, chaseSprite, null);
                break;

            case EnemyState.Stunned:
                CreateOrUpdateEnemyStateUI(target, stunnedSprite, null);
                break;
        }
    }

    private void CreateOrUpdateEnemyStateUI(Transform target, Sprite main, Sprite sub)
    {
        GameObject enemyStateUI;

        if (activeEnemyUIs.ContainsKey(target))
            enemyStateUI = activeEnemyUIs[target];
        else
        {
            enemyStateUI = Instantiate(enemyStateUIPrefab, enemyStateUIParent);
            activeEnemyUIs.Add(target, enemyStateUI);

            if (enemyStateUI.TryGetComponent(out UITargetTracking tracking))
                tracking.SetTarget(target);
        }

        if (enemyStateUI.TryGetComponent(out EnemyStateUI ui))
        {
            ui.mainImg.sprite = main;

            if (sub != null)
            {
                ui.subImg.enabled = true;
                ui.subImg.sprite = sub;
            }
            else
                ui.subImg.enabled = false;
        }
    }

    private void RemoveEnemyStateUI(Transform target)
    {
        if (activeEnemyUIs.TryGetValue(target, out GameObject uiInstance))
        {
            Destroy(uiInstance);
            activeEnemyUIs.Remove(target);
        }
    }
    
    public void UpdateConfirmUI(Transform target, float fillAmount)
    {
        if(activeEnemyUIs.TryGetValue(target, out GameObject uiInstance))
        {
            if(uiInstance.TryGetComponent(out EnemyStateUI ui))
            {
                ui.mainImg.fillAmount = fillAmount;
            }
        }
    }
}