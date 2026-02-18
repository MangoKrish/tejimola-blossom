using System.Collections.Generic;
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

        // Active beat notes moving toward hit zone
        private readonly List<RectTransform> activeNotes = new List<RectTransform>();
        private const float NoteSpawnY    = 200f;   // spawn y (top of track)
        private const float NoteHitY      = -200f;  // hit zone y (bottom)
        private const float NoteTravelTime = 1.5f;  // seconds to reach hit zone

        // Vision choices cached for forwarding to engine
        private VisionChoice[] cachedVisionChoices;

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
            rhythmEngine.OnBeatResult        += OnBeatResult;
            rhythmEngine.OnExhaustionChanged += OnExhaustionChanged;
            rhythmEngine.OnBeatApproaching   += OnBeatApproaching;
            rhythmEngine.OnRhythmComplete    += OnComplete;
            rhythmEngine.OnVisionTriggered   += OnVision;
        }

        void OnDestroy()
        {
            if (rhythmEngine != null)
            {
                rhythmEngine.OnBeatResult        -= OnBeatResult;
                rhythmEngine.OnExhaustionChanged -= OnExhaustionChanged;
                rhythmEngine.OnBeatApproaching   -= OnBeatApproaching;
                rhythmEngine.OnRhythmComplete    -= OnComplete;
                rhythmEngine.OnVisionTriggered   -= OnVision;
            }
        }

        void Update()
        {
            if (rhythmEngine == null || !rhythmEngine.IsPlaying) return;

            if (bpmText != null)
                bpmText.text = $"BPM: {rhythmEngine.CurrentBPM:F0}";

            if (ratingDisplayTimer > 0)
            {
                ratingDisplayTimer -= Time.deltaTime;
                if (ratingDisplayTimer <= 0 && ratingText != null)
                    ratingText.text = "";
            }

            // Move active beat notes toward hit zone
            UpdateBeatNotes();

            UpdateExhaustionVisuals();

            // Vision choice keyboard input (1/2/3 while vision panel open)
            if (visionPanel != null && visionPanel.activeSelf && cachedVisionChoices != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectVision(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectVision(1);
                else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectVision(2);
            }
        }

        void OnBeatResult(BeatRating rating)
        {
            if (ratingText != null)
            {
                switch (rating)
                {
                    case BeatRating.Perfect:
                        ratingText.text  = "PERFECT!";
                        ratingText.color = GameColors.Gold;
                        comboCount++;
                        break;
                    case BeatRating.Good:
                        ratingText.text  = "GOOD";
                        ratingText.color = GameColors.SkyBlue;
                        comboCount++;
                        break;
                    case BeatRating.Miss:
                        ratingText.text  = "MISS";
                        ratingText.color = GameColors.DarkMagenta;
                        comboCount = 0;
                        break;
                }
                ratingDisplayTimer = 0.5f;
            }

            if (comboText != null)
                comboText.text = comboCount > 1 ? $"x{comboCount}" : "";

            if (flashOverlay != null)
            {
                Color flashColor = rating == BeatRating.Perfect ? GameColors.Gold :
                                   rating == BeatRating.Good    ? GameColors.SkyBlue :
                                                                  GameColors.DarkMagenta;
                flashColor.a = 0.3f;
                flashOverlay.color = flashColor;
                StartCoroutine(FadeImage(flashOverlay, 0f, 0.3f));
            }

            // Remove the oldest note (it was just hit or missed)
            if (activeNotes.Count > 0)
            {
                var note = activeNotes[0];
                activeNotes.RemoveAt(0);
                if (note != null)
                    Destroy(note.gameObject);
            }
        }

        void OnExhaustionChanged(float value)
        {
            if (exhaustionFill != null)
            {
                float percent = value / GameConstants.ExhaustionStart;
                exhaustionFill.fillAmount = percent;

                if (percent > 0.5f)
                    exhaustionFill.color = Color.Lerp(Color.yellow, GameColors.ForestGreen, (percent - 0.5f) * 2f);
                else
                    exhaustionFill.color = Color.Lerp(Color.red, Color.yellow, percent * 2f);
            }
        }

        void OnBeatApproaching(BeatData beat)
        {
            if (leftKeyText != null)
                leftKeyText.color = beat.inputKey == KeyCode.Q ? GameColors.Gold : Color.white;
            if (rightKeyText != null)
                rightKeyText.color = beat.inputKey == KeyCode.E ? GameColors.Gold : Color.white;

            SpawnBeatNote(beat);
        }

        void SpawnBeatNote(BeatData beat)
        {
            if (beatNotePrefab == null) return;

            bool isLeft = beat.inputKey == KeyCode.Q;
            RectTransform track = isLeft ? beatTrackLeft : beatTrackRight;
            if (track == null) return;

            var noteObj = Instantiate(beatNotePrefab, track);
            var noteRT  = noteObj.GetComponent<RectTransform>();
            if (noteRT == null) noteRT = noteObj.AddComponent<RectTransform>();

            noteRT.anchoredPosition = new Vector2(0, NoteSpawnY);
            noteRT.sizeDelta        = new Vector2(60, 60);

            var img = noteObj.GetComponent<Image>();
            if (img != null)
            {
                img.color = beat.difficulty switch
                {
                    1 => GameColors.Gold,
                    2 => GameColors.SkyBlue,
                    3 => Color.yellow,
                    _ => GameColors.TriumphRed
                };
            }

            activeNotes.Add(noteRT);
        }

        void UpdateBeatNotes()
        {
            float speed = (NoteSpawnY - NoteHitY) / NoteTravelTime;

            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                var note = activeNotes[i];
                if (note == null)
                {
                    activeNotes.RemoveAt(i);
                    continue;
                }

                note.anchoredPosition += Vector2.down * speed * Time.deltaTime;

                // Destroy if past hit zone
                if (note.anchoredPosition.y < NoteHitY - 80f)
                {
                    activeNotes.RemoveAt(i);
                    Destroy(note.gameObject);
                }
            }
        }

        void UpdateExhaustionVisuals()
        {
            float percent = rhythmEngine.ExhaustionPercent;

            if (vignetteOverlay != null)
            {
                Color c = vignetteOverlay.color;
                c.a = Mathf.Lerp(0.7f, 0f, percent);
                vignetteOverlay.color = c;
            }
        }

        void OnVision(VisionChoice[] choices)
        {
            cachedVisionChoices = choices;
            if (visionPanel != null)
            {
                visionPanel.SetActive(true);
                if (visionChoiceTexts != null)
                {
                    for (int i = 0; i < choices.Length && i < visionChoiceTexts.Length; i++)
                    {
                        if (visionChoiceTexts[i] != null)
                            visionChoiceTexts[i].text = $"[{i+1}] {choices[i].text}";
                    }
                }
            }
        }

        public void SelectVision(int index)
        {
            visionPanel?.SetActive(false);

            // Forward selection to engine so it records the story flag
            if (rhythmEngine != null && cachedVisionChoices != null)
                rhythmEngine.SelectVisionChoice(index, cachedVisionChoices);

            cachedVisionChoices = null;
        }

        void OnComplete()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
            comboCount = 0;
            activeNotes.Clear();
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);

            foreach (var note in activeNotes)
                if (note != null) Destroy(note.gameObject);
            activeNotes.Clear();
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
