using UnityEngine;

public class WalkingController : MonoBehaviour
{
    private AudioSource walkingAudioSource;
    private Transform player;
    private Vector3 lastPosition;

    void Start()
    {
        walkingAudioSource = GetComponent<AudioSource>();
        player = GameObject.Find("Player").transform;
        lastPosition = player.position;
    }

    void Update()
    {
        
        bool isGrounded = Physics.Raycast(player.position, Vector3.down, 1.1f);
        
        Vector3 currentPosition = player.position;
        bool isMoving = Vector3.Distance(currentPosition, lastPosition) > 0.001f;
        lastPosition = currentPosition;

        if (isGrounded && isMoving)
        {
            if (!walkingAudioSource.isPlaying)
                walkingAudioSource.Play();
        }
        else
        {
            if (walkingAudioSource.isPlaying)
                walkingAudioSource.Stop();
        }
    }
}