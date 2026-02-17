using UnityEngine;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
    public class SpiritPulseEffect : MonoBehaviour
    {
        private float maxRadius;
        private float expandSpeed;
        private float currentRadius;
        private SpriteRenderer ringRenderer;
        private float lifetime;
        private float maxLifetime;

        public void Initialize(float radius, float speed)
        {
            maxRadius = radius;
            expandSpeed = speed;
            currentRadius = 0f;
            maxLifetime = radius / speed + 0.5f;
            lifetime = 0f;

            // Create ring visual
            ringRenderer = GetComponent<SpriteRenderer>();
            if (ringRenderer == null)
            {
                ringRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // Use a circle sprite - we'll generate this procedurally
            ringRenderer.sprite = CreateRingSprite();
            ringRenderer.color = new Color(GameColors.SpiritPurple.r, GameColors.SpiritPurple.g, GameColors.SpiritPurple.b, 0.6f);
            ringRenderer.sortingOrder = 100;

            transform.localScale = Vector3.zero;
        }

        void Update()
        {
            lifetime += Time.deltaTime;

            // Expand
            currentRadius = Mathf.MoveTowards(currentRadius, maxRadius, expandSpeed * Time.deltaTime);
            float scale = (currentRadius / maxRadius) * maxRadius * 2f;
            transform.localScale = new Vector3(scale, scale, 1f);

            // Fade out
            float alpha = Mathf.Lerp(0.6f, 0f, lifetime / maxLifetime);
            if (ringRenderer != null)
            {
                Color c = ringRenderer.color;
                c.a = alpha;
                ringRenderer.color = c;
            }

            if (lifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }

        Sprite CreateRingSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color ring = Color.white;
            float center = size / 2f;
            float outerRadius = size / 2f - 2f;
            float innerRadius = outerRadius - 6f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist >= innerRadius && dist <= outerRadius)
                    {
                        float edgeFade = 1f - Mathf.Abs(dist - (innerRadius + outerRadius) / 2f) / ((outerRadius - innerRadius) / 2f);
                        tex.SetPixel(x, y, new Color(ring.r, ring.g, ring.b, edgeFade));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }

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
                    // Fade out
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

            // Enable interaction
            var interactable = GetComponent<Characters.Interactable>();
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

            var interactable = GetComponent<Characters.Interactable>();
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
