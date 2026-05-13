using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this to the Player GameObject (alongside your normal movement script).
/// While CheatsManager.Flight is on, this script takes over and lets you fly freely.
///
/// Controls:
///   WASD          - horizontal flight (camera-relative)
///   Space         - up
///   Left Ctrl     - down
///   Left Shift    - speed boost
///
/// </summary>
public class FlightController : MonoBehaviour
{
    [Header("Optional: script to disable while flying (e.g. your ground movement)")]
    [SerializeField] private MonoBehaviour disableScriptWhileFlying;

    [Header("Tuning")]
    [SerializeField] private float verticalSpeed = 10f;

    private Rigidbody rb;
    private CharacterController cc;
    private Camera cam;

    private bool wasFlying;
    private bool prevUseGravity;
    private RigidbodyConstraints prevConstraints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        var cheats = CheatsManager.Instance;
        bool flying = cheats != null && cheats.CheatsEnabled && cheats.Flight;

        if (flying && !wasFlying)  EnterFlight();
        if (!flying && wasFlying)  ExitFlight();
        wasFlying = flying;

        if (!flying) return;

        Vector3 input = ReadMove();
        float speed = cheats.FlightSpeed *
            (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed ? cheats.FlightFastMultiplier : 1f);

        Vector3 fwd = cam != null ? cam.transform.forward : transform.forward;
        Vector3 right = cam != null ? cam.transform.right : transform.right;

        Vector3 velocity = (fwd * input.z + right * input.x + Vector3.up * input.y) * speed;

        if (rb != null && !cheats.NoClip)
        {
            // Use velocity for Rigidbody so collisions still register
            rb.linearVelocity = velocity;
        }
        else if (cc != null && !cheats.NoClip)
        {
            cc.Move(velocity * Time.deltaTime);
        }
        else
        {
            // No-clip / fallback: move transform directly
            transform.position += velocity * Time.deltaTime;
        }
    }

    private void EnterFlight()
    {
        if (disableScriptWhileFlying != null) disableScriptWhileFlying.enabled = false;

        if (rb != null)
        {
            prevUseGravity = rb.useGravity;
            prevConstraints = rb.constraints;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void ExitFlight()
    {
        if (disableScriptWhileFlying != null) disableScriptWhileFlying.enabled = true;

        if (rb != null)
        {
            rb.useGravity = prevUseGravity;
            rb.constraints = prevConstraints;
            rb.linearVelocity = Vector3.zero;
        }

        // Also zero out the Player's vertical velocity so re-entry to normal movement
        // doesn't carry over huge negative Y velocity (= instant death from fall damage)
        var player = GetComponent("Player");
        if (player != null)
        {
            var field = player.GetType().GetField("verticalVelocity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(player, 0f);
        }
    }

    private Vector3 ReadMove()
    {
        var k = Keyboard.current;
        if (k == null) return Vector3.zero;

        float x = (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0);
        float z = (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0);
        float y = (k.spaceKey.isPressed ? 1 : 0) - (k.leftCtrlKey.isPressed ? 1 : 0);
        return new Vector3(x, y, z);
    }
}
