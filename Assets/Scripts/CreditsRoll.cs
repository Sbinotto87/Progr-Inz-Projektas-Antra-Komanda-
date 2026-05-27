using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Added to fix the Input System error!

public class CreditsRoll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 45f;      // How fast the text moves up
    [SerializeField] private float exitYPosition = 1200f;  // The Y coordinate where the script stops

    [Header("Destination")]
    [SerializeField] private string mainMenuSceneName = "Menu";

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Start the credits completely below the screen view
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = -800f;
        rectTransform.anchoredPosition = pos;

        // Hide cursor while watching credits roll calmly
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Move the UI element straight up over time
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        // NEW INPUT SYSTEM CHECK: Safely checks the keyboard for Escape or Space keys
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ExitCredits();
            }
        }

        // Check if the bottom of the text has cleared the screen completely
        if (rectTransform.anchoredPosition.y >= exitYPosition)
        {
            ExitCredits();
        }
    }

    private void ExitCredits()
    {
        // Unfreeze time if it was left paused from gameplay
        Time.timeScale = 1f;

        // Free the mouse cursor so players can interact with the Main Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load your main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}