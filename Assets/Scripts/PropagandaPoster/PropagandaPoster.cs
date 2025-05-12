using System.Collections.Generic;
using UnityEngine;

public class PropagandaPoster : MonoBehaviour
{

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] List<Material> posters;

    static System.Random random;

    void Start()
    {
        if(random == null) random = new System.Random();

        meshRenderer.material = posters[random.Next(0, posters.Count)];
    }
}
