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
    Transform player;
    CancellationTokenSource tokenSource;

    private void Awake()
    {
        tokenSource = new();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Interact()
    {
        if (isLocked) return;
        if (isOpen) Close();
        else Open();
        isOpen = !isOpen;
    }

    private void Close()
    {
        Move(0f);
    }

    private void Open()
    {
        float doorAngle = DetermineOpeningDirection();
        Move(doorAngle);
    }

    private float DetermineOpeningDirection()
    {
        if (player == null) return angle;
        Vector3 doorToPlayer = player.position - door.transform.position;
        float dotProduct = Vector3.Dot(transform.right, doorToPlayer.normalized);
        Debug.Log(dotProduct);
        return dotProduct > 0 ? -angle : angle;
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

            if (startAngle > 180f)
                startAngle -= 360f;

            // Choose the shortest path to the target angle
            if (isOpen && Math.Abs(startAngle - angle) > 180f)
            {
                if (startAngle > angle)
                    startAngle -= 360f;
                else
                    startAngle += 360f;
            }

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
