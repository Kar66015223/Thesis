using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneSequence", menuName = "Scriptable Objects/CutsceneSequence")]
public class CutsceneSequence : ScriptableObject
{
    [TextArea] public string text;
}
