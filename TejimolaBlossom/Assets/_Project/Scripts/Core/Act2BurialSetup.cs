using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Scenes
{
    public class Act2BurialSetup : SceneSetup
    {
        protected override void Start()
        {
            sceneAct = GameAct.Act2_Burial;
            dialogueFile = "act2_dialogue";
            openingConversationId = "act2_burial";
            objectiveText = "Witness the burial of Tejimola";
            ambientColor = new Color(0.1f, 0.1f, 0.15f);
            ambientIntensity = 0.5f;
            base.Start();
        }

        protected override void OnSceneReady()
        {
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, OnBurialDialogueEnded);
        }

        void OnBurialDialogueEnded()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, OnBurialDialogueEnded);
            Invoke(nameof(GoToAct3), 3f);
        }

        void GoToAct3()
        {
            SceneLoader.Instance.LoadSceneWithTitle("Act3_DomArrival", "ACT III: THE DRUMMER", 3f);
        }
    }
}
