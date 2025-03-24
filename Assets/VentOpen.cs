using UnityEngine;

public class VentOpen : MonoBehaviour
{
    [SerializeField] Item item;
    [SerializeField] Animator anim;

    public void OpenVentGrate()
    {
        if (PlayerSimpleInventory.Instance.ContainsItem(item))
        {
            anim.SetTrigger("Open");
        }
    }
}
