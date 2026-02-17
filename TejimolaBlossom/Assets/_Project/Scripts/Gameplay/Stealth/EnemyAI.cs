using UnityEngine;
using System.Collections;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Characters;

namespace Tejimola.Gameplay
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float waitAtPoint = 2f;

        [Header("Detection")]
        [SerializeField] private float detectionRadius = GameConstants.DetectionRadius;
        [SerializeField] private float detectionAngle = GameConstants.DetectionAngle;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float catchDistance = 1f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer exclamationMark;
        [SerializeField] private SpriteRenderer questionMark;

        // State
        private DetectionState currentState = DetectionState.Unaware;
        private int currentPatrolIndex;
        private bool isPatrolling;
        private Transform playerTransform;
        private TejimolaBehaviour playerController;
        private SpriteRenderer spriteRenderer;
        private float suspicionTimer;
        private float searchTimer;
        private Vector3 lastKnownPosition;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (exclamationMark != null) exclamationMark.enabled = false;
            if (questionMark != null) questionMark.enabled = false;
        }

        void Start()
        {
            // Find player
            playerController = FindFirstObjectByType<TejimolaBehaviour>();
            if (playerController != null)
                playerTransform = playerController.transform;
        }

        void Update()
        {
            if (!isPatrolling || GameManager.Instance.IsGamePaused) return;

            switch (currentState)
            {
                case DetectionState.Unaware:
                    Patrol();
                    CheckForPlayer();
                    break;
                case DetectionState.Suspicious:
                    HandleSuspicion();
                    break;
                case DetectionState.Alerted:
                    ChasePlayer();
                    break;
                case DetectionState.Searching:
                    SearchArea();
                    break;
            }
        }

        public void StartPatrol()
        {
            isPatrolling = true;
            currentState = DetectionState.Unaware;
            currentPatrolIndex = 0;
        }

        public void StopPatrol()
        {
            isPatrolling = false;
        }

        void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Transform target = patrolPoints[currentPatrolIndex];
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * patrolSpeed * Time.deltaTime;

            // Flip sprite
            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x < 0;

            // Reached patrol point
            if (Vector2.Distance(transform.position, target.position) < 0.3f)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }

        IEnumerator WaitAtPatrolPoint()
        {
            float originalSpeed = patrolSpeed;
            patrolSpeed = 0f;
            yield return new WaitForSeconds(waitAtPoint);
            patrolSpeed = originalSpeed;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        void CheckForPlayer()
        {
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > detectionRadius) return;

            // Check if player is hiding
            if (playerController != null && playerController.GetComponent<CharacterController2D>() != null)
            {
                // Player hidden check via sprite alpha
                var playerSprite = playerTransform.GetComponent<SpriteRenderer>();
                if (playerSprite != null && playerSprite.color.a < 0.5f)
                    return; // Player is hidden
            }

            // Check line of sight
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Vector2 facing = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;

            float angle = Vector2.Angle(facing, dirToPlayer);
            if (angle > detectionAngle) return;

            // Raycast for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distance, obstacleLayer);
            if (hit.collider != null) return; // Blocked by obstacle

            // Player detected!
            SetState(DetectionState.Suspicious);
            lastKnownPosition = playerTransform.position;
        }

        void HandleSuspicion()
        {
            suspicionTimer += Time.deltaTime;

            // Show question mark
            if (questionMark != null) questionMark.enabled = true;

            // Move slowly toward player
            Vector3 dir = (lastKnownPosition - transform.position).normalized;
            transform.position += dir * (patrolSpeed * 0.5f) * Time.deltaTime;

            // Check if player is still visible
            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist < detectionRadius * 0.7f)
                {
                    SetState(DetectionState.Alerted);
                    return;
                }
            }

            if (suspicionTimer > 3f)
            {
                SetState(DetectionState.Unaware);
            }
        }

        void ChasePlayer()
        {
            if (exclamationMark != null) exclamationMark.enabled = true;
            if (questionMark != null) questionMark.enabled = false;

            if (playerTransform == null)
            {
                SetState(DetectionState.Searching);
                return;
            }

            // Check if player is hiding
            var playerSprite = playerTransform.GetComponent<SpriteRenderer>();
            if (playerSprite != null && playerSprite.color.a < 0.5f)
            {
                lastKnownPosition = playerTransform.position;
                SetState(DetectionState.Searching);
                return;
            }

            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;

            if (spriteRenderer != null)
                spriteRenderer.flipX = dir.x < 0;

            lastKnownPosition = playerTransform.position;

            // Catch player
            if (Vector2.Distance(transform.position, playerTransform.position) < catchDistance)
            {
                CatchPlayer();
            }
        }

        void CatchPlayer()
        {
            EventManager.Instance.Publish(EventManager.Events.PlayerDetected);
            playerController?.OnCaught();
            SetState(DetectionState.Unaware);
            currentPatrolIndex = 0;

            // Teleport back to nearest patrol point
            if (patrolPoints.Length > 0)
                transform.position = patrolPoints[0].position;
        }

        void SearchArea()
        {
            searchTimer += Time.deltaTime;

            if (questionMark != null) questionMark.enabled = true;
            if (exclamationMark != null) exclamationMark.enabled = false;

            // Move toward last known position
            Vector3 dir = (lastKnownPosition - transform.position).normalized;
            transform.position += dir * (patrolSpeed * 0.7f) * Time.deltaTime;

            // Check for player while searching
            CheckForPlayer();

            if (searchTimer > 5f || Vector2.Distance(transform.position, lastKnownPosition) < 0.5f)
            {
                SetState(DetectionState.Unaware);
            }
        }

        void SetState(DetectionState newState)
        {
            currentState = newState;
            suspicionTimer = 0f;
            searchTimer = 0f;

            if (exclamationMark != null)
                exclamationMark.enabled = newState == DetectionState.Alerted;
            if (questionMark != null)
                questionMark.enabled = newState == DetectionState.Suspicious || newState == DetectionState.Searching;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Detection cone
            Vector3 facing = Application.isPlaying && spriteRenderer != null && spriteRenderer.flipX
                ? Vector3.left : Vector3.right;

            Vector3 leftBound = Quaternion.Euler(0, 0, detectionAngle) * facing * detectionRadius;
            Vector3 rightBound = Quaternion.Euler(0, 0, -detectionAngle) * facing * detectionRadius;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftBound);
            Gizmos.DrawLine(transform.position, transform.position + rightBound);

            // Patrol path
            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < patrolPoints.Length - 1; i++)
                {
                    if (patrolPoints[i] != null && patrolPoints[i + 1] != null)
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }
        }
    }
}
