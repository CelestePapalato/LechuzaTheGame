using UnityEngine;

public class LechuzaAnimPlayer : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private PlatformerMovement playerMovement;
    [SerializeField]
    private Health playerHealth;

    private string groundedHash = "isGrounded";
    private string jumpHash = "Jump";
    private string dashHash = "Dash";
    private string deathHash = "Death";
    private string damageHash = "Damage";
    private string speedXHash = "SpeedX";
    private string speedYHash = "SpeedY";

    private bool isDead = false;

    private void FlipSprite()
    {
        float vel = playerMovement.CurrentVelocity.x;

        if (Mathf.Abs(vel) > 0.05f)
        {
            sprite.flipX = vel < 0;
        }
    }

    private void SetDashTrigger()
    {
        animator.SetTrigger(dashHash);
    }

    private void SetJumpTrigger()
    {
        animator.SetTrigger(jumpHash);
    }

    private void SetDamageTrigger(int current, int max)
    {
        animator.SetTrigger(damageHash);
    }

    private void SetDeathTrigger()
    {
        isDead = false;
        animator.SetTrigger(deathHash);
    }

    private void UpdateGroundedBoolean()
    {
        animator.SetBool(groundedHash, playerMovement.OnFloor);
    }

    private void UpdateSpeed()
    {
        animator.SetFloat(speedXHash, Mathf.Abs(playerMovement.CurrentVelocity.x));
        animator.SetFloat(speedYHash, playerMovement.CurrentVelocity.y);
    }
    private void OnEnable()
    {
        playerMovement.SubscribeToDashEvent(SetDashTrigger);
        playerMovement.SubscribeToJumpEvent(SetJumpTrigger);
        playerHealth.OnDamage += SetDamageTrigger;
        playerHealth.OnDeath += SetDeathTrigger;
    }

    private void OnDisable()
    {
        playerMovement.UnsubscribeFromDashEvent(SetDashTrigger);
        playerMovement.UnsubscribeFromJumpEvent(SetJumpTrigger);
        playerHealth.OnDamage -= SetDamageTrigger;
        playerHealth.OnDeath -= SetDeathTrigger;
    }

    private void Update()
    {
        if (isDead) return;
        UpdateGroundedBoolean();
        UpdateSpeed();
        FlipSprite();
    }
}
