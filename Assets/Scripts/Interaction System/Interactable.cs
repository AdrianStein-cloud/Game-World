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

    Material[] initialMaterials;
    Renderer renderer;

    public void Interact() => OnInteract?.Invoke();

    private void Awake()
    {
        OnHoverIn.AddListener(BeginOutline);
        OnHoverOut.AddListener(EndOutline);

        renderer = GetComponent<Renderer>();
        initialMaterials = renderer.materials;
    }

    public void Hover()
    {
        OnHoverIn?.Invoke();
    }

    public void HoverOut()
    {
        OnHoverOut?.Invoke();
    }

    public void BeginOutline()
    {
        // Create a new list and add the outline material
        List<Material> materials = new List<Material>(renderer.materials);
        materials.Add(PlayerInteraction.Instance.normalOutlineMaterial);

        // Assign back to the renderer
        renderer.materials = materials.ToArray();
    }

    public void EndOutline()
    {
        renderer.materials = initialMaterials;
    }
}
