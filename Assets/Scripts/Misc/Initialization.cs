using UnityEngine;
using UnityEngine.Rendering;

public class Initialization : MonoBehaviour
{
    private void Awake()
    {
        new InputManager();
        new PostProcessingHandler(GetComponentInChildren<Volume>());
    }
}