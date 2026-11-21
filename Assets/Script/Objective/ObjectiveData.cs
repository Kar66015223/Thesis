using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "Scriptable Objects/Objective")]
public class ObjectiveData : ScriptableObject
{
    [TextArea] public string objectiveText;
}
