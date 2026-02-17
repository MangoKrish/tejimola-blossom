using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Characters
{
    public class TejimolaBehaviour : CharacterController2D
    {
        [Header("Tejimola Specific")]
        [SerializeField] private float hideTransitionTime = 0.3f;

        // Stealth state
        private bool isBeingChased;
        private HidingSpot currentHidingSpot;

        // Footprint system
        [SerializeField] private GameObject footprintPrefab;
        private float footprintTimer;
        private float footprintInterval = 0.5f;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            if (GameManager.Instance.CurrentPhase == GamePhase.Stealth)
            {
                HandleStealthInput();
                LeaveFootprints();
            }
        }

        void HandleStealthInput()
        {
            // Hide in nearby spot
            if (Input.GetKeyDown(KeyCode.Space) && currentHidingSpot != null && !isHiding)
            {
                EnterHidingSpot(currentHidingSpot);
            }
            else if (Input.GetKeyDown(KeyCode.Space) && isHiding)
            {
                ExitHidingSpot();
            }
        }

        void LeaveFootprints()
        {
            if (isHiding || isCrouching) return;

            float speed = Mathf.Abs(moveInput.x);
            if (speed > 0.1f && footprintPrefab != null)
            {
                footprintTimer += Time.deltaTime;
                if (footprintTimer >= footprintInterval)
                {
                    footprintTimer = 0f;
                    SpawnFootprint();
                }
            }
        }

        void SpawnFootprint()
        {
            if (footprintPrefab == null) return;
            var fp = Instantiate(footprintPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
            // Footprint auto-destroys via FootprintBehaviour component
        }

        public void EnterHidingSpot(HidingSpot spot)
        {
            currentHidingSpot = spot;
            SetHiding(true);
            transform.position = spot.HidePosition;

            // Reduce alpha to show hiding
            var color = spriteRenderer.color;
            color.a = 0.4f;
            spriteRenderer.color = color;
        }

        public void ExitHidingSpot()
        {
            SetHiding(false);
            var color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
            currentHidingSpot = null;
        }

        public void OnCaught()
        {
            if (isHiding) return;

            isBeingChased = false;
            SetCanMove(false);
            PlayCryAnimation();
            GameManager.Instance.IncrementCatchCount();

            // Brief pause then resume
            Invoke(nameof(ResumeAfterCatch), 2f);
        }

        void ResumeAfterCatch()
        {
            SetCanMove(true);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var hidingSpot = other.GetComponent<HidingSpot>();
            if (hidingSpot != null)
            {
                currentHidingSpot = hidingSpot;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var hidingSpot = other.GetComponent<HidingSpot>();
            if (hidingSpot != null && currentHidingSpot == hidingSpot)
            {
                currentHidingSpot = null;
            }
        }
    }
}
