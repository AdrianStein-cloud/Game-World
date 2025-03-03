using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] GameObject[] itemSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerSimpleInventory.Instance.onInventorySlotChange += OnItemSlotUpdated;   
    }

    void OnItemSlotUpdated(int index, Item item)
    {
        itemSlots[index].GetComponent<Image>().sprite = item.sprite;
    }
}
