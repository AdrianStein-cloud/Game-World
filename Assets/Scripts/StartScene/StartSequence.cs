using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StartSequence : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool runStartSequence = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip powerOutageSound;
    [SerializeField] private AudioClip powerOutageAIAnnouncement;
    [SerializeField] private AudioClip startupSound;

    [Header("UI Elements")]
    [SerializeField] private Image blackOverlay;


    public void Start()
    {

        if (runStartSequence)
        {
            blackOverlay.gameObject.SetActive(true);
            InputManager.InputActions.Player.Disable();
            InputManager.InputActions.UI.Enable();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(WakeUpFailSafe());
        }
        else blackOverlay.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        blackOverlay.gameObject.SetActive(false);
    }

    private IEnumerator WakeUpFailSafe()
    {
        AudioSource.PlayClipAtPoint(powerOutageSound, Vector3.zero);
        yield return new WaitForSeconds(3);
        AudioSource.PlayClipAtPoint(powerOutageAIAnnouncement, Vector3.zero);
        yield return new WaitForSeconds(3);
        AudioSource.PlayClipAtPoint(startupSound, Vector3.zero);
        yield return new WaitForSeconds(3);

        InputManager.InputActions.Player.Enable();
        InputManager.InputActions.UI.Disable();

        float duration = 2f;
        while (duration > 0)
        {
            blackOverlay.color = Color.Lerp(new Color(0,0,0,1), new Color(0,0,0,0), (2f - duration) / duration);
            duration -= Time.deltaTime;
            yield return null;
        }
        blackOverlay.gameObject.SetActive(false);
    }

}
