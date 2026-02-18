using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Gameplay;

namespace Tejimola.Scenes
{
    public class Act3DualTimelineSetup : SceneSetup
    {
        [SerializeField] private PuzzleManager puzzleManager;

        protected override void Start()
        {
            sceneAct = GameAct.Act3_DualTimeline;
            dialogueFile = "act3_dialogue";
            openingConversationId = "act3_dual_intro";
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
}
