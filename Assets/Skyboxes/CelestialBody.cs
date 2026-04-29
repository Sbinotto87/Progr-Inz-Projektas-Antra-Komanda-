using Assets.Scripts;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Header("References")]
    public GameObject World;
    private World world;

    private Transform player;

    [Header("Light")]
    public Light sun;

    [Header("Visuals")]
    public Transform sunVisual;
    public Transform moonVisual;

    [Header("Settings")]
    public float orbitDistance = 100f;

    [Header("Sun Color")]
    public Gradient sunColorGradient;

    [Header("Intensity")]
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        world = World.GetComponent<World>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float t = (world.DayTime + world.Tick / 20f) / 1440f % 1;

        UpdateSunRotation(t);
        UpdateSunLighting(t);
        UpdateCelestialBodies(t);
    }

    void LateUpdate()
    {
        FacePlayer(sunVisual);
        FacePlayer(moonVisual);
    }

    void UpdateSunRotation(float t)
    {
        float angle = t * 360f - 90f;

        // slight tilt (also helps shadow artifacts)
        sun.transform.rotation = Quaternion.Euler(angle, 165f, 0f);
    }

    void UpdateSunLighting(float t)
    {
        sun.color = sunColorGradient.Evaluate(t);
        sun.intensity = intensityCurve.Evaluate(t);
    }

    void UpdateCelestialBodies(float t)
    {
        float angle = t * 360f - 90f;

        Quaternion rotation = Quaternion.Euler(angle, 165f, 0f);
        Vector3 direction = rotation * Vector3.forward;

        Vector3 center = player.position;

        // Sun position
        moonVisual.position = center + direction * orbitDistance;

        // Moon opposite
        sunVisual.position = center - direction * orbitDistance;

        // Optional: fade near horizon
        float height = Vector3.Dot(direction, Vector3.up);
        float visibility = Mathf.Clamp01(height);

        SetAlpha(moonVisual, visibility);
        SetAlpha(sunVisual, 1f - visibility);
    }

    void FacePlayer(Transform obj)
    {
        obj.LookAt(player);
        obj.Rotate(0f, 180f, 0f);
    }

    void SetAlpha(Transform obj, float a)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color c = renderer.material.color;
            c.a = a;
            renderer.material.color = c;
        }
    }
}