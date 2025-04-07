using UnityEngine;

[CreateAssetMenu(fileName = "Flashlight", menuName = "Item/Special Items/New Flashlight")]
public class Flashlight : Item
{
    public GameObject flashlightPrefab;

    private FlashlightController flashlightInstance;

    public override void OnPickup()
    {
        if (flashlightPrefab != null)
        {
            var flashlightParent = Camera.main.gameObject;
            GameObject instance = Instantiate(flashlightPrefab, flashlightParent.transform);
            flashlightInstance = instance.GetComponent<FlashlightController>();

            flashlightInstance.Init(this);
        }
    }

    public override void OnDrop()
    {
        flashlightInstance.ToggleFlashlight(false);
    }

    public override void Use()
    {
        flashlightInstance.ToggleFlashlight();
    }
}
