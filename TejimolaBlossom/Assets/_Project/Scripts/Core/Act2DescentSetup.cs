using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Gameplay;

namespace Tejimola.Scenes
{
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
            EventManager.Instance.Subscribe(EventManager.Events.DialogueEnded, StartStealthPhase);
            EventManager.Instance.Subscribe(EventManager.Events.StealthComplete, OnStealthComplete);
        }

        void StartStealthPhase()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.DialogueEnded, StartStealthPhase);
            if (stealthManager != null)
                stealthManager.StartStealth();
            EventManager.Instance.Publish<string>(EventManager.Events.ObjectiveUpdated, "Avoid Ranima. Hide when she approaches.");
        }

        void OnStealthComplete()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.StealthComplete, OnStealthComplete);
            SceneLoader.Instance.LoadSceneWithTitle("Act2_Dheki", "THE DHEKI", 2f);
        }
    }
}
