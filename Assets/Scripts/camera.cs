using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;  // Reference to the target object
    public Vector3 offset = new Vector3(0f, 2f, -5f);  // Offset from the target object

    public float smoothSpeed = 5f;  // Smoothing factor for camera movement

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
