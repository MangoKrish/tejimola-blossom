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

}
