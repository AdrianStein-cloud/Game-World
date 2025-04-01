using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] Item item;

    public void PickupItem()
    {
        if (PlayerSimpleInventory.Instance.TryPickupItem(item, this.gameObject))
        {
            this.gameObject.SetActive(false);
        }
    }
}
