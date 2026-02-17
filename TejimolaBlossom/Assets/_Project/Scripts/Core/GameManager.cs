using System.Collections.Generic;
using UnityEngine;
using Tejimola.Utils;

namespace Tejimola.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Game State")]
        public GameAct CurrentAct { get; private set; } = GameAct.MainMenu;
        public ActiveCharacter CurrentCharacter { get; private set; } = ActiveCharacter.None;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Exploration;
        public bool IsGamePaused { get; private set; }

        [Header("Progress")]
        public int CurrentDay { get; private set; } = 1;
        public int CatchCount { get; private set; } = 0;
        public float ExhaustionLevel { get; private set; } = GameConstants.ExhaustionStart;
        public HashSet<string> CollectedItems { get; private set; } = new HashSet<string>();
        public HashSet<PuzzleType> SolvedPuzzles { get; private set; } = new HashSet<PuzzleType>();
        public Dictionary<string, bool> StoryFlags { get; private set; } = new Dictionary<string, bool>();
        public int SpiritOrbCount { get; private set; } = 0;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartNewGame()
        {
            CurrentAct = GameAct.Act1_HappyHome;
            CurrentCharacter = ActiveCharacter.Tejimola;
            CurrentPhase = GamePhase.Exploration;
            CurrentDay = 1;
            CatchCount = 0;
            ExhaustionLevel = GameConstants.ExhaustionStart;
            CollectedItems.Clear();
            SolvedPuzzles.Clear();
            StoryFlags.Clear();
            SpiritOrbCount = 0;
            IsGamePaused = false;

            EventManager.Instance.Publish(EventManager.Events.GameStarted);
            SceneLoader.Instance.LoadScene("Act1_HappyHome");
        }

        public void SetAct(GameAct act)
        {
            CurrentAct = act;

            // Switch character based on act
            switch (act)
            {
                case GameAct.Act1_HappyHome:
                case GameAct.Act1_Funeral:
                case GameAct.Act2_Descent:
                case GameAct.Act2_Dheki:
                case GameAct.Act2_Burial:
                    CurrentCharacter = ActiveCharacter.Tejimola;
                    break;
                case GameAct.Act3_DomArrival:
                case GameAct.Act3_DualTimeline:
                case GameAct.Act4_Confrontation:
                    CurrentCharacter = ActiveCharacter.Dom;
                    break;
                case GameAct.Epilogue:
                    CurrentCharacter = ActiveCharacter.Dom;
                    break;
            }

            EventManager.Instance.Publish<GameAct>(EventManager.Events.ActChanged, act);
            EventManager.Instance.Publish<ActiveCharacter>(EventManager.Events.CharacterSwitched, CurrentCharacter);
        }

        public void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            EventManager.Instance.Publish<GamePhase>(EventManager.Events.PhaseChanged, phase);
        }

        public void PauseGame()
        {
            IsGamePaused = true;
            Time.timeScale = 0f;
            EventManager.Instance.Publish(EventManager.Events.GamePaused);
        }

        public void ResumeGame()
        {
            IsGamePaused = false;
            Time.timeScale = 1f;
            EventManager.Instance.Publish(EventManager.Events.GameResumed);
        }

        public void IncrementDay()
        {
            CurrentDay++;
        }

        public void IncrementCatchCount()
        {
            CatchCount++;
            EventManager.Instance.Publish<int>(EventManager.Events.CatchCountChanged, CatchCount);
            if (CatchCount >= GameConstants.MaxCatches)
            {
                EventManager.Instance.Publish(EventManager.Events.StealthComplete);
                IncrementDay();
            }
        }

        public void SetExhaustion(float value)
        {
            ExhaustionLevel = Mathf.Clamp(value, 0f, GameConstants.ExhaustionStart);
            EventManager.Instance.Publish<float>(EventManager.Events.ExhaustionChanged, ExhaustionLevel);
        }

        public void CollectItem(string itemId)
        {
            CollectedItems.Add(itemId);
            EventManager.Instance.Publish<string>(EventManager.Events.ItemCollected, itemId);
        }

        public void CollectSpiritOrb()
        {
            SpiritOrbCount++;
        }

        public bool UseSpiritOrb()
        {
            if (SpiritOrbCount > 0)
            {
                SpiritOrbCount--;
                return true;
            }
            return false;
        }

        public void SolvePuzzle(PuzzleType puzzle)
        {
            SolvedPuzzles.Add(puzzle);
            EventManager.Instance.Publish<PuzzleType>(EventManager.Events.PuzzleSolved, puzzle);
        }

        public void SetStoryFlag(string flag, bool value)
        {
            StoryFlags[flag] = value;
        }

        public bool GetStoryFlag(string flag)
        {
            return StoryFlags.ContainsKey(flag) && StoryFlags[flag];
        }

        // Save data structure
        public SaveData GetSaveData()
        {
            return new SaveData
            {
                currentAct = CurrentAct,
                currentCharacter = CurrentCharacter,
                currentDay = CurrentDay,
                catchCount = CatchCount,
                exhaustionLevel = ExhaustionLevel,
                collectedItems = new List<string>(CollectedItems),
                solvedPuzzles = new List<PuzzleType>(SolvedPuzzles),
                storyFlags = new Dictionary<string, bool>(StoryFlags),
                spiritOrbCount = SpiritOrbCount
            };
        }

        public void LoadSaveData(SaveData data)
        {
            CurrentAct = data.currentAct;
            CurrentCharacter = data.currentCharacter;
            CurrentDay = data.currentDay;
            CatchCount = data.catchCount;
            ExhaustionLevel = data.exhaustionLevel;
            CollectedItems = new HashSet<string>(data.collectedItems);
            SolvedPuzzles = new HashSet<PuzzleType>(data.solvedPuzzles);
            StoryFlags = new Dictionary<string, bool>(data.storyFlags);
            SpiritOrbCount = data.spiritOrbCount;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public GameAct currentAct;
        public ActiveCharacter currentCharacter;
        public int currentDay;
        public int catchCount;
        public float exhaustionLevel;
        public List<string> collectedItems;
        public List<PuzzleType> solvedPuzzles;
        public Dictionary<string, bool> storyFlags;
        public int spiritOrbCount;
    }
}
