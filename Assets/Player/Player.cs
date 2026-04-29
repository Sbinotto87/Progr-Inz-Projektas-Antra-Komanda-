using Assets.Scripts;
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
    public float mouseSensitivity = 0.45f;
    [SerializeField] private float minFov = 70f;
    [SerializeField] private float maxFov = 80f;
    [SerializeField] private float fovLerpSpeed = 8f;
    [SerializeField] private float speedForMaxFov = 7f;
    [SerializeField] private float sneakEdgeTolerance = 0.22f;

    public float health = 100f;
    public float hunger = 100f;
    public float thirst = 100f;


    private bool grounded = true;

    public float swimSpeed = 2.0f;
    public float waterBuoyancy = -2f; // Slower sinking than gravity
    public float swimUpStrength = 5f;
    public float waterDrag = 0.9f; // To smooth out movement
    private bool inWater = false;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sneakAction;

    private float xRotation = 0f;

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

        SpawnPosition();
    }


    void Update()
    {
        inWater = CheckWater(transform.position.x, transform.position.y - HalfHeight + SkinWidth, transform.position.z);

        Vector3 positionBeforeMove = transform.position;
        bool sprintingInput = sprintAction.IsPressed();
        CameraControl();
        Movement();
        UpdateDynamicFov(positionBeforeMove, transform.position, sprintingInput);
        ApplyGravity();
        transform.Translate(verticalVelocity * Time.deltaTime * Vector3.up, Space.World);

        if (inWater)
        {
            if (jumpAction.IsPressed()) // Swim Up
            {
                verticalVelocity = grounded ? jumpStrength * 0.7f : swimUpStrength;
            }
            else if (sneakAction.IsPressed()) // Swim Down
            {
                verticalVelocity = -swimUpStrength;
            }
        }
        else if (jumpAction.IsPressed() && grounded && verticalVelocity <= 0f)
        {
            Jump();
        }

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
        bool swimming = IsSwimming();
        bool sprinting = sprintAction.IsPressed();

        float speed;
        if (inWater)
            speed = swimSpeed; 
        else
            speed = sneaking ? sneakSpeed : (sprinting ? sprintSpeed : walkSpeed);

        Vector3 move = (transform.right * movement.x + transform.forward * movement.y).normalized * speed * Time.deltaTime;

        Vector3 pos = transform.position;

        // ---------- X AXIS ----------
        float newX = pos.x + move.x;

        if (move.x > 0)
        {
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
        if (inWater)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, waterBuoyancy, Time.deltaTime * 2f);
        }
        else if (!grounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 pos = transform.position;
        float newY = pos.y + verticalVelocity * Time.deltaTime;

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
            return chunk.MyBlocks.block[blockID].isSwimable;
            // Alternatively, if water is a specific ID (e.g., 5): return blockID == 5;
        }
        return false;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.05f, 5f);
    }

    public void SetRenderDistance(int distance)
    {
        world.viewDistance = Mathf.Clamp(distance, 1, 100);
    }
}
