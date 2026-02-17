using UnityEngine;

namespace Tejimola.Utils
{
    public static class GameColors
    {
        // Act I - Innocence & Warmth
        public static readonly Color Gold = new Color(1f, 0.843f, 0f);              // #FFD700 - Innocence
        public static readonly Color SkyBlue = new Color(0.529f, 0.808f, 0.922f);   // #87CEEB - Freedom
        public static readonly Color EarthBrown = new Color(0.545f, 0.271f, 0.075f); // #8B4513 - Home
        public static readonly Color WarmBrown = new Color(0.396f, 0.263f, 0.129f);  // #654321 - Earth

        // Act II - Suffering & Darkness
        public static readonly Color DarkSlate = new Color(0.184f, 0.310f, 0.310f);  // #2F4F4F - Suffering
        public static readonly Color DarkMagenta = new Color(0.545f, 0f, 0.345f);    // #8B0058 - Corruption

        // Act III - Awakening
        public static readonly Color HopeBlue = new Color(0.392f, 0.584f, 0.929f);   // #6495ED
        public static readonly Color Silver = new Color(0.753f, 0.753f, 0.753f);      // #C0C0C0 - Awakening

        // Act IV & Epilogue - Rebirth & Triumph
        public static readonly Color ForestGreen = new Color(0.133f, 0.545f, 0.133f); // #228B22 - Rebirth
        public static readonly Color SpiritPurple = new Color(0.294f, 0f, 0.510f);    // #4B0082 - Spirit
        public static readonly Color TriumphRed = new Color(0.863f, 0.078f, 0.235f);  // #DC143C - Triumph
        public static readonly Color Pure = new Color(1f, 1f, 1f);                     // #FFFFFF - Purity
    }

    public static class GameConstants
    {
        // Spirit Pulse
        public const float SpiritPulseRadius = 5f;
        public const float SpiritPulseCooldown = 3f;
        public const float SpiritPulseExpandSpeed = 8f;
        public const float SpiritPulseDuration = 1.5f;

        // Stealth
        public const int MaxCatches = 5;
        public const float FootprintFadeDuration = 3f;
        public const float DetectionRadius = 4f;
        public const float DetectionAngle = 60f;

        // Rhythm
        public const float BaseBPM = 90f;
        public const float MaxBPM = 150f;
        public const float BaseHitWindow = 0.25f;
        public const float MinHitWindow = 0.1f;
        public const float ExhaustionStart = 100f;
        public const float ExhaustionHitCost = 1f;
        public const float ExhaustionMissCost = 0f;

        // Movement
        public const float MoveSpeed = 5f;
        public const float CrouchSpeed = 2.5f;
        public const float InteractionRange = 2f;

        // Camera
        public const float CameraFollowSpeed = 5f;
        public const float ParallaxForegroundSpeed = 1f;
        public const float ParallaxMidgroundSpeed = 0.6f;
        public const float ParallaxBackgroundSpeed = 0.3f;
        public const float ParallaxSkySpeed = 0.1f;

        // Game Structure
        public const float TotalGameMinutes = 90f;
        public const int TotalActs = 4;
        public const int TotalPuzzles = 5;

        // Save
        public const string SaveFileName = "tejimola_save.json";
        public const string SettingsFileName = "tejimola_settings.json";
    }

    public enum GameAct
    {
        MainMenu,
        Act1_HappyHome,
        Act1_Funeral,
        Act2_Descent,
        Act2_Dheki,
        Act2_Burial,
        Act3_DomArrival,
        Act3_DualTimeline,
        Act4_Confrontation,
        Epilogue
    }

    public enum ActiveCharacter
    {
        None,
        Tejimola,
        Dom
    }

    public enum GamePhase
    {
        Exploration,
        Dialogue,
        Stealth,
        Rhythm,
        Puzzle,
        BossFight,
        Cutscene,
        Paused
    }

    public enum DetectionState
    {
        Unaware,
        Suspicious,
        Alerted,
        Searching
    }

    public enum BeatRating
    {
        Perfect,
        Good,
        Miss
    }

    public enum PuzzleType
    {
        Well,
        Hairpin,
        Lullaby,
        Boat,
        NahorSeed
    }

    public enum BossPhase
    {
        Phase1_Navigate,
        Phase2_SpiritOrbs,
        Phase3_BarrelPursuit
    }
}
