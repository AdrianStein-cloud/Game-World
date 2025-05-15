using UnityEngine;

public class RequiredItem : MonoBehaviour, ILockChecker
{
    [SerializeField] private Item requiredItem;

    public bool IsLocked()
    {
        return !PlayerSimpleInventory.Instance.HoldsItem(requiredItem);
    }
}
