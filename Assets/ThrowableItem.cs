using UnityEngine;

[CreateAssetMenu(fileName = "Throwable Item", menuName = "Item/New Throwable Item")]

public class ThrowableItem : Item
{
    [SerializeField] float force;
    static float forwardOffset = 0.15f;
    static float upOffset = -0.15f;
    static float rightOffset = 0.15f;

    public override void Use()
    {
        var groundItem = PlayerSimpleInventory.Instance.DropItem();

        groundItem.transform.position = Camera.main.transform.position + 
            (Camera.main.transform.forward * forwardOffset) + 
            upOffset * Vector3.up + 
            rightOffset * Camera.main.transform.right;
        groundItem.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * force);
    }
}
