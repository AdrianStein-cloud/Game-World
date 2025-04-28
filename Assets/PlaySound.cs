using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool playOnAwake, playOnTrigger, playOnCollision, once;
    bool played;

    private void Awake()
    {
        if (playOnAwake) PlayAudio();
    }

    public void PlayAudio()
    {
        if (once && played) return;

        played = true;
        audioSource.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnTrigger) audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playOnCollision) audioSource.Play();
    }
}
