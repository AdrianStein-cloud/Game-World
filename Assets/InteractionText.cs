using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionText : MonoBehaviour
{
    public static InteractionText Instance;

    [SerializeField] TextMeshProUGUI textmeshpro;
    [SerializeField] Vector3 offset;
    [SerializeField] float toPlayerOffset;
    GameObject player;
    Vector3 originalLossyScale;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayerInteraction.Instance.OnHoverInteractable.AddListener(UpdateInteractionText);
        PlayerInteraction.Instance.OnHoverOutInteractable.AddListener((_) => transform.GetChild(0).gameObject.SetActive(false));
        player = GameObject.FindGameObjectWithTag("Player");
        originalLossyScale = transform.lossyScale;
    }

    public void UpdateInteractionText(Interactable interactable)
    {
        string displayText = interactable.nametext;

        // If there is any text to display...
        if (!string.IsNullOrEmpty(displayText))
        {
            transform.GetChild(0).gameObject.SetActive(true);

            textmeshpro.text = displayText;
            textmeshpro.ForceMeshUpdate(); // Force text update

            // Optional: if your UI scales with layout
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textmeshpro.transform.parent);

            // Reparent
            transform.SetParent(interactable.transform, worldPositionStays: false); // Note: false keeps local position/rotation/scale

            // Restore scale manually
            Vector3 parentLossyScale = interactable.transform.lossyScale;
            transform.localScale = new Vector3(
                originalLossyScale.x / parentLossyScale.x,
                originalLossyScale.y / parentLossyScale.y,
                originalLossyScale.z / parentLossyScale.z
            );

            // Then reposition
            transform.localPosition = Vector3.zero;
            transform.position += interactable.transform.forward * toPlayerOffset + offset;

            transform.position += interactable.HovertextOffset;
        }
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
