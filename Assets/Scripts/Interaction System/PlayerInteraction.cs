using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] LayerMask interactionLayer;
    [SerializeField] float interactionDistance;
    Interactable hoveringObject;
    Transform mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main.transform;
        var playerInputs = InputManager.Player;
        playerInputs.Interact.started += TryInteract;
    }

    // Update is called once per frame
    void Update()
    {
        TryHoverInteractable();
    }

    void TryHoverInteractable()
    {
        var hasHit = Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, interactionDistance, interactionLayer);

        // Looking at interactable
        if (hasHit && hit.transform.TryGetComponent(out Interactable interactable))
        {
            if (hoveringObject != interactable)
            {
                hoveringObject?.HoverOut();
            }

            hoveringObject = interactable;
            interactable.Hover();
        }
        else if (hoveringObject != null)
        {
            hoveringObject?.HoverOut();
            hoveringObject = null;
        }
    }

    void TryInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interacting");
        hoveringObject?.Interact();
    }
}
