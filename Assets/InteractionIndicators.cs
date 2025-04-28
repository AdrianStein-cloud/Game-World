using TMPro;
using UnityEngine;

public class InteractionIndicators : MonoBehaviour
{
    public static InteractionIndicators Instance;

    [SerializeField] GameObject UseIndicator, InteractIndicator;
    [SerializeField] TextMeshProUGUI UseText, InteractText;

    private void Awake()
    {
        Instance = this;

        UseIndicator.SetActive(false);
        InteractIndicator.SetActive(false);
    }

    public void IndicateUse(Item item)
    {
        UpdateIndicator(UseIndicator, UseText, item?.UseText);

    }

    public void IndicateInteraction(Interactable interactable)
    {
        UpdateIndicator(InteractIndicator, InteractText, interactable?.actiontext);
    }

    private void UpdateIndicator(GameObject indicator, TextMeshProUGUI textmesh, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            indicator.SetActive(false);
            return;
        }

        indicator.SetActive(true);
        textmesh.text = text;
    }
}
