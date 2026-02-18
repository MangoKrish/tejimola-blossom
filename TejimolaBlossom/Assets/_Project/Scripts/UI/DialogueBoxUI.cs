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
        private bool initialized;

        void Awake()
        {
            // Initialize CanvasGroup here so it works even when the GO starts inactive
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Hide via alpha so we don't deactivate ourselves
            HideImmediate();
        }

        void OnEnable()
        {
            // Subscribe each time we become active (safe to call multiple times)
            EnsureInitialized();
        }

        void OnDisable()
        {
            UnsubscribeFromDialogue();
        }

        void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;

            dialogueManager = DialogueManager.Instance;
            SubscribeToDialogue();

            // Setup choice buttons
            if (choiceButtons != null)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] == null) continue;
                    int index = i;
                    choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
                }
            }
        }

        void SubscribeToDialogue()
        {
            if (dialogueManager == null) return;
            dialogueManager.OnDialogueLineStarted += OnLineStarted;
            dialogueManager.OnTextUpdated += OnTextUpdated;
            dialogueManager.OnChoicesPresented += OnChoicesPresented;
            dialogueManager.OnConversationEnded += OnConversationEnded;
            dialogueManager.OnSpeakerChanged += OnSpeakerChanged;
        }

        void UnsubscribeFromDialogue()
        {
            if (dialogueManager == null) return;
            dialogueManager.OnDialogueLineStarted -= OnLineStarted;
            dialogueManager.OnTextUpdated -= OnTextUpdated;
            dialogueManager.OnChoicesPresented -= OnChoicesPresented;
            dialogueManager.OnConversationEnded -= OnConversationEnded;
            dialogueManager.OnSpeakerChanged -= OnSpeakerChanged;
        }

        void OnDestroy()
        {
            UnsubscribeFromDialogue();
        }

        void OnLineStarted(DialogueEntry entry)
        {
            Show();
            choicePanel?.SetActive(false);
            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            if (speakerNameText != null)
            {
                speakerNameText.text = entry.speaker;
                speakerNameText.color = GetSpeakerColor(entry.speaker);
            }

            if (assameseText != null && !string.IsNullOrEmpty(entry.textAssamese))
            {
                assameseText.gameObject.SetActive(true);
                assameseText.text = entry.textAssamese;
            }
            else if (assameseText != null)
            {
                assameseText.gameObject.SetActive(false);
            }

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

            if (choiceButtons == null || choiceTexts == null) return;

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null) continue;
                if (i < choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    if (i < choiceTexts.Length && choiceTexts[i] != null)
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
            // Portrait animation could go here
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
            // Ensure we're initialized (handles case where GO was initially inactive)
            EnsureInitialized();

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }

        void Hide()
        {
            HideImmediate();
        }

        void HideImmediate()
        {
            // Use CanvasGroup alpha so we don't deactivate our own GameObject
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            choicePanel?.SetActive(false);
        }
    }
}
