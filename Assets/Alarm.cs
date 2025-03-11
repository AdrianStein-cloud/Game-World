using System.Collections;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    [SerializeField] private float interval = 2f;
    [SerializeField] private GameObject bulb;
    [SerializeField] private float fullIntensity = 1f;    // Maximum brightness when "on"
    [SerializeField] private float leastIntensity = 0.2f; // Minimum brightness when "off"
    [SerializeField] private float fadeDuration = 0.5f;   // Time to fade

    private Material glowMaterial;
    private Light light;
    private float timer;
    private bool isOn = false;
    private Coroutine fadeCoroutine;
    private Color originalEmissionColor;
    private Color leastEmissionColor; // Dim emission when "off"

    void Start()
    {
        light = GetComponentInChildren<Light>();
        timer = interval;

        if (bulb != null)
        {
            glowMaterial = bulb.GetComponent<Renderer>().material;
            originalEmissionColor = glowMaterial.GetColor("_EmissionColor"); // Full emission when "on"
            leastEmissionColor = originalEmissionColor * (leastIntensity / fullIntensity); // Dim emission
            glowMaterial.SetColor("_EmissionColor", leastEmissionColor * 3);
        }

        // Start with dim light
        light.intensity = leastIntensity;
        light.enabled = true; // Always on, but varies in brightness
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = interval;
            TriggerAlarm();
        }
    }

    private void TriggerAlarm()
    {
        isOn = !isOn;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeLight(isOn));
    }

    private IEnumerator FadeLight(bool turnOn)
    {
        float elapsed = 0f;
        float startIntensity = light.intensity;
        float targetIntensity = turnOn ? fullIntensity : leastIntensity;
        Color startEmission = glowMaterial != null ? glowMaterial.GetColor("_EmissionColor") : Color.black;
        Color targetEmission = turnOn ? originalEmissionColor : leastEmissionColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Lerp light intensity
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            // Lerp emission color
            if (glowMaterial != null)
            {
                Color newEmission = Color.Lerp(startEmission, targetEmission, t);
                glowMaterial.SetColor("_EmissionColor", newEmission);
            }

            yield return null;
        }

        // Ensure final values
        light.intensity = targetIntensity;
        if (glowMaterial != null)
            glowMaterial.SetColor("_EmissionColor", targetEmission);
    }
}
