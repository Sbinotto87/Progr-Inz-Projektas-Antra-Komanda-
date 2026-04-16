using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelUI : MonoBehaviour
{
    public static SettingsPanelUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Sliders")]
    [SerializeField] private Slider minFovSlider;
    [SerializeField] private Slider maxFovSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider renderDistanceSlider;

    [Header("Optional Toggle")]
    [SerializeField] private bool pauseTimeWhenOpen;

    [Header("Optional Parent Menus")]
    [SerializeField] private GameObject menuToHideWhenOpen;
    private bool listenersRegistered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        RegisterSliderListeners();
        SyncSlidersFromSettings();
    }

    private void OnDestroy()
    {
        UnregisterSliderListeners();
    }

    public void Open()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings panel reference is missing.");
            return;
        }

        if (menuToHideWhenOpen != null)
        {
            menuToHideWhenOpen.SetActive(false);
        }

        settingsPanel.SetActive(true);
        SyncSlidersFromSettings();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);

        if (menuToHideWhenOpen != null)
        {
            menuToHideWhenOpen.SetActive(true);
        }
    }

    public void OnMinFovChanged(float value)
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;

        float max = maxFovSlider != null ? maxFovSlider.value : manager.MaxFov;
        manager.SetFovRange(value, max);

        if (maxFovSlider != null && maxFovSlider.value < manager.MinFov)
        {
            maxFovSlider.SetValueWithoutNotify(manager.MinFov);
        }
    }

    public void OnMouseSensitivityChanged(float value)
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;
        manager.SetMouseSensitivity(value);
    }

    public void OnMaxFovChanged(float value)
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;

        float min = minFovSlider != null ? minFovSlider.value : manager.MinFov;
        manager.SetFovRange(min, value);

        if (minFovSlider != null && minFovSlider.value > manager.MaxFov)
        {
            minFovSlider.SetValueWithoutNotify(manager.MaxFov);
        }
    }

    public void OnRenderDistanceChanged(float value)
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;
        manager.SetRenderDistance((int) value);
    }

    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;
        manager.SetMasterVolume(value);
    }

    private void SyncSlidersFromSettings()
    {
        SettingsManager manager = EnsureManager();
        if (manager == null) return;

        if (minFovSlider != null)
            minFovSlider.SetValueWithoutNotify(manager.MinFov);
        if (maxFovSlider != null)
            maxFovSlider.SetValueWithoutNotify(manager.MaxFov);
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.SetValueWithoutNotify(manager.MouseSensitivity);
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(manager.MasterVolume);
        if (renderDistanceSlider != null)
            renderDistanceSlider.SetValueWithoutNotify(manager.RenderDistance);
    }

    private SettingsManager EnsureManager()
    {
        if (SettingsManager.Instance != null)
        {
            return SettingsManager.Instance;
        }

        SettingsManager manager = FindFirstObjectByType<SettingsManager>();
        if (manager != null)
        {
            return manager;
        }

        GameObject managerObject = new GameObject("SettingsManager");
        return managerObject.AddComponent<SettingsManager>();
    }

    private void RegisterSliderListeners()
    {
        if (listenersRegistered)
            return;

        if (minFovSlider != null)
            minFovSlider.onValueChanged.AddListener(OnMinFovChanged);
        if (maxFovSlider != null)
            maxFovSlider.onValueChanged.AddListener(OnMaxFovChanged);
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (renderDistanceSlider != null)
            renderDistanceSlider.onValueChanged.AddListener(OnRenderDistanceChanged);

        listenersRegistered = true;
    }

    private void UnregisterSliderListeners()
    {
        if (!listenersRegistered)
            return;

        if (minFovSlider != null)
            minFovSlider.onValueChanged.RemoveListener(OnMinFovChanged);
        if (maxFovSlider != null)
            maxFovSlider.onValueChanged.RemoveListener(OnMaxFovChanged);
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (renderDistanceSlider != null)
            renderDistanceSlider.onValueChanged.RemoveListener(OnRenderDistanceChanged);

        listenersRegistered = false;
    }
}
