using UnityEngine;

public class RequiredItem : MonoBehaviour, ILockChecker
{
    [SerializeField] private Item requiredItem;
    [SerializeField] private bool requireHoldingItem;

    public bool IsLocked()
    {
        return requireHoldingItem ? !PlayerSimpleInventory.Instance.HoldsItem(requiredItem) : !PlayerSimpleInventory.Instance.ContainsItem(requiredItem);
    }

    public void LockedInteract()
    {
        
    }
}
