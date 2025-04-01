using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] SliderSetting brightnessSetting;

    PlayerInput playerInput;
    bool isPaused = true;

    private void Start()
    {
        brightnessSetting.Init(PostProcessingHandler.GetEffect<ColorAdjustments>().postExposure.value, SetBrightness);

        playerInput = FindFirstObjectByType<PlayerInput>();
        playerInput.actions["Pause"].started += _ => Pause();
        playerInput.actions["Resume"].started += _ => Resume();

        Resume();
    }

    private void SetBrightness(float value)
    {
        PostProcessingHandler.GetEffect<ColorAdjustments>().postExposure.value = value;
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
