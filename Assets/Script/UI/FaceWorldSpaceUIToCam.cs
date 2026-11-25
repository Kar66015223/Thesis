using UnityEngine;

public class FaceWorldSpaceUIToCam : MonoBehaviour
{
    private Camera cam;

    void Start() => cam = Camera.main;

    private void LateUpdate()
    {
        if (cam == null)
            return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(forward);
    }
}
