using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Start button
    public void StartGame()
    {

        SaveWorld.Save();

        SceneManager.LoadScene(1);
    }

    // Quit button
    public void QuitGame()
    {
        Debug.Log("Quit button clicked. ");
        Application.Quit();
    }

    public void OpenSettings()
    {
        SettingsPanelUI panel = SettingsPanelUI.Instance;
        if (panel == null)
        {
            panel = FindFirstObjectByType<SettingsPanelUI>(FindObjectsInactive.Include);
        }

        if (panel != null)
        {
            panel.Open();
            return;
        }

        Debug.LogError("SettingsPanelUI was not found. Ensure a SettingsPanelUI component exists in scene.");
    }

}

