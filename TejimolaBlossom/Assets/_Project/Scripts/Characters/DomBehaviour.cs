using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Gameplay;

namespace Tejimola.Characters
{
    public class DomBehaviour : CharacterController2D
    {
        [Header("Spirit Pulse")]
        [SerializeField] private GameObject spiritPulseVFX;
        private float pulseCooldownTimer;
        private bool canPulse = true;

        [Header("Drum")]
        [SerializeField] private AudioClip drumHitClip;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void HandleInput()
        {
            base.HandleInput();

            // Spirit Pulse activation
            if (Input.GetKeyDown(KeyCode.Space) && canPulse)
            {
                ActivateSpiritPulse();
            }

            // Update cooldown
            if (!canPulse)
            {
                pulseCooldownTimer -= Time.deltaTime;
                if (pulseCooldownTimer <= 0f)
                {
                    canPulse = true;
                    EventManager.Instance.Publish(EventManager.Events.SpiritPulseReady);
                }
            }
        }

        void ActivateSpiritPulse()
        {
            canPulse = false;
            pulseCooldownTimer = GameConstants.SpiritPulseCooldown;

            // Spawn VFX
            if (spiritPulseVFX != null)
            {
                var pulse = Instantiate(spiritPulseVFX, transform.position, Quaternion.identity);
                var pulseComponent = pulse.GetComponent<SpiritPulseEffect>();
                if (pulseComponent != null)
                {
                    pulseComponent.Initialize(GameConstants.SpiritPulseRadius, GameConstants.SpiritPulseExpandSpeed);
                }
            }

            // Reveal nearby hidden objects
            RevealNearbyObjects();

            EventManager.Instance.Publish(EventManager.Events.SpiritPulseActivated);

            // Play drum sound
            if (drumHitClip != null)
                AudioManager.Instance.PlaySFX(drumHitClip);
        }

        void RevealNearbyObjects()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, GameConstants.SpiritPulseRadius);
            foreach (var col in colliders)
            {
                var revealable = col.GetComponent<SpiritRevealable>();
                if (revealable != null)
                {
                    revealable.Reveal();
                    EventManager.Instance.Publish<string>(EventManager.Events.SpiritPulseRevealed, revealable.ObjectId);
                }
            }
        }

        public float GetPulseCooldownPercent()
        {
            if (canPulse) return 1f;
            return 1f - (pulseCooldownTimer / GameConstants.SpiritPulseCooldown);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = GameColors.SpiritPurple;
            Gizmos.DrawWireSphere(transform.position, GameConstants.SpiritPulseRadius);
        }
    }
}
