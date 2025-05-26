using UnityEngine;

[CreateAssetMenu(fileName = "New hacking information", menuName = "Hacking Information")]
public class HackingInformation : ScriptableObject
{
    public string Name;

    public int Age;
    public BloodType Bloodtype;
    public string[] HackedInformation;
    public AudioClip HackedSoundClip;
    public string HackedSoundName;
}

public enum BloodType
{
    A_Pos,
    A_Neg,
    B_Pos,
    B_Neg,
    AB_Pos,
    AB_Neg,
    O_Pos,
    O_Neg
}