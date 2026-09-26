using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasswordInputUI : MonoBehaviour
{
    [SerializeField] private List<PasswordSlot> allSlots = new();
    [SerializeField] private string currentPassword;
    private PasswordLock currentLock;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button submitButton;

    void Awake()
    {
        closeButton.onClick.AddListener(() => GameEvent.OnShowPasswordInputUI?.Invoke(false, null));
        submitButton.onClick.AddListener(SubmitPassword);
    }

    public void SubmitPassword()
    {
        currentPassword = "";

        foreach(PasswordSlot slot in allSlots)
        {
            currentPassword += slot.CurrentNumber.ToString();
        }

        if (currentPassword == currentLock.GetCorrectPassword())
        {
            currentLock.Unlock();
            GameEvent.OnShowPasswordInputUI?.Invoke(false, null);
        }
        else
        {
            GameEvent.OnShowTips?.Invoke("Wrong Password");
        }
    }

    public void SetCurrentLock(PasswordLock pLock) => currentLock = pLock; 
}