using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DynamicLight : MonoBehaviour
{
    [SerializeField] bool disableShadows;
    [SerializeField] float shadowRange;
    [SerializeField] bool disableLights;
    [SerializeField] float lightRange;
    /*[SerializeField] */
    float lightDuration;
    [SerializeField] List<Light> lightsToDisable;

    Light[] lights;
    Transform player;
    float intensity;
    bool lightsOn = true;
    bool shadowsOn = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        lights = GetComponentsInChildren<Light>();
        intensity = lights[0].intensity;
    }

    private void FixedUpdate()
    {
        var distance = Vector3.Distance(transform.position, player.position);
        if (disableLights && lightsToDisable.Count > 0)
        {
            if (lightsOn && distance > lightRange)
            {
                ToggleLights(false);
                lightsOn = false;
            }
            else if (!lightsOn && distance <= lightRange)
            {
                ToggleLights(true);
                lightsOn = true;
            }
        }

        if (disableShadows)
        {
            if (shadowsOn && distance > shadowRange)
            {
                ToggleShadows(false);
                shadowsOn = false;
            }
            else if (!shadowsOn && distance <= shadowRange)
            {
                ToggleShadows(true);
                shadowsOn = true;
            }
        } 
    }

    void ToggleShadows(bool on)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].shadows = on ? LightShadows.Soft : LightShadows.None;
        }
    }

    void ToggleLights(bool on)
    {
        for (int i = 0; i < lightsToDisable.Count; i++)
        {
            lightsToDisable[i].gameObject.SetActive(on);
            /*StopAllCoroutines();
            StartCoroutine(LerpLight(disableLights[i], on));*/
        }
    }

    IEnumerator LerpLight(Light light, bool on)
    {
        if (on) light.gameObject.SetActive(true);

        var currentIntensity = light.intensity;
        var targetIntensity = on ? intensity : 0f;
        var time = 0f;

        while (time < lightDuration)
        {
            light.intensity = Mathf.Lerp(currentIntensity, targetIntensity, time / lightDuration);
            time += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetIntensity;

        if (!on) light.gameObject.SetActive(false);
    }
}
