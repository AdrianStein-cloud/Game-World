using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class FogTrigger : MonoBehaviour
{
    [SerializeField] private float startDelay;
    [SerializeField] private float duration;
    [SerializeField] private float spawnrate;

    private VisualEffect fog;

    private void Awake()
    {
        fog = GetComponent<VisualEffect>();
    }

    public void Trigger()
    {
        StartCoroutine(OnTrigger());
    }

    IEnumerator OnTrigger()
    {
        yield return new WaitForSeconds(startDelay);
        fog.gameObject.SetActive(true);
        fog.SetFloat("Spawn Rate", spawnrate);

        yield return new WaitForSeconds(duration);
        fog.SetFloat("Spawn Rate", 0f);

        yield return new WaitForSeconds(fog.GetFloat("Life"));
        Destroy(gameObject);
    }
}
