using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] GameObject[] itemSlots;
    [SerializeField] GameObject itemhoverVisual;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerSimpleInventory.Instance.onInventorySlotChange += OnItemSlotUpdated;
        PlayerSimpleInventory.Instance.onSelectedSlotChange += OnSlotSelectedUpdated;

        OnSlotSelectedUpdated(0);
    }

    void OnItemSlotUpdated(int index, Item item)
    {
        itemSlots[index].GetComponent<Image>().sprite = item?.sprite;
    }

    void OnSlotSelectedUpdated(int index)
    {
        var rectTrans = itemhoverVisual.GetComponent<RectTransform>();
        rectTrans.SetParent(itemSlots[index].transform);
        rectTrans.anchoredPosition = Vector3.zero;
    }
}
