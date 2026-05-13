using UnityEngine;
using UnityEngine.UI;

public class StatBarUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();

    }
    Player player;

    public Slider HealthBar;
    public Slider HungerBar;
    public Slider ThirstBar;
    public Slider Staminabar; 

    private float radMultiplier = 1f;


    public float depletionRate = 5f; 

    // Update is called once per frame
    void Update()
    {
        // Pull cheat flags. Null-safe: if no CheatsManager in scene, all flags read false
        // and behavior is identical to your original.
        var c = CheatsManager.Instance;
        bool master = c != null && c.CheatsEnabled;
        bool infHealth = master && c.InfiniteHealth;
        bool infHunger = master && c.InfiniteHunger;
        bool infThirst = master && c.InfiniteThirst;
        bool infStamina = master && c.InfiniteStamina;
        bool noRad = master && c.NoRadiationDamage;

        // 1. Reduce hunger and thirst over time
        if (infHunger) player.hunger = 100f;
        else if (player.hunger > 0) player.hunger -= depletionRate * Time.deltaTime;

        if (infThirst) player.thirst = 100f;
        else if (player.thirst > 0) player.thirst -= depletionRate * Time.deltaTime;

        // 2. If hunger or thirst is 0, drain health
        if (!infHealth && (player.hunger <= 0 || player.thirst <= 0))
        {
            player.health -= (depletionRate / 2) * Time.deltaTime;
        }
        if (player.isInRadiation && !noRad && !infHealth)
        {
            player.health -= radMultiplier * Time.deltaTime;
        }

        if (infHealth) player.health = 1000f;
        if (infStamina) player.stamina = 100f;

        // 3. Update the visual bars 
        HungerBar.value = Mathf.Lerp(HungerBar.value, player.hunger / 100f, Time.deltaTime * 5f);
        ThirstBar.value = Mathf.Lerp(ThirstBar.value, player.thirst / 100f, Time.deltaTime * 5f);
        HealthBar.value = Mathf.Lerp(HealthBar.value, player.health / 100f, Time.deltaTime * 5f);

        // Update the stamina bar based on the player's stamina
        Staminabar.value = Mathf.Lerp(Staminabar.value, player.stamina / 100f, Time.deltaTime * 10f);
    }
}
