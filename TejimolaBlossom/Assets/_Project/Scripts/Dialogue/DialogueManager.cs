using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("DialogueManager");
                    _instance = go.AddComponent<DialogueManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Dialogue Data")]
        private Dictionary<string, DialogueConversation> conversations = new Dictionary<string, DialogueConversation>();
        private DialogueConversation currentConversation;
        private DialogueEntry currentEntry;
        private int currentEntryIndex;

        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.03f;
        [SerializeField] private float autoAdvanceDelay = 2f;

        // State
        private bool isPlaying;
        private bool isTyping;
        private bool waitingForChoice;
        private Coroutine typingCoroutine;

        // Events
        public System.Action<DialogueEntry> OnDialogueLineStarted;
        public System.Action<string> OnTextUpdated;
        public System.Action OnDialogueLineEnded;
        public System.Action<DialogueChoice[]> OnChoicesPresented;
        public System.Action OnConversationEnded;
        public System.Action<string> OnSpeakerChanged;

        public bool IsPlaying => isPlaying;
        public bool IsTyping => isTyping;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            EventManager.Instance.Subscribe<string>(EventManager.Events.DialogueStarted, StartConversation);
        }

        void OnDestroy()
        {
            EventManager.Instance.Unsubscribe<string>(EventManager.Events.DialogueStarted, StartConversation);
        }

        void Update()
        {
            if (!isPlaying) return;

            // Advance dialogue on click/key
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // Skip to full text
                    CompleteTyping();
                }
                else if (!waitingForChoice)
                {
                    AdvanceDialogue();
                }
            }
        }

        public void LoadDialogueFile(string resourcePath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"Dialogue file not found: {resourcePath}");
                return;
            }

            DialogueFile file = JsonUtility.FromJson<DialogueFile>(textAsset.text);
            if (file != null)
            {
                foreach (var conv in file.conversations)
                {
                    conversations[conv.id] = conv;
                }
            }
        }

        public void LoadDialogueFromJson(string json)
        {
            DialogueFile file = JsonUtility.FromJson<DialogueFile>(json);
            if (file != null)
            {
                foreach (var conv in file.conversations)
                {
                    conversations[conv.id] = conv;
                }
            }
        }

        public void StartConversation(string conversationId)
        {
            if (!conversations.ContainsKey(conversationId))
            {
                Debug.LogWarning($"Conversation not found: {conversationId}");
                return;
            }

            currentConversation = conversations[conversationId];
            currentEntryIndex = 0;
            isPlaying = true;

            GameManager.Instance.SetPhase(GamePhase.Dialogue);
            FindFirstObjectByType<Characters.CharacterController2D>()?.SetCanMove(false);

            PlayCurrentEntry();
        }

        void PlayCurrentEntry()
        {
            if (currentConversation == null || currentEntryIndex >= currentConversation.entries.Count)
            {
                EndConversation();
                return;
            }

            currentEntry = currentConversation.entries[currentEntryIndex];

            // Notify UI
            OnDialogueLineStarted?.Invoke(currentEntry);
            OnSpeakerChanged?.Invoke(currentEntry.speaker);

            // Play audio if available
            if (!string.IsNullOrEmpty(currentEntry.audioFile))
            {
                AudioClip clip = AudioManager.Instance.LoadClip("Audio/Voice/" + currentEntry.audioFile);
                if (clip != null)
                    AudioManager.Instance.PlaySFX(clip);
            }

            // Start typing text
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(currentEntry.text));
        }

        IEnumerator TypeText(string fullText)
        {
            isTyping = true;
            string displayText = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                displayText += fullText[i];
                OnTextUpdated?.Invoke(displayText);
                yield return new WaitForSecondsRealtime(textSpeed); // unscaled — works during pause
            }

            isTyping = false;

            // Check for choices
            if (currentEntry != null && currentEntry.choices != null && currentEntry.choices.Count > 0)
            {
                waitingForChoice = true;
                OnChoicesPresented?.Invoke(currentEntry.choices.ToArray());
            }
        }

        void CompleteTyping()
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;
            if (currentEntry == null) return; // guard against race condition

            OnTextUpdated?.Invoke(currentEntry.text);

            if (currentEntry.choices != null && currentEntry.choices.Count > 0)
            {
                waitingForChoice = true;
                OnChoicesPresented?.Invoke(currentEntry.choices.ToArray());
            }
        }

        void AdvanceDialogue()
        {
            if (currentEntry == null) return; // guard against race condition

            OnDialogueLineEnded?.Invoke();

            if (!string.IsNullOrEmpty(currentEntry.next))
            {
                // Jump to specific entry by id
                int nextIndex = currentConversation.entries.FindIndex(e => e.id == currentEntry.next);
                if (nextIndex >= 0)
                {
                    currentEntryIndex = nextIndex;
                    PlayCurrentEntry();
                    return;
                }
            }

            // Sequential advance
            currentEntryIndex++;
            PlayCurrentEntry();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (currentEntry == null || currentEntry.choices == null) return;
            if (choiceIndex < 0 || choiceIndex >= currentEntry.choices.Count) return;

            DialogueChoice choice = currentEntry.choices[choiceIndex];
            waitingForChoice = false;

            // Set story flag if applicable
            if (!string.IsNullOrEmpty(choice.flag))
            {
                GameManager.Instance.SetStoryFlag(choice.flag, true);
            }

            EventManager.Instance.Publish<string>(EventManager.Events.DialogueChoice, choice.flag ?? "");

            // Navigate to next entry
            if (!string.IsNullOrEmpty(choice.next))
            {
                int nextIndex = currentConversation.entries.FindIndex(e => e.id == choice.next);
                if (nextIndex >= 0)
                {
                    currentEntryIndex = nextIndex;
                    PlayCurrentEntry();
                    return;
                }
            }

            // Otherwise advance normally
            currentEntryIndex++;
            PlayCurrentEntry();
        }

        void EndConversation()
        {
            isPlaying = false;
            currentConversation = null;
            currentEntry = null;

            OnConversationEnded?.Invoke();
            GameManager.Instance.SetPhase(GamePhase.Exploration);
            FindFirstObjectByType<Characters.CharacterController2D>()?.SetCanMove(true);

            EventManager.Instance.Publish(EventManager.Events.DialogueEnded);
        }

        public void ForceEndDialogue()
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            EndConversation();
        }
    }

    // Data structures
    [System.Serializable]
    public class DialogueFile
    {
        public List<DialogueConversation> conversations = new List<DialogueConversation>();
    }

    [System.Serializable]
    public class DialogueConversation
    {
        public string id;
        public string act;
        public List<DialogueEntry> entries = new List<DialogueEntry>();
    }

    [System.Serializable]
    public class DialogueEntry
    {
        public string id;
        public string speaker;
        public string text;
        public string textAssamese;     // Assamese version
        public string audioFile;
        public string next;             // Next entry id
        public string emotion;          // happy, sad, angry, fearful
        public List<DialogueChoice> choices;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string text;
        public string next;     // Entry id to jump to
        public string flag;     // Story flag to set
    }
}
