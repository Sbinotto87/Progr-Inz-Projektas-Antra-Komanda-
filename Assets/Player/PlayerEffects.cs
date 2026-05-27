using System.Collections;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem jumpDustEffect;
    [SerializeField] private ParticleSystem runDustEffect;

    [SerializeField] private float flashDuration = 0.15f;

    [Header("Damage Visual Effects")]
    [SerializeField] private float effectDuration = 0.4f;         // How long the shake/flash lasts
    [SerializeField] private float maxTiltAngle = 10f;             // Z-axis roll tilt angle (like Minecraft)
    [SerializeField] private UnityEngine.UI.Image redFlashImage;   // Fullscreen red UI Image
    [SerializeField] private float maxRedAlpha = 0.4f;             // Maximum opacity of the red screen flash

    [Header("Subtle Jitter Layers (The Extra Grit)")]
    [SerializeField] private float rotationJitter = 1.2f;      // Faint rotational camera shake (degrees)
    [SerializeField] private float positionJitter = 0.02f;     // Faint physical camera displacement (units)

    [Header("Damage Threshold Trigger")]
    [SerializeField] private float damageThreshold = 5f;          // Amount of damage needed to trigger the effect

    private Player player;
    private float lastHealth;
    private float accumulatedDamage;                  ///How mach damage has been taken
    private Coroutine damageCoroutine;

    // Public properties that Player.cs reads every frame to update camera orientation
    public Vector3 CameraTilt { get; private set; }
    public Vector3 CameraPositionOffset { get; private set; }

    void Start()
    {
        player = GetComponent<Player>();
        if (player != null)
        {
            lastHealth = player.health;
        }

        // Initialize red flash image to fully transparent
        if (redFlashImage != null)
        {
            Color c = redFlashImage.color;
            c.a = 0f;
            redFlashImage.color = c;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Calculate if health dropped this frame
        float healthDifference = lastHealth - player.health;

        if (healthDifference > 0)
        {
            // Add the damage taken this frame to our running total
            accumulatedDamage += healthDifference;

            // Trigger ONLY when the accumulated damage crosses your threshold (e.g. 5 points)
            if (accumulatedDamage >= damageThreshold)
            {
                PlayDamageFlash();

                // Clear the counter back to 0 so it can start tracking the next 5 points
                accumulatedDamage = 0f;
            }
        }

        // Update tracking to match current health (handles healing smoothly without breaking the counter)
        lastHealth = player.health;
    }

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
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        damageCoroutine = StartCoroutine(SmoothTiltWithJitterRoutine());
    }

    private IEnumerator SmoothTiltWithJitterRoutine()
    {
        float elapsed = 0f;
        float tiltDirection = Random.value > 0.5f ? 1f : -1f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectDuration;

            // Base smooth curve (bell curve shape)
            float intensity = Mathf.Sin(t * Mathf.PI);

            // 1. Calculate the smooth Minecraft roll (Z-axis) & pitch (X-axis)
            float currentTiltZ = maxTiltAngle * intensity * tiltDirection;
            float currentTiltX = -maxTiltAngle * 0.15f * intensity;

            // 2. Layer a micro-rotational shake on top (adds a loose, impactful rattle)
            float jitterRotX = Random.Range(-1f, 1f) * rotationJitter * intensity;
            float jitterRotY = Random.Range(-1f, 1f) * rotationJitter * intensity;
            CameraTilt = new Vector3(currentTiltX + jitterRotX, jitterRotY, currentTiltZ);

            // 3. Layer a micro-positional offset (vibrates the camera frame ever so slightly)
            float jitterPosX = Random.Range(-1f, 1f) * positionJitter * intensity;
            float jitterPosY = Random.Range(-1f, 1f) * positionJitter * intensity;
            CameraPositionOffset = new Vector3(jitterPosX, jitterPosY, 0f);

            // 4. Handle Red UI Flash
            if (redFlashImage != null)
            {
                Color c = redFlashImage.color;
                c.a = maxRedAlpha * intensity;
                redFlashImage.color = c;
            }

            yield return null;
        }

        // Complete reset back to rest values
        CameraTilt = Vector3.zero;
        CameraPositionOffset = Vector3.zero;
        if (redFlashImage != null)
        {
            Color c = redFlashImage.color;
            c.a = 0f;
            redFlashImage.color = c;
        }
    }

    //public void PlayDamageFlash()
    //{
    //    StartCoroutine(FlashRoutine());
    //}

    private System.Collections.IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);
    }
}