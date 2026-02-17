using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Dialogue;
using Tejimola.Gameplay;

namespace Tejimola.Scenes
{
    /// <summary>
    /// Base class for scene initialization. Attach to an empty GameObject in each scene.
    /// Handles loading dialogue, setting music, configuring camera bounds, and triggering opening events.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] protected GameAct sceneAct;
        [SerializeField] protected string dialogueFile;
        [SerializeField] protected string openingConversationId;
        [SerializeField] protected string objectiveText;

        [Header("Camera Bounds")]
        [SerializeField] protected float cameraBoundsMinX = -20f;
        [SerializeField] protected float cameraBoundsMaxX = 20f;
        [SerializeField] protected float cameraBoundsMinY = -5f;
        [SerializeField] protected float cameraBoundsMaxY = 10f;

        [Header("Color Grading")]
        [SerializeField] protected Color ambientColor = Color.white;
        [SerializeField] protected float ambientIntensity = 1f;

        protected virtual void Start()
        {
            // Set game state
            GameManager.Instance.SetAct(sceneAct);

            // Load dialogue
            if (!string.IsNullOrEmpty(dialogueFile))
            {
                DialogueManager.Instance.LoadDialogueFile("Dialogue/" + dialogueFile);
            }

            // Set music
            AudioManager.Instance.PlayActMusic(sceneAct);

            // Configure camera
            var cam = FindFirstObjectByType<Camera.ParallaxCamera>();
            if (cam != null)
            {
                cam.SetBounds(cameraBoundsMinX, cameraBoundsMaxX, cameraBoundsMinY, cameraBoundsMaxY);
            }

            // Set ambient lighting
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;

            // Show objective
            if (!string.IsNullOrEmpty(objectiveText))
            {
                EventManager.Instance.Publish<string>(EventManager.Events.ObjectiveUpdated, objectiveText);
            }

            // Start opening dialogue after brief delay
            if (!string.IsNullOrEmpty(openingConversationId))
            {
                Invoke(nameof(StartOpeningDialogue), 1f);
            }

            OnSceneReady();
        }

        void StartOpeningDialogue()
        {
            EventManager.Instance.Publish<string>(EventManager.Events.DialogueStarted, openingConversationId);
        }

        protected virtual void OnSceneReady() { }
    }

    // ============ Act-Specific Scene Setups ============

    public class Act1HappyHomeSetup : SceneSetup
    {
        protected override void Start()
        {
            sceneAct = GameAct.Act1_HappyHome;
            dialogueFile = "act1_dialogue";
            openingConversationId = "act1_opening";
            objectiveText = "Explore your home";
            ambientColor = GameColors.Gold;
            ambientIntensity = 1.2f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            // Auto-save at act start
            SaveManager.Instance.SaveGame();
        }
    }

    public class Act1FuneralSetup : SceneSetup
    {
        protected override void Start()
        {
            sceneAct = GameAct.Act1_Funeral;
            dialogueFile = "act1_dialogue";
            openingConversationId = "act1_funeral";
            objectiveText = "Attend the funeral";
            ambientColor = new Color(0.7f, 0.7f, 0.8f);
            ambientIntensity = 0.8f;
            base.Start();
        }
    }

    public class Act2DescentSetup : SceneSetup
    {
        [SerializeField] private StealthManager stealthManager;

        protected override void Start()
        {
            sceneAct = GameAct.Act2_Descent;
            dialogueFile = "act2_dialogue";
            openingConversationId = "act2_descent_begin";
            objectiveText = "Survive Ranima's watch";
            ambientColor = GameColors.DarkSlate;
            ambientIntensity = 0.6f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            // Start stealth after opening dialogue ends
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, StartStealthPhase);
        }

        void StartStealthPhase()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, StartStealthPhase);
            if (stealthManager != null)
                stealthManager.StartStealth();
            EventManager.Instance.Publish<string>(EventManager.Events.ObjectiveUpdated, "Avoid Ranima. Hide when she approaches.");
        }
    }

    public class Act2DhekiSetup : SceneSetup
    {
        [SerializeField] private RhythmEngine rhythmEngine;

        protected override void Start()
        {
            sceneAct = GameAct.Act2_Dheki;
            dialogueFile = "act2_dialogue";
            openingConversationId = "act2_dheki_intro";
            objectiveText = "Work the dheki";
            ambientColor = new Color(0.5f, 0.4f, 0.3f);
            ambientIntensity = 0.7f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, StartRhythmSequence);
        }

        void StartRhythmSequence()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, StartRhythmSequence);
            if (rhythmEngine != null)
                rhythmEngine.StartRhythmSequence();
        }
    }

    public class Act3DomArrivalSetup : SceneSetup
    {
        protected override void Start()
        {
            sceneAct = GameAct.Act3_DomArrival;
            dialogueFile = "act3_dialogue";
            openingConversationId = "act3_dom_arrival";
            objectiveText = "Explore the ruins. Use Spirit Pulse [SPACE] to reveal the past.";
            ambientColor = new Color(0.3f, 0.2f, 0.4f);
            ambientIntensity = 0.5f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public class Act3DualTimelineSetup : SceneSetup
    {
        [SerializeField] private PuzzleManager puzzleManager;

        protected override void Start()
        {
            sceneAct = GameAct.Act3_DualTimeline;
            dialogueFile = "act3_dialogue";
            objectiveText = "Recover Tejimola's memories (0/5)";
            ambientColor = new Color(0.3f, 0.2f, 0.5f);
            ambientIntensity = 0.5f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            EventManager.Instance.Subscribe<PuzzleType>(EventManager.Events.PuzzleSolved, OnPuzzleSolved);
        }

        void OnPuzzleSolved(PuzzleType type)
        {
            int solved = puzzleManager != null ? puzzleManager.SolvedCount : 0;
            EventManager.Instance.Publish<string>(EventManager.Events.ObjectiveUpdated,
                $"Recover Tejimola's memories ({solved}/{GameConstants.TotalPuzzles})");

            if (solved >= GameConstants.TotalPuzzles)
            {
                // Trigger final dialogue then transition
                EventManager.Instance.Publish<string>(EventManager.Events.DialogueStarted, "act3_all_puzzles_solved");
                EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, TransitionToAct4);
            }
        }

        void TransitionToAct4()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, TransitionToAct4);
            SceneLoader.Instance.LoadSceneWithTitle("Act4_Confrontation", "ACT IV: CONFRONTATION", 3f);
        }
    }

    public class Act4ConfrontationSetup : SceneSetup
    {
        [SerializeField] private BossController bossController;

        protected override void Start()
        {
            sceneAct = GameAct.Act4_Confrontation;
            dialogueFile = "act4_epilogue_dialogue";
            openingConversationId = "act4_confrontation";
            objectiveText = "Face the corruption";
            ambientColor = GameColors.DarkMagenta;
            ambientIntensity = 0.4f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, StartBossFight);
            SaveManager.Instance.SaveGame();
        }

        void StartBossFight()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, StartBossFight);
            if (bossController != null)
                bossController.StartBossFight();
        }
    }

    public class EpilogueSetup : SceneSetup
    {
        protected override void Start()
        {
            sceneAct = GameAct.Epilogue;
            dialogueFile = "act4_epilogue_dialogue";
            openingConversationId = "epilogue_sunrise";
            ambientColor = new Color(1f, 0.9f, 0.7f);
            ambientIntensity = 1.5f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            // After epilogue dialogue, show credits
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, ShowCredits);
        }

        void ShowCredits()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, ShowCredits);
            // Return to main menu after a delay
            Invoke(nameof(ReturnToMenu), 5f);
        }

        void ReturnToMenu()
        {
            SceneLoader.Instance.LoadScene("MainMenu", 3f);
        }
    }
}
