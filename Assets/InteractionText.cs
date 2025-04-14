using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textmeshpro;
    [SerializeField] Vector3 offset;
    [SerializeField] float toPlayerOffset;
    GameObject player;

    private void Start()
    {
        PlayerInteraction.Instance.OnHoverInteractable.AddListener(UpdateInteractionText);
        PlayerInteraction.Instance.OnHoverOutInteractable.AddListener((_) => transform.GetChild(0).gameObject.SetActive(false));
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void UpdateInteractionText(Interactable interactable)
    {
        string displayText = interactable.nametext;

        // If there is any text to display...
        if (!string.IsNullOrEmpty(displayText))
        {
            textmeshpro.text = displayText;
            textmeshpro.ForceMeshUpdate(); // 🔧 Force text update

            // Optional: if your UI scales with layout
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textmeshpro.transform.parent);

            transform.GetChild(0).gameObject.SetActive(true);

            // Align and position UI
            var dirToPlayer = interactable.transform.position - player.transform.position;
            transform.parent = interactable.transform;
            transform.localPosition = Vector3.zero;
            transform.position += interactable.transform.forward * toPlayerOffset + offset;
            transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
    }

    void LateUpdate()
    {
        // Billboard.
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
