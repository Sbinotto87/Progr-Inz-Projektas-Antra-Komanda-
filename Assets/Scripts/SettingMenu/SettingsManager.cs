using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // --- PlayerPrefs keys ---
    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string MinFovKey = "MinFov";
    private const string MaxFovKey = "MaxFov";
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string RenderDistanceKey = "RenderDistance";
    private const string QualityLevelKey = "QualityLevel";
    private const string IsFullscreenKey = "IsFullscreen";

    [Header("Default Settings – Audio")]
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float defaultSFXVolume = 1f;

    [Header("Default Settings – Graphics")]
    [SerializeField] private float defaultMinFov = 70f;
    [SerializeField] private float defaultMaxFov = 80f;
    [SerializeField] private float defaultRenderDistance = 10f;
    [SerializeField] private int defaultQualityLevel = 2;
    [SerializeField] private bool defaultIsFullscreen = true;

    [Header("Default Settings – Controls")]
    [SerializeField] private float defaultMouseSensitivity = 0.45f;

    // Audio
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }

    // Graphics
    public float MinFov { get; private set; }
    public float MaxFov { get; private set; }
    public int RenderDistance { get; private set; }
    public int QualityLevel { get; private set; }
    public bool IsFullscreen { get; private set; }

    // Controls
    public float MouseSensitivity { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAllSettings();
    }

    // ---- Audio ----

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
        ApplyMasterVolume();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.Save();
        ApplySFXVolume();
    }

    // ---- Graphics ----

    public void SetFovRange(float min, float max)
    {
        MinFov = Mathf.Clamp(min, 30f, 170f);
        MaxFov = Mathf.Clamp(max, MinFov, 170f);
        PlayerPrefs.SetFloat(MinFovKey, MinFov);
        PlayerPrefs.SetFloat(MaxFovKey, MaxFov);
        PlayerPrefs.Save();
        ApplyFov();
    }

    public void SetRenderDistance(int value)
    {
        RenderDistance = Mathf.Clamp(value, 1, 100);
        PlayerPrefs.SetInt(RenderDistanceKey, RenderDistance);
        PlayerPrefs.Save();
        ApplyRenderDistance();
    }

    public void SetQualityLevel(int level)
    {
        QualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        PlayerPrefs.SetInt(QualityLevelKey, QualityLevel);
        PlayerPrefs.Save();
        ApplyQualityLevel();
    }

    public void SetFullscreen(bool fullscreen)
    {
        IsFullscreen = fullscreen;
        PlayerPrefs.SetInt(IsFullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        ApplyFullscreen();
    }

    // ---- Controls ----

    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, 0.05f, 5f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivity);
        PlayerPrefs.Save();
        ApplyMouseSensitivity();
    }

    // ---- Apply all ----

    public void ApplyAllSettings()
    {
        ApplyMasterVolume();
        ApplyMusicVolume();
        ApplySFXVolume();
        ApplyFov();
        ApplyRenderDistance();
        ApplyQualityLevel();
        ApplyFullscreen();
        ApplyMouseSensitivity();
    }

    // ---- Load ----

    private void LoadSettings()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume));
        SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFXVolumeKey, defaultSFXVolume));

        MinFov = Mathf.Clamp(PlayerPrefs.GetFloat(MinFovKey, defaultMinFov), 30f, 170f);
        MaxFov = Mathf.Clamp(PlayerPrefs.GetFloat(MaxFovKey, defaultMaxFov), MinFov, 170f);
        RenderDistance = Mathf.Clamp(PlayerPrefs.GetInt(RenderDistanceKey, (int)defaultRenderDistance), 1, 100);
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(QualityLevelKey, defaultQualityLevel), 0, QualitySettings.names.Length - 1);
        IsFullscreen = PlayerPrefs.GetInt(IsFullscreenKey, defaultIsFullscreen ? 1 : 0) == 1;

        MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity), 0.05f, 5f);
    }

    // ---- Private apply helpers ----

    private void ApplyMasterVolume()
    {
        AudioListener.volume = MasterVolume;
    }

    private void ApplyMusicVolume()
    {
        BackgroundMusic bg = FindFirstObjectByType<BackgroundMusic>();
        if (bg != null)
        {
            AudioSource src = bg.GetComponent<AudioSource>();
            if (src != null)
                src.volume = MusicVolume;
        }
    }

    private void ApplySFXVolume()
    {
        // SFX AudioSources read SFXVolume via SettingsManager.Instance.SFXVolume at play time.
        // No global Unity API exists for SFX-only volume, so individual sources handle it.
    }

    private void ApplyMouseSensitivity()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.SetMouseSensitivity(MouseSensitivity);
        }
    }

    private void ApplyFov()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.SetFovRange(MinFov, MaxFov);
        }
    }

    private void ApplyRenderDistance()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.SetRenderDistance(RenderDistance);
        }
    }

    private void ApplyQualityLevel()
    {
        QualitySettings.SetQualityLevel(QualityLevel, true);
    }

    private void ApplyFullscreen()
    {
        Screen.fullScreen = IsFullscreen;
    }
}
