using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] GameObject[] itemSlots;
    [SerializeField] GameObject itemhoverVisual;

    GameObject previousSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerSimpleInventory.Instance.onInventorySlotChange += OnItemSlotUpdated;
        PlayerSimpleInventory.Instance.onSelectedSlotChange += OnSlotSelectedUpdated;

        OnSlotSelectedUpdated(0, null);
    }

    void OnItemSlotUpdated(int index, Item item)
    {
        var imageComponent = itemSlots[index].transform.Find("ItemSprite").GetComponent<Image>();
        imageComponent.sprite = item?.sprite;
        imageComponent.enabled = item != null;
    }

    void OnSlotSelectedUpdated(int index, Item item)
    {
        var rectTrans = itemhoverVisual.GetComponent<RectTransform>();
        rectTrans.SetParent(itemSlots[index].transform);
        rectTrans.anchoredPosition = Vector3.zero;
        rectTrans.transform.SetSiblingIndex(0);

        if (item != null)
        {
            DoItemTextAnim(index, item);
        }
        else
        {
            RemoveItemTextAnim(itemSlots[index]);
        }

        if ((previousSlot != null && itemSlots[index] != previousSlot))
        {
            RemoveItemTextAnim(previousSlot);
        }

        previousSlot = itemSlots[index];
    }

    void DoItemTextAnim(int index, Item item)
    {
        var itemName = itemSlots[index].transform.Find("ItemName");
        var itemNameText = itemName.GetChild(0).GetComponent<TextMeshProUGUI>();
        itemNameText.text = item.Name;
        itemName.GetComponent<Animator>().SetBool("FadeIn", true);
    }

    void RemoveItemTextAnim(GameObject slot)
    {
        var itemName = slot.transform.Find("ItemName");
        itemName.GetComponent<Animator>().SetBool("FadeIn", false);
    }
}
