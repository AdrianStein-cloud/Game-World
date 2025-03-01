using UnityEngine;

public class Alarm : MonoBehaviour
{
    [SerializeField] private float interval = 2;
    [SerializeField] private GameObject bulb;

    private Material glowMaterial;
    private Light light;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponentInChildren<Light>();
        timer = interval;

        if (bulb != null)
            glowMaterial = bulb.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer <= 0)
        {
            timer = interval;
            TriggerAlarm(light.enabled);
        }
        else timer -= Time.deltaTime;
    }

    private void TriggerAlarm(bool off)
    {
        light.enabled = !off;

        if (glowMaterial == null) return;

        if (off) glowMaterial.DisableKeyword("_EMISSION");
        else glowMaterial.EnableKeyword("_EMISSION");
    }
}
