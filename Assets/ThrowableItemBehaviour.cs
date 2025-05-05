using UnityEngine;

public class ThrowableItemBehavior : MonoBehaviour
{
    private ThrowableItem itemData;
    private Rigidbody rb;

    public void Initialize(ThrowableItem itemData)
    {
        this.itemData = itemData;
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            Shatter();
        }
    }

    void Shatter()
    {
        Instantiate(itemData.shatterObjectPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
