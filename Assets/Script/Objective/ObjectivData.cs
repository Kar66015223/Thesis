using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "Scriptable Objects/Objective")]
public class ObjectivData : ScriptableObject
{
    [TextArea] public string objectiveText;
}
