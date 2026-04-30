using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Debug menu toggled via Input Action "DebugMenu".
/// Attach to DebugMenuPanel under UI elements.
/// The key binding is set in your Input Actions asset and can be rebound anytime.
/// </summary>
public class DebugMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject debugPanel;

    [Header("Time of Day")]
    [SerializeField] private TMP_InputField hourInputField;    // type 0-23
    [SerializeField] private TextMeshProUGUI currentTimeLabel; // shows live "Current: 14:32"

    private World world;
    private PlayerInput playerInput;
    private bool isOpen = false;

    public void Start()
    {
        debugPanel.SetActive(false);

        world = FindFirstObjectByType<World>();

        // Find PlayerInput on the Player — same pattern as InventoryUI
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerInput = player.GetComponent<PlayerInput>();

        // Fires when player presses Enter in the input field
        if (hourInputField != null)
            hourInputField.onEndEdit.AddListener(OnHourSubmitted);
    }

    private void Update()
    {
        // TEMP: log every frame so we know the script is running at all
        if (playerInput == null)
        {
            Debug.Log("[DebugMenu] playerInput is NULL");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerInput = player.GetComponent<PlayerInput>();
            return;
        }

        // TEMP: log that we're checking the action
        Debug.Log("[DebugMenu] checking DebugMenu action");

        if (playerInput.actions["DebugMenu"].WasPressedThisFrame())
        {
            Debug.Log("[DebugMenu] TogglePanel called!");
            TogglePanel();
        }

        if (isOpen)
            UpdateTimeLabel();
    }

    // ─────────────────────────────────────────────
    // Panel toggle
    // ─────────────────────────────────────────────
    private void TogglePanel()
    {
        isOpen = !isOpen;
        debugPanel.SetActive(isOpen);

        if (isOpen)
            UpdateTimeLabel();
    }

    // ─────────────────────────────────────────────
    // Hour input — fires on Enter
    // ─────────────────────────────────────────────
    private void OnHourSubmitted(string input)
    {
        if (world == null) return;

        if (int.TryParse(input, out int hour))
        {
            // Clamp to valid 0-23 range
            hour = Mathf.Clamp(hour, 0, 23);

            // Preserve current minutes, only change the hour
            int currentMinutes = world.DayTime % 60;
            world.DayTime = (hour * 60) + currentMinutes;

            UpdateTimeLabel();
        }
        else
        {
            Debug.LogWarning($"[DebugMenu] Invalid hour input: '{input}' — enter a number 0-23");
        }

        // Clear the field ready for next input
        hourInputField.SetTextWithoutNotify("");
    }

    // ─────────────────────────────────────────────
    // Live label — converts DayTime to HH:MM
    // ─────────────────────────────────────────────
    private void UpdateTimeLabel()
    {
        if (currentTimeLabel == null || world == null) return;

        int hours = world.DayTime / 60;
        int minutes = world.DayTime % 60;
        currentTimeLabel.text = $"Current: {hours:00}:{minutes:00}";
    }
}