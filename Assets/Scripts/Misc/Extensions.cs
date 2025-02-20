using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public static class Extensions
{
    public static void SubscribeToAllActions(this InputAction inputAction, Action<InputAction.CallbackContext> callback)
    {
        inputAction.started += callback;
        inputAction.performed += callback;
        inputAction.canceled += callback;
    }

    public static async void Lerp(this VolumeParameter<float> parameter, float smoothTime, float targetValue)
        => await Lerp(parameter, smoothTime, targetValue, Mathf.Lerp);

    public static async void Lerp(this VolumeParameter<Vector2> parameter, float smoothTime, Vector2 targetValue)
        => await Lerp(parameter, smoothTime, targetValue, Vector2.Lerp);

    public static async void Lerp(this VolumeParameter<Color> parameter, float smoothTime, Color targetValue)
        => await Lerp(parameter, smoothTime, targetValue, Color.Lerp);

    private static async Awaitable Lerp<T>(VolumeParameter<T> parameter, float smoothTime, T targetValue, Func<T, T, float, T> lerpFunc)
    {
        var elapsedTime = 0f;
        var startValue = parameter.value;

        while (elapsedTime < smoothTime)
        {
            parameter.value = lerpFunc(startValue, targetValue, elapsedTime / smoothTime);
            elapsedTime += Time.unscaledDeltaTime;
            await Awaitable.NextFrameAsync();
        }

        parameter.value = targetValue;
    }
}
