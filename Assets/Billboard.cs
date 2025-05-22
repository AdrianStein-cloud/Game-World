using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] float toPlayerOffset;
    GameObject player;
    Vector3 originalLossyScale;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        originalLossyScale = transform.lossyScale;
    }

    public void UpdateBillboard(GameObject go)
    {
        transform.GetChild(0).gameObject.SetActive(true);

        // Reparent
        transform.SetParent(go.transform, worldPositionStays: false); // Note: false keeps local position/rotation/scale

        // Restore scale manually
        Vector3 parentLossyScale = go.transform.lossyScale;
        //transform.localScale = new Vector3(
        //    originalLossyScale.x / parentLossyScale.x,
        //    originalLossyScale.y / parentLossyScale.y,
        //    originalLossyScale.z / parentLossyScale.z
        //);

        // Then reposition
        transform.localPosition = Vector3.zero;
        transform.position += go.transform.forward * toPlayerOffset + offset;
    }

    public void Detach()
    {
        transform.SetParent(null, worldPositionStays: false);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // Billboard.
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
