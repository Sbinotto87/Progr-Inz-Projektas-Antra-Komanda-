using Assets.Scripts;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DisplayText : MonoBehaviour
{



    private World world;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject World = GameObject.Find("World");
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
            coordinateText.text = $"X: {pos.x:F1} | Y: {pos.y:F1} | Z: {pos.z:F1}";
        }
        TimeText.text = $"{world.DayTime}";
    }

}