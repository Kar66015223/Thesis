using UnityEngine;

public class ScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject scanVFX; 

    public float cooldownTime = 1f;
    public bool isCooldown = false;
    public float currentCooldown = 0f;

    private bool isSkillActive = false;

    void Update()
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;
            }
        }
    }
    
    public void Scan(bool flag)
    {
        if (scanVFX == null)
            return;

        if (flag)
        {
            if (isCooldown)
                return;

            isSkillActive = true;
            scanVFX.SetActive(true);
        }
        else
        {
            if (isSkillActive)
            {
                isSkillActive = false;
                isCooldown = true;
                currentCooldown = cooldownTime;
            }
            scanVFX.SetActive(false);
        }
    }
}