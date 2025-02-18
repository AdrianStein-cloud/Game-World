using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] float animationDampTime = 0.1f;

    Animator animator;
    PlayerMovement movement;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        animator.SetBool("Climbing", movement.IsClimbing);
        animator.SetBool("Vaulting", movement.Vaulting);
        animator.SetBool("Falling", !movement.IsGrounded && !movement.IsWallrunning);
        animator.SetBool("Crouching", movement.IsCrouching);

        var moveDir = movement.Velocity.normalized * (movement.IsRunning || movement.IsWallrunning ? 2f : 1f);
        float moveX = Vector3.Dot(moveDir, transform.right);
        float moveY = Vector3.Dot(moveDir, transform.forward);

        animator.SetFloat("MoveX", moveX, animationDampTime, Time.deltaTime);
        animator.SetFloat("MoveY", moveY, animationDampTime, Time.deltaTime);
    }
}
