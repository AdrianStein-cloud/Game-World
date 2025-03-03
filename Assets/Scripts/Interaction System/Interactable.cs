using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] UnityEvent OnInteract, OnHoverIn, OnHoverOut;

    public void Interact() => OnInteract?.Invoke();

    public void Hover()
    {
        // Add logic for outline as well
        Debug.Log("Hovering: " + gameObject.name);

        OnHoverIn?.Invoke();
    }

    public void HoverOut()
    {
        // Add logic for outline as well

        OnHoverOut?.Invoke();
    }
}
