using System.Collections.Generic;
using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
    public class PuzzleManager : MonoBehaviour
    {
        [Header("Puzzles")]
        [SerializeField] private List<MemoryPuzzle> puzzles = new List<MemoryPuzzle>();

        private MemoryPuzzle activePuzzle;
        private int solvedCount;

        public int SolvedCount => solvedCount;
        public int TotalPuzzles => puzzles.Count;
        public bool AllSolved => solvedCount >= GameConstants.TotalPuzzles;

        void Start()
        {
            EventManager.Instance.Subscribe<PuzzleType>(EventManager.Events.PuzzleSolved, OnPuzzleSolved);

            // Restore already-solved puzzles from save
            foreach (var puzzle in puzzles)
            {
                if (GameManager.Instance.SolvedPuzzles.Contains(puzzle.Type))
                {
                    puzzle.SetSolvedState();
                    solvedCount++;
                }
            }
        }

        void OnDestroy()
        {
            EventManager.Instance.Unsubscribe<PuzzleType>(EventManager.Events.PuzzleSolved, OnPuzzleSolved);
        }

        public void ActivatePuzzle(MemoryPuzzle puzzle)
        {
            if (puzzle.IsSolved) return;

            activePuzzle = puzzle;
            GameManager.Instance.SetPhase(GamePhase.Puzzle);
            puzzle.StartPuzzle();
            EventManager.Instance.Publish<PuzzleType>(EventManager.Events.PuzzleStarted, puzzle.Type);
        }

        void OnPuzzleSolved(PuzzleType type)
        {
            solvedCount++;
            if (activePuzzle != null)
            {
                activePuzzle.OnSolved();
                activePuzzle = null;
            }
            GameManager.Instance.SetPhase(GamePhase.Exploration);

            // Check if all puzzles solved
            if (AllSolved)
            {
                // Trigger Act IV transition
                EventManager.Instance.Publish(EventManager.Events.EnvironmentChanged);
            }
        }
    }

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

        // Events
        public System.Action<int, int> OnStepProgress; // currentStep, totalSteps
        public System.Action OnPuzzleCompleted;

        public void StartPuzzle()
        {
            currentStepIndex = 0;

            // Enable split-screen if applicable
            if (pastCamera != null && presentCamera != null)
            {
                EnableSplitScreen();
            }

            // Show memory flashback
            if (memoryFlashback != null)
                memoryFlashback.SetActive(true);

            // Apply sepia/misty effect to past view
            ApplyPastVisuals();

            // Start first step
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
                // Puzzle solved!
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

            // Transform environment
            if (beforeState != null) beforeState.SetActive(false);
            if (afterState != null) afterState.SetActive(true);
            if (transformVFX != null) transformVFX.Play();

            // Disable split screen
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
            // Apply sepia-washed, misty look to past memory sprites
            Color sepiaColor = new Color(0.94f, 0.87f, 0.73f, 0.8f); // Warm sepia
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
        public string requiredItemId;    // Item needed from inventory
        public string interactionType;   // "examine", "place", "combine"
        public bool isActive;
        public bool isComplete;

        public void Activate()
        {
            isActive = true;
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                // Add highlight effect
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

    // Specific puzzle implementations
    public class WellPuzzle : MemoryPuzzle
    {
        // Well puzzle: dry well in present, pull up memories
        // Past: Tejimola's secret spot her father showed her
        // Result: Well fills with spirit water
    }

    public class HairpinPuzzle : MemoryPuzzle
    {
        // Hairpin puzzle: find on floorboards
        // Must sneak through room while performing action to clear area
        // Witnessing memory gains strength for player
    }
}
