using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the code from the digit slots and submits it to the lock. It never sees the correct code.</summary>
public class PasswordInputUI : MonoBehaviour
{
    [SerializeField] private List<PasswordSlot> allSlots = new();
    [SerializeField] private Button closeButton;
    [SerializeField] private Button submitButton;

    private IPasswordLock currentLock;

    void Awake()
    {
        closeButton.onClick.AddListener(() => UIEvents.TogglePasswordPanel(false));
        submitButton.onClick.AddListener(SubmitPassword);
    }

    public void SetCurrentLock(IPasswordLock passwordLock) => currentLock = passwordLock;

    public void SubmitPassword()
    {
        if (currentLock == null)
            return;

        if (currentLock.TrySubmit(BuildCode()))
            UIEvents.TogglePasswordPanel(false);
        else
            UIEvents.ShowTip("Wrong Password");
    }

    private string BuildCode()
    {
        StringBuilder code = new(allSlots.Count);
        foreach (PasswordSlot slot in allSlots)
            code.Append(slot.CurrentNumber);

        return code.ToString();
    }
}
