using UnityEngine;

namespace Tejimola.Camera
{
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
