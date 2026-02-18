using UnityEngine;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
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
