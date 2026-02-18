using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Scenes
{
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
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, ShowCredits);
        }

        void ShowCredits()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, ShowCredits);
            Invoke(nameof(ReturnToMenu), 5f);
        }

        void ReturnToMenu()
        {
            SceneLoader.Instance.LoadScene("MainMenu", 3f);
        }
    }
}
