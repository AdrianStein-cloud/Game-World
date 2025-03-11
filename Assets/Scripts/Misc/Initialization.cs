using UnityEngine;
using UnityEngine.Rendering;

public class Initialization : MonoBehaviour
{
    bool isInitialized;

    private void Awake()
    {
        if (isInitialized)
            return;
        isInitialized = true;

        new InputManager();
        new PostProcessingHandler(GetComponentInChildren<Volume>());
    }
}