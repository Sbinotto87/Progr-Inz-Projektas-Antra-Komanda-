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
///     MasterVolumeSlider  (Slider)
///     MusicVolumeSlider   (Slider)
///     SFXVolumeSlider     (Slider)
///   GraphicsPanel/
///     MinFovSlider        (Slider)
///     MaxFovSlider        (Slider)
///     RenderDistanceSlider(Slider)
///     QualityDropdown     (TMP_Dropdown)
///     FullscreenToggle    (Toggle)
///   KeybindsPanel/
///     MouseSensitivitySlider (Slider)
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

    // ── Audio sliders ─────────────────────────────────────────────────────────
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    // ── Graphics controls ─────────────────────────────────────────────────────
    [Header("Graphics Controls")]
    [SerializeField] private Slider minFovSlider;
    [SerializeField] private Slider maxFovSlider;
    [SerializeField] private Slider renderDistanceSlider;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // ── Keybinds / Controls ───────────────────────────────────────────────────
    [Header("Keybinds / Controls")]
    [SerializeField] private Slider mouseSensitivitySlider;

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
            return;

        Instance = this;
    }

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PopulateQualityDropdown();
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
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetSFXVolume(value);
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
    }

    public void OnMaxFovChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m == null) return;

        float min = minFovSlider != null ? minFovSlider.value : m.MinFov;
        m.SetFovRange(min, value);

        if (minFovSlider != null && minFovSlider.value > m.MaxFov)
            minFovSlider.SetValueWithoutNotify(m.MaxFov);
    }

    public void OnRenderDistanceChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetRenderDistance((int)value);
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

    // ── Controls callbacks ────────────────────────────────────────────────────

    public void OnMouseSensitivityChanged(float value)
    {
        SettingsManager m = EnsureManager();
        if (m != null) m.SetMouseSensitivity(value);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ShowTab(GameObject activePanel)
    {
        if (audioPanel != null)    audioPanel.SetActive(audioPanel == activePanel);
        if (graphicsPanel != null) graphicsPanel.SetActive(graphicsPanel == activePanel);
        if (keybindsPanel != null) keybindsPanel.SetActive(keybindsPanel == activePanel);
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

        if (mouseSensitivitySlider != null) mouseSensitivitySlider.SetValueWithoutNotify(m.MouseSensitivity);
    }

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

        // Controls
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

        listenersRegistered = false;
    }
}
