using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts;

/// <summary>
/// Dev testing menu, toggled with the backquote (`) key.
/// Lets you change time of day and quickly toggle the survival cheats so you can
/// test features without dying every two minutes.
///
/// Drop this on any persistent GameObject in your gameplay scene
/// (e.g. the same one that holds GameMenuManager). No prefab/canvas setup needed -
/// it draws with IMGUI.
/// </summary>
public class DevMenuUI : MonoBehaviour
{
    [Header("Toggle key")]
    [SerializeField] private Key toggleKey = Key.Backquote;

    [Header("Visuals")]
    [SerializeField] private Vector2 panelPosition = new Vector2(20, 80);
    [SerializeField] private Vector2 panelSize = new Vector2(480, 560);

    private bool isOpen;
    private float prevTimeScale = 1f;
    private bool pausedByUs = false;

    // Cached input/cursor state so we can restore it on close
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;
    private PlayerInput playerInput;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (isOpen) Close();
            else Open();
        }
    }

    private void Open()
    {
        isOpen = true;
        prevCursorVisible = Cursor.visible;
        prevCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        // We DON'T pause time - the whole point of this menu is to observe live changes.
        // If you'd rather pause: uncomment the next two lines.
        // prevTimeScale = Time.timeScale; pausedByUs = true; Time.timeScale = 0f;

        // Disable player input so opening the menu doesn't also place a block, etc.
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) playerInput = playerGO.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;
    }

    private void Close()
    {
        isOpen = false;
        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevCursorLock;
        if (pausedByUs) { Time.timeScale = prevTimeScale; pausedByUs = false; }
        if (playerInput != null) playerInput.enabled = true;
    }

    private Vector2 scroll;

    private void OnGUI()
    {
        if (!isOpen) return;

        var cheats = CheatsManager.Instance;

        GUI.Box(new Rect(panelPosition.x, panelPosition.y, panelSize.x, panelSize.y), "Dev Testing Menu (`)");

        GUILayout.BeginArea(new Rect(panelPosition.x + 10, panelPosition.y + 25, panelSize.x - 20, panelSize.y - 35));
        scroll = GUILayout.BeginScrollView(scroll);

        // ----- Time of day -----
        GUILayout.Label("<b>Time of day</b>", RichLabel());
        int day = cheats.GetCurrentDay();
        int dayTime = cheats.GetDayTime();
        GUILayout.Label($"Day {day}  -  {FormatTime(dayTime)}  ({dayTime}/1440)");
        int newTime = (int)GUILayout.HorizontalSlider(dayTime, 0, 1439);
        if (newTime != dayTime) cheats.SetDayTime(newTime);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dawn (300)")) cheats.SetDayTime(300);
        if (GUILayout.Button("Noon (720)")) cheats.SetDayTime(720);
        if (GUILayout.Button("Dusk (1080)")) cheats.SetDayTime(1080);
        if (GUILayout.Button("Midnight (0)")) cheats.SetDayTime(0);
        GUILayout.EndHorizontal();

        bool freeze = ToggleButton("Freeze time", cheats.FreezeTime);
        if (freeze != cheats.FreezeTime)
        {
            cheats.FreezeTime = freeze;
            if (freeze) cheats.FrozenDayTime = cheats.GetDayTime();
            cheats.NotifyChanged();
        }

        GUILayout.Space(10);

        // ----- Master switch -----
        bool master = ToggleButton("ENABLE CHEATS (master)", cheats.CheatsEnabled);
        if (master != cheats.CheatsEnabled) { cheats.CheatsEnabled = master; cheats.NotifyChanged(); }

        GUI.enabled = cheats.CheatsEnabled;

        GUILayout.Space(6);
        GUILayout.Label("<b>Survival</b>", RichLabel());
        cheats.InfiniteHealth = ToggleButton("Infinite health", cheats.InfiniteHealth);
        cheats.InfiniteHunger = ToggleButton("Infinite hunger", cheats.InfiniteHunger);
        cheats.InfiniteThirst = ToggleButton("Infinite thirst", cheats.InfiniteThirst);
        cheats.InfiniteStamina = ToggleButton("Infinite stamina", cheats.InfiniteStamina);
        cheats.NoRadiationDamage = ToggleButton("No radiation damage", cheats.NoRadiationDamage);

        GUILayout.Space(6);
        GUILayout.Label("<b>Movement</b>", RichLabel());
        cheats.Flight = ToggleButton("Flight  (Space=up, Ctrl=down, Shift=fast)", cheats.Flight);
        cheats.NoClip = ToggleButton("No-clip (needs FlightController)", cheats.NoClip);

        GUILayout.Space(6);
        GUILayout.Label("<b>Combat (boss test)</b>", RichLabel());
        cheats.OneShotKill = ToggleButton("One-shot kill enemies", cheats.OneShotKill);

        GUILayout.Space(6);
        GUILayout.Label("<b>Boss</b>", RichLabel());
        if (GUILayout.Button("Spawn boss NOW", GUILayout.ExpandWidth(true)))
        {
            cheats.ForceBossSpawn = true;
        }

        var spawner = UnityEngine.Object.FindFirstObjectByType<BossSpawner>();
        if (spawner != null)
        {
            GUILayout.Label($"Next boss day: {spawner.nextBossDay}  (current day: {cheats.GetCurrentDay()})");
            GUILayout.Label($"Interval: {spawner.bossDayInterval} days");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Set next boss day to:", GUILayout.Width(150));
            if (GUILayout.Button("Today", GUILayout.Width(80))) cheats.BossDayOverride = cheats.GetCurrentDay();
            if (GUILayout.Button("+1 day", GUILayout.Width(80))) cheats.BossDayOverride = cheats.GetCurrentDay() + 1;
            if (GUILayout.Button("+5 days", GUILayout.Width(80))) cheats.BossDayOverride = cheats.GetCurrentDay() + 5;
            GUILayout.EndHorizontal();

            if (spawner.HasActiveBoss && spawner.CurrentBoss != null)
            {
                GUILayout.Label($"<b>Boss alive</b>: HP {Mathf.CeilToInt(spawner.CurrentBoss.health)} / {Mathf.CeilToInt(spawner.CurrentBoss.maxHealth)} - {spawner.CurrentBoss.CurrentPhase.phaseName}", RichLabel());
                if (GUILayout.Button("Kill boss (insta-die)"))
                {
                    spawner.CurrentBoss.TakeDamage(spawner.CurrentBoss.maxHealth + 1);
                }
            }
            else
            {
                GUILayout.Label("No boss currently alive.");
            }
        }
        else
        {
            GUILayout.Label("(No BossSpawner found in scene)");
        }

        GUI.enabled = true;

        GUILayout.Space(10);
        if (GUILayout.Button("Close  (`)")) Close();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private static bool ToggleButton(string label, bool value)
    {
        GUILayout.BeginHorizontal();
        string state = value ? "[ON] " : "[OFF]";
        if (GUILayout.Button($"{state}  {label}", GUILayout.ExpandWidth(true)))
            value = !value;
        GUILayout.EndHorizontal();
        return value;
    }

    private static GUIStyle _rich;
    private static GUIStyle RichLabel()
    {
        if (_rich == null) _rich = new GUIStyle(GUI.skin.label) { richText = true };
        return _rich;
    }

    private static string FormatTime(int minutes)
    {
        int h = (minutes / 60) % 24;
        int m = minutes % 60;
        return $"{h:00}:{m:00}";
    }
}
