using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] SliderSetting gammaSetting;

    PlayerInput playerInput;
    bool isPaused = true;

    private void Start()
    {
        gammaSetting.Init(PostProcessingHandler.GetEffect<LiftGammaGain>().gamma.value[3], SetGamma);

        playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.actions["Pause"].started += _ => Pause();
        playerInput.actions["Resume"].started += _ => Resume();

        Resume();
    }

    private void SetGamma(float value)
    {
        PostProcessingHandler.GetEffect<LiftGammaGain>().gamma.value = new Vector4(1f,1f,1f,value / 10);
    }

    private void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        playerInput.SwitchCurrentActionMap("UI");
        menu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        playerInput.SwitchCurrentActionMap("Player");
        menu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
