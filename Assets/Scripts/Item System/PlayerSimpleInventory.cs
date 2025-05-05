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
    public UnityAction<int, Item> onSelectedSlotChange;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryUseItem();
        }
    }

    void TryUseItem()
    {
        items[currentlySelectedSlot]?.Use();
    }

    public GameObject DropItem()
    {
        GameObject groundItem = null;

        if (selectedItem != null)
        {
            groundItem = groundItems[currentlySelectedSlot];
            groundItem.SetActive(true);
            //Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, dropItemLayer);
            //groundItem.transform.position = hit.point + new Vector3(0, (groundItem.transform.localScale.y / 2), 0);
            groundItem.transform.position = transform.position + 0.1f * transform.forward;
        }
        items[currentlySelectedSlot]?.OnDrop();
        items[currentlySelectedSlot] = null;
        groundItems[currentlySelectedSlot] = null;
        selectedItem = null;
        onInventorySlotChange?.Invoke(currentlySelectedSlot, null);
        onSelectedSlotChange?.Invoke(currentlySelectedSlot, null);

        return groundItem;
    }

    void SelectItem(int index)
    {
        currentlySelectedSlot = index;
        selectedItem = items[currentlySelectedSlot];
        InteractionIndicators.Instance.IndicateUse(selectedItem);
    }

    void InventoryButtons(InputAction.CallbackContext context)
    {
        var keyboardKey = (int)context.ReadValue<float>() - 1;

        if (keyboardKey == -1) return; // Unity is weird

        SelectItem(keyboardKey);

        onSelectedSlotChange?.Invoke(keyboardKey, selectedItem);

        PlayerInteraction.Instance.UpdateHover();
    }

    public bool TryPickupItem(Item item, GameObject go)
    {
        var freeSlotIndex = -1;

        if (items[currentlySelectedSlot] == null)
        {
            freeSlotIndex = currentlySelectedSlot;
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    freeSlotIndex = i;
                    break;
                }
            }
        if (freeSlotIndex == -1) return false;
        }

        PickUpItem(freeSlotIndex, item, go);
        return true;
    }

    void PickUpItem(int slotIndex, Item item, GameObject go)
    {
        items[slotIndex] = item;
        groundItems[slotIndex] = go;
        items[slotIndex].OnPickup();

        if (currentlySelectedSlot == slotIndex)
        {
            SelectItem(slotIndex);
            onSelectedSlotChange?.Invoke(slotIndex, item);
        }
        onInventorySlotChange?.Invoke(slotIndex, item);

        InteractionText.Instance.Detach();
    }

    public bool ContainsItem(Item item)
    {
        return items.Contains(item);
    }

    public bool HoldsItem(Item item)
    {
        return items[currentlySelectedSlot] == item;
    }
}
