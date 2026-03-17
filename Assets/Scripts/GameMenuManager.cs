using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerInput))]
public class GameMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    GameObject pauseMenuUI;

    PlayerInput SettingsInput;

    Player playerController;

    Volume volume;
    DepthOfField dof;
    ColorAdjustments colorAdjustments;

    GameObject UIelements;

    void Start()
    {
        SettingsInput = GetComponent<PlayerInput>();

        SettingsInput.actions["Pause"].started += ctx => Pause();

        volume = GameObject.Find("Volume (effects after render)").GetComponent<Volume>();
        Debug.Log(volume.profile.TryGet(out dof)); 
        volume.profile.TryGet(out colorAdjustments);

        //colorAdjustments.postExposure.value = -0.5f;
        //colorAdjustments.contrast.value = 0f;
        //colorAdjustments.saturation.value = -60f;

        //dof.focusDistance.value = 0.1f;
        //dof.aperture.value = 32f;
        //dof.focalLength.value = 300f;

        UIelements = GameObject.FindWithTag("UI");
    }

    /// <summary>
    /// ensure timeScale = 1
    /// </summary>
    private void Awake()
    {
        Time.timeScale = 1;
    }




    public void Pause()
    {

        playerController = GameObject.Find("Player").GetComponent<Player>();
        

        if (Time.timeScale == 1) // If the game is currently running, pause it
        {
            playerController.GetComponent<PlayerInput>().enabled = false; // Disable player input to prevent movement while paused
            playerController.enabled = false; // Disable player controls

            UIelements.SetActive(false);
            dof.active = true;
            colorAdjustments.active = true;
            pauseMenuUI.SetActive(true);


            Time.timeScale = 0;
            Debug.Log("Game paused.");
        }
        else // If the game is currently paused, unpause it
        {
            playerController.GetComponent<PlayerInput>().enabled = true; // Disable player input to prevent movement while paused
            playerController.enabled = true; // enable player controls

            UIelements.SetActive(true);
            dof.active = false;
            colorAdjustments.active = false;

            pauseMenuUI.SetActive(false);
            Time.timeScale = 1;
            Debug.Log("Game unpaused.");
        }
    }
}
