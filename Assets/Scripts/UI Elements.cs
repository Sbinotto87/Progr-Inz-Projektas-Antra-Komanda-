using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElements : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Slider HealthBar;
    public Slider HungerBar;
    public Slider ThirstBar;



    public float depletionRate = 2f;

    // Text for coordinates
    public TMP_Text coordinateText;

    Player player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }



    // Update is called once per frame
    void Update()
    {
        // 1. Reduce hunger and thirst over time
        if (player.hunger > 0) player.hunger -= depletionRate * Time.deltaTime;
        if (player.thirst > 0) player.thirst -= depletionRate * Time.deltaTime;

        // 2. If hunger or thirst is 0, drain health
        if (player.hunger <= 0 || player.thirst <= 0)
        {
            player.health -= (depletionRate / 2) * Time.deltaTime;
        }

        // 3. Update the visual bars 
        HungerBar.value = Mathf.Lerp(HungerBar.value, player.hunger / 100f, Time.deltaTime * 5f);
        ThirstBar.value = Mathf.Lerp(ThirstBar.value, player.thirst / 100f, Time.deltaTime * 5f);
        HealthBar.value = Mathf.Lerp(HealthBar.value, player.health / 100f, Time.deltaTime * 5f);


    }
}
