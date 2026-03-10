using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Start button
    public void StartGame()
    {
        WorldGenerator.GenerateWorld();

        SaveWorld.Save();

        SceneManager.LoadScene("GameScene");
    }

    // Quit button
    public void QuitGame()
    {
        Debug.Log("Quit button clicked. ");
        Application.Quit();
    }

    public void OpenSettings()
    {
        Debug.Log("Settings button clicked. ");
        // iki kol dar neturim settings pasirinkimu
    }

}

