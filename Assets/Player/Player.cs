using Assets.Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Gets the world data class
    /// </summary>
    [SerializeField] private World world;
    /// <summary>
    /// Gets the playerInput class for handling player input
    /// </summary>
    private PlayerInput playerInput;
    [SerializeField] private Camera playerCamera;

    private const float SkinWidth = 0.01f;
    private const float HeadCheckEpsilon = 0.02f;
    private float verticalVelocity;

    //player size variables for collision detection
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private float playerWidth = 0.8f;
    private float HalfHeight => playerHeight * 0.5f;
    private float HalfWidth => playerWidth * 0.5f;


    public float walkSpeed = 4.0f;
    public float sprintSpeed = 7.0f;
    public float sneakSpeed = 1.6f;
    public float gravity = -23f;
    public float jumpStrength = 7.25f;

    // FLIGHT 
    [Header("Flight (cheat)")]
    public float flightSpeed = 12f;
    public float flightFastMultiplier = 3f;
    public float doubleTapWindow = 0.3f;
    private bool isFlying = false;
    private float lastJumpPressTime = -10f;
    //

    public float mouseSensitivity = 0.45f;
    [SerializeField] private float minFov = 70f;
    [SerializeField] private float maxFov = 80f;
    [SerializeField] private float fovLerpSpeed = 8f;
    [SerializeField] private float speedForMaxFov = 7f;
    [SerializeField] private float sneakEdgeTolerance = 0.22f;

    public float health = 1000f;
    public float hunger = 100f;
    public float thirst = 100f;

    [Header("Fall Damage")]
    [SerializeField] private float minimumFallVelocity = -12f;
    [SerializeField] private float fallDamageMultiplier = 4f;

    private float highestYWhileGrounded;

    [SerializeField] private float invincibilityDuration = 1f;

    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    // --- STAMINA VARIABLES ---
    public float stamina = 100f;
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 10f;
    //public bool isSprinting { get; private set; }


    private bool grounded = true;
    private bool wasOutOfStamina = false;
    float staminaRecoveryThreshold = 20f; // % needed before sprint allowed again

    public float swimSpeed = 2.0f;
    public float waterBuoyancy = -2f; // Slower sinking than gravity
    public float swimUpStrength = 5f;
    public float waterDrag = 0.9f; // To smooth out movement
    private bool inWater = false;
    public bool isSubmerged = false;
    public bool isInRadiation = false;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sneakAction;

    public bool HasOpenedInventory = false;
    public bool HasOpenedChest = false;
    public bool HasEquippedTool = false;
    public GameObject currentOpenedChest;
    public Item currentEquippedTool;

    private float xRotation = 0f;

    public OverlayEffects overlayEffects;
    public Texture2D overlayTexture;

    public int sprintLockCount = 0;
    public int jumpLockCount = 0;
    
    private bool methodCalled = false;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing on Player.");
            enabled = false;
            return;
        }

        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        sneakAction = playerInput.actions.FindAction("Sneak", throwIfNotFound: false);
        if (sneakAction == null)
        {
            sneakAction = playerInput.actions.FindAction("Crouch", throwIfNotFound: false);
        }

        minFov = PlayerPrefs.GetFloat("MinFov", minFov);
        maxFov = PlayerPrefs.GetFloat("MaxFov", maxFov);
        minFov = Mathf.Clamp(minFov, 30f, 170f);
        maxFov = Mathf.Clamp(maxFov, minFov, 170f);

        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera;
            }
        }

        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0f, HalfHeight * 0.5f, 0f);
            playerCamera.fieldOfView = minFov;
        }
        else
        {
            Debug.LogWarning("Player camera is not assigned and no MainCamera was found.");
        }
        
        if (world == null)
        {
            GameObject worldObject = GameObject.Find("World");
            if (worldObject != null)
            {
                world = worldObject.GetComponent<World>();
            }
        }

        if (world == null)
        {
            Debug.LogError("World reference is missing on Player.");
            enabled = false;
            return;
        }
        overlayEffects = GameObject.FindGameObjectWithTag("TextureOverlay").GetComponent<OverlayEffects>();
        overlayTexture = Resources.Load("OilOverlay") as Texture2D;

        highestYWhileGrounded = transform.position.y;

        SpawnPosition();
    }


    void Update()
    {
        // Sync flight state with cheats menu toggle
        var cheatsMgr = CheatsManager.Instance;
        if (cheatsMgr != null && cheatsMgr.CheatsEnabled)
        {
            if (cheatsMgr.Flight && !isFlying) { isFlying = true; verticalVelocity = 0f; grounded = false; }
            if (!cheatsMgr.Flight && isFlying) { isFlying = false; verticalVelocity = 0f; }
        }

        // Push current flight state back so the menu reflects double-tap toggles too
        if (cheatsMgr != null) cheatsMgr.Flight = isFlying;

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        inWater = CheckWater(transform.position.x, transform.position.y - HalfHeight + SkinWidth, transform.position.z);
        isSubmerged = false;
        if (inWater)
            isSubmerged = CheckWater(transform.position.x, transform.position.y + HalfHeight - SkinWidth, transform.position.z);

        Vector3 positionBeforeMove = transform.position;
        //bool sprintingInput = sprintAction.IsPressed();
        CameraControl();
        Movement();

        // Pass the stamina check directly into the FOV logic
        bool isMoving = moveAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;
        UpdateDynamicFov(positionBeforeMove, transform.position, (sprintAction.IsPressed() && stamina > 0 && isMoving && !wasOutOfStamina));


        //UpdateDynamicFov(positionBeforeMove, transform.position, sprintingInput);
        ApplyGravity();
        transform.Translate(verticalVelocity * Time.deltaTime * Vector3.up, Space.World);

        if (isSubmerged) overlayEffects.ShowOverlay(overlayTexture, 0.5f);

        else overlayEffects.HideOverlay();

        if (!isFlying && inWater)
        {
            float buoyancyMultiplier = swimSpeed / walkSpeed;

            if (jumpAction.IsPressed()) // Swim Up
            {
                verticalVelocity = grounded ? jumpStrength * 0.7f : (swimUpStrength * buoyancyMultiplier);
            }
            else if (sneakAction.IsPressed()) // Swim Down
            {
                verticalVelocity = -(swimUpStrength * buoyancyMultiplier);
            }
        }
        //else if (jumpAction.IsPressed() && grounded && verticalVelocity <= 0f && jumpLockCount == 0)
        //{
        //    Jump();
        //}


        // Double-tap space to toggle flight (only when cheats master is on) ============================== FLIGHT
        if (jumpAction.WasPressedThisFrame())
        {
            var c = CheatsManager.Instance;
            bool cheatsOn = c != null && c.CheatsEnabled;
            if (cheatsOn && Time.time - lastJumpPressTime < doubleTapWindow)
            {
                isFlying = !isFlying;
                verticalVelocity = 0f; // critical: prevents fall damage when re-enabling gravity
                if (isFlying) grounded = false;
            }
            lastJumpPressTime = Time.time;
        }

        // If flight got disabled externally (e.g. cheats turned off in menu), exit cleanly
        if (isFlying)
        {
            var c = CheatsManager.Instance;
            if (c == null || !c.CheatsEnabled) { isFlying = false; verticalVelocity = 0f; }
        }

        if (!isFlying && jumpAction.IsPressed() && grounded && verticalVelocity <= 0f && jumpLockCount == 0)
        {
            Jump();
        }
        // ========================================================================================================

        HandleFallDamage();
        ResolveGround();
        BugRemoval();
    }

    void SpawnPosition()
    {
        Vector3 pos = transform.position;
        pos.x += 0.5f;
        pos.z += 0.5f;

        for (int i = Chunk.Height; i > 0; i--)
        {
            if (CheckBlocks(pos.x, i, pos.z))
            {
                    pos.y = i + HalfHeight + 1.01f;
                    transform.position = pos;
                    break;
            }
        }
    }

    void BugRemoval()
    {
        Vector3 pos = transform.position;
        if (CheckBlocks(pos.x, pos.y+0.01f, pos.z))
        {
            pos.y += 1f;
            transform.position = pos;
        }
    }

    /// <summary>
    /// gets the player inputs, checks for collisions, checks for sprinting
    /// </summary>
    void Movement()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        bool sneaking = IsSneaking();
        bool sprinting = sprintAction.IsPressed();

        // 1. Handle Stamina Logic first
        bool wantsToSprint = sprinting && movement.sqrMagnitude > 0.01f && !sneaking && !inWater;

        if (stamina <= 0.01f) wasOutOfStamina = true;
        if (wasOutOfStamina && stamina >= staminaRecoveryThreshold) wasOutOfStamina = false;

        bool canSprint = wantsToSprint && !wasOutOfStamina && stamina > 0.02f && sprintLockCount == 0 && HasSprintResources();

        if (canSprint)
            stamina = Mathf.Max(0, stamina - staminaDrainRate * Time.deltaTime);
        else
            stamina = Mathf.Min(100, stamina + staminaRegenRate * Time.deltaTime);

        // 2. Final Speed Calculation (Priority Order: Water > Sneak > Sprint > Walk)
        float speed;
        if (isFlying)
        {
            speed = flightSpeed * (sprintAction.IsPressed() ? flightFastMultiplier : 1f);
        }
        else if (inWater)
        {
            speed = swimSpeed; // Uses the slowdown value from CheckWater()
        }
        else if (sneaking)
        {
            speed = sneakSpeed;
        }
        else if (canSprint)
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        Vector3 move = (transform.right * movement.x + transform.forward * movement.y).normalized * speed * Time.deltaTime;

        Vector3 pos = transform.position;

        // ---------- X AXIS ----------
        float newX = pos.x + move.x;

        if (move.x > 0)
        {
            if (!methodCalled)
            {
                methodCalled = true;
                World.addChestItems();
            }
            if (!(CheckBlocks(newX + HalfWidth, pos.y - HalfHeight + SkinWidth, pos.z + HalfWidth) ||
                  CheckBlocks(newX + HalfWidth, pos.y - HalfHeight + SkinWidth, pos.z - HalfWidth) ||
                  CheckBlocks(newX + HalfWidth, pos.y + HalfHeight, pos.z + HalfWidth) ||
                  CheckBlocks(newX + HalfWidth, pos.y + HalfHeight, pos.z - HalfWidth)) &&
                (!sneaking || HasSneakSupport(new Vector3(newX, pos.y, pos.z))))
            {
                pos.x = newX;
            }
        }
        else if (move.x < 0)
        {
            if (!(CheckBlocks(newX - HalfWidth, pos.y - HalfHeight + SkinWidth, pos.z + HalfWidth) ||
                  CheckBlocks(newX - HalfWidth, pos.y - HalfHeight + SkinWidth, pos.z - HalfWidth) ||
                  CheckBlocks(newX - HalfWidth, pos.y + HalfHeight, pos.z + HalfWidth) ||
                  CheckBlocks(newX - HalfWidth, pos.y + HalfHeight, pos.z - HalfWidth)) &&
                (!sneaking || HasSneakSupport(new Vector3(newX, pos.y, pos.z))))
            {
                pos.x = newX;
            }
        }

        // ---------- Z AXIS ----------
        float newZ = pos.z + move.z;

        if (move.z > 0)
        {
            if (!(CheckBlocks(pos.x - HalfWidth, pos.y - HalfHeight + SkinWidth, newZ + HalfWidth) ||
                  CheckBlocks(pos.x + HalfWidth, pos.y - HalfHeight + SkinWidth, newZ + HalfWidth) ||
                  CheckBlocks(pos.x - HalfWidth, pos.y + HalfHeight, newZ + HalfWidth) ||
                  CheckBlocks(pos.x + HalfWidth, pos.y + HalfHeight, newZ + HalfWidth)) &&
                (!sneaking || HasSneakSupport(new Vector3(pos.x, pos.y, newZ))))
            {
                pos.z = newZ;
            }
        }
        else if (move.z < 0)
        {
            if (!(CheckBlocks(pos.x - HalfWidth, pos.y - HalfHeight + SkinWidth, newZ - HalfWidth) ||
                  CheckBlocks(pos.x + HalfWidth, pos.y - HalfHeight + SkinWidth, newZ - HalfWidth) ||
                  CheckBlocks(pos.x - HalfWidth, pos.y + HalfHeight, newZ - HalfWidth) ||
                  CheckBlocks(pos.x + HalfWidth, pos.y + HalfHeight, newZ - HalfWidth)) &&
                (!sneaking || HasSneakSupport(new Vector3(pos.x, pos.y, newZ))))
            {
                pos.z = newZ;
            }
        }

        transform.position = pos;
        bool isRunning = grounded && movement.magnitude > 0.1f;
        GetComponent<PlayerEffects>()?.SetRunDust(isRunning);
    }

    private void UpdateDynamicFov(Vector3 positionBeforeMove, Vector3 positionAfterMove, bool sprintingInput)
    {
        if (playerCamera == null)
            return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector2 horizontalDelta = new Vector2(positionAfterMove.x - positionBeforeMove.x, positionAfterMove.z - positionBeforeMove.z);
        float horizontalSpeed = horizontalDelta.magnitude / deltaTime;
        bool isMovingHorizontally = horizontalSpeed > 0.05f;
        bool shouldUseSprintFov = sprintingInput && isMovingHorizontally && !IsSneaking();
        float speedT = shouldUseSprintFov
            ? Mathf.Clamp01(horizontalSpeed / Mathf.Max(0.01f, speedForMaxFov))
            : 0f;
        float targetFov = Mathf.Lerp(minFov, maxFov, speedT);
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
    }

    private bool HasSneakSupport(Vector3 testPosition)
    {
        float supportWidth = Mathf.Max(0.01f, HalfWidth - sneakEdgeTolerance);
        float feetY = testPosition.y - HalfHeight - SkinWidth;

        return CheckBlocks(testPosition.x + supportWidth, feetY, testPosition.z + supportWidth) ||
               CheckBlocks(testPosition.x + supportWidth, feetY, testPosition.z - supportWidth) ||
               CheckBlocks(testPosition.x - supportWidth, feetY, testPosition.z + supportWidth) ||
               CheckBlocks(testPosition.x - supportWidth, feetY, testPosition.z - supportWidth);
    }

    private bool IsSneaking()
    {
        return sneakAction != null && sneakAction.IsPressed();
    }
    private bool IsSwimming()
    {
        return sneakAction != null && sneakAction.IsPressed();
    }

    public void SetFovRange(float min, float max)
    {
        minFov = Mathf.Clamp(min, 30f, 170f);
        maxFov = Mathf.Clamp(max, minFov, 170f);
        PlayerPrefs.SetFloat("MinFov", minFov);
        PlayerPrefs.SetFloat("MaxFov", maxFov);
        PlayerPrefs.Save();
    }


    /// <summary>
    /// gets the mouse data and changes camera rotation
    /// </summary>
    private void CameraControl()
    {
        if (playerCamera == null)
            return;

        Vector2 mouse = lookAction.ReadValue<Vector2>();
        float mouseX = mouse.x * mouseSensitivity;
        float mouseY = mouse.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    /// <summary>
    /// does what the name suggests ♥
    /// </summary>
    /// <remarks>If player is not grounded, apply gravity to vertical velocity.</remarks>
    void ApplyGravity()
    {
        if (isFlying)
        {
            // No gravity in flight; vertical movement is driven by space/ctrl below.
            float upDown = 0f;
            if (jumpAction.IsPressed()) upDown += 1f;
            if (sneakAction != null && sneakAction.IsPressed()) upDown -= 1f;

            float fSpeed = flightSpeed * (sprintAction.IsPressed() ? flightFastMultiplier : 1f);
            verticalVelocity = upDown * fSpeed;
            return;
        }

        if (inWater)
        {
            // 1. Calculate a multiplier based on how much the liquid slows the player down
            // If walkSpeed is 4 and swimSpeed is 2, the multiplier is 0.5 (sinking is 50% speed)
            float buoyancyMultiplier = swimSpeed / walkSpeed;

            // 2. Scale the target buoyancy by this multiplier
            float scaledBuoyancy = waterBuoyancy * buoyancyMultiplier;

            // 3. Lerp toward the scaled target
            // We also scale the Lerp speed by the multiplier so the transition itself feels 'thicker'
            verticalVelocity = Mathf.Lerp(verticalVelocity, scaledBuoyancy, Time.deltaTime * 2f * buoyancyMultiplier);
        }
        else if (!grounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Apply the movement (same as before)
        Vector3 pos = transform.position;
        float newY = pos.y + verticalVelocity * Time.deltaTime;

        // Head collision check...
        if (verticalVelocity > 0 &&
              (CheckBlocks(pos.x + HalfWidth, newY + HalfHeight + HeadCheckEpsilon, pos.z + HalfWidth) ||
               CheckBlocks(pos.x + HalfWidth, newY + HalfHeight + HeadCheckEpsilon, pos.z - HalfWidth) ||
               CheckBlocks(pos.x - HalfWidth, newY + HalfHeight + HeadCheckEpsilon, pos.z + HalfWidth) ||
               CheckBlocks(pos.x - HalfWidth, newY + HalfHeight + HeadCheckEpsilon, pos.z - HalfWidth)))
        {
            verticalVelocity = 0f;
        }
    }


    /// <summary>
    /// checks if a block is present at the given position, used for collision detection and stuff
    /// </summary>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="posz"></param>
    /// <returns>true if block exists</returns>
    public bool CheckBlocks(float posx, float posy, float posz)
    {
        int blockX = Mathf.FloorToInt(posx);
        int blockY = Mathf.FloorToInt(posy);
        int blockZ = Mathf.FloorToInt(posz);

        if (blockY < 0 || blockY >= Chunk.Height)
            return false;

        int chunkX = Mathf.FloorToInt((float)blockX / Chunk.Width);
        int chunkZ = Mathf.FloorToInt((float)blockZ / Chunk.Width);

        if (chunkX < 0 || chunkX >= World.WorldSize || chunkZ < 0 || chunkZ >= World.WorldSize)
            return false;

        Chunk chunk = world.chunks[chunkX, chunkZ];
        if (chunk == null)
            return false;

        int localX = blockX - chunkX * Chunk.Width;
        int localZ = blockZ - chunkZ * Chunk.Width;

        if (chunk.blocks[localX, blockY, localZ] != -1)
            return chunk.MyBlocks.block[chunk.blocks[localX, blockY, localZ]].isSolid;

        return false;


    }

    /// <summary>
    /// checks if the block below the player is solid, used for checking if the player is grounded and can jump
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public bool IsGrounded(Vector3 playerPos)
    {
        return CheckBlocks(playerPos.x + HalfWidth, playerPos.y - HalfHeight - SkinWidth, playerPos.z + HalfWidth) ||
                CheckBlocks(playerPos.x + HalfWidth, playerPos.y - HalfHeight - SkinWidth, playerPos.z - HalfWidth) ||
                CheckBlocks(playerPos.x - HalfWidth, playerPos.y - HalfHeight - SkinWidth, playerPos.z + HalfWidth) ||
                CheckBlocks(playerPos.x - HalfWidth, playerPos.y - HalfHeight - SkinWidth, playerPos.z - HalfWidth);
    }

    /// <summary>
    /// Determines whether the object is grounded and adjusts its vertical position to align with the ground level if
    /// necessary.
    /// </summary>
    /// <remarks>This method sets the grounded state based on the object's current position. If the object is
    /// grounded and falling, it resets the vertical velocity and ensures the object's position is corrected to the
    /// ground level. Use this method to maintain accurate ground contact and prevent the object from sinking below the
    /// floor.</remarks>
    void ResolveGround()
    {
        if (isFlying)
        {
            grounded = false;
            return;
        }

        if (IsGrounded(transform.position) && verticalVelocity < 0)
        {
            grounded = true;
            verticalVelocity = 0f;

            float feetY = transform.position.y - HalfHeight - SkinWidth;
            float floorY = Mathf.Floor(feetY) + 1f;
            float targetY = floorY + HalfHeight;

            if (transform.position.y < targetY)
            {
                Vector3 pos = transform.position;
                pos.y = targetY;
                transform.position = pos;
            }
        }
        else
        {
            grounded = false;
        }
    }

    /// <summary>
    /// jumps by setting the vertical velocity to the jump strength, only works if the player is grounded to prevent double jumping and stuff
    /// </summary>
    void Jump()
    {
        if (grounded)
        {
            verticalVelocity = jumpStrength; // Adjust jump strength as needed
            grounded = false; // Player is now in the air
            GetComponent<PlayerEffects>()?.PlayJumpDust(); // Dust while jumping
        }
    }
    public bool CheckWater(float posx, float posy, float posz)
    {
        int blockX = Mathf.FloorToInt(posx);
        int blockY = Mathf.FloorToInt(posy);
        int blockZ = Mathf.FloorToInt(posz);

        if (blockY < 0 || blockY >= Chunk.Height) return false;

        int chunkX = Mathf.FloorToInt((float)blockX / Chunk.Width);
        int chunkZ = Mathf.FloorToInt((float)blockZ / Chunk.Width);

        if (chunkX < 0 || chunkX >= World.WorldSize || chunkZ < 0 || chunkZ >= World.WorldSize) return false;

        Chunk chunk = world.chunks[chunkX, chunkZ];
        if (chunk == null) return false;

        int localX = blockX - chunkX * Chunk.Width;
        int localZ = blockZ - chunkZ * Chunk.Width;

        int blockID = chunk.blocks[localX, blockY, localZ];
        if (blockID != -1)
        {
            swimSpeed = chunk.MyBlocks.block[blockID].swimSlowdown;
            int id = chunk.blocks[localX, (int)(blockY + HalfHeight - SkinWidth), localZ];
            if (id == 11 || (id >= 13 && id <= 19)) // effect
            return chunk.MyBlocks.block[blockID].isSwimable;
        }
        return false;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.05f, 5f);
    }

    // To eat/drink ======
    public void AddHunger(float amount)
    {
        hunger = Mathf.Min(hunger + amount, 100f);
        Debug.Log($"Ate food! Hunger is now: {hunger}");
    }

    public void AddThirst(float amount)
    {
        thirst = Mathf.Min(thirst + amount, 100f);
        Debug.Log($"Drank water! Thirst is now: {thirst}");
    }
    
    public void AddHealth(float amount)
    {
        health = Mathf.Min(health + amount, 1000f);
        Debug.Log($"Healed! Health is now: {health}");
    }
    
    public void SetRenderDistance(int distance)
    {
        world.viewDistance = Mathf.Clamp(distance, 1, 100);
    }
    public void TakeDamage(float damage)
    {
        if (isInvincible)
            return;

        health -= damage;

        StartInvincibilityFrames();
    }

    private void StartInvincibilityFrames()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void HandleFallDamage()
    {
        // Skip fall damage when flight cheat is on or while invincibility window is active
        var c = CheatsManager.Instance;
        if (c != null && c.CheatsEnabled && (c.Flight || c.InfiniteHealth))
        {
            highestYWhileGrounded = transform.position.y;
            return;
        }

        if (grounded)
        {
            highestYWhileGrounded = transform.position.y;
            return;
        }

        if (!grounded && IsGrounded(transform.position))
        {
            float impactVelocity = verticalVelocity;
            if (impactVelocity < minimumFallVelocity)
            {
                GetComponent<StatusEffectManager>()?.AddEffect(
                    duration: 10f,
                    onApply: p =>
                    {
                        p.sprintLockCount++;
                        p.jumpLockCount++;
                    },
                    onRemove: p =>
                    {
                        p.sprintLockCount--;
                        p.jumpLockCount--;
                    }
                );
                float damage = Mathf.Abs(impactVelocity - minimumFallVelocity) * fallDamageMultiplier;

                TakeDamage(damage);
            }
        }
    }

    private bool HasSprintResources()
    {
        return hunger > 0f && thirst > 0f;
    }
}
