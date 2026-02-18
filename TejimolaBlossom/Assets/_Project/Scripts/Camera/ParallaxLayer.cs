using UnityEngine;

namespace Tejimola.Camera
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float parallaxFactor = 0.5f;
        [SerializeField] private bool infiniteHorizontal = true;
        [SerializeField] private float spriteWidth = 20f;

        private Transform[] children;

        void Start()
        {
            if (infiniteHorizontal)
            {
                SetupInfiniteScroll();
            }
        }

        void SetupInfiniteScroll()
        {
            children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            if (children.Length > 0)
            {
                var sr = children[0].GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    spriteWidth = sr.bounds.size.x;
                }
            }
        }

        public void Move(Vector3 delta)
        {
            float moveAmount = delta.x * parallaxFactor;
            transform.position += new Vector3(moveAmount, delta.y * parallaxFactor * 0.5f, 0);

            if (infiniteHorizontal && children != null)
            {
                foreach (var child in children)
                {
                    if (child == null) continue;
                    float camPosX = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform.position.x : 0;

                    if (child.position.x < camPosX - spriteWidth * 1.5f)
                    {
                        child.position += new Vector3(spriteWidth * children.Length, 0, 0);
                    }
                    else if (child.position.x > camPosX + spriteWidth * 1.5f)
                    {
                        child.position -= new Vector3(spriteWidth * children.Length, 0, 0);
                    }
                }
            }
        }
    }
}
