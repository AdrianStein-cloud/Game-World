using System.Collections.Generic;
using UnityEngine;
public class ZombieSound : MonoBehaviour
{
    [SerializeField] AudioSource footstepSource, otherSource;

    [SerializeField] List<AudioClip> footsteps;
    [SerializeField] AudioClip bite, idle, chase, slash;
    [SerializeField] PlaySound freakout;

    float lastFootstep;
    float minFootstepDelay = 0.15f;

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
        switch (enemySound)
        {
            case EnemySound.Freakout:
                otherSource.Stop();
                freakout.PlayAudio();
                return;
            case EnemySound.Bite:
                clip = bite;
                break;
            case EnemySound.Idle:
                clip = idle;
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

        footstepSource.pitch = Random.Range(0.9f, 1.1f);
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
