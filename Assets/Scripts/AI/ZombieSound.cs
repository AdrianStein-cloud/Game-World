using System.Collections.Generic;
using UnityEngine;
public class ZombieSound : MonoBehaviour
{
    [SerializeField] List<AudioClip> _sounds;
    PlaySound _playSound;
    void Start()
    {
    }
    void Awake()
    {
        _playSound = GetComponent<PlaySound>();
    }
    void PlayFromList(string soundName){
        var (index,range) = SoundMap(soundName);
        Debug.Log("playing sound no: " + index + ", with a range of: " + range);
        //_playSound.PlayNewSound(_sounds[index], range);
    }
    (int,float) SoundMap(string soundName){
        switch (soundName)
        {   case "footStep":
                return (0,0);
            
            default:
            return (0,0);
        }
    }
}
