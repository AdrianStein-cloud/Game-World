using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : IDisposable
{
    public static InputSystem_Actions InputActions { get; private set; }

    public static InputSystem_Actions.PlayerActions Player => InputActions.Player;

    public InputManager()
    {
        InputActions = new InputSystem_Actions();
        InputActions.Enable();
    }

    public void Dispose()
    {
        InputActions.Disable();
        InputActions.Dispose();
    }
}
