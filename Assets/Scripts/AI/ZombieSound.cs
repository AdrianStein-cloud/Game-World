using System.Collections.Generic;
using UnityEngine;
public class ZombieSound : MonoBehaviour
{
    [SerializeField] List<AudioClip> _sounds;
    PlaySound _playSound;
    private float _timer;
    private float _targetTime;
    void Start()
    {
    }
    void Awake()
    {
        _playSound = GetComponent<PlaySound>();
    }
    void Update()
    {
        _timer += Time.deltaTime;
    }
    public void PlayFromList(string soundName){
        if (_timer < _targetTime) return;
        var (index,range,delay) = SoundMap(soundName);
        if (index == 0 && range == 0 && delay == 0) return;
        Debug.Log("playing sound no: " + index + ", with a range of: " + range);
        _playSound.PlayNewSound(_sounds[index], range);
        _timer = 0;
        _targetTime = delay;
    }
    (int,float,float) SoundMap(string soundName){
        switch (soundName)
        {   case "footStep":
                return (0,0,0);
            case "freak":
                return (0, 15, 0);
            default:
            return (0,0,0);
        }
    }
}
