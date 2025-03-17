using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] LayerMask interactionLayer;
    [SerializeField] float interactionDistance;
    Interactable hoveringObject;
    GameObject mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main.gameObject;
        var playerInputs = InputManager.Player;
        playerInputs.Interact.SubscribeToAllActions(TryInteract);
    }

    // Update is called once per frame
    void Update()
    {
        TryHoverInteractable();
    }

    void TryHoverInteractable()
    {
        var hasHit = Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, interactionDistance, interactionLayer);

        // Looking at interactable
        if (hasHit && hit.transform.gameObject.TryGetComponent(out Interactable interactable))
        {
            if (hoveringObject != interactable)
            {
                hoveringObject?.HoverOut();
            }

            hoveringObject = interactable;
            interactable.Hover();
        }
    }

    void TryInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interacting");
        hoveringObject?.Interact();
    }
}
