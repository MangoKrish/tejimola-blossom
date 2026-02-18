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
        public bool AllSolved => puzzles.Count > 0 && solvedCount >= puzzles.Count;

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

}
