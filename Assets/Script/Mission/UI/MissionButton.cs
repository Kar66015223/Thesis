using TMPro;
using UnityEngine;

public class MissionButton : MonoBehaviour
{
    public Mission curMission;
    public TMP_Text nameText;
    public TMP_Text statusText;

    public void UpdateUI()
    {
        nameText.text = curMission.missionName;

        statusText.text = curMission.missionStatus.ToString();
        statusText.color = curMission.missionStatus switch
        {
            MissionStatus.OnProgress => Color.yellow,
            MissionStatus.Completed => Color.green,
            _ => Color.white,
        };
    }
}
