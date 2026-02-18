using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Dialogue;

namespace Tejimola.Scenes
{
    /// <summary>
    /// Base class for scene initialization. Attach to an empty GameObject in each scene.
    /// Handles loading dialogue, setting music, configuring camera bounds, and triggering opening events.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] protected GameAct sceneAct;
        [SerializeField] protected string dialogueFile;
        [SerializeField] protected string openingConversationId;
        [SerializeField] protected string objectiveText;

        [Header("Camera Bounds")]
        [SerializeField] protected float cameraBoundsMinX = -20f;
        [SerializeField] protected float cameraBoundsMaxX = 20f;
        [SerializeField] protected float cameraBoundsMinY = -5f;
        [SerializeField] protected float cameraBoundsMaxY = 10f;

        [Header("Color Grading")]
        [SerializeField] protected Color ambientColor = Color.white;
        [SerializeField] protected float ambientIntensity = 1f;

        protected virtual void Start()
        {
            // Set game state
            GameManager.Instance.SetAct(sceneAct);

            // Load dialogue
            if (!string.IsNullOrEmpty(dialogueFile))
            {
                DialogueManager.Instance.LoadDialogueFile("Dialogue/" + dialogueFile);
            }

            // Set music
            AudioManager.Instance.PlayActMusic(sceneAct);

            // Configure camera
            var cam = FindFirstObjectByType<Camera.ParallaxCamera>();
            if (cam != null)
            {
                cam.SetBounds(cameraBoundsMinX, cameraBoundsMaxX, cameraBoundsMinY, cameraBoundsMaxY);
            }

            // Set ambient lighting
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;

            // Show objective
            if (!string.IsNullOrEmpty(objectiveText))
            {
                EventManager.Instance.Publish<string>(EventManager.Events.ObjectiveUpdated, objectiveText);
            }

            // Start opening dialogue after brief delay
            if (!string.IsNullOrEmpty(openingConversationId))
            {
                Invoke(nameof(StartOpeningDialogue), 1f);
            }

            OnSceneReady();
        }

        void StartOpeningDialogue()
        {
            EventManager.Instance.Publish<string>(EventManager.Events.DialogueStarted, openingConversationId);
        }

        protected virtual void OnSceneReady() { }
    }

}
