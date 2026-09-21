using UnityEngine;

public class ObstacleSide : MonoBehaviour
{
    public bool isPlayerOnThisSide = false;
    public Transform playerTransform;

    [SerializeField] private Transform targetPos;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnThisSide = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerOnThisSide = false;
            playerTransform = null;
        }
    }
}