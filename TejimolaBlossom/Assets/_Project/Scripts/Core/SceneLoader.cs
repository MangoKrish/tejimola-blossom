using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tejimola.Core
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SceneLoader");
                    _instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Transition Settings")]
        private CanvasGroup _fadeCanvasGroup;
        private float _fadeDuration = 1f;
        private bool _isLoading;

        public bool IsLoading => _isLoading;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }

        void CreateFadeCanvas()
        {
            var canvasGo = new GameObject("FadeCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            _fadeCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(canvasGo.transform, false);
            var image = imageGo.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;

            var rectTransform = image.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
        }

        public void LoadScene(string sceneName, float fadeTime = 1f)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneRoutine(sceneName, fadeTime));
        }

        public void LoadScene(int sceneIndex, float fadeTime = 1f)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneByIndexRoutine(sceneIndex, fadeTime));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, float fadeTime)
        {
            _isLoading = true;
            _fadeDuration = fadeTime;

            // Fade out
            yield return StartCoroutine(Fade(1f));

            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
                yield return null;

            // Small delay for scene setup
            yield return new WaitForSeconds(0.2f);

            // Fade in
            yield return StartCoroutine(Fade(0f));

            _isLoading = false;
        }

        private IEnumerator LoadSceneByIndexRoutine(int sceneIndex, float fadeTime)
        {
            _isLoading = true;
            _fadeDuration = fadeTime;

            yield return StartCoroutine(Fade(1f));

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncLoad.isDone)
                yield return null;

            yield return new WaitForSeconds(0.2f);

            yield return StartCoroutine(Fade(0f));

            _isLoading = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            _fadeCanvasGroup.blocksRaycasts = true;
            float startAlpha = _fadeCanvasGroup.alpha;
            float timer = 0f;

            while (timer < _fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / _fadeDuration);
                yield return null;
            }

            _fadeCanvasGroup.alpha = targetAlpha;
            if (targetAlpha == 0f)
                _fadeCanvasGroup.blocksRaycasts = false;
        }

        // Show act title card during transition
        public void LoadSceneWithTitle(string sceneName, string titleText, float displayTime = 2f)
        {
            if (_isLoading) return;
            StartCoroutine(LoadWithTitleRoutine(sceneName, titleText, displayTime));
        }

        private IEnumerator LoadWithTitleRoutine(string sceneName, string titleText, float displayTime)
        {
            _isLoading = true;

            yield return StartCoroutine(Fade(1f));

            // Show title (handled by UI system listening for events)
            EventManager.Instance.Publish<string>(EventManager.Events.ShowNotification, titleText);

            yield return new WaitForSecondsRealtime(displayTime);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
                yield return null;

            yield return new WaitForSeconds(0.3f);

            yield return StartCoroutine(Fade(0f));

            _isLoading = false;
        }
    }
}
