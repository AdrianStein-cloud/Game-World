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
    [SerializeField] bool cosmetic = false;
    [SerializeField] protected UnityEvent OnInteract, OnHoverIn, OnHoverOut;
    [field:SerializeField] public Vector3 HovertextOffset { get; private set; }

    [SerializeField] private MonoBehaviour lockCheckerComponent;

    [TextArea(1, 3)] public string nametext;
    [TextArea(1, 3)] public string actiontext;

    protected List<Renderer> renderers;
    protected Dictionary<Renderer, Material[]> originalMaterials;
    protected bool hovering;

    

    public void Interact()
    {
        // If there isn't set any interaction, then we don't want anything to happen on interact.
        if (IsLocked() || cosmetic) return;

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
        if (hovering) OnHoverOut?.Invoke();
        OnHoverIn?.Invoke();
        hovering = true;
        InteractionIndicators.Instance.IndicateInteraction(this);
    }

    public void HoverOut()
    {
        hovering = false;
        OnHoverOut?.Invoke();
        InteractionIndicators.Instance.IndicateInteraction(null);
    }

    public virtual void BeginOutline()
    {
        foreach (Renderer renderer in renderers)
        {
            List<Material> materials = new List<Material>(renderer.materials);

            var playerInteract = PlayerInteraction.Instance;
            if(IsLocked()) materials.Add(playerInteract.denyOutlineMaterial);
            else materials.Add(playerInteract.normalOutlineMaterial);
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

    public bool IsLocked()
    {
        if (lockCheckerComponent != null && lockCheckerComponent is ILockChecker checker)
        {
            return checker.IsLocked();
        }
        // If no lockChecker is assigned, or it doesn't implement ILockChecker, assume it's unlocked.
        return false;
    }

    private void OnDestroy()
    {
        if (hovering) HoverOut();
    }
}
