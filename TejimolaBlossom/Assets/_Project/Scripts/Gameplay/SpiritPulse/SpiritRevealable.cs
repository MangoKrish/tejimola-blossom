using UnityEngine;
using Tejimola.Characters;

namespace Tejimola.Gameplay
{
    public class SpiritRevealable : MonoBehaviour
    {
        [SerializeField] private string objectId;
        [SerializeField] private float revealDuration = 5f;
        [SerializeField] private bool permanentReveal = false;

        public string ObjectId => objectId;

        private SpriteRenderer spriteRenderer;
        private Collider2D objectCollider;
        private bool isRevealed;
        private float revealTimer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            objectCollider = GetComponent<Collider2D>();
            Hide();
        }

        void Update()
        {
            if (isRevealed && !permanentReveal)
            {
                revealTimer -= Time.deltaTime;
                if (revealTimer <= 0f)
                {
                    Hide();
                }
                else if (revealTimer < 1f)
                {
                    if (spriteRenderer != null)
                    {
                        Color c = spriteRenderer.color;
                        c.a = revealTimer;
                        spriteRenderer.color = c;
                    }
                }
            }
        }

        public void Reveal()
        {
            isRevealed = true;
            revealTimer = revealDuration;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            if (objectCollider != null)
                objectCollider.enabled = true;

            var interactable = GetComponent<Interactable>();
            if (interactable != null)
                interactable.CanInteract = true;
        }

        void Hide()
        {
            isRevealed = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            if (objectCollider != null && !permanentReveal)
                objectCollider.enabled = false;

            var interactable = GetComponent<Interactable>();
            if (interactable != null)
                interactable.CanInteract = false;
        }

        public void SetPermanent()
        {
            permanentReveal = true;
            Reveal();
        }
    }
}
