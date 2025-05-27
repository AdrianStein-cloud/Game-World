using System.Collections.Generic;
using UnityEngine;
public class ZombieSound : MonoBehaviour
{
    [SerializeField] AudioSource footstepSource, otherSource;

    [SerializeField] List<AudioClip> footsteps;
    [SerializeField] List<AudioClip> idles;
    [SerializeField] AudioClip bite, chase, slash;
    [SerializeField] PlaySound freakout;

    float lastFootstep;
    float minFootstepDelay = 0.15f;

    float lastIdle;
    float idleDelay = 7f;

    float lastFreakout;
    Vector2 freakoutDelay = new Vector2(2f, 5f);

    public void PlayFootstepSound()
    {
        if (Time.time < lastFootstep + minFootstepDelay) return;

        lastFootstep = Time.time;
        footstepSource.clip = footsteps[Random.Range(0, footsteps.Count)];
        footstepSource.pitch = Random.Range(0.85f, 1.15f);
        footstepSource.Play();
    }

    public void PlaySound(EnemySound enemySound)
    {
        AudioClip clip = null;
        //Debug.Log("Playing sound: " + enemySound.ToString());
        switch (enemySound)
        {
            case EnemySound.Freakout:
                if (Time.time < lastFreakout + Random.Range(freakoutDelay.x, freakoutDelay.y)) return;
                otherSource.Stop();
                freakout.PlayAudio();
                return;
            case EnemySound.Bite:
                clip = bite;
                break;
            case EnemySound.Idle:
                if (otherSource.clip != null && Time.time < lastIdle + idleDelay) return;
                clip = idles[Random.Range(0, idles.Count)];
                break;
            case EnemySound.Chase:
                clip = chase;
                break;
            case EnemySound.Slash:
                clip = slash;
                break;
        }

        if (enemySound is EnemySound.Idle or EnemySound.Chase) otherSource.loop = true;
        else otherSource.loop = false;

        PlaySound(clip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (otherSource.clip == clip) return;

        otherSource.pitch = Random.Range(0.8f, 1.2f);
        otherSource.clip = clip;
        otherSource.Play();
    }
}

public enum EnemySound
{
    Freakout,
    Bite,
    Idle,
    Chase,
    Slash
}
