using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;

    [SerializeField] LayerMask interactionLayer;
    [SerializeField] float interactionDistance;

    [field: SerializeField] public Material normalOutlineMaterial { get; private set; }
    [field: SerializeField] public Material denyOutlineMaterial { get; private set; }

    public UnityEvent<Interactable> OnHoverInteractable, OnHoverOutInteractable;

    public Interactable hoveringObject {get; private set;}
    
    Transform mainCamera;

    private void Awake()
    {
        Instance = this;
    }

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
             
                hoveringObject = interactable;
                interactable.Hover();
                OnHoverInteractable?.Invoke(hoveringObject);
            }
        }
        else
        {
            OnHoverOutInteractable?.Invoke(hoveringObject);
            hoveringObject?.HoverOut();
            hoveringObject = null;
        }
    }

    void TryInteract(InputAction.CallbackContext context)
    {
        hoveringObject?.Interact();
    }
}
