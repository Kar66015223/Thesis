using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class WaveExpander : MonoBehaviour
{
    public float scanDuration = 5f; 
    public float maxRadius = 50f;
    public AnimationCurve growthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private SphereCollider sphereCollider;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 0f;
        
        // เริ่มขยายคลื่นทันทีที่เกิด
        StartCoroutine(ExpandWave());
    }

    private IEnumerator ExpandWave()
    {
        float elapsedTime = 0f;
        while (elapsedTime < scanDuration)
        {
            elapsedTime += Time.deltaTime;
            float curveValue = growthCurve.Evaluate(elapsedTime / scanDuration);
            sphereCollider.radius = Mathf.Lerp(0f, maxRadius, curveValue);
            yield return null;
        }
        
        // ลบตัวเองทิ้งเมื่อสแกนจบ
        Destroy(gameObject);
    }
}