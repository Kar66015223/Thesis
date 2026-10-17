using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Walks through the level's objectives in order.</summary>
public class ObjectiveManager : MonoBehaviour
{
    [FormerlySerializedAs("AllObjtive")]
    [SerializeField] private List<ObjectiveHighlight> allObjectives = new();

    [FormerlySerializedAs("curObjtiveIndex")]
    [SerializeField] private int currentIndex;

    public ObjectiveHighlight CurrentObjective =>
        currentIndex >= 0 && currentIndex < allObjectives.Count ? allObjectives[currentIndex] : null;

    void OnEnable() => WorldEvents.ObjectiveCompleted += HandleObjectiveCompleted;
    void OnDisable() => WorldEvents.ObjectiveCompleted -= HandleObjectiveCompleted;

    void Start() => SetCurrent(0);

    [ContextMenu("Debug/Next Objective")]
    public void NextObjective()
    {
        if (currentIndex >= allObjectives.Count - 1)
            return;

        SetCurrent(currentIndex + 1);
    }

    private void HandleObjectiveCompleted(ObjectiveHighlight completed)
    {
        if (completed == CurrentObjective)
            NextObjective();
    }

    private void SetCurrent(int index)
    {
        currentIndex = index;

        for (int i = 0; i < allObjectives.Count; i++)
        {
            if (allObjectives[i] != null)
                allObjectives[i].SetCurrent(i == index);
        }
    }
}
