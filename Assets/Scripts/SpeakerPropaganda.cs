using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerPropaganda : MonoBehaviour
{
    [SerializeField] private List<AudioClip> propagandaVoiceLines = new List<AudioClip>();
    [SerializeField] private float minTimeBetweenPropaganda = 10f;
    [SerializeField] private float maxTimeBetweenPropaganda = 30f;

    // static RNG and event so all speakers share one timer and one pick
    private static System.Random rnd = new System.Random();
    private static event System.Action<AudioClip> OnPlayClip;
    private static bool timerStarted = false;

    void Awake()
    {
        // every speaker subscribes its PlayVoiceLine handler
        OnPlayClip += PlayVoiceLine;

        // only the first Awake kicks off the coroutine
        if (!timerStarted)
        {
            timerStarted = true;
            StartCoroutine(PropagandaRoutine());
        }
    }

    void OnDestroy()
    {
        // clean up subscription
        OnPlayClip -= PlayVoiceLine;
    }

    private IEnumerator PropagandaRoutine()
    {
        while (true)
        {
            // wait a random interval
            float delay = Random.Range(minTimeBetweenPropaganda, maxTimeBetweenPropaganda);
            yield return new WaitForSeconds(delay);

            // pick a random clip
            if (propagandaVoiceLines.Count == 0)
                continue;

            int idx = rnd.Next(0, propagandaVoiceLines.Count);
            AudioClip clip = propagandaVoiceLines[idx];

            // broadcast to all subscribers
            OnPlayClip?.Invoke(clip);
        }
    }

    private void PlayVoiceLine(AudioClip clip)
    {
        // each speaker plays the clip at its own position
        AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
