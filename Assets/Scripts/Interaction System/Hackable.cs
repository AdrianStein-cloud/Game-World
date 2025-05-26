using System.Collections.Generic;
using UnityEngine;

public class Hackable : Interactable
{
    public HackingInformation hackingInfo;
    public bool scanned;

    public bool hacked { get; private set; }

    private void Start()
    {
        OnHoverIn.RemoveAllListeners();
        OnHoverOut.RemoveAllListeners();
        OnInteract.RemoveAllListeners();
    }

    public override void BeginOutline()
    {
        foreach (Renderer renderer in renderers)
        {
            List<Material> materials = new List<Material>(renderer.materials);

            var playerInteract = PlayerInteraction.Instance;
            materials.Add(hacked ? playerInteract.hackcooldownOutlineMaterial : (scanned ? playerInteract.hackOutlineMaterial : playerInteract.notHackedOutlineMaterial));
            renderer.materials = materials.ToArray();
        }
    }

    public void Hack()
    {
        if (hacked) return;
        hacked = true;

        EndOutline();
        BeginOutline();

        //Effects?

        //Call zombie ai
        GetComponent<ZomibeAI>().Hack();

        Invoke(nameof(DisableHackState), 30);
    }

    public void DisableHackState()
    {
        hacked = false;
    }
}
