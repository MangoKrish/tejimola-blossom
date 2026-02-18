using System.Collections.Generic;
using UnityEngine;
using Tejimola.Core;

namespace Tejimola.Gameplay
{
    public class MemoryPuzzle : MonoBehaviour
    {
        [Header("Puzzle Config")]
        [SerializeField] private PuzzleType puzzleType;
        [SerializeField] private string puzzleName;
        [SerializeField] private string puzzleDescription;

        [Header("Present Day Object")]
        [SerializeField] private GameObject presentObject;
        [SerializeField] private SpriteRenderer presentRenderer;

        [Header("Memory (Past) Elements")]
        [SerializeField] private GameObject memoryFlashback;
        [SerializeField] private SpriteRenderer[] memorySprites;

        [Header("Environment Change")]
        [SerializeField] private GameObject beforeState;
        [SerializeField] private GameObject afterState;
        [SerializeField] private ParticleSystem transformVFX;

        [Header("Split Screen")]
        [SerializeField] private UnityEngine.Camera pastCamera;
        [SerializeField] private UnityEngine.Camera presentCamera;

        [Header("Interaction")]
        [SerializeField] private List<PuzzleStep> steps = new List<PuzzleStep>();
        private int currentStepIndex;

        public PuzzleType Type => puzzleType;
        public bool IsSolved { get; private set; }
        public string PuzzleName => puzzleName;

        public System.Action<int, int> OnStepProgress;
        public System.Action OnPuzzleCompleted;

        public void StartPuzzle()
        {
            currentStepIndex = 0;

            if (pastCamera != null && presentCamera != null)
            {
                EnableSplitScreen();
            }

            if (memoryFlashback != null)
                memoryFlashback.SetActive(true);

            ApplyPastVisuals();

            if (steps.Count > 0)
                steps[0].Activate();

            EventManager.Instance.Publish<string>(EventManager.Events.MemoryTriggered, puzzleName);
        }

        public void AdvanceStep()
        {
            if (currentStepIndex < steps.Count)
                steps[currentStepIndex].Complete();

            currentStepIndex++;
            OnStepProgress?.Invoke(currentStepIndex, steps.Count);

            if (currentStepIndex >= steps.Count)
            {
                GameManager.Instance.SolvePuzzle(puzzleType);
            }
            else
            {
                steps[currentStepIndex].Activate();
            }
        }

        public void OnSolved()
        {
            IsSolved = true;

            if (beforeState != null) beforeState.SetActive(false);
            if (afterState != null) afterState.SetActive(true);
            if (transformVFX != null) transformVFX.Play();

            if (pastCamera != null)
                pastCamera.gameObject.SetActive(false);

            if (memoryFlashback != null)
                memoryFlashback.SetActive(false);

            OnPuzzleCompleted?.Invoke();
        }

        public void SetSolvedState()
        {
            IsSolved = true;
            if (beforeState != null) beforeState.SetActive(false);
            if (afterState != null) afterState.SetActive(true);
        }

        void EnableSplitScreen()
        {
            if (pastCamera != null)
            {
                pastCamera.gameObject.SetActive(true);
                pastCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
            }
            if (presentCamera != null)
            {
                presentCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            }
        }

        void ApplyPastVisuals()
        {
            Color sepiaColor = new Color(0.94f, 0.87f, 0.73f, 0.8f);
            foreach (var sprite in memorySprites)
            {
                if (sprite != null)
                    sprite.color = sepiaColor;
            }
        }
    }

    [System.Serializable]
    public class PuzzleStep
    {
        public string description;
        public GameObject targetObject;
        public string requiredItemId;
        public string interactionType;
        public bool isActive;
        public bool isComplete;

        public void Activate()
        {
            isActive = true;
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                var renderer = targetObject.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }

        public void Complete()
        {
            isComplete = true;
            isActive = false;
        }
    }

    public class WellPuzzle : MemoryPuzzle { }

    public class HairpinPuzzle : MemoryPuzzle { }
}
