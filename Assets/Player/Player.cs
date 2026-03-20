using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Gets the world data class
    /// </summary>
    World world;
    /// <summary>
    /// Gets the playerInput class for handling player input
    /// </summary>
    PlayerInput playerInput;


    Vector3 velocity;
    private float verticalVelocity = 0.0f;


    //player size variables for collision detection
    public float playerHeight = 1.8f;
    public float playerWidth = 0.8f;

    public float halfHeight = 0.9f;
    public float halfWidth = 0.4f;


    public float walkSpeed = 4.0f;
    public float sprintSpeed = 7.0f;
    public float gravity = -9.8f;
    public float jumpStrength = 5f;
    public float mouseSensitivity = 0.45f;

    private bool grounded = true;


    //mouse look variable for clamping and stuff
    private float xRotation = 0f;
    GameObject Camera;

    void Start()
    {
        Camera = GameObject.Find("Main Camera");
        Camera.transform.parent = transform;
        Camera.transform.localPosition = new Vector3(0, 0.5f, 0);

        playerInput = GetComponent<PlayerInput>();
        world = GameObject.Find("World").GetComponent<World>();

        playerInput.actions["Jump"].performed += ctx => Jump();

    }


    void Update()
    {
        // get mouse input and rotate camera
        CameraControl();

        // move player x and z based on input and collisions
        Movement();
        // set velocity.y
        ApplyGravity();

        // apply velocity.y to player position
        transform.Translate(velocity.y * Time.deltaTime * Vector3.up, Space.World);

        // Resolve ground AFTER movement
        ResolveGround();
        grounded = IsGrounded(transform.position); // final grounded state
        //Debug.Log($"Grounded: {transform.position.y - halfHeight}");
    }



    /// <summary>
    /// gets the player inputs, checks for collisions, checks for sprinting
    /// </summary>
    void Movement()
    {
        Vector2 movement = playerInput.actions["Move"].ReadValue<Vector2>();
        bool sprinting = playerInput.actions["Sprint"].IsPressed();

        float speed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 move = (transform.right * movement.x + transform.forward * movement.y).normalized * speed * Time.deltaTime;

        Vector3 pos = transform.position;

        // ---------- X AXIS ----------
        float newX = pos.x + move.x;

        if (move.x > 0)
        {
            if (!(CheckBlocks(newX + halfWidth, pos.y - halfHeight + 0.01f, pos.z + halfWidth) ||
                  CheckBlocks(newX + halfWidth, pos.y - halfHeight + 0.01f, pos.z - halfWidth) ||
                  CheckBlocks(newX + halfWidth, pos.y + halfHeight, pos.z + halfWidth) ||
                  CheckBlocks(newX + halfWidth, pos.y + halfHeight, pos.z - halfWidth)))
            {
                pos.x = newX;
            }
        }
        else if (move.x < 0)
        {
            if (!(CheckBlocks(newX - halfWidth, pos.y - halfHeight + 0.01f, pos.z + halfWidth) ||
                  CheckBlocks(newX - halfWidth, pos.y - halfHeight + 0.01f, pos.z - halfWidth) ||
                  CheckBlocks(newX - halfWidth, pos.y + halfHeight, pos.z + halfWidth) ||
                  CheckBlocks(newX - halfWidth, pos.y + halfHeight, pos.z - halfWidth)))
            {
                pos.x = newX;
            }
        }

        // ---------- Z AXIS ----------
        float newZ = pos.z + move.z;

        if (move.z > 0)
        {
            if (!(CheckBlocks(pos.x - halfWidth, pos.y - halfHeight + 0.01f, newZ + halfWidth) ||
                  CheckBlocks(pos.x + halfWidth, pos.y - halfHeight + 0.01f, newZ + halfWidth) ||
                  CheckBlocks(pos.x - halfWidth, pos.y + halfHeight, newZ + halfWidth) ||
                  CheckBlocks(pos.x + halfWidth, pos.y + halfHeight, newZ + halfWidth)))
            {
                pos.z = newZ;
            }
        }
        else if (move.z < 0)
        {
            if (!(CheckBlocks(pos.x - halfWidth, pos.y - halfHeight + 0.01f, newZ - halfWidth) ||
                  CheckBlocks(pos.x + halfWidth, pos.y - halfHeight + 0.01f, newZ - halfWidth) ||
                  CheckBlocks(pos.x - halfWidth, pos.y + halfHeight, newZ - halfWidth) ||
                  CheckBlocks(pos.x + halfWidth, pos.y + halfHeight, newZ - halfWidth)))
            {
                pos.z = newZ;
            }
        }

        transform.position = pos;
    }


    /// <summary>
    /// gets the mouse data and changes camera rotation
    /// </summary>
    private void CameraControl()
    {
        Vector2 mouse = playerInput.actions["Look"].ReadValue<Vector2>();
        float mouseX = mouse.x * mouseSensitivity;
        float mouseY = mouse.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);
        Camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    /// <summary>
    /// does what the name suggests ♥
    /// </summary>
    /// <remarks>if player not on ground, apply gravity to velocity.y</remarks>
    void ApplyGravity()
    {
        if (!grounded)
            verticalVelocity += gravity * Time.deltaTime;

        velocity.y = verticalVelocity;
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

        return chunk.blocks[localX, blockY, localZ] != -1;

    }

    /// <summary>
    /// checks if the block below the player is solid, used for checking if the player is grounded and can jump
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public bool IsGrounded(Vector3 playerPos)
    {
        return CheckBlocks(playerPos.x + halfWidth, playerPos.y - halfHeight - 0.01f, playerPos.z + halfWidth) ||
                CheckBlocks(playerPos.x + halfWidth, playerPos.y - halfHeight - 0.01f, playerPos.z - halfWidth) ||
                CheckBlocks(playerPos.x - halfWidth, playerPos.y - halfHeight - 0.01f, playerPos.z + halfWidth) ||
                CheckBlocks(playerPos.x - halfWidth, playerPos.y - halfHeight - 0.01f, playerPos.z - halfWidth);
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
            verticalVelocity = 0;

            float feetY = transform.position.y - halfHeight - 0.01f;
            float floorY = Mathf.Floor(feetY) + 1f;
            float targetY = floorY + halfHeight;

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
        }
    }
}
