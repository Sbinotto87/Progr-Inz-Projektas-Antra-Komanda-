using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "Cheats" sub-panel inside your in-game Settings menu.
///
/// Setup in Unity:
///   1. Open the InGameSettingsMenuCanvas prefab.
///   2. Add a new panel (or tab) called "Cheats" with these UI elements:
///        - Toggle: "Enable Cheats"      -> assign to `enableCheatsToggle`
///        - Toggle: "Infinite Health"    -> assign to `infiniteHealthToggle`
///        - Toggle: "Infinite Hunger"    -> assign to `infiniteHungerToggle`
///        - Toggle: "Infinite Thirst"    -> assign to `infiniteThirstToggle`
///        - Toggle: "Infinite Stamina"   -> assign to `infiniteStaminaToggle`
///        - Toggle: "No Radiation"       -> assign to `noRadiationToggle`
///        - Toggle: "Flight"             -> assign to `flightToggle`
///        - Toggle: "No-clip"            -> assign to `noClipToggle`
///        - Toggle: "One-shot Kill"      -> assign to `oneShotKillToggle`
///        - Slider: "Damage Multiplier"  -> assign to `damageMultiplierSlider`  (range 0.1 - 50)
///   3. Add a button in the main settings panel that opens the Cheats panel
///      (calls Open()), and a Back button on the Cheats panel that calls Close().
///   4. Attach this script to the Cheats panel root and wire up the references.
///
/// The toggles are auto-disabled (greyed out via interactable=false) when the master
/// cheats toggle is off, so users can't accidentally fly without enabling cheats first.
/// </summary>
public class CheatsPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject cheatsPanel;

    [Header("Master")]
    [SerializeField] private Toggle enableCheatsToggle;

    [Header("Survival")]
    [SerializeField] private Toggle infiniteHealthToggle;
    [SerializeField] private Toggle infiniteHungerToggle;
    [SerializeField] private Toggle infiniteThirstToggle;
    [SerializeField] private Toggle infiniteStaminaToggle;
    [SerializeField] private Toggle noRadiationToggle;

    [Header("Movement")]
    [SerializeField] private Toggle flightToggle;
    [SerializeField] private Toggle noClipToggle;

    [Header("Combat")]
    [SerializeField] private Toggle oneShotKillToggle;
    [SerializeField] private Slider damageMultiplierSlider;

    [Header("Optional Parent Menus")]
    [SerializeField] private GameObject menuToHideWhenOpen;

    private bool listenersRegistered;

    private void Start()
    {
        if (cheatsPanel != null) cheatsPanel.SetActive(false);
        RegisterListeners();
        SyncFromManager();
    }

    private void OnDestroy() => UnregisterListeners();

    public void Open()
    {
        if (cheatsPanel == null) return;
        if (menuToHideWhenOpen != null) menuToHideWhenOpen.SetActive(false);
        cheatsPanel.SetActive(true);
        SyncFromManager();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void Close()
    {
        if (cheatsPanel == null) return;
        cheatsPanel.SetActive(false);
        if (menuToHideWhenOpen != null) menuToHideWhenOpen.SetActive(true);
    }

    private void SyncFromManager()
    {
        var c = CheatsManager.Instance;
        Set(enableCheatsToggle,    c.CheatsEnabled);
        Set(infiniteHealthToggle,  c.InfiniteHealth);
        Set(infiniteHungerToggle,  c.InfiniteHunger);
        Set(infiniteThirstToggle,  c.InfiniteThirst);
        Set(infiniteStaminaToggle, c.InfiniteStamina);
        Set(noRadiationToggle,     c.NoRadiationDamage);
        Set(flightToggle,          c.Flight);
        Set(noClipToggle,          c.NoClip);
        Set(oneShotKillToggle,     c.OneShotKill);
        if (damageMultiplierSlider != null)
            damageMultiplierSlider.SetValueWithoutNotify(c.DamageMultiplier);
        UpdateInteractable();
    }

    private static void Set(Toggle t, bool v) { if (t != null) t.SetIsOnWithoutNotify(v); }

    private void UpdateInteractable()
    {
        bool enabled = CheatsManager.Instance.CheatsEnabled;
        SetInter(infiniteHealthToggle,  enabled);
        SetInter(infiniteHungerToggle,  enabled);
        SetInter(infiniteThirstToggle,  enabled);
        SetInter(infiniteStaminaToggle, enabled);
        SetInter(noRadiationToggle,     enabled);
        SetInter(flightToggle,          enabled);
        SetInter(noClipToggle,          enabled);
        SetInter(oneShotKillToggle,     enabled);
        if (damageMultiplierSlider != null) damageMultiplierSlider.interactable = enabled;
    }
    private static void SetInter(Toggle t, bool v) { if (t != null) t.interactable = v; }

    private void RegisterListeners()
    {
        if (listenersRegistered) return;

        if (enableCheatsToggle != null)
            enableCheatsToggle.onValueChanged.AddListener(v => { CheatsManager.Instance.CheatsEnabled = v; UpdateInteractable(); CheatsManager.Instance.NotifyChanged(); });

        Hook(infiniteHealthToggle,  v => CheatsManager.Instance.InfiniteHealth   = v);
        Hook(infiniteHungerToggle,  v => CheatsManager.Instance.InfiniteHunger   = v);
        Hook(infiniteThirstToggle,  v => CheatsManager.Instance.InfiniteThirst   = v);
        Hook(infiniteStaminaToggle, v => CheatsManager.Instance.InfiniteStamina  = v);
        Hook(noRadiationToggle,     v => CheatsManager.Instance.NoRadiationDamage = v);
        Hook(flightToggle,          v => CheatsManager.Instance.Flight           = v);
        Hook(noClipToggle,          v => CheatsManager.Instance.NoClip           = v);
        Hook(oneShotKillToggle,     v => CheatsManager.Instance.OneShotKill      = v);

        if (damageMultiplierSlider != null)
            damageMultiplierSlider.onValueChanged.AddListener(v => CheatsManager.Instance.DamageMultiplier = v);

        listenersRegistered = true;
    }

    private static void Hook(Toggle t, System.Action<bool> setter)
    {
        if (t == null) return;
        t.onValueChanged.AddListener(v => { setter(v); CheatsManager.Instance.NotifyChanged(); });
    }

    private void UnregisterListeners()
    {
        if (!listenersRegistered) return;
        if (enableCheatsToggle != null) enableCheatsToggle.onValueChanged.RemoveAllListeners();
        if (infiniteHealthToggle != null)  infiniteHealthToggle.onValueChanged.RemoveAllListeners();
        if (infiniteHungerToggle != null)  infiniteHungerToggle.onValueChanged.RemoveAllListeners();
        if (infiniteThirstToggle != null)  infiniteThirstToggle.onValueChanged.RemoveAllListeners();
        if (infiniteStaminaToggle != null) infiniteStaminaToggle.onValueChanged.RemoveAllListeners();
        if (noRadiationToggle != null)     noRadiationToggle.onValueChanged.RemoveAllListeners();
        if (flightToggle != null)          flightToggle.onValueChanged.RemoveAllListeners();
        if (noClipToggle != null)          noClipToggle.onValueChanged.RemoveAllListeners();
        if (oneShotKillToggle != null)     oneShotKillToggle.onValueChanged.RemoveAllListeners();
        if (damageMultiplierSlider != null) damageMultiplierSlider.onValueChanged.RemoveAllListeners();
        listenersRegistered = false;
    }
}
