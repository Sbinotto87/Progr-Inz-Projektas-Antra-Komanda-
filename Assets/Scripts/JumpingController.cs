using UnityEngine;

public class JumpingController : MonoBehaviour
{
    private AudioSource jumpingAudioSource;
    private Transform player;
    private bool wasGrounded;

    void Start()
    {
        jumpingAudioSource = GetComponent<AudioSource>();
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        bool isGrounded = Physics.Raycast(player.position, Vector3.down, 1.3f); //laikini nustatymai iki kol atsilieka garsas

        if (wasGrounded && !isGrounded)
        {
            jumpingAudioSource.Play();
        }

        wasGrounded = isGrounded;
    }
}