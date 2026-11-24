using UnityEngine;
using UnityEngine.UI;

public class ItemActionButton : MonoBehaviour
{
    private Button button;

    public void Bind(ItemData data, GameObject owner)
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();

    }

    private void Use()
    {
        Debug.Log("Use");
    }

    private void Eat()
    {
        Debug.Log("Eat");
    }

    private void Equip()
    {
        Debug.Log("Equip");
    }

    private void Drop()
    {
        Debug.Log("Drop");
    }
}