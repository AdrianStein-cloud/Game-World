using Mono.Cecil;
using System;
using System.Threading;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private GameObject door;
    [SerializeField] private float angle;
    [SerializeField] private float duration;

    bool isOpen = false;
    bool doorMoving = false;
    CancellationTokenSource tokenSource;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isOpen)
            {
                LightFlicker.LightsOn = false;
                Close();
            }
            else
            {
                LightFlicker.LightsOn = true;
                Open();
            }
            isOpen = !isOpen;
        }
    }

    private void Awake()
    {
        tokenSource = new();
    }

    private void Close()
    {
        Move(0f);
    }

    private void Open()
    {
        Move(angle);
    }

    private async void Move(float angle)
    {
        if (doorMoving)
            return;

        doorMoving = true;

        try
        {
            var timeElapsed = 0f;
            var startAngle = door.transform.localRotation.eulerAngles.y;

            while (timeElapsed < duration && !tokenSource.IsCancellationRequested)
            {
                door.transform.localRotation = Quaternion.Euler(0, Mathf.Lerp(startAngle, angle, timeElapsed / duration), 0);
                timeElapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(tokenSource.Token);
            }
        }
        catch (OperationCanceledException) { }

        door.transform.localRotation = Quaternion.Euler(0, angle, 0);
        doorMoving = false;
    }

    private void OnDestroy()
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }
}
