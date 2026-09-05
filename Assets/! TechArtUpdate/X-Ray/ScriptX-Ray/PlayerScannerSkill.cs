using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject scanVFXPrefab; 
    public float cooldownTime = 10f;

    [Header("Debug")]
    public bool isCooldown = false;
    public float currentCooldown = 0f;

    void Update()
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
                isCooldown = false;
        }

        // if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && !isCooldown)
        // {
        //     isCooldown = true;
        //     currentCooldown = cooldownTime;

        //     if (scanVFXPrefab != null)
        //     {
        //         GameObject newVFX = Instantiate(scanVFXPrefab, transform.position, Quaternion.identity);
        //         // ขยับแกน Y ลง -1
        //         newVFX.transform.position += new Vector3(0f, -1f, 0f);
        //     }
        // }
    }
    
    public void Scan()
    {
        if(!isCooldown)
        {
            isCooldown = true;
            currentCooldown = cooldownTime;

            if (scanVFXPrefab != null)
            {
                GameObject newVFX = Instantiate(scanVFXPrefab, transform.position, Quaternion.identity);
                // ขยับแกน Y ลง -1
                newVFX.transform.position += new Vector3(0f, -1f, 0f);
            }
        }
    }
}