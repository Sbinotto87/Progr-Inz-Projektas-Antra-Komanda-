using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField]
    GameObject gameMenuManager;
    GameMenuManager gameMenuManagerScript;

    private void Awake()
    {
        gameMenuManagerScript = gameMenuManager.GetComponent<GameMenuManager>();

    }
    public void Restart()
    {
        //reload
        SceneManager.LoadScene(1);
    }

    // settings 
    public void Settings()
    {
        if (SettingsPanelUI.Instance != null)
        {
            SettingsPanelUI.Instance.Open();
        }
        else
        {
            Debug.LogWarning("SettingsPanelUI instance not found in scene.");
        }
    }

    //main menu
    public void Quit()
    {
        Debug.Log("back to main menu button clicked. ");
        SceneManager.LoadScene(0);
    }
}
