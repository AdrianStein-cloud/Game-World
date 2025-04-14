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
        var imageComponent = itemSlots[index].transform.Find("ItemSprite").GetComponent<Image>();
        imageComponent.sprite = item?.sprite;
        imageComponent.enabled = item != null;
    }

    void OnSlotSelectedUpdated(int index)
    {
        var rectTrans = itemhoverVisual.GetComponent<RectTransform>();
        rectTrans.SetParent(itemSlots[index].transform);
        rectTrans.anchoredPosition = Vector3.zero;
        rectTrans.transform.SetSiblingIndex(0);
    }
}
