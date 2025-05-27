using UnityEngine;

public class VentOpen : MonoBehaviour, ILockChecker
{
    [SerializeField] Item item;
    [SerializeField] Animator anim;

    public bool IsLocked()
    {
        return !PlayerSimpleInventory.Instance.HoldsItem(item);
    }

    public void LockedInteract()
    {
    }

    public void OpenVentGrate()
    {
        anim.SetTrigger("Open");
        GetComponent<PlaySound>().PlayAudio();
        Destroy(this.GetComponent<Interactable>());
    }
}
