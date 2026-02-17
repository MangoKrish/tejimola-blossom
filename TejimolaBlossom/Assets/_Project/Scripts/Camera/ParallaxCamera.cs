using UnityEngine;
using Tejimola.Utils;

namespace Tejimola.Camera
{
    public class ParallaxCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = GameConstants.CameraFollowSpeed;
        [SerializeField] private Vector3 offset = new Vector3(0, 1f, -10f);

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private float minX = -20f;
        [SerializeField] private float maxX = 20f;
        [SerializeField] private float minY = -5f;
        [SerializeField] private float maxY = 10f;

        [Header("Parallax Layers")]
        [SerializeField] private ParallaxLayer[] parallaxLayers;

        [Header("Shake")]
        private float shakeTimer;
        private float shakeIntensity;

        private Vector3 lastPosition;

        void Start()
        {
            if (target == null)
            {
                // Find player
                var player = FindFirstObjectByType<Characters.CharacterController2D>();
                if (player != null)
                    target = player.transform;
            }

            if (target != null)
                transform.position = target.position + offset;

            lastPosition = transform.position;
        }

        void LateUpdate()
        {
            if (target == null) return;

            // Smooth follow
            Vector3 targetPos = target.position + offset;

            // Apply bounds
            if (useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
                targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

            // Apply screen shake
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = Random.insideUnitCircle * shakeIntensity;
                transform.position += new Vector3(shakeOffset.x, shakeOffset.y, 0);
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0, Time.deltaTime * 5f);
            }

            // Update parallax
            Vector3 deltaMovement = transform.position - lastPosition;
            UpdateParallax(deltaMovement);
            lastPosition = transform.position;
        }

        void UpdateParallax(Vector3 delta)
        {
            if (parallaxLayers == null) return;

            foreach (var layer in parallaxLayers)
            {
                if (layer != null)
                    layer.Move(delta);
            }
        }

        public void Shake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeTimer = duration;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(float xMin, float xMax, float yMin, float yMax)
        {
            minX = xMin;
            maxX = xMax;
            minY = yMin;
            maxY = yMax;
        }
    }

    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float parallaxFactor = 0.5f;
        [SerializeField] private bool infiniteHorizontal = true;
        [SerializeField] private float spriteWidth = 20f;

        private Transform[] children;
        private float leftBound;
        private float rightBound;

        void Start()
        {
            if (infiniteHorizontal)
            {
                SetupInfiniteScroll();
            }
        }

        void SetupInfiniteScroll()
        {
            // Setup wrap-around for infinite scrolling backgrounds
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

            // Infinite scroll: reposition children
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

    public class SplitScreenController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera pastCamera;
        [SerializeField] private UnityEngine.Camera presentCamera;
        [SerializeField] private UnityEngine.Camera mainCamera;

        [Header("Split Settings")]
        [SerializeField] private float splitPosition = 0.5f;

        private bool isSplit;

        public void EnableSplitScreen()
        {
            isSplit = true;
            if (pastCamera != null)
            {
                pastCamera.gameObject.SetActive(true);
                pastCamera.rect = new Rect(0, 0, splitPosition, 1);
            }
            if (presentCamera != null)
            {
                presentCamera.gameObject.SetActive(true);
                presentCamera.rect = new Rect(splitPosition, 0, 1 - splitPosition, 1);
            }
            if (mainCamera != null)
                mainCamera.gameObject.SetActive(false);
        }

        public void DisableSplitScreen()
        {
            isSplit = false;
            if (pastCamera != null)
                pastCamera.gameObject.SetActive(false);
            if (presentCamera != null)
            {
                presentCamera.rect = new Rect(0, 0, 1, 1);
            }
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(true);
                mainCamera.rect = new Rect(0, 0, 1, 1);
            }
        }
    }
}
