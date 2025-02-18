using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] bool enable = true;
    [SerializeField] float stamina;
    [SerializeField] float drainSpeed;
    [SerializeField] float jumpStaminaUse;
    [SerializeField] float recoverySpeed;
    [SerializeField] float recoveryDelay;
    [SerializeField] float recoveryDelayAtZero;

    public float MaxStamina => stamina;
    public float CurrentStamina { get; private set; }
    public bool FullStamina => CurrentStamina == MaxStamina;
    public bool SufficientStamina => CurrentStamina > 0 || !enable;

    public Action OnChange { get; set; }

    PlayerMovement movement;
    float lastRecoveryTime;

    private void Awake()
    {
        CurrentStamina = stamina;

        if (!enable) return;

        movement = GetComponent<PlayerMovement>();
        OnChange += () =>
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
            if (!SufficientStamina) movement.StopRun();
        };

        movement.OnUpdate += UpdateStamina;

        movement.OnJump += () =>
        {
            if (enable)
            {
                CurrentStamina -= jumpStaminaUse;
                lastRecoveryTime = Time.time;
                OnChange?.Invoke();
            }
        };
    }

    private void UpdateStamina()
    {
        if (enable && movement.IsRunning)
        {
            CurrentStamina -= drainSpeed * Time.deltaTime;
            lastRecoveryTime = Time.time;
            OnChange?.Invoke();
        }
        else if (!FullStamina && Time.time > lastRecoveryTime + (SufficientStamina ? recoveryDelay : recoveryDelayAtZero) && !movement.IsSliding && movement.IsGrounded)
        {
            CurrentStamina += recoverySpeed * Time.deltaTime;
            OnChange?.Invoke();
        }
    }
}
