using UnityEngine;
using TMPro;

namespace Tejimola.Characters
{
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private string interactionPrompt = "Press E";
        [SerializeField] private bool oneTimeUse = false;

        [Header("Dialogue")]
        [SerializeField] private string dialogueId;

        [Header("Item")]
        [SerializeField] private string itemId;
        [SerializeField] private bool isCollectible;

        [Header("Visual")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private SpriteRenderer highlightRenderer;

        public bool CanInteract { get; set; } = true;
        public string DialogueId => dialogueId;
        public string ItemId => itemId;

        // Events
        public System.Action<CharacterController2D> OnInteracted;

        void Start()
        {
            if (promptUI != null)
                promptUI.SetActive(false);
        }

        public void OnInteract(CharacterController2D character)
        {
            if (!CanInteract) return;

            OnInteracted?.Invoke(character);

            // Collect item if applicable
            if (isCollectible && !string.IsNullOrEmpty(itemId))
            {
                Core.GameManager.Instance.CollectItem(itemId);
                if (oneTimeUse)
                {
                    CanInteract = false;
                    gameObject.SetActive(false);
                }
            }

            // Trigger dialogue if applicable
            if (!string.IsNullOrEmpty(dialogueId))
            {
                Core.EventManager.Instance.Publish<string>(Core.EventManager.Events.DialogueStarted, dialogueId);
            }

            if (oneTimeUse)
                CanInteract = false;
        }

        public void ShowPrompt()
        {
            if (promptUI != null)
                promptUI.SetActive(true);
            if (highlightRenderer != null)
                highlightRenderer.enabled = true;
        }

        public void HidePrompt()
        {
            if (promptUI != null)
                promptUI.SetActive(false);
            if (highlightRenderer != null)
                highlightRenderer.enabled = false;
        }
    }
}
