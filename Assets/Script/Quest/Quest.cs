using UnityEngine;

public class Quest : MonoBehaviour
{
    [SerializeField] private QuestData data;
    [SerializeField] private QuestStatus status = QuestStatus.Pending;
}