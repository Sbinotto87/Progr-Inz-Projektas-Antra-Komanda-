using Assets.Scripts;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    [SerializeField]
    GameObject World;


    private World world;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        world = World.GetComponent<World>();
    }

    
    [SerializeField] private TMP_Text coordinateText;
    [SerializeField] private TMP_Text TimeText;
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
        TimeText.text = $"{world.DayTime}";
    }

}