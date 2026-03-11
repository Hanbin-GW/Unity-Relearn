using UnityEngine;

public class ThirdPersonDebugCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -4);
    public float smooth = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smooth
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}