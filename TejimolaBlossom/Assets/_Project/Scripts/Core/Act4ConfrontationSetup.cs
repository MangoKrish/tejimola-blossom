using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Gameplay;

namespace Tejimola.Scenes
{
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
}
