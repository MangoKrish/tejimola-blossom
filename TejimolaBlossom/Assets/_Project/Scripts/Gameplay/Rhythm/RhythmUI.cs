using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
    public class RhythmUI : MonoBehaviour
    {
        [Header("Beat Display")]
        [SerializeField] private RectTransform beatTrackLeft;
        [SerializeField] private RectTransform beatTrackRight;
        [SerializeField] private RectTransform hitZoneLeft;
        [SerializeField] private RectTransform hitZoneRight;
        [SerializeField] private GameObject beatNotePrefab;

        [Header("Exhaustion")]
        [SerializeField] private Image exhaustionBar;
        [SerializeField] private Image exhaustionFill;
        [SerializeField] private Image vignetteOverlay;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI ratingText;
        [SerializeField] private TextMeshProUGUI bpmText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Vision")]
        [SerializeField] private GameObject visionPanel;
        [SerializeField] private TextMeshProUGUI[] visionChoiceTexts;
        [SerializeField] private Image flashOverlay;

        [Header("Key Prompts")]
        [SerializeField] private TextMeshProUGUI leftKeyText;
        [SerializeField] private TextMeshProUGUI rightKeyText;

        private RhythmEngine rhythmEngine;
        private float ratingDisplayTimer;
        private int comboCount;
        private CanvasGroup canvasGroup;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Hide();
        }

        public void Initialize(RhythmEngine engine)
        {
            rhythmEngine = engine;
            rhythmEngine.OnBeatResult += OnBeatResult;
            rhythmEngine.OnExhaustionChanged += OnExhaustionChanged;
            rhythmEngine.OnBeatApproaching += OnBeatApproaching;
            rhythmEngine.OnRhythmComplete += OnComplete;
            rhythmEngine.OnVisionTriggered += OnVision;
        }

        void OnDestroy()
        {
            if (rhythmEngine != null)
            {
                rhythmEngine.OnBeatResult -= OnBeatResult;
                rhythmEngine.OnExhaustionChanged -= OnExhaustionChanged;
                rhythmEngine.OnBeatApproaching -= OnBeatApproaching;
                rhythmEngine.OnRhythmComplete -= OnComplete;
                rhythmEngine.OnVisionTriggered -= OnVision;
            }
        }

        void Update()
        {
            if (rhythmEngine == null || !rhythmEngine.IsPlaying) return;

            // Update BPM display
            if (bpmText != null)
                bpmText.text = $"BPM: {rhythmEngine.CurrentBPM:F0}";

            // Fade rating text
            if (ratingDisplayTimer > 0)
            {
                ratingDisplayTimer -= Time.deltaTime;
                if (ratingDisplayTimer <= 0 && ratingText != null)
                    ratingText.text = "";
            }

            // Exhaustion visual effects
            UpdateExhaustionVisuals();
        }

        void OnBeatResult(BeatRating rating)
        {
            if (ratingText != null)
            {
                switch (rating)
                {
                    case BeatRating.Perfect:
                        ratingText.text = "PERFECT!";
                        ratingText.color = GameColors.Gold;
                        comboCount++;
                        break;
                    case BeatRating.Good:
                        ratingText.text = "GOOD";
                        ratingText.color = GameColors.SkyBlue;
                        comboCount++;
                        break;
                    case BeatRating.Miss:
                        ratingText.text = "MISS";
                        ratingText.color = GameColors.DarkMagenta;
                        comboCount = 0;
                        break;
                }
                ratingDisplayTimer = 0.5f;
            }

            if (comboText != null)
            {
                comboText.text = comboCount > 1 ? $"x{comboCount}" : "";
            }

            // Flash effect
            if (flashOverlay != null)
            {
                Color flashColor = rating == BeatRating.Perfect ? GameColors.Gold :
                                   rating == BeatRating.Good ? GameColors.SkyBlue :
                                   GameColors.DarkMagenta;
                flashColor.a = 0.3f;
                flashOverlay.color = flashColor;
                LeanTweenFallback(flashOverlay, 0f, 0.3f);
            }
        }

        void OnExhaustionChanged(float value)
        {
            if (exhaustionFill != null)
            {
                float percent = value / GameConstants.ExhaustionStart;
                exhaustionFill.fillAmount = percent;

                // Color transition: green -> yellow -> red
                if (percent > 0.5f)
                    exhaustionFill.color = Color.Lerp(Color.yellow, GameColors.ForestGreen, (percent - 0.5f) * 2f);
                else
                    exhaustionFill.color = Color.Lerp(Color.red, Color.yellow, percent * 2f);
            }
        }

        void UpdateExhaustionVisuals()
        {
            float percent = rhythmEngine.ExhaustionPercent;

            // Vignette overlay increases as exhaustion drops
            if (vignetteOverlay != null)
            {
                Color c = vignetteOverlay.color;
                c.a = Mathf.Lerp(0.7f, 0f, percent);
                vignetteOverlay.color = c;
            }
        }

        void OnBeatApproaching(BeatData beat)
        {
            // Highlight the correct key
            if (leftKeyText != null)
                leftKeyText.color = beat.inputKey == KeyCode.Q ? GameColors.Gold : Color.white;
            if (rightKeyText != null)
                rightKeyText.color = beat.inputKey == KeyCode.E ? GameColors.Gold : Color.white;
        }

        void OnVision(VisionChoice[] choices)
        {
            if (visionPanel != null)
            {
                visionPanel.SetActive(true);
                for (int i = 0; i < choices.Length && i < visionChoiceTexts.Length; i++)
                {
                    visionChoiceTexts[i].text = $"[{choices[i].label}] {choices[i].text}";
                }
            }
        }

        public void SelectVision(int index)
        {
            if (visionPanel != null)
                visionPanel.SetActive(false);
        }

        void OnComplete()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            comboCount = 0;
        }

        public void Hide()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        // Simple alpha tween fallback (no DOTween dependency)
        void LeanTweenFallback(Image img, float targetAlpha, float duration)
        {
            StartCoroutine(FadeImage(img, targetAlpha, duration));
        }

        System.Collections.IEnumerator FadeImage(Image img, float target, float dur)
        {
            float start = img.color.a;
            float timer = 0f;
            while (timer < dur)
            {
                timer += Time.deltaTime;
                Color c = img.color;
                c.a = Mathf.Lerp(start, target, timer / dur);
                img.color = c;
                yield return null;
            }
        }
    }
}
