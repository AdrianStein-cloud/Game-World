using UnityEngine;
using UnityEngine.Events;

public class TriggerInteractable : Interactable
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cosmetic = IsLocked();
            Interact();
        }
    }
}
