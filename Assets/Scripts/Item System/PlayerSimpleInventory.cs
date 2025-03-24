using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSimpleInventory : MonoBehaviour
{
    public static PlayerSimpleInventory Instance;

    [SerializeField] Item[] items;
    [SerializeField] Item selectedItem;
    GameObject[] groundItems;
    int inventorySize = 5;
    int currentlySelectedSlot = 0;

    public UnityAction<int, Item> onInventorySlotChange;
    public UnityAction<int> onSelectedSlotChange;

    [SerializeField] LayerMask dropItemLayer;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        items = new Item[inventorySize];
        groundItems = new GameObject[inventorySize];

        var playerInputs = InputManager.Player;
        playerInputs.InventoryButtons.performed += InventoryButtons;

        onInventorySlotChange += SelectItem;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Dropping item: " + selectedItem?.Name);

            if (selectedItem != null)
            {
                var groundItem = groundItems[currentlySelectedSlot];
                groundItem.SetActive(true);
                Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, dropItemLayer);
                groundItem.transform.position = hit.point + new Vector3(0, (groundItem.transform.localScale.y / 2), 0);
            }
            onInventorySlotChange?.Invoke(currentlySelectedSlot, null);
            items[currentlySelectedSlot] = null;
            groundItems[currentlySelectedSlot] = null;
        }
    }

    void SelectItem(int index, Item _ = null)
    {
        currentlySelectedSlot = index;
        selectedItem = items[currentlySelectedSlot];
    }

    void InventoryButtons(InputAction.CallbackContext context)
    {
        var keyboardKey = (int)context.ReadValue<float>() - 1;

        if (keyboardKey == -1) return; // Unity is weird

        onSelectedSlotChange?.Invoke(keyboardKey);

        SelectItem(keyboardKey);
    }

    public bool TryPickupItem(Item item, GameObject go)
    {
        if (items[currentlySelectedSlot] != null) return false;

        PickUpItem(item, go);
        return true;
    }

    void PickUpItem(Item item, GameObject go)
    {
        items[currentlySelectedSlot] = item;
        groundItems[currentlySelectedSlot] = go;

        onInventorySlotChange?.Invoke(currentlySelectedSlot, item);
    }

    public bool ContainsItem(Item item) => items.Contains(item);
}
