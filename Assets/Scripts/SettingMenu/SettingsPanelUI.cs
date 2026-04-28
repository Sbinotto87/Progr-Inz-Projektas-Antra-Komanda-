using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the settings overlay UI.
/// The panel is divided into three category tabs: Audio, Graphics, and Keybinds.
///
/// Required UI hierarchy under settingsPanel:
///   TabBar/
///     AudioTabButton    (Button)
///     GraphicsTabButton (Button)
///     KeybindsTabButton (Button)
///   AudioPanel/
///     MasterVolumeSlider  (Slider)  + MasterVolumeLabel  (TMP_Text – shows "80%")
///     MusicVolumeSlider   (Slider)  + MusicVolumeLabel   (TMP_Text – shows "80%")
///     SFXVolumeSlider     (Slider)  + SFXVolumeLabel     (TMP_Text – shows "100%")
///   GraphicsPanel/
///     MinFovSlider        (Slider)
///     MaxFovSlider        (Slider)
///     FovRangeLabel       (TMP_Text – shows "70 – 90", shared label for both FOV sliders)
///     RenderDistanceSlider(Slider)  + RenderDistanceLabel(TMP_Text – shows "10")
///     QualityDropdown     (TMP_Dropdown)
///     FullscreenToggle    (Toggle)
///   KeybindsPanel/
///     MouseSensitivitySlider (Slider)  + MouseSensitivityLabel (TMP_Text – shows "0.45")
///     KeybindEntriesContainer (Transform – populated at runtime by KeybindManager)
/// </summary>
public class SettingsPanelUI : MonoBehaviour
{
    public static SettingsPanelUI Instance { get; private set; }

    [Header("Root Panel")]
    [SerializeField] private GameObject settingsPanel;

    // ── Tab buttons ──────────────────────────────────────────────────────────
    [Header("Category Tab Buttons")]
    [SerializeField] private Button audioTabButton;
    [SerializeField] private Button graphicsTabButton;
    [SerializeField] private Button keybindsTabButton;

    // ── Category panels ───────────────────────────────────────────────────────
    [Header("Category Panels")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject keybindsPanel;

    // ── Audio sliders + labels ────────────────────────────────────────────────
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeLabel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeLabel;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeLabel;

    // ── Graphics controls + labels ────────────────────────────────────────────
    [Header("Graphics Controls")]
    [SerializeField] private Slider minFovSlider;
    [SerializeField] private Slider maxFovSlider;
    /// <summary>Single combined label that shows the FOV range, e.g. "70 – 90".</summary>
    [SerializeField] private TMP_Text fovRangeLabel;
    [SerializeField] private Slider renderDistanceSlider;
    [SerializeField] private TMP_Text renderDistanceLabel;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown blockMaterialDropdown;

    // ── Keybinds / Controls ───────────────────────────────────────────────────
    [Header("Keybinds / Controls")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TMP_Text mouseSensitivityLabel;

    // ── Behaviour options ─────────────────────────────────────────────────────
    [Header("Optional Behaviour")]
    [SerializeField] private bool pauseTimeWhenOpen;

    [Header("Optional Parent Menu")]
    [SerializeField] private GameObject menuToHideWhenOpen;

    private bool listenersRegistered;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Instance already points to the valid singleton; destroy this duplicate.
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PopulateQualityDropdown();
        PopulateBlockMaterialDropdown();
        RegisterListeners();
        SyncAllFromSettings();
        ShowTab(audioPanel);
    }

    private void OnDestroy()
    {
        UnregisterListeners();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Open()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("SettingsPanelUI: settingsPanel reference is missing.");
            return;
        }

        if (menuToHideWhenOpen != null)
            menuToHideWhenOpen.SetActive(false);

        settingsPanel.SetActive(true);
        SyncAllFromSettings();
        ShowTab(audioPanel);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (pauseTimeWhenOpen)
            Time.timeScale = 0f;
    }

    public void Close()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);

        if (menuToHideWhenOpen != null)
            menuToHideWhenOpen.SetActive(true);
    }

    // ── Tab switching (called by tab buttons via OnClick) ─────────────────────

    public void ShowAudioTab()     => ShowTab(audioPanel);
    public void ShowGraphicsTab()  => ShowTab(graphicsPanel);
    public void ShowKeybindsTab()  => ShowTab(keybindsPanel);

    // ── Audio callbacks ───────────────────────────────────────────────────────

    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetMasterVolume(value);
        SetLabel(masterVolumeLabel, FormatPercent(value));
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetMusicVolume(value);
        SetLabel(musicVolumeLabel, FormatPercent(value));
    }

    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetSFXVolume(value);
        SetLabel(sfxVolumeLabel, FormatPercent(value));
    }

    // ── Graphics callbacks ────────────────────────────────────────────────────

    public void OnMinFovChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m == null) return;

        float max = maxFovSlider != null ? maxFovSlider.value : m.MaxFov;
        m.SetFovRange(value, max);

        if (maxFovSlider != null && maxFovSlider.value < m.MinFov)
            maxFovSlider.SetValueWithoutNotify(m.MinFov);

        UpdateFovLabel(m.MinFov, m.MaxFov);
    }

    public void OnMaxFovChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m == null) return;

        float min = minFovSlider != null ? minFovSlider.value : m.MinFov;
        m.SetFovRange(min, value);

        if (minFovSlider != null && minFovSlider.value > m.MaxFov)
            minFovSlider.SetValueWithoutNotify(m.MaxFov);

        UpdateFovLabel(m.MinFov, m.MaxFov);
    }

    public void OnRenderDistanceChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetRenderDistance((int)value);
        SetLabel(renderDistanceLabel, Mathf.RoundToInt(value).ToString());
    }

    public void OnQualityChanged(int index)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetQualityLevel(index);
    }

    public void OnFullscreenChanged(bool value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetFullscreen(value);
    }

    public void OnBlockMaterialChanged(int index)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetBlockMaterialIndex(index);
    }

    // ── Controls callbacks ────────────────────────────────────────────────────

    public void OnMouseSensitivityChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetMouseSensitivity(value);
        SetLabel(mouseSensitivityLabel, value.ToString("F2"));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ShowTab(GameObject activePanel)
    {
        if (audioPanel != null)    audioPanel.SetActive(audioPanel == activePanel);
        if (graphicsPanel != null) graphicsPanel.SetActive(graphicsPanel == activePanel);
        if (keybindsPanel != null) keybindsPanel.SetActive(keybindsPanel == activePanel);
    }

    private void PopulateBlockMaterialDropdown()
    {
        if (blockMaterialDropdown == null) return;

        SettingsManager m = EnsureManager();
        blockMaterialDropdown.ClearOptions();
        string[] names = m != null
            ? m.GetBlockMaterialNames()
            : new string[] { "Default" };
        blockMaterialDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
    }

    private void PopulateQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);
    }

    private void SyncAllFromSettings()
    {
        SettingsManager m = EnsureManager();
        if (m == null) return;

        if (masterVolumeSlider != null)     masterVolumeSlider.SetValueWithoutNotify(m.MasterVolume);
        if (musicVolumeSlider != null)      musicVolumeSlider.SetValueWithoutNotify(m.MusicVolume);
        if (sfxVolumeSlider != null)        sfxVolumeSlider.SetValueWithoutNotify(m.SFXVolume);

        if (minFovSlider != null)           minFovSlider.SetValueWithoutNotify(m.MinFov);
        if (maxFovSlider != null)           maxFovSlider.SetValueWithoutNotify(m.MaxFov);
        if (renderDistanceSlider != null)   renderDistanceSlider.SetValueWithoutNotify(m.RenderDistance);
        if (qualityDropdown != null)        qualityDropdown.SetValueWithoutNotify(m.QualityLevel);
        if (fullscreenToggle != null)       fullscreenToggle.SetIsOnWithoutNotify(m.IsFullscreen);
        if (blockMaterialDropdown != null)  blockMaterialDropdown.SetValueWithoutNotify(m.BlockMaterialIndex);

        if (mouseSensitivitySlider != null) mouseSensitivitySlider.SetValueWithoutNotify(m.MouseSensitivity);

        UpdateAllLabels(m);
    }

    /// <summary>Refreshes every value label to match the current settings state.</summary>
    private void UpdateAllLabels(SettingsManager m)
    {
        SetLabel(masterVolumeLabel,      FormatPercent(m.MasterVolume));
        SetLabel(musicVolumeLabel,       FormatPercent(m.MusicVolume));
        SetLabel(sfxVolumeLabel,         FormatPercent(m.SFXVolume));
        UpdateFovLabel(m.MinFov, m.MaxFov);
        SetLabel(renderDistanceLabel,    m.RenderDistance.ToString());
        SetLabel(mouseSensitivityLabel,  m.MouseSensitivity.ToString("F2"));
    }

    /// <summary>Sets the combined FOV label to "minFov – maxFov" (integer values).</summary>
    private void UpdateFovLabel(float min, float max)
    {
        SetLabel(fovRangeLabel, $"{Mathf.RoundToInt(min)} – {Mathf.RoundToInt(max)}");
    }

    private static void SetLabel(TMP_Text label, string text)
    {
        if (label != null)
            label.text = text;
    }

    private static string FormatPercent(float value)
        => Mathf.RoundToInt(value * 100f) + "%";

    private SettingsManager EnsureManager()
    {
        if (SettingsManager.Instance != null)
            return SettingsManager.Instance;

        SettingsManager manager = FindFirstObjectByType<SettingsManager>();
        if (manager != null)
            return manager;

        GameObject obj = new GameObject("SettingsManager");
        return obj.AddComponent<SettingsManager>();
    }

    private void RegisterListeners()
    {
        if (listenersRegistered) return;

        // Tab buttons
        if (audioTabButton != null)    audioTabButton.onClick.AddListener(ShowAudioTab);
        if (graphicsTabButton != null) graphicsTabButton.onClick.AddListener(ShowGraphicsTab);
        if (keybindsTabButton != null) keybindsTabButton.onClick.AddListener(ShowKeybindsTab);

        // Audio
        if (masterVolumeSlider != null)   masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)    musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)      sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Graphics
        if (minFovSlider != null)         minFovSlider.onValueChanged.AddListener(OnMinFovChanged);
        if (maxFovSlider != null)         maxFovSlider.onValueChanged.AddListener(OnMaxFovChanged);
        if (renderDistanceSlider != null) renderDistanceSlider.onValueChanged.AddListener(OnRenderDistanceChanged);
        if (qualityDropdown != null)      qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (fullscreenToggle != null)     fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (blockMaterialDropdown != null) blockMaterialDropdown.onValueChanged.AddListener(OnBlockMaterialChanged);

        // Controls
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        listenersRegistered = true;
    }

    private void UnregisterListeners()
    {
        if (!listenersRegistered) return;

        // Tab buttons
        if (audioTabButton != null)    audioTabButton.onClick.RemoveListener(ShowAudioTab);
        if (graphicsTabButton != null) graphicsTabButton.onClick.RemoveListener(ShowGraphicsTab);
        if (keybindsTabButton != null) keybindsTabButton.onClick.RemoveListener(ShowKeybindsTab);

        // Audio
        if (masterVolumeSlider != null)   masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)    musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)      sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        // Graphics
        if (minFovSlider != null)         minFovSlider.onValueChanged.RemoveListener(OnMinFovChanged);
        if (maxFovSlider != null)         maxFovSlider.onValueChanged.RemoveListener(OnMaxFovChanged);
        if (renderDistanceSlider != null) renderDistanceSlider.onValueChanged.RemoveListener(OnRenderDistanceChanged);
        if (qualityDropdown != null)      qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        if (fullscreenToggle != null)     fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (blockMaterialDropdown != null) blockMaterialDropdown.onValueChanged.RemoveListener(OnBlockMaterialChanged);

        // Controls
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

        listenersRegistered = false;
    }
}
