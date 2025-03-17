using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] Item item;

    public void PickupItem()
    {
        if (PlayerSimpleInventory.Instance.TryPickupItem(item))
        {
            Destroy(this.gameObject);
        }
    }
}
