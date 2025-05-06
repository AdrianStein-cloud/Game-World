using UnityEngine;

[CreateAssetMenu(fileName = "New hacking information", menuName = "Hacking Information")]
public class HackingInformation : ScriptableObject
{
    public string Name;

    public string[] HackedInformation;
    public AudioClip[] HackedSoundClips;
}
