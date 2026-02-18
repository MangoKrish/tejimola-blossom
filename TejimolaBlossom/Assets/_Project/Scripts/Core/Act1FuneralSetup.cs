using UnityEngine;
using Tejimola.Core;

namespace Tejimola.Scenes
{
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
}
