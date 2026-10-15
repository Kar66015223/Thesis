using TMPro;
using UnityEngine;

/// <summary>One row of the mission list. Refreshes itself when its mission's status changes.</summary>
public class MissionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;

    private Mission mission;

    public void Bind(Mission newMission)
    {
        Unbind();

        mission = newMission;
        if (mission == null)
            return;

        mission.StatusChanged += Refresh;
        Refresh(mission);
    }

    void OnDestroy() => Unbind();

    private void Unbind()
    {
        if (mission != null)
            mission.StatusChanged -= Refresh;

        mission = null;
    }

    private void Refresh(Mission source)
    {
        nameText.text = source.Name;
        statusText.text = source.Status.ToString();
        statusText.color = source.Status switch
        {
            MissionStatus.OnProgress => Color.yellow,
            MissionStatus.Completed => Color.green,
            _ => Color.white,
        };
    }
}
