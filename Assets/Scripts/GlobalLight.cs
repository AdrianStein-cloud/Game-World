using UnityEngine;

public class GlobalLight : MonoBehaviour
{
    private static bool lightsOn;
    public static bool LightsOn
    {
        get => lightsOn;
        set
        {
            if (lightsOn != value) // Only trigger when value changes
            {
                lightsOn = value;
                OnLightsChanged?.Invoke(lightsOn); // Notify all listeners
            }
        }
    }
    public static event System.Action<bool> OnLightsChanged;

    [SerializeField] private GameObject bulb;
    [SerializeField] private bool isAlarm = false;
    [SerializeField] private Light light;

    private Material glowMaterial;
    private Color originalEmissionColor;
    private float originalIntensity;

    private void Start()
    {
        HandleLightChange(lightsOn);

        if (bulb != null)
        {
            glowMaterial = bulb.GetComponent<Renderer>().material;
            originalEmissionColor = glowMaterial.GetColor("_EmissionColor");
        }

        originalIntensity = light.intensity;

        OnLightsChanged += HandleLightChange;
    }

    private void HandleLightChange(bool state)
    {
        if (state == !isAlarm)
            TurnOn();
        else
            TurnOff();
    }

    private void TurnOn()
    {
        if (glowMaterial != null)
            glowMaterial.SetColor("_EmissionColor", originalEmissionColor);
        if (light != null)
            light.intensity = originalIntensity;
    }

    private void TurnOff()
    {
        if (glowMaterial != null)
            glowMaterial.SetColor("_EmissionColor", new Color(0, 0, 0));
        light.intensity = 0;
    }

    private void OnDestroy()
    {
        OnLightsChanged -= HandleLightChange; // Unsubscribe to prevent memory leaks
    }
}
