using UnityEngine;

public class ThrowableSound : MonoBehaviour
{
    [SerializeField] float velocityThreshold = 10f;  // Minimum velocity required to trigger the sound
    private Rigidbody rb;

    float lastSoundTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        float currentVelocity = rb.linearVelocity.magnitude;

        if (currentVelocity > velocityThreshold && Time.time > lastSoundTime + 0.35f)
        {
            lastSoundTime = Time.time;
            PlayCollisionSound();
        }
    }

    private void PlayCollisionSound()
    {
        GetComponent<PlaySound>().PlayAudio();
    }
}
