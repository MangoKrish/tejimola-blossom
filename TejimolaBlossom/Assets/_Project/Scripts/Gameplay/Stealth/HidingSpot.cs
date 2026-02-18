using UnityEngine;

namespace Tejimola.Gameplay
{
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
}
