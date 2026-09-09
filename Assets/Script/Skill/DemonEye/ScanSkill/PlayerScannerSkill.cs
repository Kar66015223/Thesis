using UnityEngine;

public class PlayerScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject scanVFX; 
    public float cooldownTime = 10f;

    [Header("Debug")]
    public bool isCooldown = false;
    public float currentCooldown = 0f;

    // void Update()
    // {
    //     if (isCooldown)
    //     {
    //         currentCooldown -= Time.deltaTime;
    //         if (currentCooldown <= 0f)
    //             isCooldown = false;
    //     }
    // }
    
    public void Scan(bool flag)
    {
        // if (!isCooldown)
        // {
        //     // isCooldown = true;
        //     // currentCooldown = cooldownTime;

        //     if (scanVFX != null)
        //     {
        //         scanVFX.SetActive(flag);
        //     }
        // }
        
        if (scanVFX != null)
        {
            scanVFX.SetActive(flag);
        }
    }
}