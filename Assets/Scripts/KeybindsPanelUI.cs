using UnityEngine;
using UnityEngine.UI;

public class KeybindsPanelUI : MonoBehaviour
{
    public static KeybindsPanelUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject keybindsPanel;

    [Header("Optional Toggle")]
    [SerializeField] private bool pauseTimeWhenOpen;

    [Header("Optional Parent Menus")]
    [SerializeField] private GameObject menuToHideWhenOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (keybindsPanel != null)
        {
            keybindsPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open()
    {
        if (keybindsPanel == null)
        {
            Debug.LogWarning("Settings panel reference is missing.");
            return;
        }

        if (menuToHideWhenOpen != null)
        {
            menuToHideWhenOpen.SetActive(false);
        }

        keybindsPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        if (keybindsPanel == null)
            return;

        keybindsPanel.SetActive(false);

        if (menuToHideWhenOpen != null)
        {
            menuToHideWhenOpen.SetActive(true);
        }
    }
}
