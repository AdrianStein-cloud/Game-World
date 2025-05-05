using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private int sceneToLoadIndex = 1;
    [SerializeField] private float fadeInMultiplier = 1.0f; // Fade in duration
    [SerializeField] private float fadeOutMultiplier = 1.0f; // Fade out duration


    private void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void StartGame()
    {
        Initiate.Fade(sceneToLoadIndex, Color.black, fadeInMultiplier, fadeOutMultiplier);
    }

    private void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
