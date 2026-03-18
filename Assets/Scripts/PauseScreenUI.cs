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
        Debug.Log("settings button clicked. ");
        //Application.Quit();

        // iki kol dar neturim settings pasirinkimu
    }

    //main menu
    public void Quit()
    {
        Debug.Log("back to main menu button clicked. ");
        SceneManager.LoadScene(0);
    }
}
