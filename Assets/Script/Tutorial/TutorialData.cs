using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Objects/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string headerText;
    [TextArea] public string bodyText;
    public Sprite sprite;
}