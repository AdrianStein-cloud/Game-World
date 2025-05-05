using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlightLight;
    private bool isOn = false;
    private Flashlight flashlightData;

    public void Init(Flashlight data)
    {
        flashlightData = data;
    }

    public void ToggleFlashlight(bool force = false)
    {
        if (force)
        {
            flashlightLight.enabled = false;
            isOn = false;
            return;
        }

        isOn = !isOn;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isOn;
        }
    }
}
