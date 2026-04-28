using Assets.Scripts;
using UnityEngine;

public class SkyboxGradientCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float timeOfDay = 12f;
    public float dayDuration = 120f; // seconds for full cycle

    [Header("Skybox Material")]
    public Material skyboxMaterial;

    [Header("Color Gradient")]
    public Gradient skyColorGradient;

    [Header("Exposure Settings")]
    public AnimationCurve exposureCurve = AnimationCurve.Linear(0, 0.3f, 1, 1.3f);

    [Header("Atmosphere Settings")]
    public AnimationCurve atmosphereCurve = AnimationCurve.Linear(0, 1.2f, 1, 0.4f);

    public GameObject World;

    private World world;

    private float t;
    
    private void Start()
    {
        world = World.GetComponent<World>();
    }
    void Update()
    {
        // Advance time
        t = (world.DayTime + world.Tick / 20f) / 1440f % 1;

        UpdateSkybox(t);
    }

    void UpdateSkybox(float t)
    {
        // Gradient color
        Color skyColor = skyColorGradient.Evaluate(t);
        skyboxMaterial.SetColor("_SkyTint", skyColor);

        // Exposure
        float exposure = exposureCurve.Evaluate(t);
        skyboxMaterial.SetFloat("_Exposure", exposure);

        // Atmosphere thickness
        float atmosphere = atmosphereCurve.Evaluate(t);
        skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphere);

        // Optional: sync fog (recommended)
        RenderSettings.fogColor = skyColor;
    }
}