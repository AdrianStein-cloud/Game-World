using UnityEngine;

public class TimelineAction : MonoBehaviour
{
    [SerializeField] private float fadeInMultiplier = 1f;
    [SerializeField] private float fadeOutMultiplier = 1f;

    public void SceneTransition(int sceneIndex)
    {
        Initiate.Fade(sceneIndex, Color.black, fadeInMultiplier, fadeOutMultiplier);
    }
}
