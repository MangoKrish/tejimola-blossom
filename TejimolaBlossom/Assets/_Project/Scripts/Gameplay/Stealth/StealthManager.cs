using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Characters;

namespace Tejimola.Gameplay
{
    public class StealthManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyAI ranimaAI;
        [SerializeField] private TejimolaBehaviour tejimola;

        [Header("Settings")]
        [SerializeField] private int maxCatches = GameConstants.MaxCatches;

        private bool stealthActive;

        void Start()
        {
            EventManager.Instance.Subscribe(EventManager.Events.StealthComplete, OnStealthComplete);
        }

        void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.StealthComplete, OnStealthComplete);
        }

        public void StartStealth()
        {
            stealthActive = true;
            GameManager.Instance.SetPhase(GamePhase.Stealth);
            if (ranimaAI != null)
                ranimaAI.StartPatrol();
        }

        public void StopStealth()
        {
            stealthActive = false;
            if (ranimaAI != null)
                ranimaAI.StopPatrol();
        }

        void OnStealthComplete()
        {
            StopStealth();
            GameManager.Instance.SetPhase(GamePhase.Exploration);
        }
    }

    public class HidingSpot : MonoBehaviour
    {
        [SerializeField] private Transform hidePoint;
        [SerializeField] private string spotName = "hiding spot";

        public Vector3 HidePosition => hidePoint != null ? hidePoint.position : transform.position;
        public string SpotName => spotName;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Add trigger collider
            var col = GetComponent<BoxCollider2D>();
            if (col == null)
                col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        public void Highlight(bool show)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = show ? 1f : 0.5f;
                spriteRenderer.color = c;
            }
        }
    }

    public class FootprintBehaviour : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = GameConstants.FootprintFadeDuration;

        private SpriteRenderer spriteRenderer;
        private float timer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateFootprintSprite();
                spriteRenderer.color = new Color(0.4f, 0.3f, 0.2f, 0.6f);
                spriteRenderer.sortingOrder = -1;
            }
            timer = fadeDuration;
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // Fade
            float alpha = timer / fadeDuration * 0.6f;
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        Sprite CreateFootprintSprite()
        {
            int w = 16, h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color fill = Color.white;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    tex.SetPixel(x, y, clear);

            // Simple footprint shape
            for (int x = 3; x < 13; x++)
                for (int y = 2; y < 18; y++)
                {
                    float cx = (x - 8f) / 5f;
                    float cy = (y - 10f) / 8f;
                    if (cx * cx + cy * cy < 1f)
                        tex.SetPixel(x, y, fill);
                }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
