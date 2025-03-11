using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Assignables assignables;
    [SerializeField] MovementProperties movementProperties;
    [SerializeField] RunProperties runProperties;
    [SerializeField] JumpProperties jumpProperties;
    [SerializeField] CrouchProperties crouchProperties;
    [SerializeField] SlideProperties slideProperties;
    [SerializeField] SlopeProperties slopeProperties;
    [SerializeField] WallProperties wallProperties;
    [SerializeField] WallrunningProperties wallrunningProperties;
    [SerializeField] WallclimbingProperties wallclimbingProperties;
    [SerializeField] DashProperties dashProperties;
    [SerializeField] GravityProperties gravityProperties;

    [Serializable]
    private class Assignables
    {
        public Transform Head;
        public Transform GroundCheck;
        public Transform Model;
        public CinemachineController CameraController;
    }

    [Serializable]
    private class MovementProperties
    {
        public float WalkSpeed;
        public float Acceleration;
        public float Deacceleration;
        public float AddedForceDeacceleration;
        public float PushForce;
        public bool ApplyCurrentSpeedToObjects;
    }

    [Serializable]
    private class RunProperties
    {
        public bool Enable = true;
        public RunMode Mode;
        public bool AllowRunningInAllDirections;
        public float ForwardSpeed;
        public float StrafeSpeed;
        [HideInInspector] public float BackwardSpeed;
    }

    [Serializable]
    private class CrouchProperties
    {
        public bool Enable = true;
        public bool CanCrouchInAir;
        public InputMode Mode;
        public float Speed;
        public float Height;
        public float CameraOffset;
        public float SmoothTime;
        public float CeilingCheckDistance;
        public LayerMask CeilingCheckMask;
    }

    [Serializable]
    private class SlideProperties
    {
        public bool Enable;
        public bool UseSeperateKey;
        public float Force;
        public float StrafeSpeedMultiplier;
        public float TimeMultiplier;
        public float SpeedThreshold;
        public float StopSpeedThreshold;
        public float Cooldown;
    }

    [Serializable]
    private class SlopeProperties
    {
        public float MinAngle;
        public float MaxAngle;
        public float SpeedMultiplier;
    }

    [Serializable]
    private class JumpProperties
    {
        public bool Enable = true;
        public bool HoldJump;
        public float Force;
        public float Cooldown;
    }

    [Serializable]
    private class GravityProperties
    {
        public LayerMask GroundMask;
        public float GroundCheckRadius = 0.4f;
        public float Gravity;
        public float AirGravity;
        public float MaxFallSpeed = 60f;
    }

    [Serializable]
    private class WallProperties
    {
        public bool CanUseSameWall;
        public LayerMask WallMask;
        public float BottomCheckDistance;
        public float TopCheckDistance;
        public float CheckOffsetY;
    }

    [Serializable]
    private class WallrunningProperties
    {
        public bool Enable;
        public float ForceToWall;
        public Vector3 JumpForce;
        public float SpeedThreshold;
        public float MinDistanceFromGround;
        [Range(-90f, 90f)] public float FrontCheckOffsetAngle;
        [Range(0f, 90f)] public float BackCheckOffsetAngle;
        [Range(0f, 90f)] public float MaxWallAngle;
        public float Gravity;
        public float Duration;
        public bool ResetTimer;

        [Header("Camera Properties")]
        public float CameraAngle;
        public float CameraOffsetX;
    }

    [Serializable]
    private class WallclimbingProperties
    {
        public bool Enable;
        public float Force;
        public float VaultForce;
        public float FrontCheckDistance;
        public float TopCheckOffsetY;
        [Range(0f, 180f)] public float MinJumpAngle;
        public Vector3 JumpForce;
    }

    [Serializable]
    private class DashProperties
    {
        public bool Enable;
        public bool canDashInAir;
        public float Force;
        public float Cooldown;
        public float TimeScale;
        public float TimeScaleLerpSpeed;
        public float SlowMoDuration;
    }

    public RunMode RunMode
    {
        get => runProperties.Mode; 
        set => runProperties.Mode = value;
    }

    public InputMode CrouchMode
    {
        get => crouchProperties.Mode;
        set => crouchProperties.Mode = value;
    }

    public bool UseSeperateSlideKey
    {
        get => slideProperties.UseSeperateKey; 
        set => slideProperties.UseSeperateKey = value;
    }

    public bool IsWalking => inputDirection.magnitude > 0f && !IsRunning && IsGrounded && !IsSliding;
    public bool IsRunning => run && IsGrounded && (inputDirection.y > 0 || (inputDirection.magnitude > 0f && runProperties.AllowRunningInAllDirections)) && !IsCrouching && !IsSliding;
    public bool IsGrounded => Physics.CheckSphere(assignables.GroundCheck.position, gravityProperties.GroundCheckRadius, gravityProperties.GroundMask);
    public bool IsCrouching { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsWallrunning { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool Vaulting { get; private set; }
    
    public Vector3 Velocity => controller.velocity;
    public float ControllerVelocity => controller.velocity.magnitude;
    public float PlanarVelocity => new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
    public float DownVelocity => controller.velocity.y;

    bool CanStand => !Physics.Raycast(assignables.GroundCheck.position, Vector3.up, crouchProperties.CeilingCheckDistance, crouchProperties.CeilingCheckMask, QueryTriggerInteraction.Ignore);

    public Vector3 AddedForce { get; set; }
    public Action OnUpdate { get; set; }
    public Action OnJump { get; set; }

    CharacterController controller;
    InputAction.CallbackContext crouchContext;

    Vector2 inputDirection;
    Vector3 cameraPosition;
    Vector3 modelPosition;
    Vector3 groundCheckPosition;

    float movementSpeed;
    float airSpeed;
    float currentSpeed;
    float currentRunSpeed;
    float currentGravity;
    float controllerHeight;
    float currentSlideForce;

    bool ungrounded;
    bool jumping;
    bool jumped;
    bool inAir;
    bool run;
    bool readyToJump;
    bool readyToSlide;
    bool readyToDash;
    bool slidingHorizontal;

    float wallrunGravity;

    bool stopWallrun;
    bool canUseWall;
    bool frontWall;
    bool backWall;
    bool leftWall;
    bool rightWall;
    bool onLeftWall; // Used to make sure the player cant change direction while wallrunning

    bool canClimb = true;

    RaycastHit frontWallHit;
    RaycastHit backWallHit;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    RaycastHit wallHit;

    Transform currentWall;
    CancellationTokenSource wallrunningTokenSource;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        airSpeed = currentSpeed = movementProperties.WalkSpeed;
        cameraPosition = assignables.Head.localPosition;
        modelPosition = assignables.Model.localPosition;
        groundCheckPosition = assignables.GroundCheck.localPosition;
        controllerHeight = controller.height;
        run = runProperties.Mode == RunMode.ReverseHold;
        readyToSlide = readyToJump = readyToDash = true;
        currentSlideForce = 1f;
    }

    private void Start()
    {
        new InputManager();
        var playerInputs = InputManager.Player;
        playerInputs.Move.SubscribeToAllActions(Move);
        playerInputs.Sprint.SubscribeToAllActions(Run);
        playerInputs.Crouch.SubscribeToAllActions(Crouch);
        playerInputs.Jump.SubscribeToAllActions(Jump);
    }

    private void Update()
    {
        CheckRunning();
        CheckWall();
        CheckWallrunning();
        CheckWallclimbing();
        Movement();
        CheckGrounded();
        Gravity();
        OnUpdate?.Invoke();
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    private void Movement()
    {
        if (assignables.CameraController.Quickturning) return;

        movementSpeed = IsSliding ? currentSlideForce : (IsRunning || IsWallrunning || IsClimbing ? currentRunSpeed 
            : (IsCrouching ? crouchProperties.Speed : movementProperties.WalkSpeed));
        
        // Acceleration
        if (inputDirection.magnitude > 0)
        {
            var speed = IsGrounded ? movementSpeed : airSpeed;
            if (currentSpeed > currentRunSpeed && (IsRunning || !IsGrounded)) currentSpeed -= movementProperties.Deacceleration * Time.deltaTime;
            else if (currentSpeed < speed) currentSpeed += movementProperties.Acceleration * Time.deltaTime;
            else currentSpeed = movementSpeed;
        }
        else currentSpeed = 0f;

        Vector3 velocityX;
        Vector3 velocityZ;

        if (IsWallrunning) 
        {
            var normal = leftWall ? leftWallHit.normal : rightWallHit.normal;
            var forward = Vector3.Cross(normal, transform.up);

            if ((transform.forward - forward).magnitude > (transform.forward + forward).magnitude)
                forward = -forward;

            velocityX = -normal * wallrunningProperties.ForceToWall;
            velocityZ = forward * currentSpeed;
        }
        else if (IsClimbing)
        {
            velocityX = Vector3.zero;
            velocityZ = (wallHit.normal != Vector3.zero ? -wallHit.normal : transform.forward) * 5f;
        }
        else
        {
            var slideMultiplier = slideProperties.StrafeSpeedMultiplier / currentSpeed;
            velocityX = transform.right * inputDirection.x * (IsRunning ? runProperties.StrafeSpeed : currentSpeed) * (IsSliding && !slidingHorizontal ? slideMultiplier : 1f);
            velocityZ = transform.forward * inputDirection.y * currentSpeed * (IsSliding && slidingHorizontal ? slideMultiplier : 1f);
        }

        AddedForce = Vector3.Lerp(AddedForce, Vector3.zero, movementProperties.AddedForceDeacceleration * Time.deltaTime);

        var velocity = velocityX + velocityZ + new Vector3(0f, currentGravity) + AddedForce;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (!runProperties.Enable) return;

        switch (runProperties.Mode)
        {
            case RunMode.Hold:
                run = context.performed;
                break;
            case RunMode.Toggle:
                run = true;
                break;
            case RunMode.StickyToggle:
                if (context.started) run = !run;
                break;
            case RunMode.ReverseHold:
                run = !context.performed;
                break;
        }
    }

    private void CheckRunning()
    {
        if (runProperties.Mode == RunMode.ReverseHold && IsCrouching) return;

        if ((inputDirection.y <= 0 && !runProperties.AllowRunningInAllDirections) || inputDirection.magnitude == 0)
        {
            airSpeed = movementProperties.WalkSpeed;
            if (runProperties.Mode == RunMode.Toggle) run = false;
        }

        if (IsRunning || (CanStand && IsCrouching && !crouchContext.performed)) StopCrouch();

        if (runProperties.AllowRunningInAllDirections)
        {
            currentRunSpeed = inputDirection.y >= 0f ? runProperties.ForwardSpeed : runProperties.BackwardSpeed;
        }
        else currentRunSpeed = runProperties.ForwardSpeed;
    }

    public void StopRun() => run = false;

    public void Jump(InputAction.CallbackContext context)
    {
        var angle = Vector3.Angle(-wallHit.normal, transform.forward);
        var canWallJump = Mathf.Abs(angle) >= wallclimbingProperties.MinJumpAngle;

        if (!jumpProperties.Enable || !readyToJump || (!IsGrounded && !IsWallrunning && (!IsClimbing || !canWallJump)) 
            || (!CanStand && IsCrouching) || !context.performed) return;
        if (!crouchProperties.CanCrouchInAir) StopCrouch();

        jumped = true;
        readyToJump = false;
        airSpeed = movementSpeed;
        var forceY = jumpProperties.Force;

        if (IsWallrunning)
        {
            stopWallrun = true;
            AddedForce += (rightWall ? rightWallHit.normal : leftWallHit.normal) * wallrunningProperties.JumpForce.x;
            AddedForce += transform.forward * wallrunningProperties.JumpForce.z;
            forceY = wallrunningProperties.JumpForce.y;
        }
        else if (IsClimbing)
        {
            AddedForce += (frontWall ? frontWallHit.normal : transform.forward) * wallclimbingProperties.JumpForce.z;
            forceY = wallclimbingProperties.JumpForce.y;
        }

        IsClimbing = false;
        currentGravity = Mathf.Sqrt(forceY * 2f * gravityProperties.AirGravity);
        OnJump?.Invoke();
        ResetJump(context);
    }

    private async void ResetJump(InputAction.CallbackContext context)
    {
        await Awaitable.WaitForSecondsAsync(jumpProperties.Cooldown);
        jumped = false;
        readyToJump = true;
        if (jumpProperties.HoldJump)
        {
            while (context.performed && !IsGrounded)
            {
                await Awaitable.NextFrameAsync();
            }

            if (context.performed) Jump(context);
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        crouchContext = context;
        if (context.started)
        {
            stopWallrun = true;
            IsClimbing = false;
        }

        if (!crouchProperties.Enable || (crouchProperties.Mode == InputMode.Toggle && !context.performed) || context.started || (IsCrouching && !CanStand)) return;
        IsCrouching = (crouchProperties.CanCrouchInAir || IsGrounded) && (crouchProperties.Mode == InputMode.Toggle ? !IsCrouching : context.performed);
        SmoothCrouch();
        if (!slideProperties.UseSeperateKey) CheckSlide();
    }

    private async void SmoothCrouch()
    {
        var time = 0f;

        var cameraStart = assignables.Head.localPosition;
        var cameraEnd = IsCrouching ? cameraPosition + new Vector3(0f, crouchProperties.CameraOffset, 0f) : cameraPosition;

        var groundStart = assignables.GroundCheck.localPosition;
        var groundEnd = IsCrouching ? groundCheckPosition + new Vector3(0f, (controllerHeight - crouchProperties.Height) / 2f, 0f) : groundCheckPosition;

        var heightStart = controller.height;
        var heightEnd = IsCrouching ? crouchProperties.Height : controllerHeight;

        var modelStart = assignables.Model.localPosition;
        var modelEnd = IsCrouching ? modelPosition / (controllerHeight / crouchProperties.Height) : modelPosition;

        while (time < crouchProperties.SmoothTime)
        {
            var t = time / crouchProperties.SmoothTime;
            assignables.Head.localPosition = Vector3.Lerp(cameraStart, cameraEnd, t);
            assignables.GroundCheck.localPosition = Vector3.Lerp(groundStart, groundEnd, t);
            assignables.Model.localPosition = Vector3.Lerp(modelStart, modelEnd, t);
            controller.height = Mathf.Lerp(heightStart, heightEnd, t);
            time += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        assignables.Head.localPosition = cameraEnd;
        assignables.GroundCheck.localPosition = groundEnd;
        assignables.Model.localPosition = modelEnd;
        controller.height = heightEnd;
    }

    private void StopCrouch()
    {
        if (!IsCrouching || (!CanStand && IsCrouching)) return;
        IsCrouching = false;
        SmoothCrouch();
    }

    public void Slide(InputAction.CallbackContext context)
    {
        if (!slideProperties.UseSeperateKey) return;
        Crouch(context);
        if (!CheckSlide()) StopCrouch();
    }

    private bool CheckSlide()
    {
        if (!slideProperties.Enable || (!run && !OnSlope()) || !readyToSlide || !IsCrouching || ControllerVelocity < slideProperties.SpeedThreshold || !CanSlide()) return false;

        IsSliding = true;
        readyToSlide = false;
        slidingHorizontal = inputDirection.y == 0;

        HandleSlideForce();
        return true;
    }

    private async void HandleSlideForce()
    {
        currentSlideForce = slideProperties.Force;
        
        while (IsCrouching && currentSlideForce > slideProperties.StopSpeedThreshold && !jumping)
        {
            if (OnSlope() && CanSlide())
            {
                currentSlideForce += GetSlopeAngle() * slopeProperties.SpeedMultiplier * Time.deltaTime;
            }
            else currentSlideForce -= currentSlideForce / slideProperties.TimeMultiplier * Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        currentSlideForce = 0f;
        IsSliding = false;

        await Awaitable.WaitForSecondsAsync(slideProperties.Cooldown);
        readyToSlide = true;
    }

    public bool OnSlope()
    {
        var angle = GetSlopeAngle();
        return angle < slopeProperties.MaxAngle && angle != 0;
    }

    private float GetSlopeAngle()
    {
        if (Physics.Raycast(assignables.GroundCheck.position, Vector3.down, out RaycastHit slopeHit, 1f))
        {
            return Vector3.Angle(Vector3.up, slopeHit.normal);
        }
        return 0f;
    }

    private bool CanSlide()
    {
        if (Physics.Raycast(assignables.GroundCheck.position, Vector3.down, out RaycastHit slopeHit, 1f))
        {
            var projection = Vector3.ProjectOnPlane(controller.velocity, slopeHit.normal);
            return projection.y < slopeProperties.MinAngle && IsGrounded;
            //return Vector3.Angle(transform.forward, slopeHit.normal) <= 90f - slopeProperties.MinAngle && IsGrounded;
        }
        return false;
    }

    private void CheckGrounded()
    {
        // Only called once when getting off the ground
        if (!IsGrounded && !inAir)
        {
            inAir = true;
            if (jumped) jumping = true;
            if (!jumping) airSpeed = movementSpeed;
        }

        // Only called once when getting on the ground
        if (inAir && IsGrounded)
        {
            jumping = jumped = inAir = stopWallrun = IsClimbing = false;
            canClimb = true;
            currentWall = null;
            if (crouchContext.performed) Crouch(crouchContext);
            wallrunningTokenSource?.Cancel();
        }
    }

    private void Gravity()
    {
        // Ungrounded is used to do this one more when getting off the ground as well
        // This avoids using the fast ground gravity when going off a ledge                                 
        if ((IsGrounded || (!IsGrounded && !ungrounded) || DownVelocity == 0) && currentGravity < 0 && !jumping && !jumped) // Fix jump on onto slope where not grounded
        {
            currentGravity = -2f;
            ungrounded = !IsGrounded;
        }

        currentGravity -= (IsWallrunning ? wallrunGravity : 
            (IsGrounded && !jumping && !jumped ? gravityProperties.Gravity : gravityProperties.AirGravity)) * Time.deltaTime;

        if (currentGravity < -gravityProperties.MaxFallSpeed && !IsGrounded) currentGravity = -gravityProperties.MaxFallSpeed;
    }

    private void CheckWall()
    {
        if (!wallclimbingProperties.Enable && !wallrunningProperties.Enable) return;

        var direction = transform.forward;

        if (wallclimbingProperties.Enable)
        {
            frontWall = OnWall(direction, wallclimbingProperties.FrontCheckDistance, out frontWallHit, false);
            backWall = OnWall(-direction, wallProperties.BottomCheckDistance, out backWallHit, false);
        }

        if (wallrunningProperties.Enable)
        {
            var frontOffsetAngle = wallrunningProperties.FrontCheckOffsetAngle;
            var backOffsetAngle = wallrunningProperties.BackCheckOffsetAngle;
            direction = transform.right;

            leftWall = OnWall(-direction, out leftWallHit, frontOffsetAngle)
                    || OnWall(-direction, out leftWallHit, -backOffsetAngle);

            rightWall = OnWall(direction, out rightWallHit, -frontOffsetAngle)
                     || OnWall(direction, out rightWallHit, backOffsetAngle);
        }

        wallHit = frontWall ? frontWallHit : (backWall ? backWallHit : (leftWall ? leftWallHit : rightWallHit));
        var wall = wallHit.transform;
        canUseWall = wallProperties.CanUseSameWall || wall != currentWall;
        if (wall != null && (IsWallrunning || IsClimbing)) currentWall = wall;
    }

    private bool OnWall(Vector3 direction, out RaycastHit hit, float angle = 0f, bool requireBothHits = true)
    {
        var offset = new Vector3(0f, wallProperties.CheckOffsetY);
        var bottomDistance = wallProperties.BottomCheckDistance;
        var topDistance = wallProperties.TopCheckDistance;
        var mask = wallProperties.WallMask;
        var rotation = Quaternion.AngleAxis(angle, Vector3.up);

        bool topHit = Physics.Raycast(transform.position + offset, rotation * direction, out hit, topDistance, mask);
        bool bottomHit = Physics.Raycast(transform.position - offset, rotation * direction, bottomDistance, mask);

        return requireBothHits ? (topHit && bottomHit) : (topHit || bottomHit);
    }

    private bool OnWall(Vector3 direction, float distance, out RaycastHit hit, bool requireBothHits = true)
    {
        var offset = new Vector3(0f, wallProperties.CheckOffsetY);
        var mask = wallProperties.WallMask;

        bool topHit = Physics.Raycast(transform.position + offset, direction, out hit, distance, mask);
        bool bottomHit = Physics.Raycast(transform.position - offset, direction, distance, mask);

        return requireBothHits ? (topHit && bottomHit) : (topHit || bottomHit);
    }

    private bool OnWall(Vector3 direction, float angle = 0f)
    {
        var mask = wallProperties.WallMask;
        var rotation = Quaternion.AngleAxis(angle, Vector3.up);
        return Physics.Raycast(transform.position, rotation * direction, wallProperties.TopCheckDistance, mask);
    }

    private void CheckWallrunning()
    {
        if (!wallrunningProperties.Enable) return;

        var normal = leftWall ? leftWallHit.normal : rightWallHit.normal;
        var angle = 90f - Vector3.Angle(Vector3.up, normal);

        var aboveGround = !Physics.Raycast(assignables.GroundCheck.position, Vector3.down, wallrunningProperties.MinDistanceFromGround, gravityProperties.GroundMask);
        
        if (!leftWall && !rightWall) stopWallrun = false;

        if (!IsWallrunning && (leftWall || rightWall) && canUseWall && aboveGround && !IsGrounded 
            && currentSpeed >= wallrunningProperties.SpeedThreshold && angle <= wallrunningProperties.MaxWallAngle 
            && angle >= -1 && !stopWallrun && !IsClimbing)
        {
            onLeftWall = leftWall;
            StartWallrun();
        }
        else if (IsWallrunning && (inputDirection.y == 0 || IsGrounded || (!leftWall && !rightWall) || stopWallrun
            || (!onLeftWall && leftWall) || (onLeftWall && rightWall)))
        {
            StopWallrun();
        }
    }

    private void StartWallrun()
    {
        IsWallrunning = true;
        currentGravity = 0f;
        wallrunGravity = 0f;
        HandleWallrun();
        var angle = wallrunningProperties.CameraAngle;
        var cameraOffset = wallrunningProperties.CameraOffsetX;
        assignables.CameraController.LerpLean(rightWall ? -cameraOffset : cameraOffset, rightWall ? angle : -angle);
    }

    private void StopWallrun()
    {
        IsWallrunning = false;
        if (wallrunningProperties.ResetTimer) wallrunningTokenSource.Cancel();
        assignables.CameraController.LerpLean(0f, 0f);
    }

    private async void HandleWallrun()
    {
        wallrunningTokenSource = new();

        try
        {
            await Awaitable.WaitForSecondsAsync(wallrunningProperties.Duration, wallrunningTokenSource.Token);
            wallrunGravity = wallrunningProperties.Gravity;
        }
        catch (Exception) { }
    }

    private void CheckWallclimbing()
    {
        if (!wallclimbingProperties.Enable) return;

        var topFront = Physics.Raycast(transform.position + Vector3.up * wallclimbingProperties.TopCheckOffsetY, transform.forward, wallclimbingProperties.FrontCheckDistance);
        
        // Vaulting
        if (!topFront && frontWall && !IsGrounded && !IsWallrunning && inputDirection.y > 0)
        {
            IsClimbing = true;
            Vaulting = true;
            if (currentGravity < wallclimbingProperties.VaultForce) currentGravity = wallclimbingProperties.VaultForce;
        }
        else Vaulting = false;

        var direction = transform.right;

        var left = OnWall(-direction, 45) || OnWall(-direction, -45);
        var right = OnWall(direction, 45) || OnWall(direction, -45);

        // Climbing
        if ((frontWall && jumping && !IsGrounded && canClimb && (canUseWall || IsClimbing) && !IsWallrunning)
            || (IsClimbing && (backWall || left || right)))
        {
            IsClimbing = true;
            if (!backWall) currentGravity += wallclimbingProperties.Force * Time.deltaTime;
        }
        else if (canClimb && IsClimbing)
        {
            canClimb = IsGrounded;
            currentWall = null;
            IsClimbing = false;
        }
    }

    public async void Dash(InputAction.CallbackContext context)
    {
        if (!dashProperties.Enable || !context.performed || !readyToDash || (!IsGrounded && !dashProperties.canDashInAir)) return;
        readyToDash = false;

        if (dashProperties.TimeScale != 1)
        {
            Time.timeScale = dashProperties.TimeScale;
            var elapsedTime = 0f;
            var lerpTime = 0f;
            var duration = 1 / dashProperties.TimeScaleLerpSpeed;

            while (context.performed && elapsedTime < dashProperties.SlowMoDuration)
            {
                Time.timeScale = Mathf.Lerp(1f, dashProperties.TimeScale, lerpTime / duration);
                lerpTime += Time.unscaledDeltaTime;
                elapsedTime += Time.unscaledDeltaTime;
                if (lerpTime > duration) lerpTime = duration; 
                await Awaitable.NextFrameAsync();
            }

            Time.timeScale = 1;
        }

        var input = new Vector3(inputDirection.x, 0f, inputDirection.y);
        var dashDirection = assignables.CameraController.transform.TransformDirection(input).normalized;
        AddedForce += dashDirection * dashProperties.Force;

        await Awaitable.WaitForSecondsAsync(dashProperties.Cooldown);
        readyToDash = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.3) return;

        var pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        var speed = movementProperties.ApplyCurrentSpeedToObjects ? currentSpeed : 1f;
        rb.linearVelocity = pushDir * movementProperties.PushForce * speed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(assignables.GroundCheck.position, assignables.GroundCheck.position - new Vector3(0f, 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(assignables.GroundCheck.position, gravityProperties.GroundCheckRadius);
        Gizmos.DrawLine(assignables.GroundCheck.position, assignables.GroundCheck.position + new Vector3(0f, crouchProperties.CeilingCheckDistance));

        var offsetY = new Vector3(0f, wallProperties.CheckOffsetY);
        var bottomDistance = new Vector3(wallProperties.BottomCheckDistance, 0f);
        var topDistance = new Vector3(wallProperties.TopCheckDistance, 0f);
        Gizmos.DrawLine(transform.position + offsetY, transform.position + topDistance + offsetY);
        Gizmos.DrawLine(transform.position - offsetY, transform.position + bottomDistance - offsetY);
        Gizmos.DrawLine(transform.position + offsetY, transform.position - topDistance + offsetY);
        Gizmos.DrawLine(transform.position - offsetY, transform.position - bottomDistance - offsetY);

        var frontDistance = new Vector3(0f, 0f, wallclimbingProperties.FrontCheckDistance);
        var backDistance = new Vector3(0f, 0f, wallProperties.BottomCheckDistance);
        Gizmos.DrawLine(transform.position + offsetY, transform.position + frontDistance + offsetY);
        Gizmos.DrawLine(transform.position - offsetY, transform.position + frontDistance - offsetY);
        Gizmos.DrawLine(transform.position + offsetY, transform.position - backDistance + offsetY);
        Gizmos.DrawLine(transform.position - offsetY, transform.position - backDistance - offsetY);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * wallclimbingProperties.TopCheckOffsetY, transform.position + frontDistance + Vector3.up * wallclimbingProperties.TopCheckOffsetY);
    }

    private void OnDestroy()
    {
        wallrunningTokenSource?.Cancel();
        wallrunningTokenSource?.Dispose();
    }
}

public enum RunMode { Hold, Toggle, StickyToggle, ReverseHold }

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true); // Skip the "Script" property

        while (property.NextVisible(false))
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.PropertyField(property, true);

                if (property.isExpanded)
                {
                    // Add conditional logic for fields if necessary
                    EditorGUI.indentLevel++;

                    var allowRunningInAllDirections = property.FindPropertyRelative("AllowRunningInAllDirections");
                    if (allowRunningInAllDirections != null)
                    {
                        if (allowRunningInAllDirections.boolValue)
                        {
                            //EditorGUILayout.PropertyField(property.FindPropertyRelative("ForwardSpeed"), new GUIContent("Forward Speed"));
                            
                            //EditorGUILayout.PropertyField(property.FindPropertyRelative("StrafeSpeed"), new GUIContent("Strafe Speed"));
                            
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("BackwardSpeed"), new GUIContent("Backward Speed"));
                        }
                        else
                        {
                            //EditorGUILayout.PropertyField(property.FindPropertyRelative("Speed"), new GUIContent("Speed"));
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            else EditorGUILayout.PropertyField(property);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
