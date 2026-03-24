using UnityEngine;

public class SoundController : MonoBehaviour
{
    private AudioSource audio_source;
    private Transform player;

    // reguliuojama pagal toluma
    public float hearingDistance = 10f;

    void Start()
    {
        audio_source = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= hearingDistance )
        {
            if (!audio_source.isPlaying)
                audio_source.Play();
        }
        else
        {
            if (audio_source.isPlaying)
                audio_source.Stop();
        }
    }
}
