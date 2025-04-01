using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightFlicker : MonoBehaviour
{
    GameObject[] childObjects;
    float[] lightIntensities;

    [SerializeField] GameObject bulb;

    private Material glowMaterial;
    public static bool LightsOn;

    [SerializeField] float offFlickerSpeedMin = 0.1f;
    [SerializeField] float offFlickerSpeedMax = 0.2f;
    [SerializeField] float onFlickerSpeedMin = 0.05f;
    [SerializeField] float onFlickerSpeedMax = 20f;
    [SerializeField] bool alwaysOn;
    [SerializeField] bool hasSound;
    [SerializeField] bool canScare;
    [SerializeField] GameObject scaryObject;
    [SerializeField] private bool isAlarm = false;

    private bool reversed;
    private bool off;

    private void Start()
    {
        if (scaryObject != null)
            scaryObject.SetActive(false);

        if(bulb != null)
            glowMaterial = bulb.GetComponent<Renderer>().material;

        if (!alwaysOn && Random.Range(0, 100) < 10)
        {
            ReverseFlicker();
        }

        childObjects = transform.Cast<Transform>().Select(t => t.gameObject).ToArray();

        lightIntensities = new float[childObjects.Length];

        for (int i = 0; i < childObjects.Length; i++)
        {
            lightIntensities[i] = childObjects[i].GetComponent<Light>().intensity;
        }

        bool failed = false;

        if (!failed && LightsOn == !isAlarm)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    IEnumerator StartLife(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        TurnOff();
    }

    void ReverseFlicker()
    {
        reversed = !reversed;

        var temp = offFlickerSpeedMax;
        offFlickerSpeedMax = onFlickerSpeedMax;
        onFlickerSpeedMax = temp;

        temp = offFlickerSpeedMin;
        offFlickerSpeedMin = onFlickerSpeedMin;
        onFlickerSpeedMin = temp;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void TurnOff()
    {
        off = true;

        if (!alwaysOn)
        {
            StopAllCoroutines();
            for (int i = 0; i < childObjects.Length; i++)
            {
                childObjects[i].GetComponent<Light>().intensity = 0;
                if (glowMaterial != null)
                    glowMaterial.DisableKeyword("_EMISSION");
            }
        }
        if (hasSound)
        {
            GetComponent<AudioSource>().Stop();
        }
        StartCoroutine(CheckState());
    }

    IEnumerator CheckState()
    {
        while (off)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            if (LightsOn == !isAlarm)
            {
                if (reversed) ReverseFlicker();
                TurnOn();
                off = false;
                yield break;
            }
            else if (off && canScare && Random.Range(0, 100) < 6)
            {
                for (int i = 0; i < Random.Range(0, 3); i++)
                {
                    FlickerAllOn();
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
                    FlickerAllOff();
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
                }

                off = true;
                scaryObject.SetActive(true);

                Transform camPos = Camera.main.transform;
                scaryObject.transform.LookAt(camPos.position - camPos.forward);

                FlickerAllOn();
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
                FlickerAllOff();
                scaryObject.SetActive(false);

                for (int i = 0; i < Random.Range(0, 3); i++)
                {
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
                    FlickerAllOn();
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
                    FlickerAllOff();
                }
            }
        }
    }

    private void TurnOn()
    {
        StopAllCoroutines();
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            if (!(LightsOn == !isAlarm))
            {
                TurnOff();
                break;
            }
            if (offFlickerSpeedMin == 0 && offFlickerSpeedMax == 0)
            {
                break;
            }
            FlickerAllOff();
            yield return new WaitForSeconds(Random.Range(offFlickerSpeedMin, offFlickerSpeedMax));
            FlickerAllOn();
            yield return new WaitForSeconds(Random.Range(onFlickerSpeedMin, onFlickerSpeedMax));
        }
    }
    private void FlickerAllOn()
    {
        for (int i = 0; i < childObjects.Length; i++)
        {
            FlickerOn(childObjects[i].GetComponent<Light>(), lightIntensities[i]);
            if (glowMaterial != null)
                glowMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void FlickerAllOff()
    {
        for (int i = 0; i < childObjects.Length; i++)
        {
            FlickerOff(childObjects[i].GetComponent<Light>(), lightIntensities[i]);
            if (glowMaterial != null)
                glowMaterial.DisableKeyword("_EMISSION");
        }
    }

    private void FlickerOn(Light light, float maxIntensity)
    {
        light.intensity = maxIntensity;
    }

    private void FlickerOff(Light light, float maxIntensity)
    {
        if (!reversed && !off)
            light.intensity = Random.Range(0, maxIntensity / 3);
        else
            light.intensity = 0;
    }
}
