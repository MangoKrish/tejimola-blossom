using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float moveSpeed = GameConstants.MoveSpeed;
        [SerializeField] protected float crouchSpeed = GameConstants.CrouchSpeed;
        [SerializeField] protected float jumpForce = 10f;

        [Header("Interaction")]
        [SerializeField] protected float interactionRange = GameConstants.InteractionRange;
        [SerializeField] protected LayerMask interactableLayer;

        [Header("Ground Check")]
        [SerializeField] protected Transform groundCheck;
        [SerializeField] protected float groundCheckRadius = 0.2f;
        [SerializeField] protected LayerMask groundLayer;

        // Components
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;

        // State
        protected Vector2 moveInput;
        protected bool isCrouching;
        protected bool isHiding;
        protected bool canMove = true;
        protected bool facingRight = true;
        protected bool isGrounded;
        protected bool jumpQueued;
        protected Interactable nearestInteractable;

        // Animation hashes for performance
        protected static readonly int AnimSpeed = Animator.StringToHash("Speed");
        protected static readonly int AnimCrouching = Animator.StringToHash("IsCrouching");
        protected static readonly int AnimHiding = Animator.StringToHash("IsHiding");
        protected static readonly int AnimGrounded = Animator.StringToHash("IsGrounded");
        protected static readonly int AnimInteract = Animator.StringToHash("Interact");
        protected static readonly int AnimCry = Animator.StringToHash("Cry");

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        protected virtual void Update()
        {
            if (!canMove || GameManager.Instance.IsGamePaused) return;

            HandleInput();
            CheckInteractables();
            UpdateAnimations();
        }

        protected virtual void FixedUpdate()
        {
            if (!canMove || GameManager.Instance.IsGamePaused) return;

            CheckGround();
            Move();

            if (jumpQueued && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // cancel downward velocity first
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpQueued = false;
            }
            else
            {
                jumpQueued = false; // cancel if not grounded
            }
        }

        protected virtual void HandleInput()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = 0;

            // Jump — W or Up Arrow (Space is reserved for dialogue advance / stealth hide)
            bool jumpKey = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            if (jumpKey && isGrounded && !isHiding && !isCrouching)
                jumpQueued = true;

            if (Input.GetKeyDown(KeyCode.E) && nearestInteractable != null)
            {
                Interact();
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
            {
                ToggleCrouch();
            }
        }

        protected virtual void Move()
        {
            float speed = isCrouching ? crouchSpeed : moveSpeed;
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

            // Flip sprite
            if (moveInput.x > 0 && !facingRight)
                Flip();
            else if (moveInput.x < 0 && facingRight)
                Flip();
        }

        protected void Flip()
        {
            facingRight = !facingRight;
            spriteRenderer.flipX = !facingRight;
        }

        protected void CheckGround()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                isGrounded = true; // Default to grounded for 2.5D side-scroller
            }
        }

        protected virtual void UpdateAnimations()
        {
            if (animator == null) return;

            animator.SetFloat(AnimSpeed, Mathf.Abs(moveInput.x));
            animator.SetBool(AnimCrouching, isCrouching);
            animator.SetBool(AnimHiding, isHiding);
            animator.SetBool(AnimGrounded, isGrounded);
        }

        public virtual void ToggleCrouch()
        {
            isCrouching = !isCrouching;
        }

        public bool IsHiding => isHiding;

        public void SetHiding(bool hiding)
        {
            isHiding = hiding;
            canMove = !hiding;
            if (hiding)
            {
                rb.linearVelocity = Vector2.zero;
                EventManager.Instance.Publish(EventManager.Events.PlayerHidden);
            }
        }

        public void SetCanMove(bool value)
        {
            canMove = value;
            if (!value)
                rb.linearVelocity = Vector2.zero;
        }

        protected void CheckInteractables()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
            float closestDist = float.MaxValue;
            Interactable closest = null;

            foreach (var col in colliders)
            {
                var interactable = col.GetComponent<Interactable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            if (closest != nearestInteractable)
            {
                if (nearestInteractable != null)
                    nearestInteractable.HidePrompt();
                nearestInteractable = closest;
                if (nearestInteractable != null)
                {
                    nearestInteractable.ShowPrompt();
                    EventManager.Instance.Publish(EventManager.Events.InteractionAvailable);
                }
            }
        }

        protected void Interact()
        {
            if (nearestInteractable != null)
            {
                nearestInteractable.OnInteract(this);
                EventManager.Instance.Publish(EventManager.Events.InteractionPerformed);
                if (animator != null)
                    animator.SetTrigger(AnimInteract);
            }
        }

        public void PlayCryAnimation()
        {
            if (animator != null)
                animator.SetTrigger(AnimCry);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
