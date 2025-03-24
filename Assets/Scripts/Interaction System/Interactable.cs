using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class Interactable : MonoBehaviour
{
    [SerializeField] UnityEvent OnInteract, OnHoverIn, OnHoverOut;
    [SerializeField] bool once = false;

    bool hovering;

    private List<Renderer> renderers;
    private Dictionary<Renderer, Material[]> originalMaterials;
    public void Interact()
    {
        OnInteract?.Invoke();
        if (once)
        {
            this.enabled = false;
            if (hovering) HoverOut();
        }
    }

    private void Awake()
    {
        OnHoverIn.AddListener(BeginOutline);
        OnHoverOut.AddListener(EndOutline);

        renderers = new List<Renderer>();
        originalMaterials = new Dictionary<Renderer, Material[]>();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderers.Add(renderer);
            originalMaterials[renderer] = renderer.materials;
        }
    }

    public void Hover()
    {
        hovering = true;
        OnHoverIn?.Invoke();
    }

    public void HoverOut()
    {
        hovering = false;
        OnHoverOut?.Invoke();
    }

    public void BeginOutline()
    {
        foreach (Renderer renderer in renderers)
        {
            List<Material> materials = new List<Material>(renderer.materials);
            materials.Add(PlayerInteraction.Instance.normalOutlineMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    public void EndOutline()
    {
        foreach (Renderer renderer in renderers)
        {
            if (originalMaterials.TryGetValue(renderer, out Material[] originalMats))
            {
                renderer.materials = originalMats;
            }
        }
    }
}
