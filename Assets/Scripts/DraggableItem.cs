using UnityEngine;
using UnityEngine.EventSystems; // This is the "magic" include for dragging
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Item itemData;
    private Inventory playerInventory;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

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

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if we are hovering over ANY UI (the Panel, the Scrollbar, etc.)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // We are over the 3D world! Delete it.
            playerInventory.RemoveItem(itemData);
            Destroy(gameObject);
        }
        else
        {
            // We dropped it back on the UI. Put it back in the vertical list.
            transform.SetParent(originalParent);
        }
    }
}