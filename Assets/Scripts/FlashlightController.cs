using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;

    [Header("Battery")]
    public float maxBattery = 100f;
    public float drainRate = 10f;      // per second when ON
    public float rechargeRate = 5f;    // per second when OFF
    public float restartDelay = 3f;    // cooldown after battery hits 0

    [Header("UI")]
    public Slider batteryBar;

    private float currentBattery;
    private bool isOn = false;
    private bool isBroken = false;
    private bool canUse = true;

    void Start()
    {
        currentBattery = maxBattery;

        if (flashlight != null)
            flashlight.enabled = false;

        UpdateUI();
    }

    void Update()
    {
        HandleInput();
        HandleBattery();
        UpdateUI();
    }

    void HandleInput()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame &&
            canUse &&
            !isBroken)
        {
            isOn = !isOn;

            if (flashlight != null)
                flashlight.enabled = isOn;
        }
    }

    void HandleBattery()
    {
        if (isOn)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                BreakFlashlight();
            }
        }
        else
        {
            if (currentBattery < maxBattery)
            {
                currentBattery += rechargeRate * Time.deltaTime;
            }
        }

        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
    }

    void BreakFlashlight()
    {
        isOn = false;

        if (flashlight != null)
            flashlight.enabled = false;

        if (!isBroken)
        {
            StartCoroutine(RestartCooldown());
        }
    }

    IEnumerator RestartCooldown()
    {
        isBroken = true;
        canUse = false;

        yield return new WaitForSeconds(restartDelay);

        isBroken = false;
        canUse = true;
    }

    void UpdateUI()
    {
        if (batteryBar != null)
        {
            batteryBar.value = currentBattery / maxBattery;

            batteryBar.gameObject.SetActive(isOn || currentBattery < maxBattery);
        }
    }
}