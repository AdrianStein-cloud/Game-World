using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance {get; private set;}
    [SerializeField] private List<ZomibeAI> zombies = new List<ZomibeAI>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Register(ZomibeAI zai){
        if (!zombies.Contains(zai)) zombies.Add(zai);
    }
    public void NudgeAllZombies(ZomibeAI zai){
        foreach (var zombie in zombies)
        {
            if (zombie != zai) zombie.Strike();
        }
    }
    public List<ZomibeAI> GetZomibes() => zombies;
}
