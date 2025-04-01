using UnityEngine;

public class VentOpen : MonoBehaviour, ILockChecker
{
    [SerializeField] Item item;
    [SerializeField] Animator anim;

    public bool IsLocked()
    {
        return !PlayerSimpleInventory.Instance.ContainsItem(item);
    }

    public void OpenVentGrate()
    {
        if (PlayerSimpleInventory.Instance.ContainsItem(item))
        {
            anim.SetTrigger("Open");
        }
    }
}
