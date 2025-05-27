using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalPanel : MonoBehaviour, ILockChecker
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip switchSoundEffect;
    [SerializeField] private AudioClip poweringUpSound;
    [SerializeField] private AudioClip ventFallingSound;
    [SerializeField] private AudioClip aiVoicePowerRestored;
    [Header("Other")]
    [SerializeField] Item item;
    [SerializeField] Transform ventSoundOrigin;
    public bool IsLocked()
    {
        return !PlayerSimpleInventory.Instance.ContainsItem(item);
    }

    public void LockedInteract()
    {
    }

    public void TurnOn()
    {
        LightFlicker.LightsOn = true;
        StartCoroutine(PlaySounds());
    }

    private IEnumerator PlaySounds()
    {
        AudioSource.PlayClipAtPoint(switchSoundEffect, transform.position);
        yield return new WaitForSeconds(0.5f);
        AudioSource.PlayClipAtPoint(aiVoicePowerRestored, transform.position, 0.5f);
        AudioSource.PlayClipAtPoint(poweringUpSound, transform.position, 0.5f);
        yield return new WaitForSeconds(0.5f);
        AudioSource.PlayClipAtPoint(ventFallingSound, ventSoundOrigin.position, 1f);
    }
}
