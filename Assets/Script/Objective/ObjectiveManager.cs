using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectiveManager : MonoBehaviour
{
    [FormerlySerializedAs("AllObjtive")]
    [SerializeField] private List<ObjectiveHighlight> allObjectives = new();

    [FormerlySerializedAs("curObjtiveIndex")]
    [SerializeField] private int currentIndex;

    public int CurrentIndex => currentIndex;
    public ObjectiveHighlight CurrentObjective =>
        currentIndex >= 0 && currentIndex < allObjectives.Count ? allObjectives[currentIndex] : null;

    void OnEnable()
    {
        WorldEvents.ObjectiveShow += ToggleShowCurrent;
        WorldEvents.ObjectiveCompleted += HandleObjectiveCompleted;
    }
    
    void OnDisable()
    {
        WorldEvents.ObjectiveShow -= ToggleShowCurrent;
        WorldEvents.ObjectiveCompleted -= HandleObjectiveCompleted;
    } 

    void Start() => SetCurrent(0);

    [ContextMenu("Debug/Next Objective")]
    public void NextObjective()
    {
        if (currentIndex >= allObjectives.Count - 1)
            return;

        SetCurrent(currentIndex + 1);
    }

    public void RestoreObjective(int index) => SetCurrent(index);

    private void HandleObjectiveCompleted(ObjectiveHighlight completed)
    {
        if (completed == CurrentObjective)
            NextObjective();
    }

    private void SetCurrent(int index)
    {
        currentIndex = index;

        ObjectiveHighlight current = CurrentObjective;
        foreach (ObjectiveHighlight objective in allObjectives)
        {
            if (objective != null)
                objective.SetCurrent(objective == current);
        }
    }

    private void ToggleShowCurrent(bool isShow)
    {
        if (CurrentObjective == null)
            return;

        CurrentObjective.SetShow(isShow);
    }
}
