using System.Collections.Generic;
using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
    public class RhythmEngine : MonoBehaviour
    {
        [Header("Rhythm Settings")]
        [SerializeField] private float startBPM = GameConstants.BaseBPM;
        [SerializeField] private float maxBPM = GameConstants.MaxBPM;
        [SerializeField] private float bpmIncreasePerBeat = 0.5f;
        [SerializeField] private float baseHitWindow = GameConstants.BaseHitWindow;
        [SerializeField] private float minHitWindow = GameConstants.MinHitWindow;

        [Header("Exhaustion")]
        [SerializeField] private float exhaustionStart = GameConstants.ExhaustionStart;
        [SerializeField] private float exhaustionHitCost = GameConstants.ExhaustionHitCost;
        [SerializeField] private float exhaustionMissCost = GameConstants.ExhaustionMissCost;

        [Header("Audio")]
        [SerializeField] private AudioClip dhekiHitSound;
        [SerializeField] private AudioClip dhekiMissSound;
        [SerializeField] private AudioClip heartbeatSound;
        [SerializeField] private AudioClip musicTrack;

        [Header("Input Keys")]
        [SerializeField] private KeyCode leftKey = KeyCode.Q;
        [SerializeField] private KeyCode rightKey = KeyCode.E;

        // State
        private bool isPlaying;
        private float currentBPM;
        private float currentHitWindow;
        private float exhaustion;
        private double songStartDspTime;
        private double nextBeatDspTime;
        private int currentBeatIndex;
        private int totalBeats;
        private int perfectCount;
        private int goodCount;
        private int missCount;
        private bool waitingForInput;
        private double currentBeatTime;

        // Beat map
        private List<BeatData> beatMap = new List<BeatData>();
        private BeatData currentBeat;

        // Vision response system
        private bool visionActive;
        private int visionBeatThreshold = 10; // Every N beats, trigger a vision

        // Events for UI
        public System.Action<BeatRating> OnBeatResult;
        public System.Action<float> OnExhaustionChanged;
        public System.Action<BeatData> OnBeatApproaching;
        public System.Action OnRhythmComplete;
        public System.Action<VisionChoice[]> OnVisionTriggered;

        public float CurrentBPM => currentBPM;
        public float Exhaustion => exhaustion;
        public float ExhaustionPercent => exhaustion / exhaustionStart;
        public int CurrentBeatIndex => currentBeatIndex;
        public int TotalBeats => totalBeats;
        public bool IsPlaying => isPlaying;

        public void StartRhythmSequence(BeatMapData mapData = null)
        {
            isPlaying = true;
            currentBPM = startBPM;
            currentHitWindow = baseHitWindow;
            exhaustion = exhaustionStart;
            currentBeatIndex = 0;
            perfectCount = 0;
            goodCount = 0;
            missCount = 0;

            GameManager.Instance.SetPhase(GamePhase.Rhythm);

            // Generate or load beat map
            if (mapData != null)
                beatMap = mapData.beats;
            else
                GenerateDefaultBeatMap();

            totalBeats = beatMap.Count;

            // Start audio with precise timing
            songStartDspTime = AudioSettings.dspTime + 0.5;
            if (musicTrack != null)
            {
                var source = AudioManager.Instance.PlaySFXLooping(musicTrack);
                if (source != null)
                    source.PlayScheduled(songStartDspTime);
            }

            nextBeatDspTime = songStartDspTime;
            ScheduleNextBeat();

            EventManager.Instance.Publish(EventManager.Events.RhythmStarted);
        }

        void GenerateDefaultBeatMap()
        {
            beatMap.Clear();
            float bpm = startBPM;
            float beatInterval = 60f / bpm;

            // Generate progression: Beats 1-3 (easy), 4-6 (medium), 7-9 (hard), 10+ (intense)
            for (int i = 0; i < 30; i++)
            {
                // Snapshot the BPM at generation time so timing is stable
                float snapshotBpm = Mathf.Min(maxBPM, startBPM + (i * bpmIncreasePerBeat));
                float interval    = 60f / snapshotBpm;

                BeatData beat = new BeatData();
                beat.index       = i;
                beat.time        = i * beatInterval;
                beat.snapshotBPM = snapshotBpm;

                // Alternate left/right with patterns
                if (i < 3)
                {
                    beat.inputKey = i % 2 == 0 ? leftKey : rightKey;
                    beat.difficulty = 1;
                }
                else if (i < 6)
                {
                    beat.inputKey = i % 2 == 0 ? rightKey : leftKey;
                    beat.difficulty = 2;
                }
                else if (i < 9)
                {
                    // Faster alternation
                    beat.inputKey = i % 2 == 0 ? leftKey : rightKey;
                    beat.difficulty = 3;
                }
                else
                {
                    // Random with increasing speed
                    beat.inputKey = Random.value > 0.5f ? leftKey : rightKey;
                    beat.difficulty = 4;
                }

                // Vision triggers
                if (i > 0 && i % visionBeatThreshold == 0)
                {
                    beat.triggersVision = true;
                }

                beatMap.Add(beat);
            }
        }

        void Update()
        {
            if (!isPlaying || GameManager.Instance.IsGamePaused) return;
            if (visionActive) return; // pause beat processing while vision choice is pending

            double currentDspTime = AudioSettings.dspTime;

            // Check if it's time for the next beat
            if (currentBeatIndex < beatMap.Count)
            {
                currentBeat = beatMap[currentBeatIndex];
                // Use the beat's snapshot BPM (stored at generation time) to avoid
                // retroactive timing shifts from live BPM updates
                double beatBPM = currentBeat.snapshotBPM > 0 ? currentBeat.snapshotBPM : currentBPM;
                double beatTime = songStartDspTime + (60.0 / beatBPM * currentBeatIndex);

                // Signal approaching beat for UI
                double timeUntilBeat = beatTime - currentDspTime;
                if (timeUntilBeat <= 1.0 && timeUntilBeat > 0)
                {
                    OnBeatApproaching?.Invoke(currentBeat);
                }

                // Within hit window
                if (Mathf.Abs((float)(currentDspTime - beatTime)) <= currentHitWindow)
                {
                    waitingForInput = true;
                    currentBeatTime = beatTime;

                    // Check input
                    if (Input.GetKeyDown(currentBeat.inputKey))
                    {
                        float accuracy = Mathf.Abs((float)(currentDspTime - beatTime));
                        ProcessHit(accuracy);
                    }
                    else if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(rightKey))
                    {
                        // Wrong key
                        ProcessMiss();
                    }
                }
                else if (waitingForInput && currentDspTime > beatTime + currentHitWindow)
                {
                    // Missed the window
                    ProcessMiss();
                }
            }

            // Update BPM progression
            UpdateDifficulty();

            // Check exhaustion effects
            UpdateExhaustionEffects();

            // Check completion
            if (currentBeatIndex >= beatMap.Count)
            {
                CompleteSequence();
            }
        }

        void ProcessHit(float accuracy)
        {
            waitingForInput = false;
            BeatRating rating;

            if (accuracy <= currentHitWindow * 0.3f)
            {
                rating = BeatRating.Perfect;
                perfectCount++;
            }
            else
            {
                rating = BeatRating.Good;
                goodCount++;
            }

            // Decrease exhaustion
            exhaustion -= exhaustionHitCost;
            exhaustion = Mathf.Max(0, exhaustion);

            if (dhekiHitSound != null)
                AudioManager.Instance.PlaySFX(dhekiHitSound);

            OnBeatResult?.Invoke(rating);
            OnExhaustionChanged?.Invoke(exhaustion);
            EventManager.Instance.Publish<float>(EventManager.Events.ExhaustionChanged, exhaustion);

            // Check for vision trigger
            if (currentBeat.triggersVision)
            {
                TriggerVision();
            }

            currentBeatIndex++;
        }

        void ProcessMiss()
        {
            waitingForInput = false;
            missCount++;

            // Exhaustion doesn't increase on miss per doc (stays same)
            // But miss accelerates exhaustion visual effects

            if (dhekiMissSound != null)
                AudioManager.Instance.PlaySFX(dhekiMissSound);

            OnBeatResult?.Invoke(BeatRating.Miss);
            EventManager.Instance.Publish(EventManager.Events.BeatMiss);

            currentBeatIndex++;
        }

        void UpdateDifficulty()
        {
            // BPM increases gradually
            currentBPM = Mathf.Min(maxBPM, startBPM + (currentBeatIndex * bpmIncreasePerBeat));

            // Hit window narrows
            float progress = (float)currentBeatIndex / totalBeats;
            currentHitWindow = Mathf.Lerp(baseHitWindow, minHitWindow, progress);
        }

        void UpdateExhaustionEffects()
        {
            float percent = exhaustion / exhaustionStart;

            // At low exhaustion, trigger visual effects
            if (percent < 0.3f)
            {
                // Vision blur effect
                EventManager.Instance.Publish<float>(EventManager.Events.ExhaustionChanged, exhaustion);
            }

            // Auto-fail if exhaustion hits zero - per doc, the sequence automatically fails
            if (exhaustion <= 0f)
            {
                // Sequence fails but story continues
                FailSequence();
            }
        }

        void TriggerVision()
        {
            visionActive = true;
            EventManager.Instance.Publish(EventManager.Events.VisionTriggered);

            // Present vision choices
            VisionChoice[] choices = GetVisionChoices(currentBeatIndex);
            OnVisionTriggered?.Invoke(choices);
        }

        VisionChoice[] GetVisionChoices(int beatIndex)
        {
            // Generate context-appropriate vision choices
            int visionIndex = beatIndex / visionBeatThreshold;
            switch (visionIndex)
            {
                case 1:
                    return new VisionChoice[]
                    {
                        new VisionChoice("A", "I see... father's boat!", "triggers_memory_boat"),
                        new VisionChoice("B", "The river... it's calling...", "triggers_memory_river"),
                        new VisionChoice("C", "Where are you?...", "triggers_memory_search")
                    };
                case 2:
                    return new VisionChoice[]
                    {
                        new VisionChoice("A", "The nahor tree... blooming!", "triggers_memory_tree"),
                        new VisionChoice("B", "I hear singing...", "triggers_memory_song"),
                        new VisionChoice("C", "Help me... please...", "triggers_memory_plea")
                    };
                default:
                    return new VisionChoice[]
                    {
                        new VisionChoice("A", "I remember now...", "triggers_memory_general"),
                        new VisionChoice("B", "The light... it's warm...", "triggers_memory_light"),
                        new VisionChoice("C", "Don't forget me...", "triggers_memory_forget")
                    };
            }
        }

        public void SelectVisionChoice(int choiceIndex, VisionChoice[] choices)
        {
            if (choiceIndex >= 0 && choiceIndex < choices.Length)
            {
                GameManager.Instance.SetStoryFlag(choices[choiceIndex].flagToSet, true);
            }
            visionActive = false;
        }

        void CompleteSequence()
        {
            isPlaying = false;
            GameManager.Instance.SetPhase(GamePhase.Exploration);

            OnRhythmComplete?.Invoke();
            EventManager.Instance.Publish(EventManager.Events.RhythmEnded);

            Debug.Log($"Rhythm Complete - Perfect: {perfectCount}, Good: {goodCount}, Miss: {missCount}");
        }

        void FailSequence()
        {
            isPlaying = false;
            GameManager.Instance.SetPhase(GamePhase.Cutscene);

            // Per doc: "automatically fails (cannot win)" - this is a narrative beat
            EventManager.Instance.Publish(EventManager.Events.RhythmEnded);
            Debug.Log("Rhythm sequence failed - narrative continues");
        }

        public void StopSequence()
        {
            isPlaying = false;
        }

        void ScheduleNextBeat()
        {
            if (currentBeatIndex < beatMap.Count)
            {
                nextBeatDspTime = songStartDspTime + (60.0 / currentBPM * currentBeatIndex);
            }
        }
    }

    [System.Serializable]
    public class BeatData
    {
        public int index;
        public float time;
        public KeyCode inputKey;
        public int difficulty;
        public bool triggersVision;
        public float snapshotBPM; // BPM at the time this beat was scheduled (prevents retroactive timing shifts)
    }

    [System.Serializable]
    public class BeatMapData
    {
        public string name;
        public float baseBPM;
        public List<BeatData> beats = new List<BeatData>();
    }

    [System.Serializable]
    public class VisionChoice
    {
        public string label;
        public string text;
        public string flagToSet;

        public VisionChoice(string label, string text, string flag)
        {
            this.label = label;
            this.text = text;
            this.flagToSet = flag;
        }
    }
}
