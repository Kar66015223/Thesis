using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] public List<ObjectiveHighlight> AllObjtive = new();
    private ObjectiveHighlight curObjtive;
    public int curObjtiveIndex;

    public bool next = false;

    void OnEnable()
    {
        GameEvent.OnChangeObjective += NextObjtive;
    }

    void OnDisable()
    {
        GameEvent.OnChangeObjective -= NextObjtive;
    }

    void Start()
    {
        SetHighlight(0);
    }

    void Update()
    {
        if (next)
            NextObjtive();
    }

    public void NextObjtive()
    {
        next = false;

        if (curObjtiveIndex >= AllObjtive.Count - 1)
            return;

        curObjtiveIndex++;
        SetHighlight(curObjtiveIndex);
    }

    private void SetHighlight(int index)
    {
        foreach (var objtive in AllObjtive)
            objtive.canHighlight = false;

        if(index < AllObjtive.Count)
        {
            curObjtive = AllObjtive[index];
            curObjtive.canHighlight = true;
            GameEvent.OnObjectiveReset?.Invoke(curObjtive);
        }
    }
}
