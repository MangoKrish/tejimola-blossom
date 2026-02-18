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
            objectiveText = "";
            ambientColor = new Color(0.1f, 0.1f, 0.15f);
            ambientIntensity = 0.5f;
            base.Start();
        }
    }
}
