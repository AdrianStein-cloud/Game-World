using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] bool hideMouseOnStart = true;
    [SerializeField] Assignables assignables;
    [SerializeField] FOVProperties fovProperties;
    [SerializeField] NoiseProperties noiseProperties;
    [SerializeField] LeanProperties leanProperties;
    [SerializeField] QuickturnProperties quickturnProperties;
    [SerializeField] ZoomProperties zoomProperties;
    [SerializeField] FreelookProperties freelookProperties;

    [Serializable]
    private class Assignables
    {
        public Transform CameraPosition;
        public PlayerMovement Player;
        public CinemachineVirtualCameraBase VirtualCamera;
    }

    [Serializable]
    private class FOVProperties
    {
        public bool Enable;
        public FOVState DefaultFOV;
        public FOVState RunFOV;
        public FOVState SlideFOV;
    }

    [Serializable]
    private class FOVState
    {
        public float Multiplier;
        public float ChangeSpeed;
    }

    [Serializable]
    private class NoiseProperties
    {
        public bool Enable;
        public Noise DefaultNoise;
        public Noise CrouchNoise;
        public Noise WalkNoise;
        public Noise RunNoise;
        public LandingNoise LandingNoise;
    }

    [Serializable]
    private class Noise
    {
        public float AmplituteGain;
        public float FrequencyGain;
        public float ChangeSpeed;
    }

    [Serializable]
    private class TimedNoise : Noise
    {
        public float Duration;
    }

    [Serializable]
    private class LandingNoise : TimedNoise
    {
        public float SpeedThreshold;
    }

    [Serializable]
    private class LeanProperties
    {
        public bool Enable;
        public InputMode Mode;
        public float Angle;
        public float Speed;
        public float Offset;
    }

    [Serializable]
    private class QuickturnProperties
    {
        public bool Enable;
        public TiltMode TiltMode;
        public float Speed;
        public float Cooldown;
    }

    [Serializable]
    private class ZoomProperties
    {
        public bool Enable;
        public float Multiplier;
        public float Speed;
        public float SensitivityMultiplier;
    }

    [Serializable]
    private class FreelookProperties
    {
        public bool Enable;
        public float ReturnSpeed;
        [Range(0f, 360f)] public float Range;
    }

    public bool Quickturning { get; private set; }
    public float Sensitivity
    {
        get => xAxis.Gain;
        set
        {
            xAxis.Gain = value;
            yAxis.Gain = -value;
        }
    }

    public InputMode LeanMode
    {
        get => leanProperties.Mode;
        set => leanProperties.Mode = value;
    }

    public TiltMode QuickturnTiltMode
    {
        get => quickturnProperties.TiltMode;
        set => quickturnProperties.TiltMode = value;
    }

    public bool Bobbing
    {
        get => noiseProperties.Enable;
        set => noiseProperties.Enable = value;
    }

    CinemachineRecomposer recomposer;
    CinemachinePanTilt panTilt;
    CinemachineBasicMultiChannelPerlin noise;
    CinemachineInputAxisController inputAxis;

    CinemachineInputAxisController.Reader xAxis;
    CinemachineInputAxisController.Reader yAxis;

    FOVState currentFOVState;
    Noise currentNoiseState;
    List<(FOVState, Func<bool>)> fovStates;
    List<(Noise, Func<bool>)> noiseStates;

    Vector2 panRange;
    float panValue;

    bool leaningLeft;
    bool leaningRight;
    bool readyToQuickturn;
    bool noisePlaying;
    bool zooming;
    bool zoomed;
    bool freelooking;
    bool freelooked;

    private void Awake()
    {
        Application.targetFrameRate = 144;

        var vc = assignables.VirtualCamera;
        recomposer = vc.GetComponent<CinemachineRecomposer>();
        panTilt = vc.GetComponent<CinemachinePanTilt>();
        noise = vc.GetComponent<CinemachineBasicMultiChannelPerlin>();
        inputAxis = vc.GetComponent<CinemachineInputAxisController>();
        xAxis = inputAxis.Controllers[0].Input;
        yAxis = inputAxis.Controllers[1].Input;

        if (hideMouseOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        readyToQuickturn = true;
        panRange = panTilt.PanAxis.Range;
        
        var player = assignables.Player;

        fovStates = new()
        {
            (fovProperties.RunFOV, () => player.IsRunning || player.IsWallrunning),
            (fovProperties.SlideFOV, () => player.IsSliding),
            (fovProperties.DefaultFOV, () => true),
        };

        noiseStates = new()
        {
            (noiseProperties.LandingNoise, () => player.DownVelocity < -noiseProperties.LandingNoise.SpeedThreshold && player.IsGrounded && !noisePlaying),
            (noiseProperties.WalkNoise, () => player.IsCrouching && player.IsWalking),
            (noiseProperties.WalkNoise, () => player.IsWalking),
            (noiseProperties.RunNoise, () => player.IsRunning || player.IsWallrunning),
            (noiseProperties.DefaultNoise, () => true),
        };
    }

    private void Start()
    {
        var playerInputs = InputManager.Player;
        playerInputs.LeanLeft.SubscribeToAllActions(LeanLeft);
        playerInputs.LeanRight.SubscribeToAllActions(LeanRight);
        playerInputs.Quickturn.SubscribeToAllActions(Quickturn);
        playerInputs.Zoom.SubscribeToAllActions(Zoom);
        playerInputs.Freelook.SubscribeToAllActions(Freelook);
    }

    public void LeanLeft(InputAction.CallbackContext context)
    {
        Lean(context, ToggleLeanLeft, () => leaningRight);
    }

    private void ToggleLeanLeft()
    {
        leaningLeft = !leaningLeft;
        if (leaningLeft) LerpLean(-leanProperties.Offset, leanProperties.Angle);
        else LerpLean(0f, 0f);
        leaningRight = false;
    }

    public void LeanRight(InputAction.CallbackContext context)
    {
        Lean(context, ToggleLeanRight, () => leaningLeft);
    }

    private void ToggleLeanRight()
    {
        leaningRight = !leaningRight;
        if (leaningRight) LerpLean(leanProperties.Offset, -leanProperties.Angle);
        else LerpLean(0f, 0f);
        leaningLeft = false;
    }

    private void Lean(InputAction.CallbackContext context, Action lean, Func<bool> leaning)
    {
        if (!leanProperties.Enable || !assignables.Player.IsGrounded) return;
        if (context.started)
        {
            if (leanProperties.Mode == InputMode.Hold) StopLeaning();
            lean();
        }
        else if (context.canceled && leanProperties.Mode == InputMode.Hold && !leaning()) StopLeaning();
    }

    private void StopLeaning()
    {
        LerpLean(0f, 0f);
        leaningLeft = false;
        leaningRight = false;
    }

    public async void LerpLean(float positionValue, float rotationValue)
    {
        float elapsedTime = 0;
        var startPositionX = assignables.CameraPosition.localPosition.x;
        var startRotationZ = recomposer.Dutch;
        startRotationZ = startRotationZ > 180 ? -(360 - startRotationZ) : startRotationZ;
        var duration = 1 / leanProperties.Speed;

        while (elapsedTime < duration)
        {
            var pos = assignables.CameraPosition.localPosition;
            pos.x = Mathf.Lerp(startPositionX, positionValue, elapsedTime / duration);
            assignables.CameraPosition.localPosition = pos;

            recomposer.Dutch = Mathf.Lerp(startRotationZ, rotationValue, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        recomposer.Dutch = rotationValue;
        var position = assignables.CameraPosition.localPosition;
        position.x = positionValue;
        assignables.CameraPosition.localPosition = position;
    }

    public void Quickturn(InputAction.CallbackContext context)
    {
        if (!context.started || Quickturning || !readyToQuickturn) return;
        LerpQuickturn();
    }

    private async void LerpQuickturn()
    {
        Quickturning = true;
        readyToQuickturn = false;

        float elapsedTime = 0f;
        var duration = 1 / quickturnProperties.Speed;

        float startTilt = panTilt.TiltAxis.Value;
        var targetTilt = quickturnProperties.TiltMode switch
        {
            TiltMode.Reset => 0f,
            TiltMode.Invert => -startTilt,
            TiltMode.Dynamic => assignables.Player.OnSlope() ? -startTilt : 0f,
            _ => 0f,
        };

        float startPan = panTilt.PanAxis.Value;
        float targetPan = startPan + 180f;

        while (elapsedTime < duration)
        {
            if (quickturnProperties.TiltMode != TiltMode.None) panTilt.TiltAxis.Value = Mathf.Lerp(startTilt, targetTilt, elapsedTime / duration);
            panTilt.PanAxis.Value = Mathf.Lerp(startPan, targetPan, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            await Awaitable.NextFrameAsync();
        }

        if (quickturnProperties.TiltMode != TiltMode.None) panTilt.TiltAxis.Value = targetTilt;
        panTilt.PanAxis.Value = targetPan;

        Quickturning = false;

        await Awaitable.WaitForSecondsAsync(quickturnProperties.Cooldown);
        readyToQuickturn = true;
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        if (!zoomProperties.Enable || context.started) return;
        zoomed = context.performed;
        LerpZoom(zoomed ? zoomProperties.Multiplier : 1f);
    }

    private async void LerpZoom(float zoomScale)
    {
        zooming = true;
        var multiplier = zoomProperties.SensitivityMultiplier;
        multiplier = zoomed ? multiplier : 1 / multiplier;
        xAxis.Gain *= multiplier;
        yAxis.Gain *= multiplier;

        float elapsedTime = 0f;
        float startZoom = recomposer.ZoomScale;
        var duration = 1 / zoomProperties.Speed;

        while (elapsedTime < duration)
        {
            recomposer.ZoomScale = Mathf.Lerp(startZoom, zoomScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            await Awaitable.NextFrameAsync();
        }

        recomposer.ZoomScale = zoomScale;
        zooming = false;
    }

    public void Freelook(InputAction.CallbackContext context)
    {
        if (!freelookProperties.Enable || context.started) return;
        freelooked = context.performed;

        if (freelooked) StartFreelook();
        else StopFreelook();
    }

    private void StartFreelook()
    {
        freelooking = true;
        var angle = freelookProperties.Range / 2f;
        panValue = panTilt.PanAxis.Value;
        panTilt.PanAxis.Range = new Vector2(panValue - angle, panValue + angle);
        panTilt.PanAxis.Wrap = false;
    }

    private async void StopFreelook()
    {
        float elapsedTime = 0f;
        var duration = 1 / freelookProperties.ReturnSpeed;

        float startTilt = panTilt.TiltAxis.Value;
        var targetTilt = 0f;

        float startPan = panTilt.PanAxis.Value;
        float targetPan = panValue;

        while (elapsedTime < duration)
        {
            panTilt.TiltAxis.Value = Mathf.Lerp(startTilt, targetTilt, elapsedTime / duration);
            panTilt.PanAxis.Value = Mathf.Lerp(startPan, targetPan, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            await Awaitable.NextFrameAsync();
        }

        panTilt.TiltAxis.Value = targetTilt;
        panTilt.PanAxis.Value = targetPan;

        panTilt.PanAxis.Range = panRange;
        panTilt.PanAxis.Wrap = true;

        freelooking = false;
    }

    public void SetRange(float range)
    {
        var angle = range / 2f;
        panValue = panTilt.PanAxis.Value;
        panTilt.PanAxis.Range = new Vector2(panValue - angle, panValue + angle);
        panTilt.PanAxis.Wrap = false;
    }

    public void ResetRange()
    {
        panTilt.PanAxis.Range = panRange;
        panTilt.PanAxis.Wrap = true;
    }

    private void UpdateFOV()
    {
        if (zooming || zoomed) return;
        if (!fovProperties.Enable)
        {
            recomposer.ZoomScale = 1f;
            return;
        }
        currentFOVState = fovStates.FirstOrDefault(s => s.Item2()).Item1;
        recomposer.ZoomScale = Mathf.Lerp(recomposer.ZoomScale, currentFOVState.Multiplier, currentFOVState.ChangeSpeed * Time.deltaTime);
    }

    private void UpdateNoise()
    {
        if (!noiseProperties.Enable)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
            return;
        }

        var state = noiseStates.FirstOrDefault(s => s.Item2()).Item1;

        if (noisePlaying || state == currentNoiseState)
        {
            noise.AmplitudeGain = Mathf.Lerp(noise.AmplitudeGain, currentNoiseState.AmplituteGain, currentNoiseState.ChangeSpeed * Time.deltaTime);
            noise.FrequencyGain = Mathf.Lerp(noise.FrequencyGain, currentNoiseState.FrequencyGain, currentNoiseState.ChangeSpeed * Time.deltaTime);
            return;
        }

        currentNoiseState = state;

        if (currentNoiseState is TimedNoise tn && tn.Duration > 0)
        {
            noisePlaying = true;
            StopNoise(tn);
        }
    }

    private async void StopNoise(TimedNoise noise)
    {
        await Awaitable.WaitForSecondsAsync(noise.Duration);
        noisePlaying = false;
    }

    private void Update()
    {
        UpdateFOV();
        UpdateNoise();

        if (assignables.Player.IsRunning && (leaningLeft || leaningRight)) StopLeaning();

        if (freelooking) return;
        var cameraForward = transform.forward;
        cameraForward.y = 0;
        if (cameraForward == Vector3.zero) cameraForward = Vector3.forward;
        assignables.Player.transform.rotation = Quaternion.LookRotation(cameraForward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (leanProperties.Enable && !other.gameObject.CompareTag("Player"))
        {
            if (leaningLeft || leaningRight) StopLeaning();
        }
    }
}

public enum TiltMode { None, Reset, Invert, Dynamic }

