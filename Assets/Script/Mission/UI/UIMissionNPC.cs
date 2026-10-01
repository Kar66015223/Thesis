using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMissionNPC : MonoBehaviour
{
    [Header("Mission List")]
    [SerializeField] private GameObject rootPanel;

    [SerializeField] private GameObject missionListScrollView;
    [SerializeField] private GameObject missionButtonPrefab;
    [SerializeField] private Transform missionButtonParent;

    private Mission selectedMission;

    [SerializeField] private Button closeButton;

    [Header("Mission Detail")]
    [SerializeField] private GameObject missionDetailPanel;

    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text missionDescText;

    [SerializeField] private Button backButton;
    [SerializeField] private Button acceptButton;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => ToggleRootPanel(false));

        if (backButton != null)
            backButton.onClick.AddListener(() => ToggleMissionDetailPanel(false));

        if (acceptButton != null)
            acceptButton.onClick.AddListener(() => selectedMission.OnAccept());
    }

    void OnEnable()
    {
        GameEvent.OnToggleUIQuestNPC += ToggleRootPanel;
    }

    void OnDisable()
    {
        GameEvent.OnToggleUIQuestNPC -= ToggleRootPanel;
    }

    void Start()
    {
        DisplayMission();
    }

    private void ToggleRootPanel(bool isOn)
    {
        rootPanel.SetActive(isOn);
        if (isOn)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            ToggleMissionDetailPanel(false);
        }

        GameEvent.OnSwitchActionMap?.Invoke(
            isOn ? PlayerConstants.ACTIONMAP_UI : PlayerConstants.ACTIONMAP_PLAYER);
    }

    private void ToggleMissionDetailPanel(bool isOn)
    {
        missionDetailPanel.SetActive(isOn);
        missionListScrollView.SetActive(!isOn);
    }

    public void DisplayMission()
    {
        if (MissionManager.instance == null)
        {
            Debug.LogError("no instance");
            return;
        }

        if (MissionManager.instance.GetAvailableMissions().Count > 0)
        {
            foreach (var mission in MissionManager.instance.GetAvailableMissions())
            {
                GameObject missionButton = Instantiate(missionButtonPrefab, missionButtonParent);
                mission.uiButton = missionButton;

                TMP_Text missionName = missionButton.GetComponentInChildren<TMP_Text>();
                if (missionName != null)
                    missionName.text = mission.missionName;

                if (missionButton.TryGetComponent(out Button button))
                    button.onClick.AddListener(() => OnClickMissionButton(mission));
            }
        }
        else
            Debug.LogWarning("no missions found");
    }
    
    private void OnClickMissionButton(Mission clickedMission)
    {
        ToggleMissionDetailPanel(true);

        selectedMission = clickedMission;

        missionNameText.text = clickedMission.missionName;
        missionDescText.text = clickedMission.missionDesc;
    }
}