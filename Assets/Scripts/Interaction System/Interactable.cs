using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class Interactable : MonoBehaviour
{
    [SerializeField] bool once = false;
    [SerializeField] UnityEvent OnInteract, OnHoverIn, OnHoverOut;

    private List<Renderer> renderers;
    private Dictionary<Renderer, Material[]> originalMaterials;
    private bool hovering;

    public void Interact()
    {
        OnInteract?.Invoke();
        if (once)
        {
            if (hovering) HoverOut();
            Destroy(this);
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

            var playerInteract = PlayerInteraction.Instance;
            materials.Add(playerInteract.normalOutlineMaterial);
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
