using UnityEngine;
using UniversalForwardPlusVolumetric;

public class PlaySound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool playOnAwake, playOnTrigger, playOnCollision, once;
    bool played;

    [Header("Zombie sound settings")]
    [SerializeField] bool zombiesCanHear;
    [SerializeField] float audioRange;
    [SerializeField] LayerMask zombieLayer;

    private void Awake()
    {
        if (playOnAwake) PlayAudio();
    }

    public void PlayNewSound(AudioClip clip, float range = 0){
        audioSource.clip = clip;
        audioRange = range;
        PlayAudio();
    }

    public void PlayAudio()
    {
        if (once && played) return;

        played = true;
        audioSource.Play();

        if (zombiesCanHear)
        {
            var zombiesInRange = Physics.SphereCastAll(transform.position, audioRange, Vector3.up, Mathf.Infinity, zombieLayer);
            foreach (var zombie in zombiesInRange)
            {
                if (zombie.transform.gameObject.TryGetComponent(out ZomibeAI ai))
                {
                    ai.HearNoise(transform.position);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnTrigger) audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playOnCollision) audioSource.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (zombiesCanHear) Gizmos.DrawWireSphere(transform.position, audioRange);
    }
}
