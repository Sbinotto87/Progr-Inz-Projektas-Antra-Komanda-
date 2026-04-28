using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single row in the Keybinds settings panel.
/// Displays an action's current keyboard binding and provides a Rebind button.
///
/// Usage
/// -----
/// • Attach to a prefab that contains a label (TMP_Text), a binding display text (TMP_Text),
///   and a rebind Button.
/// • Set <see cref="actionName"/> in the Inspector to the Input System action name
///   (e.g. "Jump", "Sprint", "Interact").
/// • Place instances inside the KeybindEntriesContainer inside the Keybinds panel.
/// </summary>
public class KeybindEntryUI : MonoBehaviour
{
    [Header("Action")]
    [Tooltip("The Player action map action name to rebind (e.g. Jump, Sprint, Interact).")]
    [SerializeField] private string actionName;

    [Header("UI References")]
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private TMP_Text rebindButtonLabel;

    private const string WaitingText = "Press a key...";
    private const string CancelledText = "(cancelled)";

    private void Start()
    {
        if (actionLabel != null)
            actionLabel.text = actionName;

        RefreshBindingDisplay();

        if (rebindButton != null)
            rebindButton.onClick.AddListener(OnRebindClicked);
    }

    private void OnDestroy()
    {
        if (rebindButton != null)
            rebindButton.onClick.RemoveListener(OnRebindClicked);
    }

    // ── Public ────────────────────────────────────────────────────────────────

    /// <summary>Refreshes the binding label from the current state of KeybindManager.</summary>
    public void RefreshBindingDisplay()
    {
        if (bindingText == null) return;

        KeybindManager km = KeybindManager.Instance;
        bindingText.text = km != null
            ? km.GetBindingDisplayString(actionName)
            : "?";
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnRebindClicked()
    {
        KeybindManager km = KeybindManager.Instance;
        if (km == null)
        {
            Debug.LogWarning("KeybindEntryUI: KeybindManager.Instance is null.");
            return;
        }

        SetRebindButtonInteractable(false);
        if (bindingText != null) bindingText.text = WaitingText;

        km.StartRebind(actionName, success =>
        {
            if (bindingText != null)
                bindingText.text = success ? km.GetBindingDisplayString(actionName) : CancelledText;

            SetRebindButtonInteractable(true);
        });
    }

    private void SetRebindButtonInteractable(bool interactable)
    {
        if (rebindButton != null)
            rebindButton.interactable = interactable;

        if (rebindButtonLabel != null)
            rebindButtonLabel.text = interactable ? "Rebind" : WaitingText;
    }
}
