using System.Collections;
using UnityEngine;

public class ShatterObject : MonoBehaviour
{
    [SerializeField] float physicsDisableDelay;
    [SerializeField] float force = 50f;
    [SerializeField] float randomTorqueStrength = 50f;

    private void Awake()
    {
        ExplodePhysics();
        StartCoroutine(DisablePhysics());
    }

    IEnumerator DisablePhysics()
    {
        yield return new WaitForSeconds(physicsDisableDelay);

        foreach(var rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
    }

    void ExplodePhysics()
    {
        foreach (var rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 directionToShard = rb.transform.position - transform.position;

            Vector3 randomDirection = directionToShard.normalized + Random.onUnitSphere;

            rb.AddForce(randomDirection * force, ForceMode.Impulse);


            Vector3 randomTorque = Random.onUnitSphere * randomTorqueStrength;
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }
}
