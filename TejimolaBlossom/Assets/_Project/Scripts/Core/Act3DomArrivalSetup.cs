using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Scenes
{
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
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, OnFirstEncounterEnded);
        }

        void OnFirstEncounterEnded()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, OnFirstEncounterEnded);
            Invoke(nameof(GoToDualTimeline), 2f);
        }

        void GoToDualTimeline()
        {
            SceneLoader.Instance.LoadSceneWithTitle("Act3_DualTimeline", "THE MEMORIES", 2f);
        }
    }
}
