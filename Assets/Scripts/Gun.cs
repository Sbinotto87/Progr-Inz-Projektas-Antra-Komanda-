using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    private ToolBarUI toolBar;
    private Player player;
    private PlayerInput playerInput;
    public InputAction shootAction;
    public Camera cam;


    public Item gunItem;
    private CreatedItems createdItems;

    [Header("Gun")]
    private float damage = 35f;
    private float range = 50f;
    private float fireRate = 0.8f;

    [Header("Ammo")]
    public Item ammoItem;
    public Inventory inventory;

    [Header("Visual")]
    public LineRenderer tracer;

    float nextFireTime;

    private float ammoRegenTime = 5f;

    float nextAmmoTime;

    private void OnEnable()
    {
        shootAction.Enable();
    }

    private void OnDisable()
    {
        shootAction.Disable();
    }
    private void Start()
    {
        toolBar = Object.FindFirstObjectByType<ToolBarUI>();

        createdItems = GameObject.Find("CreatedItems").GetComponent<CreatedItems>();
        if (gunItem == null)
        {
            //Debug.LogError("Gun item is not assigned.");
            //enabled = false;
            gunItem = createdItems.items[18];
            return;
        }
        playerInput = GetComponent<PlayerInput>();
        player = GetComponent<Player>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing on Player.");
            enabled = false;
            return;
        }

        shootAction = playerInput.actions["Shoot"];
        cam = Camera.main;
    }
    private void Update()
    {
        if (Time.time >= nextAmmoTime)
        {
            inventory.AddItem(ammoItem);

            nextAmmoTime = Time.time + ammoRegenTime;
        }
        
        if (shootAction.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();

        }


    }

    void Shoot()
    {
        // Use selected toolBar slot item
        Item heldItem = toolBar.GetSelectedItem();
        if (heldItem == null) return; // empty slot
        if (heldItem.category != ItemCategory.Gun) return; // if tool or item selected does nothing here


        if (!HasAmmo())
        {
            //Debug.Log("No ammo");
            return;
        }

        // Remove 1 ammo
        inventory.RemoveItem(ammoItem);


        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        Vector3 endPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            endPoint = hit.point;

            //Debug.Log("Hit: " + hit.collider.name);

            EnemyController enemy = hit.collider.GetComponent<EnemyController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

  
        Vector3 tracerStart = ray.origin + ray.direction * 0.5f + cam.transform.right * 0.4f + cam.transform.up * -0.1f;
        DrawTracer(tracerStart, endPoint);
    }

    bool HasAmmo()
    {
        foreach (InventorySlot slot in inventory.slots)
        {
            if (slot.itemData.category == ItemCategory.Ammo && slot.count > 0)
                return true;
        }

        return false;
    }

    void DrawTracer(Vector3 start, Vector3 end)
    {
        StopAllCoroutines();
        StartCoroutine(TracerRoutine(start, end));
    }

    System.Collections.IEnumerator TracerRoutine(Vector3 start, Vector3 end)
    {
        tracer.enabled = true;

        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);

        yield return new WaitForSeconds(0.03f);

        tracer.enabled = false;
    }
}