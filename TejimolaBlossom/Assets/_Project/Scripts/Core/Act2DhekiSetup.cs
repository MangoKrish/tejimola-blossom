using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Gameplay;

namespace Tejimola.Scenes
{
    public class Act2DhekiSetup : SceneSetup
    {
        [SerializeField] private RhythmEngine rhythmEngine;
        [SerializeField] private RhythmUI rhythmUI;

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
            EventManager.Instance.Subscribe(EventManager.Events.RhythmEnded, OnRhythmEnded);
        }

        void StartRhythmSequence()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, StartRhythmSequence);

            // Initialize RhythmUI with the engine before starting
            if (rhythmUI != null && rhythmEngine != null)
                rhythmUI.Initialize(rhythmEngine);

            if (rhythmEngine != null)
                rhythmEngine.StartRhythmSequence();
        }

        void OnRhythmEnded()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.RhythmEnded, OnRhythmEnded);
            Invoke(nameof(GoToBurial), 2f);
        }

        void GoToBurial()
        {
            SceneLoader.Instance.LoadSceneWithTitle("Act2_Burial", "THE BURIAL", 2f);
        }
    }
}
