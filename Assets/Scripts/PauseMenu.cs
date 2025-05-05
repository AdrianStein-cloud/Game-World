using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject menu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] SliderSetting gammaSetting;
    [SerializeField] Button resumeButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button backButton;
    [SerializeField] Image background;

    PlayerInput playerInput;
    bool isPaused = true;

    private void Start()
    {
        gammaSetting.Init(PostProcessingHandler.GetEffect<LiftGammaGain>().gamma.value[3], SetGamma);

        playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.actions["Pause"].started += _ => Pause();
        playerInput.actions["Resume"].started += _ => Resume();

        exitButton.onClick.AddListener(BackToMainMenu);
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        backButton.onClick.AddListener(CloseOptions);

        Resume();
    }

    private void SetGamma(float value)
    {
        PostProcessingHandler.GetEffect<LiftGammaGain>().gamma.value = new Vector4(1f,1f,1f,value / 10);
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        playerInput.SwitchCurrentActionMap("UI");
        pauseMenu.SetActive(true);
        menu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        CloseOptions();
    }

    private void OpenOptions()
    {
        menu.SetActive(false);
        optionsMenu.SetActive(true);
        background.gameObject.SetActive(false);
    }

    private void CloseOptions()
    {
        menu.SetActive(true);
        optionsMenu.SetActive(false);
        background.gameObject.SetActive(true);
    }

    private void Resume()
    {
        if (!isPaused) return;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
        playerInput.SwitchCurrentActionMap("Player");
        pauseMenu.SetActive(false);
        Cursor.visible = false;
    }
}
