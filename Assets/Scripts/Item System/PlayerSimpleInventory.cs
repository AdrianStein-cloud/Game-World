using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSimpleInventory : MonoBehaviour
{
    public static PlayerSimpleInventory Instance;

    [SerializeField] Item[] items;
    int inventorySize = 5;
    int index = 0;

    public UnityAction<int, Item> onInventorySlotChange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        items = new Item[inventorySize];

        var playerInputs = InputManager.Player;
        playerInputs.InventoryButtons.SubscribeToAllActions(InventoryButtons);
    }

    void InventoryButtons(InputAction.CallbackContext context)
    {
        var keyboardKey = (int)context.ReadValue<float>();
        Debug.Log("Pressed: " + keyboardKey);
    }

    public bool TryPickupItem(Item item)
    {
        if (index >= inventorySize) return false;

        PickUpItem(item);
        return true;
    }

    void PickUpItem(Item item)
    {
        onInventorySlotChange?.Invoke(index, item);
        items[index] = item;
        index++;
    }
}
