using System.Collections.Generic;
using UnityEngine;

public class ObjectiveUIManager : MonoBehaviour
{
    public static ObjectiveUIManager Instance;

    public List<ObjectiveData> allObjectives = new();
    private ObjectiveData curObjective;
    public ObjectiveData CurObjective => curObjective;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        UIEvents.ChangeObjectiveRequest += SetObjective;
    }

    void OnDisable()
    {
        UIEvents.ChangeObjectiveRequest -= SetObjective;
    }

    private void SetObjective(ObjectiveData objTive)
    {
        if (curObjective == null)
            curObjective = objTive;
        else
        {
            if(allObjectives.Contains(curObjective) && curObjective != objTive)
            {
                allObjectives.Remove(curObjective);
                curObjective = objTive;
            }
        }
    }
}