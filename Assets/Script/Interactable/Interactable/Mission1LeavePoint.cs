using UnityEngine;

public class Mission1LeavePoint : Interactable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if(interactor.TryGetComponent(out PlayerController ctrl))
        {
            if (ctrl.hasMorseCode)
                SceneLoader.LoadScene(SceneConstants.HIDEOUT_NAME);
            else
                GameEvent.OnShowTips?.Invoke("Don't have morse code, can't return yet");
        }
    }
}