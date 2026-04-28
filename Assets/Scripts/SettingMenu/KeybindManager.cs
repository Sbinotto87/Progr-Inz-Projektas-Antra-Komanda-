using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages runtime keybind rebinding for the Player action map.
/// Bindings are persisted in PlayerPrefs using Unity Input System's
/// SaveBindingOverridesAsJson / LoadBindingOverridesFromJson API.
///
/// Usage
/// -----
/// • Call <see cref="StartRebind"/> from a <see cref="KeybindEntryUI"/> button press.
/// • Call <see cref="ResetAllBindings"/> to wipe custom overrides.
/// • Attach this component to the same GameObject as <see cref="SettingsPanelUI"/>
///   or any persistent manager object, and assign the <see cref="inputActionAsset"/>
///   reference in the Inspector to "InputSystem_Actions".
/// </summary>
public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance { get; private set; }

    private const string BindingOverridesKey = "KeybindOverrides";
    private const string PlayerMapName = "Player";

    [SerializeField] private InputActionAsset inputActionAsset;

    private InputActionRebindingExtensions.RebindingOperation activeRebind;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadBindingOverrides();
    }

    private void OnDestroy()
    {
        CancelRebind();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the display string for a keyboard/mouse binding of an action.
    /// </summary>
    /// <param name="actionName">Action name as defined in the Player action map.</param>
    public string GetBindingDisplayString(string actionName)
    {
        if (inputActionAsset == null) return "?";

        InputAction action = inputActionAsset.FindActionMap(PlayerMapName)?.FindAction(actionName);
        if (action == null) return "?";

        Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
        Mouse mouse = InputSystem.GetDevice<Mouse>();

        int index = -1;
        if (keyboard != null)
            index = action.GetBindingIndexForControl(keyboard);
        if (index < 0 && mouse != null)
            index = action.GetBindingIndexForControl(mouse);
        if (index < 0 && action.bindings.Count > 0)
            index = 0;

        return index >= 0
            ? InputControlPath.ToHumanReadableString(
                action.bindings[index].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice)
            : "?";
    }

    /// <summary>
    /// Starts interactive rebinding for an action. Calls <paramref name="onComplete"/>
    /// (success=true) or <paramref name="onComplete"/> (success=false) when finished.
    /// </summary>
    public void StartRebind(string actionName, System.Action<bool> onComplete = null)
    {
        if (inputActionAsset == null)
        {
            Debug.LogWarning("KeybindManager: inputActionAsset is not assigned.");
            onComplete?.Invoke(false);
            return;
        }

        InputAction action = inputActionAsset.FindActionMap(PlayerMapName)?.FindAction(actionName);
        if (action == null)
        {
            Debug.LogWarning($"KeybindManager: action '{actionName}' not found in map '{PlayerMapName}'.");
            onComplete?.Invoke(false);
            return;
        }

        CancelRebind();
        action.Disable();

        activeRebind = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(_ =>
            {
                action.Enable();
                SaveBindingOverrides();
                activeRebind.Dispose();
                activeRebind = null;
                onComplete?.Invoke(true);
            })
            .OnCancel(_ =>
            {
                action.Enable();
                activeRebind.Dispose();
                activeRebind = null;
                onComplete?.Invoke(false);
            })
            .Start();
    }

    /// <summary>Cancels any in-progress rebind operation.</summary>
    public void CancelRebind()
    {
        activeRebind?.Cancel();
    }

    /// <summary>
    /// Resets all bindings in the Player map to their defaults and saves.
    /// </summary>
    public void ResetAllBindings()
    {
        if (inputActionAsset == null) return;

        InputActionMap map = inputActionAsset.FindActionMap(PlayerMapName);
        if (map == null) return;

        map.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(BindingOverridesKey);
        PlayerPrefs.Save();
    }

    /// <summary>Returns true if a rebind is currently waiting for input.</summary>
    public bool IsRebinding => activeRebind != null;

    // ── Persistence ───────────────────────────────────────────────────────────

    private void SaveBindingOverrides()
    {
        if (inputActionAsset == null) return;

        string json = inputActionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BindingOverridesKey, json);
        PlayerPrefs.Save();
    }

    private void LoadBindingOverrides()
    {
        if (inputActionAsset == null) return;

        string json = PlayerPrefs.GetString(BindingOverridesKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
            inputActionAsset.LoadBindingOverridesFromJson(json);
    }
}
