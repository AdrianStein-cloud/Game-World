using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalPanel : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip switchSoundEffect;
    [SerializeField] private AudioClip poweringUpSound;

    public void TurnOn()
    {
        LightFlicker.LightsOn = true;
        StartCoroutine(PlaySounds());
    }

    private IEnumerator PlaySounds()
    {
        AudioSource.PlayClipAtPoint(switchSoundEffect, transform.position);
        yield return new WaitForSeconds(0.5f);
        AudioSource.PlayClipAtPoint(poweringUpSound, transform.position, 0.5f);
    }
}
