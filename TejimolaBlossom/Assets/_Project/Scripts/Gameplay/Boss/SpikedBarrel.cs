using UnityEngine;
using Tejimola.Core;
using Tejimola.Characters;

namespace Tejimola.Gameplay
{
    public class SpikedBarrel : MonoBehaviour
    {
        private Transform target;
        private float speed;
        private float lifetime = 8f;
        private Rigidbody2D rb;

        public void Initialize(Transform target, float speed)
        {
            this.target = target;
            this.speed = speed;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
                col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (target == null) return;

            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            transform.Rotate(0, 0, -speed * 50f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<DomBehaviour>();
            if (player != null)
            {
                EventManager.Instance.Publish(EventManager.Events.PlayerDamaged);
                Destroy(gameObject);
            }
        }
    }
}
