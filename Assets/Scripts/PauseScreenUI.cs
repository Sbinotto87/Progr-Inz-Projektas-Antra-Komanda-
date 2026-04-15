using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreenUI : MonoBehaviour
{
    [SerializeField]
     GameObject gameMenuManager;
    GameMenuManager gameMenuManagerScript;

    private void Awake()
    {
        gameMenuManagerScript = gameMenuManager.GetComponent<GameMenuManager>();

    }
    public void ContinueGame()
    {
        gameMenuManagerScript.Pause();
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
