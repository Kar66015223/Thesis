using UnityEngine;
using UnityEngine.UI;

public class ItemActionButton : MonoBehaviour
{
    private Button button;

    public void Bind(ItemData data, ItemAction action, GameObject owner)
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        switch (action)
        {
            case ItemAction.Use:
                button.onClick.AddListener(() => data.Use());
                break;

            case ItemAction.Eat:
                button.onClick.AddListener(() => data.Eat(owner));
                break;

            case ItemAction.Drop:
                button.onClick.AddListener(() => data.Drop(owner));
                break;
        }
    }
}