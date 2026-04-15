using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string MinFovKey = "MinFov";
    private const string MaxFovKey = "MaxFov";
    private const string MasterVolumeKey = "MasterVolume";

    [Header("Default Settings")]
    [SerializeField] private float defaultMouseSensitivity = 0.45f;
    [SerializeField] private float defaultMinFov = 70f;
    [SerializeField] private float defaultMaxFov = 80f;
    [SerializeField] private float defaultMasterVolume = 1f;

    public float MouseSensitivity { get; private set; }
    public float MinFov { get; private set; }
    public float MaxFov { get; private set; }
    public float MasterVolume { get; private set; }

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

    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, 0.05f, 5f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivity);
        PlayerPrefs.Save();
        ApplyMouseSensitivity();
    }

    public void SetFovRange(float min, float max)
    {
        MinFov = Mathf.Clamp(min, 30f, 170f);
        MaxFov = Mathf.Clamp(max, MinFov, 170f);
        PlayerPrefs.SetFloat(MinFovKey, MinFov);
        PlayerPrefs.SetFloat(MaxFovKey, MaxFov);
        PlayerPrefs.Save();
        ApplyFov();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
        ApplyMasterVolume();
    }

    public void ApplyAllSettings()
    {
        ApplyMouseSensitivity();
        ApplyFov();
        ApplyMasterVolume();
    }

    private void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity);
        MinFov = PlayerPrefs.GetFloat(MinFovKey, defaultMinFov);
        MaxFov = PlayerPrefs.GetFloat(MaxFovKey, defaultMaxFov);
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume);

        MouseSensitivity = Mathf.Clamp(MouseSensitivity, 0.05f, 5f);
        MinFov = Mathf.Clamp(MinFov, 30f, 170f);
        MaxFov = Mathf.Clamp(MaxFov, MinFov, 170f);
        MasterVolume = Mathf.Clamp01(MasterVolume);
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

    private void ApplyMasterVolume()
    {
        AudioListener.volume = MasterVolume;
    }
}
