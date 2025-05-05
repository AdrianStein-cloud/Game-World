using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad = 2; // The index of the scene to load
    [SerializeField] private float fadeInMultiplier = 5.0f; // Fade in duration
    [SerializeField] private float fadeOutMultiplier = 1.0f; // Fade out duration

    public void ChangeScene()
    {
        // Assuming you have a method to load the scene by index
        Initiate.Fade(sceneIndexToLoad, Color.black, fadeInMultiplier, fadeOutMultiplier);
    }
}
