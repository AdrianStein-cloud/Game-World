using UnityEngine;

[CreateAssetMenu(fileName = "Flashlight", menuName = "Item/Special Items/New Screwdriver")]
public class Screwdriver : Item
{
    public override void Use()
    {
        var playerInteract = PlayerInteraction.Instance;

        if (playerInteract.hoveringObject.TryGetComponent(out VentOpen vent))
        {
            vent.OpenVentGrate();
        }
    }
}
