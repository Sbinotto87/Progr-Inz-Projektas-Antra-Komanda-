using UnityEngine;
using UnityEngine.EventSystems; // This is the "magic" include for dragging
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public Item itemData;
    private Inventory playerInventory;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    public Item equippedItem;
    private bool equipped;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Find the player automatically so we don't have to drag it in the inspector
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInventory = player.GetComponent<Inventory>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        // 1. Find the actual Canvas component in your scene
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // 2. Move the item to the Canvas directly so it's above the Panels
            transform.SetParent(canvas.transform);
        }

        // 3. Force it to the very front of the draw order
        transform.SetAsLastSibling();

        // 4. Safety: Reset scale so it doesn't shrink or grow
        transform.localScale = Vector3.one;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the mouse, but force Z to 0 so it stays on the UI plane
        Vector3 mousePos = eventData.position;
        mousePos.z = 0;
        transform.position = mousePos;
    }

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    canvasGroup.alpha = 1f;
    //    canvasGroup.blocksRaycasts = true;

    //    // Check if we are hovering over ANY UI (the Panel, the Scrollbar, etc.)
    //    if (!EventSystem.current.IsPointerOverGameObject())
    //    {
    //        // We are over the 3D world! Delete it.
    //        playerInventory.RemoveItem(itemData);
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        // We dropped it back on the UI. Put it back in the vertical list.
    //        transform.SetParent(originalParent);
    //    }
    //}

    // To eat food ===================
    public void OnPointerClick(PointerEventData eventData)
    {
        // Double click check
        if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
        {
            if (itemData.category == ItemCategory.Food || itemData.category == ItemCategory.Drink)
                ConsumeItem();
            if (itemData.category == ItemCategory.Weapon || itemData.category == ItemCategory.Tool)
                EquipItem();
        }
    }

    private void ConsumeItem()
    {
        // Safety check: do we have item data and a player inventory reference?
        if (itemData == null || playerInventory == null) return;

        if (itemData.category == ItemCategory.Food || itemData.category == ItemCategory.Drink)
        {
            // Change this line to look for the "Player" script instead of "PlayerStats"
            Player playerScript = playerInventory.GetComponent<Player>();

            if (playerScript != null)
            {
                // Apply the restore value based on type
                if (itemData.category == ItemCategory.Food)
                    playerScript.AddHunger(itemData.hungerRestoreValue);
                else
                    playerScript.AddThirst(itemData.hungerRestoreValue);

                // Remove 1 item from the stack
                playerInventory.RemoveItem(itemData);

                // Redraw the UI so the numbers update (or the button vanishes)
                InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                if (ui != null) ui.RefreshUI();

                Debug.Log($"Consumed {itemData.itemName}!");
            }
        }
    }


    // Temporary copy of method OnEndDrag to test food eating by dragging out of inventory and consuming it if it is food, will add a dedicated button or function to just consume it with an action
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if we dropped it OUTSIDE the UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // If it's food/drink, call our unified Consume method
            if (itemData.category == ItemCategory.Food || itemData.category == ItemCategory.Drink)
            {
                ConsumeItem();
            }
            else
            {
                // If it's not food, just discard the stack
                playerInventory.RemoveFullStack(itemData);
                InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                if (ui != null) ui.RefreshUI();
            }

            // The button itself needs to be destroyed since it was "dragged out"
            Destroy(gameObject);
        }
        else
        {
            // Snap back to inventory if dropped inside
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
    
    public void EquipItem()
    {
        Transform tool = GameObject.Find("UI elements").transform.Find("tool");
        if (!equipped)
        {
            Image toolImage = tool.GetComponent<Image>();
            tool.gameObject.SetActive(true);
            toolImage.sprite = itemData.icon;
            equippedItem = itemData;
            equipped = !equipped;
        }
        else
        {
            tool.gameObject.SetActive(false);
            equipped = !equipped;
        }
        
    }
}