# Tejimola: The Blossom From Clay — System Architecture

## Overview

This document describes the internal architecture of the Tejimola game, explaining how different systems communicate and work together to create the complete narrative experience.

---

## 🏗️ Core Architecture Pattern: Event-Driven Pub/Sub

The game uses a **centralized event bus** (EventManager) to decouple all systems, allowing them to communicate without direct references.

```
┌─────────────────────────────────────────────────────────────┐
│                     EventManager (Singleton)                │
│                   Central Event Bus / Hub                    │
└─────────────────────────────────────────────────────────────┘
         ↑                                           ↑
    [Publish]                                  [Subscribe]
         ↓                                           ↓
    ┌────────────────┐    ┌─────────────────────────────┐
    │  GameSystems   │    │   Listeners / Subscribers   │
    ├────────────────┤    ├─────────────────────────────┤
    │ DialogueStart  │───→│ UI Updates                  │
    │ SpiritPulse    │───→│ Visual Effects              │
    │ BeatHit        │───→│ Audio Feedback              │
    │ PuzzleSolved   │───→│ Environment Changes         │
    │ BossFight      │───→│ Game State Updates          │
    └────────────────┘    └─────────────────────────────┘
```

**Why This Pattern?**
- **Loose Coupling**: Systems don't need to know about each other
- **Scalability**: Easy to add new systems without modifying existing ones
- **Testability**: Each system can be tested independently
- **Maintainability**: Clear flow of communication

---

## 🎮 Core Manager Systems

### 1. GameManager (Singleton)
**Responsibility**: Central game state and persistence

```
GameManager
├── Current Game State
│   ├── CurrentAct (Act 1-4, Epilogue)
│   ├── CurrentCharacter (Tejimola / Dom)
│   ├── CurrentPhase (Normal, Stealth, Rhythm, Boss, etc.)
│   └── CurrentDay (1-5)
├── Progress Tracking
│   ├── CatchCount (0-5 stealth catches)
│   ├── ExhaustionLevel (0-100% rhythm meter)
│   ├── CollectedItems (List of item IDs)
│   ├── SolvedPuzzles (5 memory puzzles)
│   ├── StoryFlags (Branching dialogue choices)
│   └── SpiritOrbCount (For boss fight)
└── Methods
    ├── SetAct(GameAct)
    ├── SetPhase(GamePhase)
    ├── PauseGame()
    ├── IncrementDay()
    ├── CollectItem()
    ├── SolveAndProgressPuzzle()
    └── SaveGameState() → SaveData JSON
```

**Save File Structure** (`AppData\LocalLow\Tejimola Games\save.json`):
```json
{
  "CurrentAct": 2,
  "CurrentDay": 3,
  "CatchCount": 2,
  "SolvedPuzzles": [true, false, true, false, false],
  "StoryFlags": {"chose_kindness": true},
  "SpiritOrbCount": 3,
  "PlaytimeMinutes": 45
}
```

---

### 2. EventManager (Singleton)
**Responsibility**: Event publishing and subscription

```
EventManager
├── Generic Events<T>
│   └── Subscribe/Unsubscribe/Publish<T>()
├── Predefined Events
│   ├── DialogueStarted(DialogueEntry)
│   ├── DialogueChoiceSelected(choiceIndex)
│   ├── SpiritPulseActivated(position)
│   ├── PlayerDetected(catcher)
│   ├── BeatHit(rating, exhaustion)
│   ├── PuzzleSolved(puzzleID)
│   ├── ItemCollected(itemID)
│   ├── BossPhaseChanged(phase)
│   └── [20+ more events]
└── Pattern Usage
    ├── Publish: DialogueManager publishes DialogueStarted
    ├── Subscribe: DialogueBoxUI listens for DialogueStarted
    └── Result: UI updates when dialogue changes (no direct call)
```

**Example Event Flow**:
```
RhythmEngine hits perfect beat
    ↓
Publish: BeatHit(Perfect, 95%)
    ↓
[Multiple systems listening to BeatHit]
├─ GameHUD updates exhaustion bar visual
├─ AudioManager plays "perfect" SFX
├─ DialogueManager triggers vision choice
└─ VFX spawner creates "Perfect" particle
```

---

### 3. AudioManager (Singleton)
**Responsibility**: Music, SFX, and audio mixing

```
AudioManager
├── Audio Mixer Groups
│   ├── Master (overall volume)
│   ├── Music (adaptive tracks, crossfade)
│   ├── SFX (pooled sources for effects)
│   ├── Voice (dialogue audio)
│   └── Ambient (background atmospheric sounds)
├── Music System
│   ├── PlayActMusic(act) - loads adaptive track
│   ├── CrossfadeMusic(fromTrack, toTrack, 2 seconds)
│   └── CurrentMusicState (track, bpm, layer count)
├── SFX System
│   ├── Pool of 10 pre-allocated AudioSources
│   ├── PlaySFX(soundID, position) - spatial audio
│   ├── PlayUIClick() - menu feedback
│   └── StopAllSFX()
└── Initialization
    ├── Loads from: Assets/_Project/Audio/
    ├── Music tracks: 10 files (Act1-Epilogue + Boss)
    └── SFX library: 23 sound effects (footsteps, whoosh, etc.)
```

**Audio File Structure**:
```
Audio/
├── Music/
│   ├── act1_happy_home.wav (60s, Indian ragas: Bilawal)
│   ├── act2_descent.wav (60s, minor ragas: Bhairavi)
│   ├── act3_mystical.wav (60s, mystical: Yaman)
│   ├── act4_boss_theme.wav (120s, dark orchestral)
│   ├── act4_boss_phase2.wav (120s, frantic tempo)
│   ├── act4_boss_phase3.wav (120s, epic finale)
│   ├── epilogue_sunrise.wav (45s, peaceful resolution)
│   ├── dheki_rhythm_base.wav (60s, 90 BPM)
│   ├── dheki_rhythm_fast.wav (60s, 150 BPM)
│   └── menu_theme.wav (30s, ambient)
└── SFX/
    ├── footstep_wood_1.wav / 2.wav / 3.wav
    ├── footstep_grass_1.wav / 2.wav
    ├── spirit_pulse_whoosh.wav
    ├── heartbeat_fast.wav
    ├── ui_click.wav
    ├── memory_flash.wav
    └── [17 more effects]
```

---

### 4. SaveManager (Singleton)
**Responsibility**: Persistence and game state serialization

```
SaveManager
├── Save Structure
│   ├── GameData (current game state)
│   ├── GameSettings (user preferences)
│   └── Metadata (save timestamp, playtime)
├── Methods
│   ├── SaveGame() → JSON file
│   ├── LoadGame() → deserialize and restore state
│   ├── LoadSettings() → audio/display preferences
│   ├── SaveSettings() → user preferences
│   └── WipeAllSaves() → reset progress
├── Save Location
│   └── Application.persistentDataPath
│       └── Windows: %APPDATA%\LocalLow\Tejimola Games\
└── File Format
    ├── save.json (game progress)
    ├── settings.json (volume, resolution, language)
    └── auto_save_act[N].json (backup at chapter start)
```

---

### 5. SceneLoader (Singleton)
**Responsibility**: Scene transitions with visual fading

```
SceneLoader
├── CreateFadeCanvas()
│   └── Instantiate black CanvasGroup overlay
├── LoadScene(sceneName)
│   ├── Fade to black (1 second)
│   ├── Load scene async
│   ├── Call SceneSetup.Initialize()
│   └── Fade from black (1 second)
├── LoadSceneWithTitle(sceneName, titleText)
│   ├── Fade to black
│   ├── Load scene
│   └── Display title card (3 seconds)
└── FadeTransition(duration)
    ├── Coroutine-based fade
    └── Smooth CanvasGroup.alpha lerp
```

**Transition Examples**:
```
Act 1 → Act 2:
  Fade to black (1s)
  ↓
  Display: "Act II: The Descent"
  ↓
  Load Act2_Descent scene
  ↓
  Fade from black (1s)
  ↓
  GameHUD shows new day

Act 2 Stealth Failed:
  Fade to black (0.5s - quick)
  ↓
  Load: "Act2_Descent" (restart)
  ↓
  Fade in (1s)
```

---

### 6. DialogueManager (Singleton)
**Responsibility**: Dialogue system and branching narrative

```
DialogueManager
├── DialogueData Structure
│   ├── DialogueConversation (array of DialogueEntry)
│   └── DialogueEntry
│       ├── id: "act1_opening_1"
│       ├── speaker: "Tejimola"
│       ├── text: "English text..."
│       ├── textAssamese: "অসমীয়া টেক্সট..."
│       ├── emotion: "happy"
│       ├── audioFile: "dialogue_tejimola_greeting.wav"
│       ├── nextEntry: "act1_opening_2" (auto-continue)
│       └── choices: [DialogueChoice] (branching)
├── DialogueChoice
│   ├── buttonText: "[A] Help father"
│   ├── nextEntry: "act1_help_choice"
│   └── storyFlag: "chose_kindness"
├── Loading System
│   ├── LoadConversation(conversationID)
│   ├── Loads from: Resources/Dialogue/*.json
│   └── Cached in memory after first load
├── Flow Control
│   ├── StartDialogue(conversationID)
│   ├── AdvanceDialogue() - next entry
│   ├── SelectChoice(choiceIndex) - branching
│   └── IsDialogueActive() - check state
└── Events Published
    ├── DialogueStarted(currentEntry)
    ├── DialogueChoicePresented(choices)
    ├── DialogueEnded()
    └── StoryFlagSet(flagName, value)
```

**JSON Dialogue File Format**:
```json
{
  "conversationId": "act1_opening",
  "entries": [
    {
      "id": "act1_opening_1",
      "speaker": "Tejimola",
      "text": "This is my home...",
      "textAssamese": "এই মোৰ ঘৰ...",
      "emotion": "calm",
      "audioFile": "voice_tejimola_1.wav",
      "nextEntry": "act1_opening_2",
      "choices": null
    },
    {
      "id": "act1_opening_2",
      "speaker": "Tejimola",
      "text": "What should I do?",
      "choices": [
        {
          "buttonText": "[A] Help father",
          "nextEntry": "act1_help_start",
          "storyFlag": "chose_kindness=true"
        },
        {
          "buttonText": "[B] Explore alone",
          "nextEntry": "act1_explore_start",
          "storyFlag": "chose_independent=true"
        }
      ]
    }
  ]
}
```

---

## 🎭 Character Systems

### CharacterController2D (Base Class)
```
CharacterController2D
├── Properties
│   ├── currentSpeed (0-5 units/sec)
│   ├── isGrounded (raycast to ground)
│   ├── isCrouching (stealth position)
│   ├── isHiding (in hiding spot)
│   └── facingRight (sprite flip state)
├── Movement
│   ├── Move(float horizontal) - update position
│   ├── Flip() - sprite direction
│   ├── Jump() - vertical velocity
│   └── ApplyGravity()
├── Interaction
│   ├── DetectNearbyInteractables() - raycast
│   ├── ShowInteractionPrompt()
│   └── InteractWithObject()
├── Animation
│   ├── UpdateAnimationState()
│   ├── PlayAnimation(stateName)
│   └── Animator hash strings (AnimSpeed, etc.)
└── Input Handling
    ├── OnMovementInput(Vector2)
    ├── OnInteractInput()
    ├── OnCrouchInput()
    └── OnActionInput()
```

### TejimolaBehaviour (Child - Acts 1-2)
```
TejimolaBehaviour : CharacterController2D
├── Stealth Mechanics
│   ├── HidingSpots[] - safe zones
│   ├── IsHiding - boolean state
│   ├── EnterHidingSpot(hidingSpot)
│   │   └── Hide sprite, disable collider
│   └── ExitHidingSpot()
│       └── Show sprite, enable collider
├── Footprint System
│   ├── LeaveFootprints() - called each move frame
│   ├── SpawnFootprint() - procedural sprite
│   ├── FootprintFadeOut(3 seconds)
│   └── FootprintCount (max 5 for caught)
├── Stealth States
│   ├── Moving (leaves footprints, visible)
│   ├── Crouching (quieter, less visible)
│   ├── Hiding (invisible, safe)
│   └── Caught (cry animation, respawn)
└── Events
    ├── OnCaught() → CatchCount++
    ├── Subscribe to EnemyAI.OnDetect
    └── Publish: PlayerCaught
```

### DomBehaviour (Child - Acts 3-4)
```
DomBehaviour : CharacterController2D
├── Spirit Pulse Ability
│   ├── pulseCooldownTimer (0-3 seconds)
│   ├── ActivateSpiritPulse()
│   │   ├── Spawn SpiritPulseEffect at position
│   │   ├── Physics2D.OverlapCircleAll(5m radius)
│   │   ├── Reveal all SpiritRevealable objects
│   │   └── Reset cooldown to 3 seconds
│   ├── GetPulseCooldownPercent() - for HUD
│   └── CanActivatePulse - cooldown check
├── Drum Interaction
│   ├── InteractWithDrum()
│   ├── StartDhekiSequence()
│   └── OnRhythmComplete()
├── Spirit Connection
│   ├── DetectSpiritObjects()
│   ├── CanSeePast() - via spirit pulse
│   └── VisualGlowEffect()
└── Events
    ├── Publish: SpiritPulseActivated
    ├── Subscribe to: RhythmEngine.OnBeatHit
    └── Subscribe to: PuzzleManager.OnPuzzleSolved
```

---

## 🎮 Gameplay Mechanics

### 1. Spirit Pulse System (Acts 3-4)

```
SpiritPulse Mechanic Flow:

Player presses Space
    ↓
DomBehaviour.ActivateSpiritPulse()
    ↓
Check cooldown (3 second recovery)
    ↓
If ready:
  ├─ Spawn SpiritPulseEffect
  │   └─ Expanding ring (0 → 5m radius, 0.5 seconds)
  │
  ├─ Physics2D.OverlapCircleAll(position, 5m)
  │   └─ Get all colliders in radius
  │
  ├─ For each collider:
  │   ├─ Check if SpiritRevealable component
  │   ├─ Call OnSpiritReveal()
  │   │   ├─ Show hidden sprite
  │   │   ├─ Enable collider
  │   │   ├─ Play reveal sound
  │   │   └─ Publish: SpiritObjectRevealed
  │   └─ Fade gold glow effect
  │
  ├─ Publish: SpiritPulseActivated
  │   └─ GameHUD updates cooldown indicator
  │
  └─ Start 3 second cooldown

If on cooldown:
  └─ Play "not_ready" sound
```

**SpiritRevealable Component**:
```csharp
class SpiritRevealable
{
  public bool IsRevealed { get; private set; }

  void OnSpiritReveal()
  {
    spriteRenderer.enabled = true;
    collider2D.enabled = true;
    StartCoroutine(GlowEffect());
    PublishEvent(ObjectRevealed);
  }

  // Example objects: Hidden memory item, Secret passage, Ghost spirit
}
```

---

### 2. Stealth System (Act 2)

```
Stealth Phase Architecture:

┌─────────────────────────────────────────┐
│      EnemyAI (Ranima)                   │
├─────────────────────────────────────────┤
│ State Machine:                          │
│ ├─ Unaware (patrol randomly)            │
│ ├─ Suspicious (moving toward last sight)│
│ ├─ Alerted (searching area)             │
│ └─ Caught (end scene, respawn)          │
└─────────────────────────────────────────┘
                  ↓
         Detection System
         ├─ Physics2D.OverlapCircleAll(3m detection radius)
         ├─ LineOfSight raycast check
         ├─ Sound detection (footprints)
         └─ Vision cone (120° forward)

When Tejimola moves:
  ├─ TejimolaBehaviour.Move()
  │   └─ SpawnFootprint() each frame
  │
  ├─ Footprints fade out (3 seconds)
  │
  ├─ EnemyAI detects footprint OR
  ├─ Ranima enters detection radius
  │
  └─ If within vision cone:
      └─ SetState(Alerted) → chase Tejimola
         └─ OnCaught() → CatchCount++

When Tejimola hides:
  ├─ TejimolaBehaviour.EnterHidingSpot()
  │   ├─ Set isHiding = true
  │   ├─ Sprite invisible
  │   └─ Collider disabled (can't be hit)
  │
  └─ Stop leaving footprints
      └─ EnemyAI can't detect in hiding spot

Catch Limit:
  ├─ CatchCount = 0-5
  ├─ If CatchCount >= 5:
  │   ├─ Skip to day progression
  │   ├─ Publish: StealthPhaseFailed
  │   └─ Load next day scene
  └─ Each catch resets position and restarts scene
```

**EnemyAI State Transitions**:
```
Unaware (patrol)
  ├─ Footprint detected? → Suspicious
  ├─ LineOfSight? → Alerted
  └─ Time elapsed? → back to patrol

Suspicious
  ├─ Another sound? → investigate direction
  ├─ No sound (5s)? → back to Unaware
  └─ LineOfSight? → Alerted

Alerted
  ├─ Has LineOfSight? → Chase (SetChaseTarget)
  ├─ Reached last known position? → Searching
  ├─ Found target? → OnCaught()
  └─ Timeout (10s)? → back to Unaware
```

---

### 3. Rhythm/Dheki System (Act 2)

```
Rhythm Engine Architecture:

Setup Phase:
├─ Load BeatMap (30 beats, increasing difficulty)
├─ Parse beat times from dspTime
├─ Set initial BPM: 90
└─ Create hit windows: ±0.25s (Perfect: ±0.1s, Good: ±0.25s)

Gameplay Loop:
Loop each frame:
  ├─ Calculate current dspTime
  ├─ Get next beat to judge
  │   └─ beat[index].time
  │
  ├─ Check if in hit window:
  │   ├─ Perfect: dspTime within ±0.1s
  │   ├─ Good: dspTime within ±0.25s
  │   ├─ Miss: outside window
  │   └─ Rate = Perfect/Good/Miss
  │
  ├─ On Q or E key input:
  │   ├─ If in window:
  │   │   ├─ UpdateExhaustionMeter(-1%)
  │   │   ├─ PlayFeedback(BeatRating)
  │   │   ├─ Publish: BeatHit
  │   │   └─ If beat.triggersVision:
  │   │       └─ ShowVisionChoice()
  │   │
  │   └─ If outside window:
  │       └─ Miss (no penalty, visual feedback)
  │
  ├─ Update BPM progression:
  │   └─ BPM = 90 + (beatIndex / 30) * 60 = 90-150
  │
  ├─ Update hit window narrowing:
  │   └─ hitWindow = 0.25s → 0.1s (tighter)
  │
  ├─ Update exhaustion:
  │   └─ ExhaustionMeter: 100% → 0% (depletes on misses)
  │
  └─ If exhaustion < 0:
      ├─ Player collapses
      ├─ Publish: RhythmPhaseFailed
      └─ Restart sequence or skip

Beat Rating Visual Feedback:
├─ Perfect: Big gold particle, "+1" text, high-pitched chime
├─ Good: Medium green particle, "+0" text, medium beep
└─ Miss: Red X particle, "-" text, low buzz

Vision Choice (triggered on beat.triggersVision):
├─ Pause rhythm temporarily
├─ Show dialogue choice (3 options)
├─ Player selects A/B/C
├─ Sets story flag
├─ Resume rhythm
```

**BeatMap Data Structure**:
```json
{
  "baselineBPM": 90,
  "beatsPerMinute": 90,
  "beats": [
    {
      "index": 0,
      "time": 0.5,
      "inputKey": "Q",
      "difficulty": 0,
      "triggersVision": false,
      "visionText": null
    },
    {
      "index": 10,
      "time": 6.67,
      "inputKey": "E",
      "difficulty": 5,
      "triggersVision": true,
      "visionText": "What did Tejimola want to say?"
    }
  ]
}
```

**Exhaustion Meter System**:
```
ExhaustionMeter: 0-100%

Initial: 100% (fresh)
├─ On perfect hit: -1% (small cost)
├─ On good hit: -1% (small cost)
├─ On miss: -0% (no penalty, but risky)
└─ Over time: -0.5% per second (passive fatigue)

Visual feedback:
├─ 100% (green): Full energy
├─ 50% (yellow): Tired
├─ 25% (orange): Exhausted
└─ 0% (red): Collapse → Fail scene

When exhaustion hits 0:
└─ Publish: ExhaustionThresholdReached
    └─ RhythmEngine.OnExhaustionComplete()
        ├─ Play collapse sound
        ├─ Fade to white
        ├─ Vision sequence begins
        └─ Scene progresses or resets
```

---

### 4. Puzzle System (Act 3)

```
Memory Puzzle Architecture:

PuzzleManager (singleton)
├── puzzles[] = 5 MemoryPuzzle objects
├── CurrentPuzzle
├── SolvedCount (0-5)
└── Methods
    ├─ StartPuzzle(index)
    ├─ InteractWithPuzzleObject()
    ├─ CheckSolutionProgress()
    ├─ OnPuzzleSolved() → Publish event
    └─ ProgressToNextScene()

MemoryPuzzle Base Class:
├── Puzzle Type (Well, Hairpin, Flame, Drum, Seed)
├── RequiredSteps: List<PuzzleStep>
├── PuzzleStep:
│   ├─ interactionType (Examine, Place, Combine)
│   ├─ objectID
│   ├─ sequence order
│   └─ story context
├─ CurrentStepIndex (0 until all completed)
├─ Split-Screen Display:
│   ├─ Left half: Present-day (ruined house)
│   ├─ Right half: Past (memory vision - misty)
│   └─ Seam visual separator
├─ OnStep1Complete() → Update UI
├─ OnStep2Complete() → Unlock next step
├─ OnAllStepsComplete() → trigger transformation
└─ TransformEnvironment()
    ├─ Set afterState active
    ├─ Set beforeState inactive
    ├─ Play VFX (memory particles)
    └─ Publish: PuzzleSolved

SplitScreenController:
├─ Create two cameras (left 50%, right 50%)
├─ Left camera: MainCamera (present day)
├─ Right camera: MemoryCamera (past vision)
└─ Overlay seam visual in center

Puzzle Examples:
├─ Well Puzzle:
│   ├─ Step 1: Examine well in present
│   ├─ Step 2: Examine well in past memory
│   ├─ Step 3: Combine water + jar
│   ├─ Step 4: Pour water (ritual action)
│   └─ Result: Well becomes blessed
│
├─ Hairpin Puzzle:
│   ├─ Step 1: Find broken hairpin in present
│   ├─ Step 2: Remember Tejimola wearing it in past
│   ├─ Step 3: Restore with thread from past
│   └─ Result: Hairpin becomes whole
│
└─ [3 more puzzles with similar structure]

UI Display During Puzzle:
├─ Split-screen environment visible
├─ "Step 1 of 4" indicator
├─ Interactive objects highlighted with prompt:
│   └─ "E to examine" / "E to place" / "E to combine"
├─ Objective text describing what to do
└─ Memory particles float as visual feedback
```

---

### 5. Boss Fight System (Act 4)

```
BossController - 3 Phase System:

Phase 1: Navigate (Duration: 5 minutes)
├─ Boss drifts toward player slowly
├─ Obstacles spawn randomly
│   ├─ Vine clusters (block path)
│   ├─ Rock formations
│   └─ Corruption pools (damage on touch)
├─ Player objective: Avoid boss, survive
├─ Boss health: Not damaged (100% persistent)
├─ Transition: After 5 minutes → Phase 2
│                OR player reaches left side
│                OR collect 3+ spirit orbs
│
└─ AI: BossPhase1Behavior()
    ├─ currentSpeed = lerp(0, 1, timeElapsed)
    ├─ Move toward player.position
    ├─ spawnObstacleTimer decrements
    ├─ If timer == 0: SpawnObstacle()

Phase 2: Spirit Orbs (Duration: 5 minutes)
├─ Boss continues moving toward player
├─ Player collects 3 spirit orbs (from environment)
├─ Spirit orbs scattered around arena
│   └─ Glow purple, rotate slowly
├─ Player presses F to use orb:
│   ├─ Orb travels to boss
│   ├─ Boss.isSlowed = true (3 second effect)
│   ├─ Boss visual slows, becomes semi-transparent
│   ├─ SpiritOrbCount--
│   └─ Publish: SpiritOrbUsed
├─ Mechanic: Use orbs to create safe distance
├─ Transition: After 5 minutes OR all orbs used → Phase 3
│
└─ AI: BossPhase2Behavior()
    ├─ If isSlowed:
    │   └─ currentSpeed *= 0.5 (half speed)
    ├─ If not slowed:
    │   └─ currentSpeed += acceleration
    └─ DetectOrbNearby() → dodge or sprint

Phase 3: Barrel Pursuit (Duration: 5 minutes)
├─ Boss health: 100 → 0 (take damage)
├─ Spiked barrel spawns, rolls toward player
│   ├─ Fast, cannot be jumped over
│   ├─ Damages boss if hits boss (obstacle collision)
│   └─ Damages player if touched
├─ Goal: Dodge barrel, use environment to slow it
│   ├─ Vine clusters redirect barrel trajectory
│   ├─ Rocks provide cover
│   └─ Positioning matters
├─ Boss position: Arena edges, summons minions
│   ├─ Corruption particles attack
│   └─ Vines grab at player
├─ Player health: 5 hit points (lose 1 per hit)
├─ Defeat condition:
│   ├─ Boss HP reaches 0 → Victory
│   ├─ Player HP reaches 0 → Game Over (restart)
│   ├─ Timer expires (5 min) → Barrel speed increases
│   └─ Last stand (30 seconds, barrel accelerates)
│
└─ AI: BossPhase3Behavior()
    ├─ If PlayerHitByBarrel():
    │   └─ boss.health--
    ├─ If PlayerHitByAttack():
    │   └─ player.health--
    └─ If boss.health <= 0:
        └─ DefeatSequence()

DefeatSequence():
├─ Boss sprite fades to transparency
├─ Corruption particles explode outward
├─ Play victory fanfare sound
├─ Fade to white (1 second)
├─ Publish: BossFailed
├─ Transition to Epilogue scene
```

**Boss Phases Visual State**:
```
Phase 1 (Navigate):
┌─────────────────────────────────────────┐
│  Boss: Dark figure, moving slow         │
│  Player: [Jumping vine obstacles]       │
│  Terrain: Vine clusters, rocks          │
└─────────────────────────────────────────┘

Phase 2 (Spirit Orbs):
┌─────────────────────────────────────────┐
│  Boss: Semi-transparent, slowed effect  │
│  Player: [Collecting purple orbs]       │
│  Items: 3 spirit orbs floating          │
│  FX: Purple pulses from used orbs       │
└─────────────────────────────────────────┘

Phase 3 (Barrel):
┌─────────────────────────────────────────┐
│  Boss: Spiked barrel rolling fast       │
│  Player: [Health bar shows 5 HP]        │
│  Timer: 5:00 → 0:00 (pressure increases)│
│  Terrain: Destructible obstacles        │
└─────────────────────────────────────────┘
```

---

## 🎨 UI System Architecture

### Canvas Hierarchy

```
Canvas
├─ MainMenuUI (acts 1-0)
│  ├─ Background image (Nahor tree)
│  ├─ Title text
│  ├─ Button group: [New Game] [Continue] [Settings] [Quit]
│  └─ Settings panel (volume sliders, language)
│
├─ GameHUD (during gameplay)
│  ├─ TopLeft: Day counter, Objective text
│  ├─ TopRight: Spirit Pulse cooldown indicator
│  ├─ BottomLeft: Catch counter (during stealth)
│  ├─ BottomRight: Exhaustion bar (during rhythm)
│  │   └─ Health bar (during boss fight)
│  └─ Center: Objective prompt
│
├─ DialogueBoxUI
│  ├─ PortraitPanel (left 20%)
│  │  └─ Speaker portrait image
│  ├─ TextPanel (right 80%)
│  │  ├─ Speaker name text
│  │  ├─ Dialogue text (typewriter effect)
│  │  └─ Subtitle toggle
│  └─ ChoicesPanel (bottom)
│     ├─ Button A: First choice
│     ├─ Button B: Second choice
│     └─ Button C: Third choice (if available)
│
├─ PauseMenuUI (triggered by Esc)
│  ├─ Background blur
│  ├─ Button group:
│  │  ├─ [Resume]
│  │  ├─ [Save]
│  │  ├─ [Load]
│  │  ├─ [Settings]
│  │  ├─ [Chapter Select]
│  │  └─ [Quit to Menu]
│  ├─ Playtime counter
│  └─ Settings sliders
│
├─ TransitionUI
│  ├─ Black fade overlay (CanvasGroup)
│  ├─ Title card text (for scene changes)
│  └─ Progress bar (if loading takes time)
│
└─ Notifications
   ├─ Item collected popup
   ├─ Objective updated
   └─ Story flag triggers
```

### UI Event Flow

```
User clicks "New Game"
    ↓
MainMenuUI.OnNewGameClicked()
    ↓
Publish: NewGameStarted
    ├─ GameManager.ResetGameState()
    ├─ SaveManager.ClearSaves()
    └─ SceneLoader.LoadScene("Act1_HappyHome")
         ↓
         Fade to black
         ├─ SceneSetup.Initialize()
         └─ Fade from black
              ↓
              GameHUD appears (day, objective)
              ├─ DialogueManager loads dialogue
              └─ Publish: DialogueStarted
                   ↓
                   DialogueBoxUI appears with portrait
                   └─ Typewriter effect plays text
```

---

## 🔊 Audio Pipeline

```
Audio Generation Flow (Offline - during asset creation):

Python Scripts → WAV Files → Unity Assets

1. generate_audio.py
   ├─ Music generation
   │  ├─ Raga-based composition (Indian classical scales)
   │  ├─ Tanpura drone (fundamental + 5th + octave)
   │  ├─ Melody synthesis (sine wave + envelope)
   │  └─ Rhythm pattern (dhol, tabla emulation)
   │
   └─ SFX generation
      ├─ Footsteps (noise burst + frequency sweep)
      ├─ Spirit pulse (whoosh + reverb)
      ├─ UI clicks (sine wave chirp + fade)
      └─ [20+ other effects]

2. Audio files saved to:
   └─ Assets/_Project/Audio/
      ├─ Music/*.wav (44.1kHz, stereo, 20-120 seconds)
      └─ SFX/*.wav (44.1kHz, mono, 0.2-3 seconds)

3. Unity AudioManager loads during gameplay
   ├─ Mixer groups configured
   ├─ Master volume control
   ├─ Music crossfading (2 second lerp)
   └─ SFX pooling (10 sources, reusable)
```

**Audio Usage Mapping**:
```
Act 1: Happy Home
├─ Music: act1_happy_home.wav (Bilawal raga, major scale)
├─ SFX: Footsteps, door creak, bird chirp, UI clicks
└─ Voice: Dialogue voice lines

Act 2: Descent
├─ Music: act2_descent.wav (Bhairavi raga, minor scale - tense)
├─ SFX: Whispered breath, heartbeat (fast), footsteps (nervous)
└─ Dheki rhythm: dheki_rhythm_90bpm.wav → dheki_rhythm_150bpm.wav

Act 3: Spirit Awakens
├─ Music: act3_mystical.wav (Yaman raga, ethereal)
├─ SFX: Memory flash, spirit pulse whoosh, puzzle solve chime
└─ Ambient: Wind, distant bells, soft whispers

Act 4: Confrontation
├─ Music: act4_boss_theme.wav (dark orchestral)
├─ SFX: Barrel roll, vine snap, boss roar, spirit orb absorb
├─ Music Phase 2: act4_boss_phase2.wav (faster tempo)
└─ Music Phase 3: act4_boss_phase3.wav (epic finale)

Epilogue:
├─ Music: epilogue_sunrise.wav (peaceful resolution)
└─ Ambient: Birds, breeze, peaceful silence
```

---

## 🔄 Save System Architecture

```
SaveData Structure (JSON):

{
  "metadata": {
    "saveVersion": "1.0.0",
    "timestamp": "2026-02-17T17:30:00Z",
    "playtimeMinutes": 45
  },
  "gameState": {
    "currentAct": 2,
    "currentCharacter": "Tejimola",
    "currentPhase": "Stealth",
    "currentDay": 3,
    "lastLoadedScene": "Act2_Descent"
  },
  "progress": {
    "catchCount": 2,
    "exhaustionLevel": 65,
    "solvedPuzzles": [true, false, true, false, false],
    "collectedItems": ["hairpin_broken", "oil_lamp"],
    "spiritOrbCount": 1
  },
  "narrative": {
    "storyFlags": {
      "chose_kindness": true,
      "helped_father": true,
      "avoided_ranima_3times": true
    },
    "visitedLocations": ["courtyard", "kitchen", "bedroom"],
    "dialogueHistory": [
      "act1_opening_1",
      "act1_opening_2",
      "act1_help_choice"
    ]
  },
  "settings": {
    "volume": {
      "master": 0.8,
      "music": 0.7,
      "sfx": 0.9,
      "voice": 0.85
    },
    "display": {
      "resolution": "1920x1080",
      "fullscreen": true,
      "brightness": 1.0
    },
    "accessibility": {
      "subtitlesEnabled": true,
      "assameseTextSize": "medium",
      "colorBlindMode": false
    }
  }
}
```

**Save Flow**:
```
Player presses Escape → PauseMenu
    ↓
Player clicks "Save"
    ↓
SaveManager.SaveGame()
    ├─ Gather all GameManager state
    ├─ Gather all GameSettings
    ├─ Serialize to JSON
    └─ Write to: AppData\LocalLow\Tejimola Games\save.json
         ↓
         Show confirmation: "Game saved ✓"
         ↓
         Auto-return to game (or list saves)
```

---

## 📊 System Initialization Order

When the game starts:

```
1. Unity Loads Scene (MainMenu)
   ├─ Awake() called on all GameObjects

2. Initialization Order:
   ├─ Singletons initialize (First)
   │  ├─ EventManager.Awake()
   │  ├─ GameManager.Awake()
   │  ├─ SaveManager.Awake()
   │  ├─ AudioManager.Awake()
   │  ├─ DialogueManager.Awake()
   │  └─ SceneLoader.Awake()
   │
   ├─ Core Systems initialize (Second)
   │  ├─ Camera setup (ParallaxCamera)
   │  ├─ UI Canvas hierarchy created
   │  └─ Physics2D initialized
   │
   └─ Scene-specific Setup (Third)
      └─ SceneSetup derived class:
         ├─ Act1HappyHomeSetup.Initialize()
         ├─ Load dialogue for this scene
         ├─ Load music for this act
         ├─ Set camera bounds
         └─ Publish: SceneReady

3. Start() methods called
   ├─ MainMenuUI.Start() → show menu buttons
   ├─ GameHUD.Start() → subscribe to events
   └─ GameManager.Start() → load saved settings

4. GameRunning
   ├─ Publish: GameStarted
   └─ Update loop begins
       ├─ Input processing
       ├─ Physics updates
       ├─ Animation updates
       └─ Event callbacks
```

---

## 🎯 Event Message Bus Reference

### All Events Published

```
GAME STATE EVENTS:
- GameStarted()
- GamePaused()
- GameResumed()
- ActChanged(GameAct newAct)
- PhaseChanged(GamePhase newPhase)
- DayIncremented(int newDay)

DIALOGUE EVENTS:
- DialogueStarted(DialogueEntry entry)
- DialogueAdvanced(DialogueEntry entry)
- DialogueChoicePresented(DialogueChoice[] choices)
- DialogueChoiceSelected(int choiceIndex)
- DialogueEnded()
- StoryFlagSet(string flagName, bool value)

GAMEPLAY EVENTS:
- SpiritPulseActivated(Vector2 position)
- SpiritObjectRevealed(GameObject revealedObject)
- PlayerDetected(EnemyAI detector)
- StealthPhaseFailed()
- BeatHit(BeatRating rating, float exhaustion)
- ExhaustionThresholdReached()
- RhythmPhaseFailed()
- PuzzleStarted(int puzzleIndex)
- PuzzleSolved(int puzzleIndex)
- ItemCollected(string itemID)
- BossPhaseChanged(BossPhase phase)
- BossDefeated()
- GameOver()

AUDIO EVENTS:
- MusicChanged(string trackID)
- SFXPlayed(string soundID, Vector2 position)
- VolumeChanged(AudioGroup group, float newVolume)

UI EVENTS:
- HUDUpdated()
- PauseMenuOpened()
- PauseMenuClosed()
- SaveCompleted()
- LoadCompleted()

SCENE EVENTS:
- SceneLoaded(string sceneName)
- SceneUnloaded(string sceneName)
- TransitionStarted(string toScene)
- TransitionCompleted()
```

---

## Performance Optimization Notes

```
Optimization Strategies Implemented:

1. Object Pooling
   ├─ AudioSources (10 pool for SFX)
   ├─ Particle systems (spirits, VFX)
   └─ Reusable UI elements (dialogue boxes)

2. Asset Streaming
   ├─ Scenes load asynchronously
   ├─ Dialogue loaded on-demand from JSON
   └─ Audio loaded into memory (44.1kHz compressed)

3. Physics Optimization
   ├─ Use OnTriggerEnter/Exit instead of OnCollisionEnter
   ├─ Raycasts cached per frame
   └─ Limited Physics2D overlap checks (every other frame)

4. Graphics Optimization
   ├─ Sprite atlasing (backgrounds, characters)
   ├─ Parallel layer rendering (parallax)
   ├─ Shader LOD (hand-painted effect quality reduced in distance)
   └─ Particle effect limits (max 50 simultaneous)

5. Memory Management
   ├─ Unload unused scenes before loading new ones
   ├─ Destroy dialogue UI when dialogue ends
   ├─ Limit save file count (keep last 5 saves)
   └─ Cache dialogue JSON after first load
```

---

## Conclusion

The Tejimola game architecture is built on three core principles:

1. **Modularity** - Systems are independent, communicate via events
2. **Scalability** - Easy to add new mechanics without modifying existing code
3. **Narrative Focus** - All systems serve the story (dialogue, choices, consequences)

The event-driven pub/sub pattern ensures that adding new mechanics (like new puzzles or boss phases) doesn't require modifying existing code — just publish new events and let systems subscribe to what they care about.
