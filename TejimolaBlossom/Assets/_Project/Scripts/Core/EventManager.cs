using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tejimola.Core
{
    public class EventManager : MonoBehaviour
    {
        private static EventManager _instance;
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("EventManager");
                    _instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, Action> _eventTable = new Dictionary<string, Action>();
        private Dictionary<string, Delegate> _eventTableGeneric = new Dictionary<string, Delegate>();

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

        // Parameterless events
        public void Subscribe(string eventName, Action listener)
        {
            if (!_eventTable.ContainsKey(eventName))
                _eventTable[eventName] = null;
            _eventTable[eventName] += listener;
        }

        public void Unsubscribe(string eventName, Action listener)
        {
            if (_eventTable.ContainsKey(eventName))
                _eventTable[eventName] -= listener;
        }

        public void Publish(string eventName)
        {
            if (_eventTable.ContainsKey(eventName))
                _eventTable[eventName]?.Invoke();
        }

        // Generic single-parameter events
        public void Subscribe<T>(string eventName, Action<T> listener)
        {
            if (!_eventTableGeneric.ContainsKey(eventName))
                _eventTableGeneric[eventName] = null;
            _eventTableGeneric[eventName] = Delegate.Combine(_eventTableGeneric[eventName], listener);
        }

        public void Unsubscribe<T>(string eventName, Action<T> listener)
        {
            if (_eventTableGeneric.ContainsKey(eventName))
                _eventTableGeneric[eventName] = Delegate.Remove(_eventTableGeneric[eventName], listener);
        }

        public void Publish<T>(string eventName, T arg)
        {
            if (_eventTableGeneric.ContainsKey(eventName))
                (_eventTableGeneric[eventName] as Action<T>)?.Invoke(arg);
        }

        // Event name constants
        public static class Events
        {
            // Game State
            public const string GameStarted = "GameStarted";
            public const string GamePaused = "GamePaused";
            public const string GameResumed = "GameResumed";
            public const string ActChanged = "ActChanged";
            public const string CharacterSwitched = "CharacterSwitched";
            public const string PhaseChanged = "PhaseChanged";

            // Dialogue
            public const string DialogueStarted = "DialogueStarted";
            public const string DialogueEnded = "DialogueEnded";
            public const string DialogueChoice = "DialogueChoice";

            // Spirit Pulse
            public const string SpiritPulseActivated = "SpiritPulseActivated";
            public const string SpiritPulseRevealed = "SpiritPulseRevealed";
            public const string SpiritPulseReady = "SpiritPulseReady";

            // Stealth
            public const string PlayerDetected = "PlayerDetected";
            public const string PlayerHidden = "PlayerHidden";
            public const string CatchCountChanged = "CatchCountChanged";
            public const string StealthComplete = "StealthComplete";

            // Rhythm
            public const string RhythmStarted = "RhythmStarted";
            public const string RhythmEnded = "RhythmEnded";
            public const string BeatHit = "BeatHit";
            public const string BeatMiss = "BeatMiss";
            public const string ExhaustionChanged = "ExhaustionChanged";
            public const string VisionTriggered = "VisionTriggered";

            // Puzzles
            public const string PuzzleStarted = "PuzzleStarted";
            public const string PuzzleSolved = "PuzzleSolved";
            public const string MemoryTriggered = "MemoryTriggered";
            public const string EnvironmentChanged = "EnvironmentChanged";

            // Boss
            public const string BossPhaseChanged = "BossPhaseChanged";
            public const string BossDefeated = "BossDefeated";
            public const string PlayerDamaged = "PlayerDamaged";

            // UI
            public const string ShowNotification = "ShowNotification";
            public const string ObjectiveUpdated = "ObjectiveUpdated";
            public const string FadeIn = "FadeIn";
            public const string FadeOut = "FadeOut";

            // Save
            public const string GameSaved = "GameSaved";
            public const string GameLoaded = "GameLoaded";

            // Interaction
            public const string InteractionAvailable = "InteractionAvailable";
            public const string InteractionPerformed = "InteractionPerformed";
            public const string ItemCollected = "ItemCollected";
        }
    }
}
