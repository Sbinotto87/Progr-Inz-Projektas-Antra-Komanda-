using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem jumpDustEffect;
    [SerializeField] private ParticleSystem runDustEffect;

    [SerializeField] private float flashDuration = 0.15f;

    public void PlayJumpDust()
    {
        if (jumpDustEffect != null)
            jumpDustEffect.Play();
    }

    public void SetRunDust(bool isRunning)
    {
        if (runDustEffect == null) return;

        if (isRunning && !runDustEffect.isPlaying)
            runDustEffect.Play();
        else if (!isRunning && runDustEffect.isPlaying)
            runDustEffect.Stop();
    }

    public void PlayDamageFlash()
    {
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);
    }
}