using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; // จัดการระบบ Visual Effect Graph

[RequireComponent(typeof(SphereCollider))]
public class FlashbangGrenade : MonoBehaviour
{
    [Header("VFX Graph Reference")]
    public VisualEffect vfxGraph;
    public string playEventName = "OnPlay"; // ชื่อ Event ใน VFX Graph (ปกติค่าเริ่มต้นคือ OnPlay)
    public AudioSource explosionAudio;

    [Header("Timing")]
    public float delayBeforeColliderExpand = 0.2f;

    [Header("Expansion Settings")]
    public float maxRadius = 15f;
    public float expansionSpeed = 30f;

    [Header("Layer Mask")]
    public LayerMask obstructionMask;

    private SphereCollider sphereCollider;
    private bool isExpanding = false;
    private HashSet<Collider> hitTargets = new HashSet<Collider>();

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 0.05f;
        sphereCollider.enabled = false;

        if (vfxGraph == null)
        {
            vfxGraph = GetComponent<VisualEffect>();
        }

        // ปิดการเล่นอัตโนมัติเมื่อเริ่มเกม
        if (vfxGraph != null)
        {
            vfxGraph.pause = false;
        }
    }

    public void Explode()
    {
        StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        // 1. สั่งยิง Event เข้า VFX Graph โดยตรง
        if (vfxGraph != null)
        {
            // รีเซ็ตสถานะและส่ง Event เพื่อบังคับให้ Spawn ทำงานทันที
            vfxGraph.Reinit();
            vfxGraph.SendEvent(playEventName);
        }

        if (explosionAudio != null)
        {
            explosionAudio.Play();
        }

        // 2. หน่วงเวลาตามจังหวะระเบิด
        yield return new WaitForSeconds(delayBeforeColliderExpand);

        // 3. เริ่มขยาย Collider
        sphereCollider.enabled = true;
        isExpanding = true;
    }

    void Update()
    {
        if (!isExpanding) return;

        if (sphereCollider.radius < maxRadius)
        {
            sphereCollider.radius += expansionSpeed * Time.deltaTime;
        }
        else
        {
            sphereCollider.enabled = false;
            isExpanding = false;
            Destroy(gameObject, 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitTargets.Contains(other)) return;

        if (other.TryGetComponent(out PlayerFlashbangReceiver player))
        {
            hitTargets.Add(other);

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (Physics.Linecast(transform.position, other.transform.position, obstructionMask))
            {
                return;
            }

            float intensity = Mathf.Clamp01(1f - (distance / maxRadius));
            player.ApplyFlash(intensity);
        }
    }
}