using UnityEngine;

public class FollowCameraNoRotation : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 10f, 0);

    void LateUpdate()
    {
        transform.position = cameraTransform.position + offset;
        transform.rotation = Quaternion.identity; // no rotation
    }
}