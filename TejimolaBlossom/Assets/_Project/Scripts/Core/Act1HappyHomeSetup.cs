using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Scenes
{
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
            SaveManager.Instance.SaveGame();
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, OnDialogueEnded);
        }

        void OnDialogueEnded()
        {
            // After ranima_intro dialogue, transition to the funeral
            if (GameManager.Instance.CollectedItems.Contains("hairpin"))
            {
                EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, OnDialogueEnded);
                SceneLoader.Instance.LoadSceneWithTitle("Act1_Funeral", "THE PASSING", 2f);
            }
        }
    }
}
