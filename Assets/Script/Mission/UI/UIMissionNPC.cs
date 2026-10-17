using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Mission board opened by the mission NPC: list -> detail -> accept.</summary>
public class UIMissionNPC : MonoBehaviour
{
    [Header("Mission List")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject missionListScrollView;
    [SerializeField] private GameObject missionButtonPrefab;
    [SerializeField] private Transform missionButtonParent;
    [SerializeField] private Button closeButton;

    [Header("Mission Detail")]
    [SerializeField] private GameObject missionDetailPanel;
    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text missionDescText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button acceptButton;

    private Mission selectedMission;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => ToggleRootPanel(false));

        if (backButton != null)
            backButton.onClick.AddListener(() => ToggleMissionDetailPanel(false));

        if (acceptButton != null)
            acceptButton.onClick.AddListener(AcceptSelectedMission);
    }

    void OnEnable() => UIEvents.MissionBoardToggled += ToggleRootPanel;

    void OnDisable()
    {
        UIEvents.MissionBoardToggled -= ToggleRootPanel;
        InputModeService.RequestUI(this, false);
    }

    void Start() => DisplayMissions();

    private void ToggleRootPanel(bool isOn)
    {
        rootPanel.SetActive(isOn);
        if (!isOn)
            ToggleMissionDetailPanel(false);

        InputModeService.RequestUI(this, isOn);
    }

    private void ToggleMissionDetailPanel(bool isOn)
    {
        missionDetailPanel.SetActive(isOn);
        missionListScrollView.SetActive(!isOn);
    }

    private void DisplayMissions()
    {
        MissionManager missions = MissionManager.Instance;
        if (missions == null)
        {
            Debug.LogWarning("UIMissionNPC: no MissionManager in the scene.", this);
            return;
        }

        if (missions.AvailableMissions.Count == 0)
            Debug.LogWarning("UIMissionNPC: no missions found.", this);

        foreach (Mission mission in missions.AvailableMissions)
        {
            GameObject buttonObject = Instantiate(missionButtonPrefab, missionButtonParent);

            if (buttonObject.TryGetComponent(out MissionButton missionButton))
                missionButton.Bind(mission);

            if (buttonObject.TryGetComponent(out Button button))
                button.onClick.AddListener(() => ShowMissionDetail(mission));
        }
    }

    private void ShowMissionDetail(Mission mission)
    {
        selectedMission = mission;
        missionNameText.text = mission.Name;
        missionDescText.text = mission.Description;

        ToggleMissionDetailPanel(true);
    }

    private void AcceptSelectedMission()
    {
        if (selectedMission != null && MissionManager.Instance != null)
            MissionManager.Instance.Accept(selectedMission);
    }
}
