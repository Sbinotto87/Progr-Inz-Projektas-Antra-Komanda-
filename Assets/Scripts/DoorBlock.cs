using UnityEngine;

public class DoorBlock : MonoBehaviour
{
    private bool isOpen = false;

    // Make this public!
    public Vector3 hingePoint;

    void Start()
    {
        hingePoint = transform.position;

        float offsetX = 0.1f;
        float offsetY = 1.0f;
        float offsetZ = 0.5f;

        transform.position += (transform.right * offsetX) +
                              (transform.up * offsetY) +
                              (transform.forward * offsetZ);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        float angle = isOpen ? 90f : -90f;
        transform.RotateAround(hingePoint, Vector3.up, angle);
    }
}