using TMPro;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    [SerializeField] private TMP_Text coordinateText;
    private Transform playerTransform;

    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            coordinateText.text = $"X: {pos.x:F0} | Y: {pos.y:F0} | Z: {pos.z:F0}";
        }
    }
}
