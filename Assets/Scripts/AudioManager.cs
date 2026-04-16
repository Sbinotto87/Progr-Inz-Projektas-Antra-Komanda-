using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip[] gruntClips;
    public AudioSource prefab;

    private int activeSounds;
    public int maxActiveSounds = 3;

    void Awake()
    {
        Instance = this;
    }

    public void PlayEnemyGrunt(Vector3 position)
    {
        if (activeSounds >= maxActiveSounds) return;

        AudioClip clip = gruntClips[Random.Range(0, gruntClips.Length)];

        AudioSource src = Instantiate(prefab, position, Quaternion.identity);
        src.clip = clip;

        src.spatialBlend = 1f;
        src.pitch = Random.Range(0.9f, 1.1f);

        activeSounds++;

        src.Play();

        Destroy(src.gameObject, clip.length);

        StartCoroutine(Release(clip.length));
    }

    System.Collections.IEnumerator Release(float t)
    {
        yield return new WaitForSeconds(t);
        activeSounds--;
    }
}   