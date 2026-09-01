using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionUI
{
    [SerializeField] private GameObject listPanel;

    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;
    [SerializeField] private TMP_Text itemNameText;

    private PlayerInteraction interact;
    private PlayerInteractionDetector detector;

    public void Initialize(PlayerInteraction interact, PlayerInteractionDetector detector)
    {
        this.interact = interact;
        this.detector = detector;
    }

    void Update()
    {
        DisplayItems();
    }
    
    private void DisplayItems()
    {
        List<IInteractable> allInteractables = detector.GetAllDetected();
        List<GameObject> allObjects = detector.GetAllDetectObj();

        foreach(IInteractable interact in allInteractables)
        {
            GameObject itemButton = Object.Instantiate(itemButtonPrefab, itemButtonParent);

            TMP_Text nameText = itemButton.GetComponentInChildren<TMP_Text>();
            nameText.text = nameText.gameObject.name;
        }
    }
}