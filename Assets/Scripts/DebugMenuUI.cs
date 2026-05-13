using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts;

/// <summary>
/// Real-time debug menu. Toggle with Home.
///
/// Shows a live readout of game state (player stats, world time, position, FPS, etc.)
/// and lets you edit any of the numeric values in place to see the effect immediately.
///
/// Drop on the same persistent GameObject as DevMenuUI. No prefab setup.
///
/// To add another watched variable, scroll to RebuildWatches() and add an entry.
/// </summary>
public class DebugMenuUI : MonoBehaviour
{
    [SerializeField] private Key toggleKey = Key.Home;
    [SerializeField] private Vector2 panelPosition = new Vector2(400, 20);
    [SerializeField] private Vector2 panelSize = new Vector2(420, 560);

    private bool isOpen;
    private Vector2 scroll;
    private float fps;
    private float fpsTimer;
    private int fpsFrames;

    // The list of variables we expose. Built from reflection so you can edit them live.
    private readonly List<Watch> watches = new();
    // Editable text buffers per watch so half-typed values don't get clobbered every frame
    private readonly Dictionary<string, string> editBuffers = new();

    private object playerObj; // cached Player component (typed dynamically to avoid hard ref)
    private World world;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (isOpen) Close();
            else Open();
        }


        // FPS counter
        fpsFrames++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            fps = fpsFrames / fpsTimer;
            fpsFrames = 0;
            fpsTimer = 0f;
        }
    }

    private void RebuildWatches()
    {
        watches.Clear();
        editBuffers.Clear();

        // Player
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerObj = playerGO.GetComponent("Player"); // string overload, avoids hard ref
            if (playerObj != null)
            {
                AddFieldWatch("Player", playerObj, "health");
                AddFieldWatch("Player", playerObj, "hunger");
                AddFieldWatch("Player", playerObj, "thirst");
                AddFieldWatch("Player", playerObj, "stamina");
                AddFieldWatch("Player", playerObj, "isInRadiation");
            }

            // Always-available transform values
            var t = playerGO.transform;
            watches.Add(new Watch("Player", "position", () => t.position.ToString("0.0"), null, false));
        }

        // World
        var worldGO = GameObject.Find("World");
        if (worldGO != null) world = worldGO.GetComponent<World>();
        if (world != null)
        {
            AddFieldWatch("World", world, "DayTime");
            AddFieldWatch("World", world, "CurrentDay");
            AddFieldWatch("World", world, "viewDistance");
            AddFieldWatch("World", world, "IsInRadiation");
            AddFieldWatch("World", world, "Seed");
        }

        // CheatsManager mirror (read-only here - edit via DevMenuUI or settings panel)
        var c = CheatsManager.Instance;
        watches.Add(new Watch("Cheats", "CheatsEnabled", () => c.CheatsEnabled.ToString(), null, false));
        watches.Add(new Watch("Cheats", "Flight", () => c.Flight.ToString(), null, false));
        watches.Add(new Watch("Cheats", "FreezeTime", () => c.FreezeTime.ToString(), null, false));

        // Boss (if a spawner and boss exist)
        var spawner = UnityEngine.Object.FindFirstObjectByType<BossSpawner>();
        if (spawner != null)
        {
            watches.Add(new Watch("Boss", "nextBossDay", () => spawner.nextBossDay.ToString(), null, false));
            watches.Add(new Watch("Boss", "interval", () => spawner.bossDayInterval.ToString(), null, false));
            watches.Add(new Watch("Boss", "alive", () => spawner.HasActiveBoss.ToString(), null, false));
            if (spawner.CurrentBoss != null)
            {
                AddFieldWatch("Boss", spawner.CurrentBoss, "health");
                AddFieldWatch("Boss", spawner.CurrentBoss, "maxHealth");
                watches.Add(new Watch("Boss", "phaseIndex", () => spawner.CurrentBoss.CurrentPhaseIndex.ToString(), null, false));
                watches.Add(new Watch("Boss", "phaseName", () => spawner.CurrentBoss.CurrentPhase.phaseName, null, false));
            }
        }
    }

    private void AddFieldWatch(string group, object target, string fieldName)
    {
        var type = target.GetType();
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) return;

        watches.Add(new Watch(
            group,
            fieldName,
            () =>
            {
                var v = field.GetValue(target);
                return v == null ? "null" : Convert.ToString(v, CultureInfo.InvariantCulture);
            },
            (string text) =>
            {
                try
                {
                    object parsed = ParseTo(text, field.FieldType);
                    if (parsed != null) field.SetValue(target, parsed);
                }
                catch { /* ignore bad input until user finishes typing */ }
            },
            editable: IsNumericOrBool(field.FieldType)
        ));
    }

    private static bool IsNumericOrBool(Type t)
    {
        return t == typeof(int) || t == typeof(float) || t == typeof(double)
            || t == typeof(long) || t == typeof(short) || t == typeof(bool);
    }

    private static object ParseTo(string s, Type t)
    {
        if (t == typeof(int))    return int.Parse(s, CultureInfo.InvariantCulture);
        if (t == typeof(float))  return float.Parse(s, CultureInfo.InvariantCulture);
        if (t == typeof(double)) return double.Parse(s, CultureInfo.InvariantCulture);
        if (t == typeof(long))   return long.Parse(s, CultureInfo.InvariantCulture);
        if (t == typeof(short))  return short.Parse(s, CultureInfo.InvariantCulture);
        if (t == typeof(bool))   return bool.Parse(s);
        return null;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        GUI.Box(new Rect(panelPosition.x, panelPosition.y, panelSize.x, panelSize.y), "Debug Menu (Home)");
        GUILayout.BeginArea(new Rect(panelPosition.x + 10, panelPosition.y + 25, panelSize.x - 20, panelSize.y - 35));

        GUILayout.Label($"FPS: {fps:0.0}    timeScale: {Time.timeScale:0.00}");
        if (GUILayout.Button("Refresh watches")) RebuildWatches();

        scroll = GUILayout.BeginScrollView(scroll);

        string lastGroup = null;
        foreach (var w in watches)
        {
            if (w.Group != lastGroup)
            {
                GUILayout.Space(6);
                GUILayout.Label($"<b>{w.Group}</b>", Rich());
                lastGroup = w.Group;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(w.Name, GUILayout.Width(120));

            string live = w.Read();
            if (w.Editable && w.Write != null)
            {
                // Detect bool vs numeric by reading the current value string
                if (live == "True" || live == "False")
                {
                    bool currentBool = live == "True";
                    string btnLabel = currentBool ? "ON " : "OFF";
                    if (GUILayout.Button(btnLabel, GUILayout.Width(60)))
                    {
                        w.Write((!currentBool).ToString());
                    }
                    GUILayout.Label("", GUILayout.Width(130)); // spacer to keep alignment
                }
                else
                {
                    // Numeric: keep the text field + Set button
                    string key = $"{w.Group}.{w.Name}";
                    if (!editBuffers.ContainsKey(key)) editBuffers[key] = live;
                    if (GUI.GetNameOfFocusedControl() != key) editBuffers[key] = live;

                    GUI.SetNextControlName(key);
                    editBuffers[key] = GUILayout.TextField(editBuffers[key], GUILayout.Width(140));

                    if (GUILayout.Button("Set", GUILayout.Width(50)))
                        w.Write(editBuffers[key]);
                }
            }
            else
            {
                GUILayout.Label(live);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(6);
        if (GUILayout.Button("Close  (Home)")) Close();
        GUILayout.EndArea();
    }

    private static GUIStyle _rich;
    private static GUIStyle Rich()
    {
        if (_rich == null) _rich = new GUIStyle(GUI.skin.label) { richText = true };
        return _rich;
    }

    private class Watch
    {
        public string Group;
        public string Name;
        public Func<string> Read;
        public Action<string> Write;
        public bool Editable;

        public Watch(string g, string n, Func<string> r, Action<string> w, bool editable)
        {
            Group = g; Name = n; Read = r; Write = w; Editable = editable;
        }
    }

    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;
    private PlayerInput playerInput;

    private void Open()
    {
        isOpen = true;
        prevCursorVisible = Cursor.visible;
        prevCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) playerInput = playerGO.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        RebuildWatches();
    }

    private void Close()
    {
        isOpen = false;
        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevCursorLock;
        if (playerInput != null) playerInput.enabled = true;
    }
}
