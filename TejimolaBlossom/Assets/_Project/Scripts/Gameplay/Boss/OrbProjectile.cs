using UnityEngine;

namespace Tejimola.Gameplay
{
    public class OrbProjectile : MonoBehaviour
    {
        private Transform target;
        private float speed;

        public void Initialize(Transform target, float speed)
        {
            this.target = target;
            this.speed = speed;
        }

        void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }
}
