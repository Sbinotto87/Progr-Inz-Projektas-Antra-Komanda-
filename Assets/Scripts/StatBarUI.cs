using UnityEngine;
using UnityEngine.UI;

public class StatBarUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public Slider HealthBar;
    public Slider HungerBar;
    public Slider ThirstBar;

    public float health = 100f;
    public float hunger = 100f;
    public float thirst = 100f;

    public float depletionRate = 10f; 

    // Update is called once per frame
    void Update()
    {
        // 1. Reduce hunger and thirst over time
        if (hunger > 0) hunger -= depletionRate * Time.deltaTime;
        if (thirst > 0) thirst -= depletionRate * Time.deltaTime;

        // 2. If hunger or thirst is 0, drain health
        if (hunger <= 0 || thirst <= 0)
        {
            health -= (depletionRate / 2) * Time.deltaTime;
        }

        // 3. Update the visual bars 
        HungerBar.value = Mathf.Lerp(HungerBar.value, hunger / 100f, Time.deltaTime * 5f);
        ThirstBar.value = Mathf.Lerp(ThirstBar.value, thirst / 100f, Time.deltaTime * 5f);
        HealthBar.value = Mathf.Lerp(HealthBar.value, health / 100f, Time.deltaTime * 5f);
    }
}
