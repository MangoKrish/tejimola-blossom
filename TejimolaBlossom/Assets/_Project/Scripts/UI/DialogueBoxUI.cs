using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tejimola.Core;
using Tejimola.Dialogue;
using Tejimola.Utils;

namespace Tejimola.UI
{
    public class DialogueBoxUI : MonoBehaviour
    {
        [Header("Dialogue Box")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image dialogueBackground;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI assameseText;

        [Header("Portrait")]
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private Image portraitFrame;

        [Header("Choices")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private TextMeshProUGUI[] choiceTexts;

        [Header("Continue Indicator")]
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private TextMeshProUGUI continueText;

        [Header("Portraits")]
        [SerializeField] private Sprite tejimolPortrait;
        [SerializeField] private Sprite domPortrait;
        [SerializeField] private Sprite fatherPortrait;
        [SerializeField] private Sprite ranimaPortrait;
        [SerializeField] private Sprite elderPortrait;
        [SerializeField] private Sprite narratorPortrait;

        private DialogueManager dialogueManager;
        private CanvasGroup canvasGroup;

        void Start()
        {
            dialogueManager = DialogueManager.Instance;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Subscribe to dialogue events
            dialogueManager.OnDialogueLineStarted += OnLineStarted;
            dialogueManager.OnTextUpdated += OnTextUpdated;
            dialogueManager.OnChoicesPresented += OnChoicesPresented;
            dialogueManager.OnConversationEnded += OnConversationEnded;
            dialogueManager.OnSpeakerChanged += OnSpeakerChanged;

            // Setup choice buttons
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i; // Capture for closure
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }

            Hide();
        }

        void OnDestroy()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLineStarted -= OnLineStarted;
                dialogueManager.OnTextUpdated -= OnTextUpdated;
                dialogueManager.OnChoicesPresented -= OnChoicesPresented;
                dialogueManager.OnConversationEnded -= OnConversationEnded;
                dialogueManager.OnSpeakerChanged -= OnSpeakerChanged;
            }
        }

        void OnLineStarted(DialogueEntry entry)
        {
            Show();
            choicePanel?.SetActive(false);
            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            // Set speaker name with color
            if (speakerNameText != null)
            {
                speakerNameText.text = entry.speaker;
                speakerNameText.color = GetSpeakerColor(entry.speaker);
            }

            // Set Assamese text if available
            if (assameseText != null && !string.IsNullOrEmpty(entry.textAssamese))
            {
                assameseText.gameObject.SetActive(true);
                assameseText.text = entry.textAssamese;
            }
            else if (assameseText != null)
            {
                assameseText.gameObject.SetActive(false);
            }

            // Set portrait
            SetPortrait(entry.speaker, entry.emotion);
        }

        void OnTextUpdated(string text)
        {
            if (dialogueText != null)
                dialogueText.text = text;
        }

        void OnChoicesPresented(DialogueChoice[] choices)
        {
            if (choicePanel == null) return;
            choicePanel.SetActive(true);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceTexts[i].text = $"[{(char)('A' + i)}] {choices[i].text}";
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        void OnConversationEnded()
        {
            Hide();
        }

        void OnSpeakerChanged(string speaker)
        {
            // Could trigger portrait animation here
        }

        void OnChoiceSelected(int index)
        {
            DialogueManager.Instance.SelectChoice(index);
            choicePanel?.SetActive(false);
        }

        void SetPortrait(string speaker, string emotion)
        {
            if (speakerPortrait == null) return;

            Sprite portrait = speaker?.ToLower() switch
            {
                "tejimola" => tejimolPortrait,
                "dom" => domPortrait,
                "father" or "baba" => fatherPortrait,
                "ranima" or "stepmother" => ranimaPortrait,
                "elder" or "village elder" => elderPortrait,
                _ => narratorPortrait
            };

            if (portrait != null)
            {
                speakerPortrait.sprite = portrait;
                speakerPortrait.gameObject.SetActive(true);
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }

            // Emotion-based tinting
            if (speakerPortrait != null && !string.IsNullOrEmpty(emotion))
            {
                Color emotionTint = emotion.ToLower() switch
                {
                    "happy" => GameColors.Gold,
                    "sad" => GameColors.SkyBlue,
                    "angry" => GameColors.TriumphRed,
                    "fearful" => GameColors.DarkSlate,
                    _ => Color.white
                };
                // Slight tint, not full replacement
                speakerPortrait.color = Color.Lerp(Color.white, emotionTint, 0.2f);
            }
        }

        Color GetSpeakerColor(string speaker)
        {
            return speaker?.ToLower() switch
            {
                "tejimola" => GameColors.Gold,
                "dom" => GameColors.SpiritPurple,
                "father" or "baba" => GameColors.EarthBrown,
                "ranima" or "stepmother" => GameColors.DarkMagenta,
                "elder" => GameColors.Silver,
                "narrator" => GameColors.SkyBlue,
                _ => Color.white
            };
        }

        void Show()
        {
            dialoguePanel?.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        void Hide()
        {
            dialoguePanel?.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }
    }
}
