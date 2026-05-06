using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform listParent;
    public GameObject slotPrefab;
    public TextMeshProUGUI weightText;

    private PlayerInput playerInput;
    private GameObject player;

    private Inventory playerInventory;
    private bool isOpen = false;
    void Start()
    {
        // Start with the menu hidden
        inventoryPanel.SetActive(false);
        GameObject.Find("UI elements").transform.Find("tool").gameObject.SetActive(false);
        FindPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. If we don't have the player yet, try to find them
        if (playerInput == null || playerInventory == null)
        {
            FindPlayer();

            // If we STILL haven't found them (e.g., player hasn't spawned), 
            // stop here and wait for the next frame.
            if (playerInput == null) return;
        }

        // 2. Now that we definitely have the player, check for the E key
        if (playerInput.actions["Inventory"].WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }

    public void OnInventory(InputValue value)
    {
        ToggleInventory();
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
            playerInput = player.GetComponent<PlayerInput>();

            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged += RefreshUI;
            }
        }
    }

    public void ToggleInventory()
    {
        // 1. Flip the true/false switch
        isOpen = !isOpen;
        player.GetComponent<Player>().HasOpenedInventory = isOpen;

        // 2. Show or hide the actual UI panel
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            // --- OPENING ---
            RefreshUI();

            // Switch to the Settings map so WASD stops working
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Settings");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // --- CLOSING ---
            // Switch back to Player map so we can move again
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Player");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void RefreshUI()
    {
        if (playerInventory == null) return;

        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }

        foreach (InventorySlot slot in playerInventory.slots)
        {
            GameObject newSlot = Instantiate(slotPrefab, listParent);

            var text = newSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                // FIXED: Added count display (x5)
                string countText = slot.itemData.isStackable ? $" x{slot.count}" : "";
                text.text = $"{slot.itemData.itemName}{countText} ({slot.itemData.weight * slot.count} kg)";
            }

            DraggableItem dragScript = newSlot.GetComponent<DraggableItem>();
            if (dragScript != null)
            {
                dragScript.itemData = slot.itemData;
            }
        }

        if (weightText != null)
        {
            weightText.text = $"Weight: {playerInventory.currentWeight} / {playerInventory.maxWeight}";
        }
    }
}