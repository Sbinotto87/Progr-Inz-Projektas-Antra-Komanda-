using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerInput))]
public class GameMenuManager : MonoBehaviour
{
    /// <summary>
    /// Sita klase editina shaderius taip pat stabdo laika ant mirties ir pauzes
    /// </summary>

    [SerializeField]
    GameObject pauseMenuUI;

    [SerializeField]
    GameObject deathMenuUI;

    [SerializeField]
    GameObject volumeObject;

    [SerializeField]
    GameObject UIelements;
    
    PlayerInput SettingsInput;

    Player playerController;

    Volume volume;
    DepthOfField dof;
    ColorAdjustments colorAdjustments;


    Player player;
    World world;

    InputAction pauseAction;
    void Start()
    {
        volume = volumeObject.GetComponent<Volume>();
        Debug.Log(volume.profile.TryGet(out dof)); 
        volume.profile.TryGet(out colorAdjustments);

        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        if (world == null)
        {
            GameObject worldObject = GameObject.Find("World");
            if (worldObject != null)
            {
                world = worldObject.GetComponent<World>();
            }
        }

        colorAdjustments.postExposure.value = -0.5f;
        colorAdjustments.contrast.value = 30f;
        colorAdjustments.saturation.value = -30f;

        dof.mode.value = DepthOfFieldMode.Gaussian;
        dof.gaussianStart.value = 2f;
        dof.gaussianEnd.value = 30f;
        dof.gaussianMaxRadius.value = 1.5f;

    }
    public void OnPause()
    {
        Pause();
    }
    //private void OnDisable()
    //{
    //    pauseAction.started -= OnPause;
    //}
    private void OnEnable()
    {
        SettingsInput = GetComponent<PlayerInput>();

        pauseAction = SettingsInput.actions["Pause"];
    }

    /// <summary>
    /// ensure timeScale = 1
    /// </summary>
    private void Awake()
    {
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        if(world.IsInRadiation)
            EnableRadiation();
        else if(!world.IsInRadiation)
            DisableRadiation();
        if (player.health <= 0)
        {
            Death();
        }
        
    }


    public void Pause()
    {


        playerController = GameObject.FindWithTag("Player").GetComponent<Player>();
        if (Time.timeScale == 1) // If the game is currently running, pause it
        {

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

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
            playerController.GetComponent<PlayerInput>().enabled = true; // enable player input to prevent movement while paused
            playerController.enabled = true; // enable player controls

            UIelements.SetActive(true);
            dof.active = false;
            colorAdjustments.active = false;

            pauseMenuUI.SetActive(false);
            Time.timeScale = 1;
            Debug.Log("Game unpaused.");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void Death()
    {

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        playerController = GameObject.FindWithTag("Player").GetComponent<Player>();
        playerController.GetComponent<PlayerInput>().enabled = false; // Disable player input to prevent movement while paused
        playerController.enabled = false; // Disable player controls
        Time.timeScale = 0;

        UIelements.SetActive(false);
        dof.active = true;
        colorAdjustments.colorFilter.value = new Color(0.5f, 0f, 0f, 1f);
        colorAdjustments.active = true;
        deathMenuUI.SetActive(true);
            

        Debug.Log("Player died.");
    }
    public void EnableRadiation()
    {
        colorAdjustments.colorFilter.value = new Color(0.77f, 0.92f, 0.74f, 0.25f);
        colorAdjustments.active = true;
        player.isInRadiation = true;
    }
    public void DisableRadiation()
    {
        colorAdjustments.colorFilter.value = new Color(0f, 0f, 0f, 0f);
        colorAdjustments.active = false;
        player.isInRadiation = false;
    }
}
