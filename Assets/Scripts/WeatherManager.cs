using UnityEngine;

public enum WeatherType
{
    Clear,
    Rain,
    Snow
}

public class WeatherManager : MonoBehaviour
{
    public WeatherType currentWeather;

    [Header("Particle Systems")]
    public GameObject rainEffect;
    public GameObject snowEffect;
    public float changeInterval = 10f;

    void Start()
    {
        InvokeRepeating(nameof(SetRandomWeather), 0f, changeInterval);
    }

    public void SetRandomWeather()
    {
        int rand = Random.Range(0, 3);
        currentWeather = (WeatherType)rand;

        ApplyWeather();
    }

    void ApplyWeather()
    {
        // Disable all first
        rainEffect.SetActive(false);
        snowEffect.SetActive(false);

        switch (currentWeather)
        {
            case WeatherType.Clear:
                break;

            case WeatherType.Rain:
                rainEffect.SetActive(true);
                break;

            case WeatherType.Snow:
                snowEffect.SetActive(true);
                break;
        }
    }
}