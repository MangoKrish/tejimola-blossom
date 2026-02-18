using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Tejimola.Characters;
using Tejimola.Core;
using Tejimola.Gameplay;
using Tejimola.Scenes;
using Tejimola.Utils;

/// <summary>
/// Programmatically builds all 10 Tejimola game scenes.
/// Run: Tejimola menu -> Build All Scenes
/// </summary>
public static class SceneBuilder
{
    // ── Layer indices ──────────────────────────────────────────
    const int L_Ground       = 6;
    const int L_Player       = 7;
    const int L_Interactable = 8;
    const int L_Obstacle     = 9;

    // ── Asset root paths ───────────────────────────────────────
    const string ART      = "Assets/_Project/Art";
    const string SCENES   = "Assets/_Project/Scenes";
    const string PREFABS  = "Assets/_Project/Prefabs";

    // Cached references populated during build
    static AnimatorController s_tejiController;
    static AnimatorController s_domController;
    static TMP_FontAsset s_defaultFont;

    static TMP_FontAsset DefaultFont()
    {
        if (s_defaultFont != null) return s_defaultFont;
        // Unity 6 TMP font paths — try common locations
        var paths = new[]
        {
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset",
            "Packages/com.unity.ugui/PackageResources/TMP/Fonts/LiberationSans SDF.asset",
            "Packages/com.unity.textmeshpro/Resources/Fonts & Materials/LiberationSans SDF.asset",
        };
        foreach (var p in paths)
        {
            try { s_defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p); } catch { }
            if (s_defaultFont != null) return s_defaultFont;
        }
        // TMP_Settings.defaultFontAsset can throw in batch mode — guard it
        try { s_defaultFont = TMP_Settings.defaultFontAsset; } catch { }
        return s_defaultFont; // callers must null-check before assigning
    }

    // ═══════════════════════════════════════════════════════════
    //  ENTRY POINT
    // ═══════════════════════════════════════════════════════════

    [MenuItem("Tejimola/Build All Scenes %#b", false, 0)]
    public static void BuildAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Tejimola Scene Builder",
            "This will rebuild all 10 game scenes with full content.\n\nContinue?",
            "Build All Scenes", "Cancel"))
            return;

        BuildAllScenesBatch();

        EditorUtility.DisplayDialog("Done!",
            "All 10 scenes built successfully!\n\n" +
            "Open MainMenu scene and press Play to start the game.",
            "Great!");
    }

    /// <summary>Batch-mode friendly version — no UI dialogs, throws on failure.</summary>
    public static void BuildAllScenesBatch()
    {
        try
        {
            Debug.Log("[SceneBuilder] Setting up layers…");
            SetupLayers();

            Debug.Log("[SceneBuilder] Creating animator controllers…");
            Directory.CreateDirectory(PREFABS);
            CreateAnimatorControllers();

            Debug.Log("[SceneBuilder] Setting up animation clips…");
            AnimationSetup.SetupAnimationsBatch();

            BuildScene("MainMenu",             BuildMainMenu);
            BuildScene("Act1_HappyHome",       BuildAct1HappyHome);
            BuildScene("Act1_Funeral",         BuildAct1Funeral);
            BuildScene("Act2_Descent",         BuildAct2Descent);
            BuildScene("Act2_Dheki",           BuildAct2Dheki);
            BuildScene("Act2_Burial",          BuildAct2Burial);
            BuildScene("Act3_DomArrival",      BuildAct3DomArrival);
            BuildScene("Act3_DualTimeline",    BuildAct3DualTimeline);
            BuildScene("Act4_Confrontation",   BuildAct4Confrontation);
            BuildScene("Epilogue",             BuildEpilogue);

            Debug.Log("[SceneBuilder] Updating Build Settings…");
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneBuilder] All 10 scenes built successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneBuilder] FAILED: {e}");
            throw;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  LAYER SETUP
    // ═══════════════════════════════════════════════════════════

    static void SetupLayers()
    {
        var tm = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/TagManager.asset"));
        var layers = tm.FindProperty("layers");

        void AddLayer(int index, string name)
        {
            var elem = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(elem.stringValue))
                elem.stringValue = name;
        }

        AddLayer(L_Ground,       "Ground");
        AddLayer(L_Player,       "Player");
        AddLayer(L_Interactable, "Interactable");
        AddLayer(L_Obstacle,     "Obstacle");
        tm.ApplyModifiedProperties();
    }

    // ═══════════════════════════════════════════════════════════
    //  ANIMATOR CONTROLLERS
    // ═══════════════════════════════════════════════════════════

    static void CreateAnimatorControllers()
    {
        s_tejiController = MakeAnimController("TejimolaAnimator");
        s_domController  = MakeAnimController("DomAnimator");
    }

    static AnimatorController MakeAnimController(string name)
    {
        string path = $"{PREFABS}/{name}.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsCrouching",AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsHiding",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Interact",   AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Cry",        AnimatorControllerParameterType.Trigger);

        var root = ctrl.layers[0].stateMachine;
        root.AddState("Idle");
        root.AddState("Walk");
        root.AddState("Crouch");
        root.AddState("Hide");
        return ctrl;
    }

    // ═══════════════════════════════════════════════════════════
    //  SCENE SHELL
    // ═══════════════════════════════════════════════════════════

    static void BuildScene(string sceneName, Action builder)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        builder();
        Directory.CreateDirectory(SCENES);
        EditorSceneManager.SaveScene(scene, $"{SCENES}/{sceneName}.unity");
        Debug.Log($"[SceneBuilder] Saved {sceneName}");
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  MAIN MENU
    // ═══════════════════════════════════════════════════════════

    static void BuildMainMenu()
    {
        // ── Singleton managers (created once, DontDestroyOnLoad) ──
        AddManager<GameManager>("GameManager");
        AddManager<EventManager>("EventManager");
        AddManager<AudioManager>("AudioManager");
        AddManager<SaveManager>("SaveManager");
        AddManager<SceneLoader>("SceneLoader");

        // ── Camera ──
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic    = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.05f, 0.02f, 0.02f);
        cam.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ── EventSystem ──
        AddEventSystem();

        // ── Main Canvas ──
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        Transform cvs = canvasGO.transform;

        // ── Background Image (fullscreen canvas) ──
        var bgPanel = MakePanel(cvs, "Background", Color.white);
        RectAt(bgPanel.transform, 0f, 0f, 1f, 1f);
        bgPanel.transform.SetAsFirstSibling();
        var bgImg = bgPanel.GetComponent<Image>();
        bgImg.sprite = Spr("UI/Menu/menu_background.png");
        bgImg.type   = Image.Type.Sliced;

        // ── Nahor Tree Image (canvas right side) ──
        var treePanel = MakePanel(cvs, "NahorTreeDecoration", Color.white);
        RectAt(treePanel.transform, 0.55f, 0.0f, 0.35f, 0.7f);
        var treeImg = treePanel.GetComponent<Image>();
        treeImg.sprite         = Spr("Sprites/Props/nahor_flower.png");
        treeImg.preserveAspect = true;

        // ── Title text ──
        var titleTMP = MakeTMP(cvs, "TitleText", "TEJIMOLA", 72, TextAlignmentOptions.Center);
        RectAt(titleTMP.transform, 0f, 0.72f, 1f, 0.12f);
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = new Color(1f, 0.85f, 0.3f);

        var subtitleTMP = MakeTMP(cvs, "SubtitleText", "THE BLOSSOM FROM CLAY", 28, TextAlignmentOptions.Center);
        RectAt(subtitleTMP.transform, 0f, 0.62f, 1f, 0.08f);
        subtitleTMP.color = new Color(0.9f, 0.85f, 0.75f);

        // ── Main Panel ──
        var mainPanel = MakePanel(cvs, "MainPanel", new Color(0, 0, 0, 0));
        RectAt(mainPanel.transform, 0.35f, 0.15f, 0.30f, 0.44f);

        var newGameBtn = MakeButton(mainPanel.transform, "NewGameButton",  "New Game",  Spr("UI/Menu/btn_new_game.png"));
        var continueBtn= MakeButton(mainPanel.transform, "ContinueButton","Continue",  Spr("UI/Menu/btn_continue.png"));
        var extrasBtn  = MakeButton(mainPanel.transform, "ExtrasButton",  "Extras",    Spr("UI/Menu/btn_extras.png"));
        var quitBtn    = MakeButton(mainPanel.transform, "QuitButton",    "Quit",      Spr("UI/Menu/btn_quit.png"));
        LayoutVertical(mainPanel.transform, 10f);

        // ── Settings Panel (inactive) ──
        var settingsPanel = MakePanel(cvs, "SettingsPanel", new Color(0.05f, 0.02f, 0.02f, 0.95f));
        RectAt(settingsPanel.transform, 0.25f, 0.15f, 0.50f, 0.65f);
        settingsPanel.SetActive(false);

        MakeTMP(settingsPanel.transform, "SettingsTitle", "SETTINGS", 36, TextAlignmentOptions.Center);
        var masterSlider = MakeSlider(settingsPanel.transform, "MasterVolumeSlider", "Master Volume", 0, 1, 1f);
        var musicSlider  = MakeSlider(settingsPanel.transform, "MusicVolumeSlider",  "Music Volume",  0, 1, 0.7f);
        var sfxSlider    = MakeSlider(settingsPanel.transform, "SFXVolumeSlider",    "SFX Volume",    0, 1, 0.8f);
        var subtitlesToggle  = MakeToggle(settingsPanel.transform, "SubtitlesToggle",  "Subtitles");
        var fullscreenToggle = MakeToggle(settingsPanel.transform, "FullscreenToggle", "Fullscreen");
        var settingsBack = MakeButton(settingsPanel.transform, "BackButton", "Back", Spr("UI/Menu/btn_back.png"));
        LayoutVertical(settingsPanel.transform, 8f);

        // ── Extras Panel (inactive) ──
        var extrasPanel = MakePanel(cvs, "ExtrasPanel", new Color(0.05f, 0.02f, 0.02f, 0.95f));
        RectAt(extrasPanel.transform, 0.15f, 0.10f, 0.70f, 0.75f);
        extrasPanel.SetActive(false);

        MakeTMP(extrasPanel.transform, "ExtrasTitle", "EXTRAS", 36, TextAlignmentOptions.Center);
        MakeButton(extrasPanel.transform, "CreditsNavButton", "Credits", null);
        MakeButton(extrasPanel.transform, "ExtrasBack", "Back", Spr("UI/Menu/btn_back.png"));
        LayoutVertical(extrasPanel.transform, 10f);

        // ── Credits Panel (inactive, opened via OnOpenCredits) ──
        var creditsPanel = MakePanel(cvs, "CreditsPanel", new Color(0.05f, 0.02f, 0.02f, 0.95f));
        RectAt(creditsPanel.transform, 0.15f, 0.10f, 0.70f, 0.75f);
        creditsPanel.SetActive(false);

        MakeTMP(creditsPanel.transform, "CreditsTitle", "CREDITS", 36, TextAlignmentOptions.Center);
        var creditsBody = MakeTMP(creditsPanel.transform, "CreditsText",
            "TEJIMOLA: THE BLOSSOM FROM CLAY\n\n" +
            "Based on the Assamese folk tale of Tejimola\n\n" +
            "A story of remembrance, resilience, and rebirth.\n\n" +
            "Developed with Unity 6\n" +
            "Music, Art, and Dialogue created for this project.",
            20, TextAlignmentOptions.Center);
        creditsBody.color = new Color(0.9f, 0.85f, 0.75f);
        MakeButton(creditsPanel.transform, "BackButton", "Back", Spr("UI/Menu/btn_back.png"));
        LayoutVertical(creditsPanel.transform, 10f);

        // ── Wire MainMenuUI ──
        var menuUI = canvasGO.AddComponent<Tejimola.UI.MainMenuUI>();
        var so = new SerializedObject(menuUI);
        so.FindProperty("newGameButton").objectReferenceValue   = newGameBtn;
        so.FindProperty("continueButton").objectReferenceValue  = continueBtn;
        so.FindProperty("extrasButton").objectReferenceValue    = extrasBtn;
        so.FindProperty("quitButton").objectReferenceValue      = quitBtn;
        so.FindProperty("mainPanel").objectReferenceValue       = mainPanel;
        so.FindProperty("settingsPanel").objectReferenceValue   = settingsPanel;
        so.FindProperty("extrasPanel").objectReferenceValue     = extrasPanel;
        so.FindProperty("creditsPanel").objectReferenceValue    = creditsPanel;
        so.FindProperty("titleText").objectReferenceValue       = titleTMP;
        so.FindProperty("subtitleText").objectReferenceValue    = subtitleTMP;
        so.FindProperty("backgroundImage").objectReferenceValue = bgImg;
        so.FindProperty("nahorTreeImage").objectReferenceValue  = treeImg;
        so.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider;
        so.FindProperty("musicVolumeSlider").objectReferenceValue  = musicSlider;
        so.FindProperty("sfxVolumeSlider").objectReferenceValue    = sfxSlider;
        so.FindProperty("subtitlesToggle").objectReferenceValue    = subtitlesToggle;
        so.FindProperty("fullscreenToggle").objectReferenceValue   = fullscreenToggle;
        so.ApplyModifiedProperties();
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 1 – HAPPY HOME
    // ═══════════════════════════════════════════════════════════

    static void BuildAct1HappyHome()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act1",
            character:  "Tejimola",
            setupType:  typeof(Act1HappyHomeSetup),
            groundY:    -2.5f,
            groundWidth: 30f,
            cameraMaxX: 12f,
            ambientColor: GameColors.Gold);

        // ── Platforms for exploration ──
        CreatePlatform("Platform_Porch",   new Vector3(-6f,  -1.5f, 0), 5f,  0.5f, GameColors.EarthBrown);
        CreatePlatform("Platform_Garden",  new Vector3( 3f,  -1.5f, 0), 6f,  0.5f, new Color(0.3f, 0.5f, 0.2f));
        CreatePlatform("Platform_Roof",    new Vector3( 7f,   0.5f, 0), 4f,  0.4f, new Color(0.6f, 0.4f, 0.2f));
        CreatePlatform("Platform_Step1",   new Vector3(-2f,  -1.8f, 0), 2f,  0.3f, GameColors.EarthBrown);
        CreatePlatform("Platform_WellArea",new Vector3(-9f,  -1.5f, 0), 4f,  0.5f, new Color(0.5f, 0.5f, 0.6f));

        // ── Interactable objects ──
        var items = new (string name, string sprite, Vector3 pos, string dialogue, string itemId)[]
        {
            ("Kitchen",      "Sprites/Props/pot.png",          new Vector3(-5f, -1.0f, 0), "act1_kitchen",     ""),
            ("Bedroom",      "Sprites/Props/gamosa.png",       new Vector3(0f,  -1.0f, 0), "act1_bedroom",     ""),
            ("NahorTree",    "Sprites/Props/nahor_flower.png", new Vector3(6f,  -1.0f, 0), "act1_explore_home",""),
            ("Hairpin",      "Sprites/Props/hairpin.png",      new Vector3(3f,  -2.0f, 0), "act1_explore_home","hairpin"),
            ("OilLamp",      "Sprites/Props/oil_lamp.png",     new Vector3(-8f, -1.0f, 0), "act1_explore_home",""),
            ("WaterPot",     "Sprites/Props/gourd.png",        new Vector3(-9f, -1.0f, 0), "act1_kitchen",     ""),
        };
        foreach (var item in items)
            CreateInteractable(item.name, item.sprite, item.pos, item.dialogue, item.itemId, item.itemId != "");

        // ── Father NPC ──
        var fatherGO = new GameObject("Father_NPC");
        var fSR = fatherGO.AddComponent<SpriteRenderer>();
        fSR.sprite = Spr("Sprites/Characters/father_spritesheet.png");
        fSR.sortingOrder = 2;
        fatherGO.transform.position = new Vector3(-7f, -2.0f, 0);
        fatherGO.layer = L_Interactable;
        fatherGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var fIA = fatherGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(fIA, "dialogueId", "act1_explore_home");
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 1 – FUNERAL
    // ═══════════════════════════════════════════════════════════

    static void BuildAct1Funeral()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act1",
            character:  "Tejimola",
            setupType:  typeof(Act1FuneralSetup),
            groundY:    -2.5f,
            groundWidth: 20f,
            cameraMaxX: 8f,
            ambientColor: new Color(0.7f, 0.7f, 0.8f));

        // ── Elevated platforms – funeral procession path ──
        CreatePlatform("Platform_RiverBank",   new Vector3(-4f, -1.5f, 0), 6f, 0.5f, new Color(0.4f, 0.5f, 0.4f));
        CreatePlatform("Platform_CremationMound", new Vector3(4f, -0.5f, 0), 4f, 0.5f, new Color(0.5f, 0.4f, 0.3f));
        CreatePlatform("Platform_GraveSide",   new Vector3(7f,  -1.8f, 0), 3f, 0.3f, new Color(0.3f, 0.35f, 0.3f));

        // ── Nahor seedling ──
        var seedGO = new GameObject("NahorSeedling");
        var seedSR = seedGO.AddComponent<SpriteRenderer>();
        seedSR.sprite = Spr("Sprites/Props/nahor_flower.png");
        seedSR.sortingOrder = 2;
        seedGO.transform.position = new Vector3(5f, -1.5f, 0);
        seedGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // ── Ranima introduction ──
        var ranimaGO = new GameObject("Ranima_Intro");
        var ranimaSR = ranimaGO.AddComponent<SpriteRenderer>();
        ranimaSR.sprite = Spr("Sprites/Characters/ranima_spritesheet.png");
        ranimaSR.sortingOrder = 2;
        ranimaGO.transform.position = new Vector3(6f, -2.0f, 0);
        ranimaGO.layer = L_Interactable;
        ranimaGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var rIA = ranimaGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(rIA, "dialogueId", "act1_ranima_intro");

        // ── Village elder NPC ──
        CreateInteractable("VillageElder", "Sprites/Characters/father_spritesheet.png",
            new Vector3(-2f, -2.0f, 0), "act1_funeral", "", false);

        // ── Memorial pot ──
        CreateInteractable("MemorialPot", "Sprites/Props/pot.png",
            new Vector3(0f, -2.0f, 0), "act1_funeral", "", false);
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 2 – DESCENT (Stealth)
    // ═══════════════════════════════════════════════════════════

    static void BuildAct2Descent()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act2",
            character:  "Tejimola",
            setupType:  null,           // we add manually below
            groundY:    -2.5f,
            groundWidth: 25f,
            cameraMaxX: 10f,
            ambientColor: GameColors.DarkSlate);

        // Stealth Manager
        var stealthGO = new GameObject("StealthManager");
        var stealth   = stealthGO.AddComponent<StealthManager>();

        // Ranima (enemy)
        var ranimaGO = new GameObject("Ranima");
        ranimaGO.layer = L_Obstacle;
        var ranimaSR   = ranimaGO.AddComponent<SpriteRenderer>();
        ranimaSR.sprite       = Spr("Sprites/Characters/ranima_spritesheet.png");
        ranimaSR.sortingOrder = 1;
        ranimaGO.transform.position = new Vector3(4f, -1.5f, 0);
        var ranimaRB = ranimaGO.AddComponent<Rigidbody2D>();
        ranimaRB.gravityScale   = 0;
        ranimaRB.freezeRotation = true;
        var ranimaCol = ranimaGO.AddComponent<BoxCollider2D>();
        var enemy     = ranimaGO.AddComponent<EnemyAI>();

        // Patrol points
        var p1 = new GameObject("PatrolPoint1"); p1.transform.position = new Vector3(-5f, -1.5f, 0);
        var p2 = new GameObject("PatrolPoint2"); p2.transform.position = new Vector3(5f,  -1.5f, 0);

        // Exclamation / Question mark sprites
        var exclGO = new GameObject("Exclamation"); exclGO.transform.SetParent(ranimaGO.transform);
        exclGO.transform.localPosition = new Vector3(0, 1.2f, 0);
        var exclSR = exclGO.AddComponent<SpriteRenderer>();
        exclSR.sortingOrder = 5; exclSR.enabled = false;

        var questGO = new GameObject("QuestionMark"); questGO.transform.SetParent(ranimaGO.transform);
        questGO.transform.localPosition = new Vector3(0, 1.2f, 0);
        var questSR = questGO.AddComponent<SpriteRenderer>();
        questSR.sortingOrder = 5; questSR.enabled = false;

        // Wire EnemyAI
        SetSOP(enemy, "patrolPoints", new Transform[] { p1.transform, p2.transform });
        SetSO(enemy,  "exclamationMark", exclSR);
        SetSO(enemy,  "questionMark",    questSR);
        SetSOf(enemy, "patrolSpeed",   2f);
        SetSOf(enemy, "chaseSpeed",    3.5f);
        SetSOi(enemy, "playerLayer",   1 << L_Player);
        SetSOi(enemy, "obstacleLayer", 1 << L_Obstacle);

        // ── Platforms ──
        CreatePlatform("Platform_Storage", new Vector3(-4f, -1.5f, 0), 4f, 0.4f, new Color(0.35f, 0.25f, 0.2f));
        CreatePlatform("Platform_Ledge",   new Vector3( 3f, -0.5f, 0), 3f, 0.4f, new Color(0.3f, 0.3f, 0.35f));
        CreatePlatform("Platform_Exit",    new Vector3( 8f, -1.5f, 0), 3f, 0.4f, new Color(0.3f, 0.3f, 0.25f));

        // ── Hiding spots ──
        CreateHidingSpot("HidingSpot_Cabinet",  new Vector3(-3f, -1.8f, 0));
        CreateHidingSpot("HidingSpot_Curtain",  new Vector3( 0f, -1.8f, 0));
        CreateHidingSpot("HidingSpot_Barrel",   new Vector3( 5f, -1.8f, 0));
        CreateHidingSpot("HidingSpot_Shadow",   new Vector3(-7f, -1.8f, 0));

        // ── Extra patrol points for longer route ──
        var p3 = new GameObject("PatrolPoint3"); p3.transform.position = new Vector3(-8f, -1.5f, 0);
        var p4 = new GameObject("PatrolPoint4"); p4.transform.position = new Vector3( 8f, -1.5f, 0);

        // Wire StealthManager
        var player = GameObject.Find("Player_Tejimola");
        SetSO(stealth, "ranimaAI", enemy);
        if (player) SetSO(stealth, "tejimola", player.GetComponent<TejimolaBehaviour>());

        // Now add SceneSetup
        var setupGO = new GameObject("SceneSetup");
        var setup   = setupGO.AddComponent<Act2DescentSetup>();
        SetSO(setup, "stealthManager", stealth);
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 2 – DHEKI (Rhythm)
    // ═══════════════════════════════════════════════════════════

    static void BuildAct2Dheki()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act2",
            character:  "Tejimola",
            setupType:  null,
            groundY:    -2.5f,
            groundWidth: 15f,
            cameraMaxX: 5f,
            ambientColor: new Color(0.5f, 0.4f, 0.3f));

        // ── Platforms in dheki area ──
        CreatePlatform("Platform_DhekiBase", new Vector3(0f,  -1.5f, 0), 5f, 0.5f, new Color(0.4f, 0.3f, 0.2f));
        CreatePlatform("Platform_Rest",      new Vector3(-4f, -1.5f, 0), 3f, 0.4f, GameColors.EarthBrown);

        // Dheki prop
        var dhekiGO = new GameObject("Dheki");
        var dhekiSR = dhekiGO.AddComponent<SpriteRenderer>();
        dhekiSR.sprite       = Spr("Sprites/Props/dheki.png");
        dhekiSR.sortingOrder = 1;
        dhekiGO.transform.position = new Vector3(1f, -1.5f, 0);
        dhekiGO.transform.localScale = new Vector3(2, 2, 1);

        // RhythmEngine
        var rhythmGO = new GameObject("RhythmEngine");
        var rhythm   = rhythmGO.AddComponent<RhythmEngine>();
        SetSO(rhythm,  "dhekiHitSound",  Clip("SFX/beat_hit_perfect"));
        SetSO(rhythm,  "dhekiMissSound", Clip("SFX/beat_miss"));
        SetSO(rhythm,  "heartbeatSound", Clip("SFX/heartbeat"));
        SetSO(rhythm,  "musicTrack",     Clip("Music/act2_dheki"));

        // Rhythm UI Canvas
        var rythmCvs = MakeWorldCanvas("RhythmUICanvas", sortOrder: 10);
        var rhythmUI = rythmCvs.AddComponent<Tejimola.Gameplay.RhythmUI>();

        // Beat tracks
        var trackLeft  = MakeRhythmTrack(rythmCvs.transform, "BeatTrackLeft",  new Vector2(-200, 0));
        var trackRight = MakeRhythmTrack(rythmCvs.transform, "BeatTrackRight", new Vector2(200, 0));
        var hitLeft    = MakeHitZone(rythmCvs.transform, "HitZoneLeft",  new Vector2(-200, -200));
        var hitRight   = MakeHitZone(rythmCvs.transform, "HitZoneRight", new Vector2(200, -200));

        // Exhaustion bar
        var exhPanel = MakePanel(rythmCvs.transform, "ExhaustionPanel", new Color(0.1f, 0.1f, 0.1f, 0.8f));
        SetRect(exhPanel.transform, new Vector2(0, 50), new Vector2(300, 30), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var exhBg   = exhPanel.GetComponent<Image>();
        var exhFill = MakePanel(exhPanel.transform, "ExhaustionFill", new Color(0.2f, 0.7f, 0.3f)).GetComponent<Image>();

        var ratingTMP = MakeTMP(rythmCvs.transform, "RatingText", "", 40, TextAlignmentOptions.Center);
        var bpmTMP    = MakeTMP(rythmCvs.transform, "BPMText",    "90 BPM", 24, TextAlignmentOptions.Left);
        var comboTMP  = MakeTMP(rythmCvs.transform, "ComboText",  "x0", 30, TextAlignmentOptions.Right);

        var visionPanel = MakePanel(rythmCvs.transform, "VisionPanel", new Color(0, 0, 0, 0.7f));
        visionPanel.SetActive(false);
        var choice0 = MakeTMP(visionPanel.transform, "VisionChoice0", "", 24, TextAlignmentOptions.Center);
        var choice1 = MakeTMP(visionPanel.transform, "VisionChoice1", "", 24, TextAlignmentOptions.Center);

        // Beat note prefab
        var beatNotePrefab = CreateBeatNotePrefab();

        // Wire RhythmUI
        var rso = new SerializedObject(rhythmUI);
        rso.FindProperty("beatTrackLeft").objectReferenceValue  = trackLeft;
        rso.FindProperty("beatTrackRight").objectReferenceValue = trackRight;
        rso.FindProperty("hitZoneLeft").objectReferenceValue    = hitLeft;
        rso.FindProperty("hitZoneRight").objectReferenceValue   = hitRight;
        rso.FindProperty("exhaustionBar").objectReferenceValue  = exhBg;
        rso.FindProperty("exhaustionFill").objectReferenceValue = exhFill;
        rso.FindProperty("ratingText").objectReferenceValue     = ratingTMP;
        rso.FindProperty("bpmText").objectReferenceValue        = bpmTMP;
        rso.FindProperty("comboText").objectReferenceValue      = comboTMP;
        rso.FindProperty("visionPanel").objectReferenceValue    = visionPanel;
        rso.FindProperty("beatNotePrefab").objectReferenceValue = beatNotePrefab;
        var choiceArr = rso.FindProperty("visionChoiceTexts");
        choiceArr.arraySize = 2;
        choiceArr.GetArrayElementAtIndex(0).objectReferenceValue = choice0;
        choiceArr.GetArrayElementAtIndex(1).objectReferenceValue = choice1;
        rso.ApplyModifiedProperties();

        // SceneSetup
        var setupGO = new GameObject("SceneSetup");
        var setup   = setupGO.AddComponent<Act2DhekiSetup>();
        SetSO(setup, "rhythmEngine", rhythm);
        SetSO(setup, "rhythmUI",     rhythmUI);  // needed so Act2DhekiSetup can call rhythmUI.Initialize()
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 2 – BURIAL (Cinematic)
    // ═══════════════════════════════════════════════════════════

    static void BuildAct2Burial()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act2",
            character:  "Tejimola",
            setupType:  typeof(Act2BurialSetup),
            groundY:    -2.5f,
            groundWidth: 15f,
            cameraMaxX: 5f,
            ambientColor: new Color(0.1f, 0.1f, 0.15f));

        // ── Burial ground platforms ──
        CreatePlatform("Platform_Mound",    new Vector3( 0f, -1.5f, 0), 5f, 0.6f, new Color(0.35f, 0.3f, 0.25f));
        CreatePlatform("Platform_Approach", new Vector3(-4f, -2.0f, 0), 3f, 0.4f, new Color(0.3f, 0.3f, 0.3f));

        // ── Nahor tree – central element ──
        var treeGO = new GameObject("NahorTree_Burial");
        var treeSR = treeGO.AddComponent<SpriteRenderer>();
        treeSR.sprite = Spr("Sprites/Props/nahor_flower.png");
        treeSR.sortingOrder = 2;
        treeGO.transform.position = new Vector3(0, 0, 0);
        treeGO.transform.localScale = new Vector3(3, 3, 1);
        treeGO.layer = L_Interactable;
        treeGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var treeIA = treeGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(treeIA, "dialogueId", "act2_burial");

        // ── Burial items ──
        var potGO = new GameObject("Burial_Pot");
        var potSR = potGO.AddComponent<SpriteRenderer>();
        potSR.sprite = Spr("Sprites/Props/pot.png");
        potSR.sortingOrder = 2;
        potGO.transform.position = new Vector3(-2f, -2.0f, 0);

        var gamosaGO = new GameObject("Burial_Gamosa");
        var gamosaSR = gamosaGO.AddComponent<SpriteRenderer>();
        gamosaSR.sprite = Spr("Sprites/Props/gamosa.png");
        gamosaSR.sortingOrder = 2;
        gamosaGO.transform.position = new Vector3(2f, -2.0f, 0);

        // ── Rain particle effect (atmosphere) ──
        var rainGO = new GameObject("RainEffect");
        var rainPS = rainGO.AddComponent<ParticleSystem>();
        var rm = rainPS.main;
        rm.startColor    = new ParticleSystem.MinMaxGradient(new Color(0.5f, 0.6f, 0.9f, 0.4f));
        rm.startSize     = 0.05f;
        rm.startLifetime = 2f;
        rm.startSpeed    = 8f;
        rm.maxParticles  = 200;
        var rShape = rainPS.shape;
        rShape.enabled     = true;
        rShape.shapeType   = ParticleSystemShapeType.Box;
        rShape.scale       = new Vector3(20f, 0.1f, 1f);
        rainGO.transform.position = new Vector3(0, 6f, 0);
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 3 – DOM ARRIVAL
    // ═══════════════════════════════════════════════════════════

    static void BuildAct3DomArrival()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act3",
            character:  "Dom",
            setupType:  typeof(Act3DomArrivalSetup),
            groundY:    -2.5f,
            groundWidth: 25f,
            cameraMaxX: 10f,
            ambientColor: new Color(0.3f, 0.2f, 0.4f));

        // ── Platforms in ruined village ──
        CreatePlatform("Platform_Ruins1",  new Vector3(-6f,  -1.5f, 0), 4f, 0.4f, new Color(0.4f, 0.4f, 0.5f));
        CreatePlatform("Platform_Ruins2",  new Vector3( 2f,  -1.0f, 0), 3f, 0.4f, new Color(0.4f, 0.4f, 0.5f));
        CreatePlatform("Platform_Ruins3",  new Vector3( 7f,  -0.5f, 0), 3f, 0.4f, new Color(0.35f, 0.35f, 0.45f));
        CreatePlatform("Platform_Path",    new Vector3(-2f,  -1.8f, 0), 3f, 0.3f, new Color(0.3f, 0.3f, 0.35f));

        // ── Spirit-revealable memories ──
        CreateSpiritRevealable("TejimolaSpiritEcho", "Sprites/Characters/tejimola_spirit_spritesheet.png",
            new Vector3(3f,  -1f, 0), "echo_1", false);
        CreateSpiritRevealable("NahorTreeSpirit",    "Sprites/Props/nahor_flower.png",
            new Vector3(5f,   0f, 0), "nahor_spirit", true);
        CreateSpiritRevealable("MemoryOfFather",     "Sprites/Characters/father_spritesheet.png",
            new Vector3(-5f, -1f, 0), "echo_2", false);
        CreateSpiritRevealable("MemoryOfHome",       "Sprites/Props/pot.png",
            new Vector3( 7f, -0.2f, 0), "echo_3", false);

        // ── Spirit orbs to collect ──
        CreateSpiritOrb("Orb_1", new Vector3(-4f,  0f, 0));
        CreateSpiritOrb("Orb_2", new Vector3( 1f,  0.5f, 0));
        CreateSpiritOrb("Orb_3", new Vector3( 6f,  0.5f, 0));

        // ── Drum prop (Dom's instrument) ──
        var drumGO = new GameObject("DholDrum");
        var drumSR = drumGO.AddComponent<SpriteRenderer>();
        drumSR.sprite       = Spr("Sprites/Props/dhol_drum.png");
        drumSR.sortingOrder = 1;
        drumGO.transform.position = new Vector3(-3f, -2f, 0);
        drumGO.layer = L_Interactable;
        drumGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var drumIA = drumGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(drumIA, "dialogueId", "act3_first_encounter");

        // ── Tejimola spirit manifestation ──
        var spiritGO = new GameObject("Tejimola_Spirit");
        var spiritSR = spiritGO.AddComponent<SpriteRenderer>();
        spiritSR.sprite = Spr("Sprites/Characters/tejimola_spirit_spritesheet.png");
        spiritSR.sortingOrder = 3;
        spiritSR.color = new Color(0.7f, 0.8f, 1f, 0.6f);
        spiritGO.transform.position = new Vector3(8f, -1.5f, 0);
        spiritGO.layer = L_Interactable;
        spiritGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var spiritIA = spiritGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(spiritIA, "dialogueId", "act3_first_encounter");
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 3 – DUAL TIMELINE (Puzzles)
    // ═══════════════════════════════════════════════════════════

    static void BuildAct3DualTimeline()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act3",
            character:  "Dom",
            setupType:  null,
            groundY:    -2.5f,
            groundWidth: 30f,
            cameraMaxX: 14f,
            ambientColor: new Color(0.3f, 0.2f, 0.5f));

        // PuzzleManager
        var pmGO = new GameObject("PuzzleManager");
        var pm   = pmGO.AddComponent<PuzzleManager>();

        // Create 5 memory puzzles
        var puzzleTypes = new (PuzzleType type, string name, Vector3 pos, string presentSprite)[]
        {
            (PuzzleType.Well,      "WellPuzzle",     new Vector3(-8f,  -1f, 0), "Sprites/Props/pot.png"),
            (PuzzleType.Hairpin,   "HairpinPuzzle",  new Vector3(-3f,  -1f, 0), "Sprites/Props/hairpin.png"),
            (PuzzleType.Lullaby,   "LullabyPuzzle",  new Vector3(0f,   -1f, 0), "Sprites/Props/dhol_drum.png"),
            (PuzzleType.Boat,      "BoatPuzzle",      new Vector3(4f,   -1f, 0), "Sprites/Props/gourd.png"),
            (PuzzleType.NahorSeed, "NahorSeedPuzzle", new Vector3(9f,  -1f, 0), "Sprites/Props/nahor_flower.png"),
        };

        var puzzleList = new List<MemoryPuzzle>();
        foreach (var (pType, pName, pPos, pSprite) in puzzleTypes)
        {
            var pGO  = new GameObject(pName);
            pGO.transform.position = pPos;
            var pSR  = pGO.AddComponent<SpriteRenderer>();
            pSR.sprite       = Spr(pSprite);
            pSR.sortingOrder = 1;
            var col  = pGO.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            var mp   = pGO.AddComponent<MemoryPuzzle>();

            // Before/after state objects
            var before = new GameObject("BeforeState");
            before.transform.SetParent(pGO.transform);
            var beforeSR = before.AddComponent<SpriteRenderer>();
            beforeSR.sprite = pSR.sprite;
            beforeSR.color  = new Color(0.5f, 0.5f, 0.5f);

            var after = new GameObject("AfterState");
            after.transform.SetParent(pGO.transform);
            var afterSR = after.AddComponent<SpriteRenderer>();
            afterSR.sprite = pSR.sprite;
            afterSR.color  = new Color(0.8f, 1f, 0.8f);
            after.SetActive(false);

            var mso = new SerializedObject(mp);
            mso.FindProperty("puzzleType").enumValueIndex = (int)pType;
            mso.FindProperty("puzzleName").stringValue    = pName;
            mso.FindProperty("beforeState").objectReferenceValue = before;
            mso.FindProperty("afterState").objectReferenceValue  = after;
            mso.ApplyModifiedProperties();

            puzzleList.Add(mp);
        }

        // Wire PuzzleManager
        var pmSO = new SerializedObject(pm);
        var puzzleProp = pmSO.FindProperty("puzzles");
        puzzleProp.arraySize = puzzleList.Count;
        for (int i = 0; i < puzzleList.Count; i++)
            puzzleProp.GetArrayElementAtIndex(i).objectReferenceValue = puzzleList[i];
        pmSO.ApplyModifiedProperties();

        // ── Platforms between puzzles ──
        CreatePlatform("Platform_Puzzle_Left",   new Vector3(-10f, -1.5f, 0), 5f, 0.4f, new Color(0.3f, 0.25f, 0.4f));
        CreatePlatform("Platform_Puzzle_Center",  new Vector3(-1f,  -1.0f, 0), 4f, 0.4f, new Color(0.3f, 0.25f, 0.4f));
        CreatePlatform("Platform_Puzzle_Right",   new Vector3( 6f,  -0.5f, 0), 4f, 0.4f, new Color(0.3f, 0.25f, 0.4f));
        CreatePlatform("Platform_Puzzle_Far",     new Vector3(11f,  -1.5f, 0), 3f, 0.4f, new Color(0.25f, 0.2f, 0.35f));

        // ── Spirit orbs as collectibles near puzzles ──
        CreateSpiritOrb("Orb_DualA", new Vector3(-6f, 0.5f, 0));
        CreateSpiritOrb("Orb_DualB", new Vector3( 2f, 0.5f, 0));
        CreateSpiritOrb("Orb_DualC", new Vector3( 9f, 0.5f, 0));

        // SceneSetup
        var setupGO = new GameObject("SceneSetup");
        var setup   = setupGO.AddComponent<Act3DualTimelineSetup>();
        SetSO(setup, "puzzleManager", pm);
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  ACT 4 – CONFRONTATION (Boss Fight)
    // ═══════════════════════════════════════════════════════════

    static void BuildAct4Confrontation()
    {
        BuildStandardGameplayScene(
            actFolder:  "Act4",
            character:  "Dom",
            setupType:  null,
            groundY:    -2.5f,
            groundWidth: 20f,
            cameraMaxX: 8f,
            ambientColor: GameColors.DarkMagenta);

        // Boss (corrupted Ranima)
        var bossGO = new GameObject("Boss_RanimaCorrupted");
        bossGO.transform.position = new Vector3(0f, 0f, 0);
        var bossSR = bossGO.AddComponent<SpriteRenderer>();
        bossSR.sprite       = Spr("Sprites/Characters/ranima_corrupted_spritesheet.png");
        bossSR.sortingOrder = 2;
        bossSR.color        = GameColors.DarkMagenta;
        bossGO.transform.localScale = new Vector3(3, 3, 1);
        var bossAnim = bossGO.AddComponent<Animator>();
        var bossCtrl = bossGO.AddComponent<BossController>();
        var bossRB   = bossGO.AddComponent<Rigidbody2D>();
        bossRB.gravityScale   = 0;
        bossRB.freezeRotation = true;

        // Obstacle spawn points
        var spawnL = new GameObject("ObstacleSpawn_Left");  spawnL.transform.position = new Vector3(-7f, 0, 0);
        var spawnR = new GameObject("ObstacleSpawn_Right"); spawnR.transform.position = new Vector3( 7f, 0, 0);
        var spawnT = new GameObject("ObstacleSpawn_Top");   spawnT.transform.position = new Vector3( 0f, 4, 0);
        var barrelSpawn = new GameObject("BarrelSpawn"); barrelSpawn.transform.position = new Vector3(8f, 0, 0);

        // Obstacle prefab (spiked barrel)
        var obstaclePrefab = CreateObstaclePrefab();
        var barrelPrefab   = CreateBarrelPrefab();
        var orbPrefab      = CreateOrbPrefab();
        var vinePrefab     = CreateVinePrefab();

        // Boss corruption VFX (particle)
        var vfxGO = new GameObject("CorruptionVFX");
        vfxGO.transform.SetParent(bossGO.transform);
        vfxGO.transform.localPosition = Vector3.zero;
        var ps = vfxGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor     = new ParticleSystem.MinMaxGradient(GameColors.DarkMagenta);
        main.startSize      = 0.3f;
        main.startLifetime  = 1f;
        main.startSpeed     = 2f;
        main.maxParticles   = 50;

        // Wire BossController audio
        SetSO(bossCtrl, "bossMusic",           Clip("Music/act4_boss"));
        SetSO(bossCtrl, "phaseTransitionSound", Clip("SFX/phase_transition"));
        SetSO(bossCtrl, "hitSound",             Clip("SFX/boss_hit"));
        SetSO(bossCtrl, "defeatSound",          Clip("SFX/boss_defeat"));

        // Wire BossController
        var bso = new SerializedObject(bossCtrl);
        var spawnProp = bso.FindProperty("obstacleSpawnPoints");
        spawnProp.arraySize = 3;
        spawnProp.GetArrayElementAtIndex(0).objectReferenceValue = spawnL.transform;
        spawnProp.GetArrayElementAtIndex(1).objectReferenceValue = spawnR.transform;
        spawnProp.GetArrayElementAtIndex(2).objectReferenceValue = spawnT.transform;
        bso.FindProperty("obstaclePrefab").objectReferenceValue      = obstaclePrefab;
        bso.FindProperty("spikedBarrelPrefab").objectReferenceValue  = barrelPrefab;
        bso.FindProperty("spiritOrbProjectile").objectReferenceValue = orbPrefab;
        bso.FindProperty("vineObstaclePrefab").objectReferenceValue  = vinePrefab;
        bso.FindProperty("barrelSpawnPoint").objectReferenceValue    = barrelSpawn.transform;
        bso.FindProperty("bossRenderer").objectReferenceValue        = bossSR;
        bso.FindProperty("bossAnimator").objectReferenceValue        = bossAnim;
        bso.FindProperty("corruptionVFX").objectReferenceValue       = ps;
        bso.ApplyModifiedProperties();

        // SceneSetup
        var setupGO = new GameObject("SceneSetup");
        var setup   = setupGO.AddComponent<Act4ConfrontationSetup>();
        SetSO(setup, "bossController", bossCtrl);
    }

    // ═══════════════════════════════════════════════════════════
    //  ██  EPILOGUE
    // ═══════════════════════════════════════════════════════════

    static void BuildEpilogue()
    {
        BuildStandardGameplayScene(
            actFolder:  "Epilogue",
            character:  "Dom",
            setupType:  typeof(EpilogueSetup),
            groundY:    -2.5f,
            groundWidth: 15f,
            cameraMaxX: 5f,
            ambientColor: new Color(1f, 0.9f, 0.7f));

        // ── Platforms – peaceful dawn ──
        CreatePlatform("Platform_Shore",  new Vector3(-3f, -1.5f, 0), 4f, 0.4f, new Color(0.5f, 0.6f, 0.4f));
        CreatePlatform("Platform_Hill",   new Vector3( 4f, -0.5f, 0), 4f, 0.4f, new Color(0.4f, 0.55f, 0.35f));

        // ── Blooming nahor tree (central focus) ──
        var treeGO = new GameObject("NahorTree_Blooming");
        var treeSR = treeGO.AddComponent<SpriteRenderer>();
        treeSR.sprite = Spr("Sprites/Props/nahor_flower.png");
        treeSR.sortingOrder = 3;
        treeSR.color  = new Color(1f, 1f, 0.8f, 1f);
        treeGO.transform.position = new Vector3(0, 0, 0);
        treeGO.transform.localScale = new Vector3(4, 4, 1);

        // ── Tejimola spirit (final form) ──
        var spiritGO = new GameObject("TejimolaSpiritFinal");
        var spiritSR = spiritGO.AddComponent<SpriteRenderer>();
        spiritSR.sprite = Spr("Sprites/Characters/tejimola_spirit_spritesheet.png");
        spiritSR.sortingOrder = 5;
        spiritSR.color  = new Color(1f, 1f, 1f, 0.9f);
        spiritGO.transform.position = new Vector3(2f, -1f, 0);
        spiritGO.layer = L_Interactable;
        spiritGO.AddComponent<BoxCollider2D>().isTrigger = true;
        var spiritIAE = spiritGO.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(spiritIAE, "dialogueId", "epilogue_sunrise");

        // ── Flowers scattered around ──
        var flowerPositions = new Vector3[]
        {
            new Vector3(-5f, -2.3f, 0), new Vector3(-3f, -2.3f, 0),
            new Vector3( 1f, -2.3f, 0), new Vector3( 3f, -2.3f, 0),
            new Vector3( 5f, -2.3f, 0),
        };
        for (int fi = 0; fi < flowerPositions.Length; fi++)
        {
            var fGO = new GameObject($"Flower_{fi}");
            var fSR = fGO.AddComponent<SpriteRenderer>();
            fSR.sprite = Spr("Sprites/Props/nahor_flower.png");
            fSR.sortingOrder = 1;
            fSR.color = new Color(1f, 0.9f, 0.5f, 0.8f);
            fGO.transform.position = flowerPositions[fi];
            fGO.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        }

        // ── Gamosa cloth (father's memory) ──
        var gamosaGO = new GameObject("Gamosa_Memorial");
        var gamosaSR = gamosaGO.AddComponent<SpriteRenderer>();
        gamosaSR.sprite = Spr("Sprites/Props/gamosa.png");
        gamosaSR.sortingOrder = 2;
        gamosaGO.transform.position = new Vector3(-4f, -2f, 0);
    }

    // ═══════════════════════════════════════════════════════════
    //  SHARED GAMEPLAY SCENE BUILDER
    // ═══════════════════════════════════════════════════════════

    static void BuildStandardGameplayScene(
        string actFolder, string character, Type setupType,
        float groundY, float groundWidth, float cameraMaxX, Color ambientColor)
    {
        AddEventSystem();

        // ── Parallax Camera ──
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor  = Color.black;
        cam.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();
        var parallaxCam = camGO.AddComponent<Tejimola.Camera.ParallaxCamera>();
        SetSOf(parallaxCam, "followSpeed", GameConstants.CameraFollowSpeed);
        SetSOf(parallaxCam, "maxX", cameraMaxX);
        SetSOf(parallaxCam, "minX", -cameraMaxX);
        SetSOf(parallaxCam, "minY", -5f);
        SetSOf(parallaxCam, "maxY", 8f);
        SetSOb(parallaxCam, "useBounds", true);

        // ── Parallax Background ──
        var bgRoot = new GameObject("ParallaxBackground");
        var layers = new (string file, float factor, int order, float scaleX)[]
        {
            ($"Backgrounds/{actFolder}/layer4_sky.png",        GameConstants.ParallaxSkySpeed,        -10, 12f),
            ($"Backgrounds/{actFolder}/layer3_background.png", GameConstants.ParallaxBackgroundSpeed,  -8, 10f),
            ($"Backgrounds/{actFolder}/layer2_midground.png",  GameConstants.ParallaxMidgroundSpeed,   -6,  9f),
            ($"Backgrounds/{actFolder}/layer1_foreground.png", GameConstants.ParallaxForegroundSpeed,  -4,  8f),
        };

        var parallaxLayerComponents = new List<Tejimola.Camera.ParallaxLayer>();
        foreach (var (file, factor, order, scaleX) in layers)
        {
            var lGO = new GameObject(Path.GetFileNameWithoutExtension(file));
            lGO.transform.SetParent(bgRoot.transform);
            var lSR = lGO.AddComponent<SpriteRenderer>();
            lSR.sprite       = Spr(file);
            lSR.sortingOrder = order;
            lGO.transform.localScale = new Vector3(scaleX, 6f, 1);

            var pl = lGO.AddComponent<Tejimola.Camera.ParallaxLayer>();
            SetSOf(pl, "parallaxFactor", factor);
            SetSOb(pl, "infiniteHorizontal", false);
            parallaxLayerComponents.Add(pl);
        }

        // Wire parallax layers to camera
        var camSO = new SerializedObject(parallaxCam);
        var layersProp = camSO.FindProperty("parallaxLayers");
        layersProp.arraySize = parallaxLayerComponents.Count;
        for (int i = 0; i < parallaxLayerComponents.Count; i++)
            layersProp.GetArrayElementAtIndex(i).objectReferenceValue = parallaxLayerComponents[i];
        camSO.ApplyModifiedProperties();

        // ── Ground platform ──
        var groundGO = new GameObject("Ground");
        groundGO.layer = L_Ground;
        var groundSR  = groundGO.AddComponent<SpriteRenderer>();
        groundSR.sprite = CreateColoredSprite(GameColors.EarthBrown);
        groundSR.sortingOrder = -3;
        groundGO.transform.position = new Vector3(0, groundY, 0);
        groundGO.transform.localScale = new Vector3(groundWidth, 1f, 1);
        var groundCol = groundGO.AddComponent<BoxCollider2D>();

        // Left/right walls
        CreateWall("WallLeft",  new Vector3(-groundWidth/2 - 0.5f, groundY + 2, 0), 1, 8);
        CreateWall("WallRight", new Vector3( groundWidth/2 + 0.5f, groundY + 2, 0), 1, 8);

        // ── Player ──
        var player = CreatePlayer(character, new Vector3(-3f, groundY + 1f, 0));

        // Wire camera target to player
        var camSOa = new SerializedObject(parallaxCam);
        camSOa.FindProperty("target").objectReferenceValue = player.transform;
        camSOa.ApplyModifiedProperties();

        // ── Ambient light ──
        RenderSettings.ambientLight = ambientColor;

        // ── SceneSetup ──
        if (setupType != null)
        {
            var setupGO = new GameObject("SceneSetup");
            setupGO.AddComponent(setupType);
        }

        // ── Game UI ──
        BuildGameUI();
    }

    // ═══════════════════════════════════════════════════════════
    //  PLAYER CREATION
    // ═══════════════════════════════════════════════════════════

    static GameObject CreatePlayer(string character, Vector3 pos)
    {
        bool isTejimola = character == "Tejimola";
        string spritePath = isTejimola
            ? "Sprites/Characters/tejimola_child_spritesheet.png"
            : "Sprites/Characters/dom_spritesheet.png";

        var go = new GameObject($"Player_{character}");
        go.layer = L_Player;
        go.tag   = "Player";
        go.transform.position = pos;

        var sr  = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr(spritePath);
        sr.sortingOrder = 5;

        var rb  = go.AddComponent<Rigidbody2D>();
        rb.gravityScale          = 3f;
        rb.freezeRotation        = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.6f);

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = isTejimola ? s_tejiController : s_domController;

        // Ground check child
        var gcGO = new GameObject("GroundCheck");
        gcGO.transform.SetParent(go.transform);
        gcGO.transform.localPosition = new Vector3(0, -0.85f, 0);

        // Add character behaviour
        if (isTejimola)
        {
            var tb = go.AddComponent<TejimolaBehaviour>();
            SetSO(tb,  "groundCheck",      gcGO.transform);
            SetSOi(tb, "groundLayer",      1 << L_Ground);
            SetSOi(tb, "interactableLayer",1 << L_Interactable);
            SetSOf(tb, "moveSpeed",        GameConstants.MoveSpeed);
            SetSOf(tb, "crouchSpeed",      GameConstants.CrouchSpeed);
            // Footprint prefab
            var fpPrefab = GetOrCreateFootprintPrefab();
            SetSO(tb, "footprintPrefab", fpPrefab);
        }
        else
        {
            var db = go.AddComponent<Tejimola.Characters.DomBehaviour>();
            SetSO(db,  "groundCheck",      gcGO.transform);
            SetSOi(db, "groundLayer",      1 << L_Ground);
            SetSOi(db, "interactableLayer",1 << L_Interactable);
            SetSOf(db, "moveSpeed",        GameConstants.MoveSpeed);
            // Spirit pulse VFX prefab
            var pulsePrefab = GetOrCreatePulsePrefab();
            SetSO(db, "spiritPulseVFX", pulsePrefab);
        }

        return go;
    }

    // ═══════════════════════════════════════════════════════════
    //  GAME UI  (HUD + Dialogue + Pause)
    // ═══════════════════════════════════════════════════════════

    static void BuildGameUI()
    {
        var canvasGO = new GameObject("GameUI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        Transform cvs = canvasGO.transform;

        // ─── HUD ───────────────────────────────────────────────
        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(cvs, false);

        // Day text (top left)
        var dayTMP = MakeTMP(hudGO.transform, "DayText", "Day 1", 24, TextAlignmentOptions.Left);
        SetRect(dayTMP.transform, new Vector2(20, -20), new Vector2(150, 40), new Vector2(0, 1), new Vector2(0, 1));

        // Objective group (top center) — transparent panel + CanvasGroup for alpha fade
        var objGroupGO = MakePanel(hudGO.transform, "ObjectiveGroup", new Color(0, 0, 0, 0));
        objGroupGO.GetComponent<Image>().raycastTarget = false;
        SetRect(objGroupGO.transform, new Vector2(0, -20), new Vector2(600, 40), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var objGroupCG = objGroupGO.AddComponent<CanvasGroup>();
        objGroupCG.alpha = 0f;
        var objTMP = MakeTMP(objGroupGO.transform, "ObjectiveText", "", 20, TextAlignmentOptions.Center);
        SetRect(objTMP.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

        // Notification group (center screen) — transparent panel + CanvasGroup for alpha fade
        var notifGroupGO = MakePanel(hudGO.transform, "NotificationGroup", new Color(0, 0, 0, 0));
        notifGroupGO.GetComponent<Image>().raycastTarget = false;
        SetRect(notifGroupGO.transform, new Vector2(0, 100), new Vector2(800, 60), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var notifGroupCG = notifGroupGO.AddComponent<CanvasGroup>();
        notifGroupCG.alpha = 0f;
        var notifTMP = MakeTMP(notifGroupGO.transform, "NotificationText", "", 32, TextAlignmentOptions.Center);
        SetRect(notifTMP.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

        // Interaction prompt (bottom center)
        var interactGO = MakePanel(hudGO.transform, "InteractionPrompt", new Color(0, 0, 0, 0.7f));
        SetRect(interactGO.transform, new Vector2(0, 80), new Vector2(300, 50), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var interactTMP = MakeTMP(interactGO.transform, "InteractionText", "[E] Interact", 20, TextAlignmentOptions.Center);
        interactGO.SetActive(false);

        // Stealth panel (catch icons + count text - top right)
        var stealthPanel = MakePanel(hudGO.transform, "StealthPanel", new Color(0, 0, 0, 0));
        SetRect(stealthPanel.transform, new Vector2(-20, -20), new Vector2(230, 70), new Vector2(1, 1), new Vector2(1, 1));
        stealthPanel.SetActive(false);

        var catchCountTMP = MakeTMP(stealthPanel.transform, "CatchCountText", "0/3", 18, TextAlignmentOptions.Left);
        SetRect(catchCountTMP.transform, new Vector2(5, -45), new Vector2(80, 24), new Vector2(0, 1), new Vector2(0, 1));

        var catchIcons = new Image[GameConstants.MaxCatches];
        for (int i = 0; i < GameConstants.MaxCatches; i++)
        {
            var iconGO = new GameObject($"CatchIcon_{i}");
            iconGO.transform.SetParent(stealthPanel.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchoredPosition = new Vector2(i * 35 - 70, 0);
            iconRT.sizeDelta = new Vector2(30, 30);
            catchIcons[i] = iconGO.AddComponent<Image>();
            catchIcons[i].sprite = Spr("UI/HUD/catch_icon_inactive.png");
        }

        // Spirit pulse indicator (bottom left)
        var pulsePanel = MakePanel(hudGO.transform, "PulseIndicator", new Color(0, 0, 0, 0));
        SetRect(pulsePanel.transform, new Vector2(20, 80), new Vector2(60, 60), new Vector2(0, 0), new Vector2(0, 0));
        var pulseIconGO = MakePanel(pulsePanel.transform, "PulseIcon", Color.white);
        pulseIconGO.GetComponent<Image>().sprite = Spr("UI/HUD/spirit_pulse_icon.png");
        var pulseFillGO = MakePanel(pulsePanel.transform, "PulseCooldownFill", new Color(0.3f, 0.8f, 1f, 0.5f));

        // Exhaustion panel (bottom - rhythm mode)
        var exhPanel = MakePanel(hudGO.transform, "ExhaustionPanel", new Color(0.1f, 0.1f, 0.1f, 0.8f));
        SetRect(exhPanel.transform, new Vector2(0, 20), new Vector2(400, 30), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var exhFill = MakePanel(exhPanel.transform, "ExhaustionFill", new Color(0.2f, 0.7f, 0.3f));
        var exhTMP  = MakeTMP(exhPanel.transform, "ExhaustionText", "Exhaustion", 16, TextAlignmentOptions.Center);
        exhPanel.SetActive(false);

        // Boss health panel (top center)
        var bossPanel = MakePanel(hudGO.transform, "BossHealthPanel", new Color(0.1f, 0.05f, 0.1f, 0.9f));
        SetRect(bossPanel.transform, new Vector2(0, -20), new Vector2(500, 60), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var bossNameTMP  = MakeTMP(bossPanel.transform, "BossNameText",    "RANIMA", 22, TextAlignmentOptions.Center);
        var bossHealthFill = MakePanel(bossPanel.transform, "BossHealthFill", new Color(0.7f, 0.1f, 0.7f));
        bossPanel.SetActive(false);

        // Wire GameHUD
        var hud = hudGO.AddComponent<Tejimola.UI.GameHUD>();
        var hso = new SerializedObject(hud);
        hso.FindProperty("dayText").objectReferenceValue          = dayTMP;
        hso.FindProperty("objectiveText").objectReferenceValue   = objTMP;
        hso.FindProperty("objectiveGroup").objectReferenceValue  = objGroupCG;
        hso.FindProperty("notificationText").objectReferenceValue  = notifTMP;
        hso.FindProperty("notificationGroup").objectReferenceValue = notifGroupCG;
        hso.FindProperty("interactionPrompt").objectReferenceValue = interactGO;
        hso.FindProperty("interactionText").objectReferenceValue   = interactTMP;
        hso.FindProperty("stealthPanel").objectReferenceValue    = stealthPanel;
        hso.FindProperty("catchCountText").objectReferenceValue  = catchCountTMP;
        hso.FindProperty("pulseIndicator").objectReferenceValue  = pulsePanel;
        hso.FindProperty("pulseIcon").objectReferenceValue      = pulseIconGO.GetComponent<Image>();
        hso.FindProperty("pulseCooldownFill").objectReferenceValue = pulseFillGO.GetComponent<Image>();
        hso.FindProperty("exhaustionPanel").objectReferenceValue = exhPanel;
        hso.FindProperty("exhaustionFill").objectReferenceValue  = exhFill.GetComponent<Image>();
        hso.FindProperty("exhaustionText").objectReferenceValue  = exhTMP;
        hso.FindProperty("bossHealthPanel").objectReferenceValue = bossPanel;
        hso.FindProperty("bossNameText").objectReferenceValue    = bossNameTMP;
        hso.FindProperty("bossHealthFill").objectReferenceValue  = bossHealthFill.GetComponent<Image>();
        var catchProp = hso.FindProperty("catchIcons");
        catchProp.arraySize = catchIcons.Length;
        for (int i = 0; i < catchIcons.Length; i++)
            catchProp.GetArrayElementAtIndex(i).objectReferenceValue = catchIcons[i];
        hso.ApplyModifiedProperties();

        // ─── Controls Hint (bottom-right, semi-transparent) ───
        var controlsHintGO = new GameObject("ControlsHint");
        controlsHintGO.transform.SetParent(cvs, false);
        var chRT = controlsHintGO.AddComponent<RectTransform>();
        chRT.anchorMin        = new Vector2(1f, 0f);
        chRT.anchorMax        = new Vector2(1f, 0f);
        chRT.anchoredPosition = new Vector2(-10f, 10f);
        chRT.sizeDelta        = new Vector2(360f, 70f);
        chRT.pivot            = new Vector2(1f, 0f);
        var chTMP = controlsHintGO.AddComponent<TextMeshProUGUI>();
        chTMP.text      = "A/D — Move    W/↑ — Jump    E — Interact\nSpace — Talk/Hide    Esc — Pause";
        chTMP.fontSize  = 14f;
        chTMP.alignment = TextAlignmentOptions.BottomRight;
        chTMP.color     = new Color(1f, 1f, 1f, 0.45f);
        var chFont = DefaultFont();
        if (chFont != null) chTMP.font = chFont;

        // ─── Dialogue Box ──────────────────────────────────────
        var dlgPanel = MakePanel(cvs, "DialoguePanel", new Color(0.05f, 0.02f, 0.02f, 0.92f));
        SetRect(dlgPanel.transform, new Vector2(0, 10), new Vector2(1400, 220), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        // Keep active — DialogueBoxUI.Awake() hides itself via CanvasGroup alpha.
        // SetActive(false) would prevent Awake from ever running, breaking dialogue subscriptions.

        var dlgBg = dlgPanel.GetComponent<Image>();
        dlgBg.sprite = Spr("UI/DialogueBox/dialogue_box.png");
        dlgBg.type   = Image.Type.Sliced;

        // Portrait
        var portraitGO = MakePanel(dlgPanel.transform, "SpeakerPortrait", Color.white);
        SetRect(portraitGO.transform, new Vector2(20, 10), new Vector2(160, 160), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var portraitImg = portraitGO.GetComponent<Image>();
        var portraitFrameGO = MakePanel(dlgPanel.transform, "PortraitFrame", new Color(0.5f, 0.4f, 0.2f));
        SetRect(portraitFrameGO.transform, new Vector2(20, 10), new Vector2(164, 164), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        portraitFrameGO.transform.SetAsFirstSibling();

        // Texts
        var speakerNameTMP = MakeTMP(dlgPanel.transform, "SpeakerNameText", "Speaker", 22, TextAlignmentOptions.Left);
        SetRect(speakerNameTMP.transform, new Vector2(200, -15), new Vector2(1100, 30), new Vector2(0, 1), new Vector2(0, 1));
        speakerNameTMP.fontStyle = FontStyles.Bold;
        speakerNameTMP.color     = new Color(1f, 0.85f, 0.3f);

        var dlgTMP = MakeTMP(dlgPanel.transform, "DialogueText", "", 20, TextAlignmentOptions.TopLeft);
        SetRect(dlgTMP.transform, new Vector2(200, -55), new Vector2(1100, 90), new Vector2(0, 1), new Vector2(0, 1));
        dlgTMP.color = Color.white;

        var assTMP = MakeTMP(dlgPanel.transform, "AssameseText", "", 18, TextAlignmentOptions.TopLeft);
        SetRect(assTMP.transform, new Vector2(200, -155), new Vector2(1100, 50), new Vector2(0, 1), new Vector2(0, 1));
        assTMP.color = new Color(0.8f, 0.9f, 1f);
        assTMP.fontStyle = FontStyles.Italic;

        var continueIndicator = MakePanel(dlgPanel.transform, "ContinueIndicator", new Color(0, 0, 0, 0));
        var continueTMP = MakeTMP(continueIndicator.transform, "ContinueText", "▼", 20, TextAlignmentOptions.Right);
        SetRect(continueIndicator.transform, new Vector2(-20, 10), new Vector2(40, 30), new Vector2(1, 0), new Vector2(1, 0));

        // Choice panel
        var choicePanel = MakePanel(dlgPanel.transform, "ChoicePanel", new Color(0, 0, 0, 0));
        SetRect(choicePanel.transform, new Vector2(200, 10), new Vector2(1000, 80), new Vector2(0, 0), new Vector2(0, 0));
        choicePanel.SetActive(false);

        var choiceButtons = new Button[4];
        var choiceTexts   = new TextMeshProUGUI[4];
        for (int i = 0; i < 4; i++)
        {
            choiceButtons[i] = MakeButton(choicePanel.transform, $"ChoiceButton_{i}", $"Choice {i+1}");
            var rt = choiceButtons[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * 250 - 375, 0);
            rt.sizeDelta        = new Vector2(240, 60);
            choiceTexts[i] = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        // Portrait sprites
        var tejiPortrait     = Spr("Sprites/Characters/tejimola_child_portrait.png");
        var domPortrait      = Spr("Sprites/Characters/dom_portrait.png");
        var fatherPortrait   = Spr("Sprites/Characters/father_portrait.png");
        var ranimaPortrait   = Spr("Sprites/Characters/ranima_portrait.png");

        // Wire DialogueBoxUI
        var dlgUI = dlgPanel.AddComponent<Tejimola.UI.DialogueBoxUI>();
        var dso   = new SerializedObject(dlgUI);
        dso.FindProperty("dialoguePanel").objectReferenceValue       = dlgPanel;
        dso.FindProperty("dialogueBackground").objectReferenceValue  = dlgBg;
        dso.FindProperty("speakerPortrait").objectReferenceValue     = portraitImg;
        dso.FindProperty("portraitFrame").objectReferenceValue       = portraitFrameGO.GetComponent<Image>();
        dso.FindProperty("speakerNameText").objectReferenceValue     = speakerNameTMP;
        dso.FindProperty("dialogueText").objectReferenceValue        = dlgTMP;
        dso.FindProperty("assameseText").objectReferenceValue        = assTMP;
        dso.FindProperty("choicePanel").objectReferenceValue         = choicePanel;
        dso.FindProperty("continueIndicator").objectReferenceValue   = continueIndicator;
        dso.FindProperty("continueText").objectReferenceValue        = continueTMP;
        dso.FindProperty("tejimolPortrait").objectReferenceValue     = tejiPortrait;
        dso.FindProperty("domPortrait").objectReferenceValue         = domPortrait;
        dso.FindProperty("fatherPortrait").objectReferenceValue      = fatherPortrait;
        dso.FindProperty("ranimaPortrait").objectReferenceValue      = ranimaPortrait;
        // elder and narrator use father portrait and a null (shown as blank) respectively
        var epFather = dso.FindProperty("elderPortrait");
        if (epFather != null) epFather.objectReferenceValue = fatherPortrait;
        var epNarr = dso.FindProperty("narratorPortrait");
        if (epNarr != null) epNarr.objectReferenceValue = tejiPortrait;
        var cbProp = dso.FindProperty("choiceButtons");
        cbProp.arraySize = 4;
        var ctProp = dso.FindProperty("choiceTexts");
        ctProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            cbProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
            ctProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceTexts[i];
        }
        dso.ApplyModifiedProperties();

        // ─── Pause Menu ───────────────────────────────────────
        var pausePanel = MakePanel(cvs, "PauseMenu", new Color(0, 0, 0, 0.85f));
        SetRect(pausePanel.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);
        // Keep active — PauseMenuUI.Awake() calls HidePanels() to hide sub-panels on start.
        // SetActive(false) would prevent Awake from running, breaking button wiring.

        MakeTMP(pausePanel.transform, "PauseTitle", "PAUSED", 48, TextAlignmentOptions.Center);
        var resumeBtn = MakeButton(pausePanel.transform, "ResumeButton",     "Resume",         Spr("UI/Menu/btn_resume.png"));
        var saveBtn   = MakeButton(pausePanel.transform, "SaveButton",       "Save",           Spr("UI/Menu/btn_save.png"));
        var loadBtn   = MakeButton(pausePanel.transform, "LoadButton",       "Load",           Spr("UI/Menu/btn_load.png"));
        var chapBtn   = MakeButton(pausePanel.transform, "ChapterSelectButton","Chapter Select",null);
        var settBtn   = MakeButton(pausePanel.transform, "SettingsButton",   "Settings",       Spr("UI/Menu/btn_settings.png"));
        var menuBtn   = MakeButton(pausePanel.transform, "MainMenuButton",   "Main Menu",      null);
        var playTimeTMP = MakeTMP(pausePanel.transform, "PlayTimeText", "00:00:00", 20, TextAlignmentOptions.Center);
        LayoutVertical(pausePanel.transform, 8f);

        var chapPanel = MakePanel(pausePanel.transform, "ChapterSelectPanel", new Color(0, 0, 0, 0.9f));
        chapPanel.SetActive(false);

        var pauseUI = pausePanel.AddComponent<Tejimola.UI.PauseMenuUI>();
        var pso = new SerializedObject(pauseUI);
        pso.FindProperty("pausePanel").objectReferenceValue          = pausePanel;
        pso.FindProperty("chapterSelectPanel").objectReferenceValue  = chapPanel;
        pso.FindProperty("resumeButton").objectReferenceValue        = resumeBtn;
        pso.FindProperty("saveButton").objectReferenceValue          = saveBtn;
        pso.FindProperty("loadButton").objectReferenceValue          = loadBtn;
        pso.FindProperty("chapterSelectButton").objectReferenceValue = chapBtn;
        pso.FindProperty("settingsButton").objectReferenceValue      = settBtn;
        pso.FindProperty("mainMenuButton").objectReferenceValue      = menuBtn;
        pso.FindProperty("playTimeText").objectReferenceValue        = playTimeTMP;
        pso.ApplyModifiedProperties();
    }

    // ═══════════════════════════════════════════════════════════
    //  PREFAB FACTORIES
    // ═══════════════════════════════════════════════════════════

    static GameObject GetOrCreateFootprintPrefab()
    {
        string path = $"{PREFABS}/FootprintPrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("FootprintPrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("VFX/footprint.png");
        sr.color  = new Color(0.4f, 0.3f, 0.2f, 0.6f);
        sr.sortingOrder = -1;
        go.AddComponent<FootprintBehaviour>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject GetOrCreatePulsePrefab()
    {
        string path = $"{PREFABS}/SpiritPulsePrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("SpiritPulsePrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("VFX/spirit_pulse_ring.png");
        sr.color  = new Color(0.5f, 0.3f, 1f, 0.8f);
        sr.sortingOrder = 10;
        go.AddComponent<SpiritPulseEffect>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateBeatNotePrefab()
    {
        string path = $"{PREFABS}/BeatNotePrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go  = new GameObject("BeatNotePrefab");
        var img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.7f, 0.2f);
        var rt  = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateObstaclePrefab()
    {
        string path = $"{PREFABS}/ObstaclePrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("ObstaclePrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Sprites/Props/spiked_barrel.png");
        sr.color  = GameColors.DarkMagenta;
        go.AddComponent<BoxCollider2D>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateBarrelPrefab()
    {
        string path = $"{PREFABS}/SpikedBarrelPrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("SpikedBarrelPrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("Sprites/Props/spiked_barrel.png");
        sr.sortingOrder = 3;
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<Rigidbody2D>().gravityScale = 0;
        go.AddComponent<Tejimola.Gameplay.SpikedBarrel>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateOrbPrefab()
    {
        string path = $"{PREFABS}/SpiritOrbPrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("SpiritOrbPrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Sprites/Props/spirit_orb.png");
        sr.color  = new Color(0.5f, 0.3f, 1f);
        sr.sortingOrder = 4;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Rigidbody2D>().gravityScale = 0;
        go.AddComponent<Tejimola.Gameplay.OrbProjectile>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateVinePrefab()
    {
        string path = $"{PREFABS}/VineObstaclePrefab.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("VineObstaclePrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("VFX/vine_obstacle.png");
        sr.color  = GameColors.ForestGreen;
        go.AddComponent<BoxCollider2D>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    // ═══════════════════════════════════════════════════════════
    //  WORLD OBJECT HELPERS
    // ═══════════════════════════════════════════════════════════

    static void CreateInteractable(string name, string spritePath, Vector3 pos,
        string dialogueId, string itemId, bool isCollectible)
    {
        var go = new GameObject(name);
        go.layer = L_Interactable;
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr(spritePath);
        sr.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var prompt = new GameObject("InteractPrompt");
        prompt.transform.SetParent(go.transform);
        prompt.transform.localPosition = new Vector3(0, 1f, 0);
        var promptSR = prompt.AddComponent<SpriteRenderer>();
        promptSR.sortingOrder = 10;
        prompt.SetActive(false);

        var ia = go.AddComponent<Tejimola.Characters.Interactable>();
        SetSO(ia,  "promptUI",     prompt);
        SetSOs(ia, "dialogueId",   dialogueId);
        SetSOs(ia, "itemId",       itemId);
        SetSOb(ia, "isCollectible",isCollectible);
        SetSOb(ia, "oneTimeUse",   isCollectible);
    }

    static void CreateHidingSpot(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("Sprites/Props/gamosa.png");
        sr.sortingOrder = 1;
        sr.color        = new Color(0.5f, 0.4f, 0.3f, 0.7f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(1.5f, 2f);

        var hidePoint = new GameObject("HidePoint");
        hidePoint.transform.SetParent(go.transform);
        hidePoint.transform.localPosition = new Vector3(0, 0.5f, 0);

        var hs = go.AddComponent<HidingSpot>();
        SetSO(hs,  "hidePoint", hidePoint.transform);
        SetSOs(hs, "spotName",  name);
    }

    static void CreateSpiritRevealable(string name, string spritePath, Vector3 pos,
        string objectId, bool permanent)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr(spritePath);
        sr.sortingOrder = 3;
        sr.color        = new Color(1f, 1f, 1f, 0f); // hidden by default

        go.AddComponent<BoxCollider2D>().isTrigger = true;

        var rev = go.AddComponent<SpiritRevealable>();
        SetSOs(rev, "objectId",       objectId);
        SetSOb(rev, "permanentReveal",permanent);
        SetSOf(rev, "revealDuration", 5f);
    }

    static void CreateWall(string name, Vector3 pos, float w, float h)
    {
        var go = new GameObject(name);
        go.layer = L_Obstacle;
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
    }

    static void CreatePlatform(string name, Vector3 pos, float width, float height, Color color)
    {
        var go = new GameObject(name);
        go.layer = L_Ground;
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateColoredSprite(color);
        sr.sortingOrder = -2;
        go.transform.localScale = new Vector3(width, height, 1f);

        go.AddComponent<BoxCollider2D>();
    }

    static void CreateSpiritOrb(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.layer = L_Interactable;
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Spr("Sprites/Props/spirit_orb.png");
        sr.sortingOrder = 4;
        sr.color        = new Color(0.5f, 0.3f, 1f, 0.9f);
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var ia = go.AddComponent<Tejimola.Characters.Interactable>();
        SetSOs(ia, "itemId",       "spirit_orb");
        SetSOb(ia, "isCollectible", true);
        SetSOb(ia, "oneTimeUse",    true);
    }

    // ═══════════════════════════════════════════════════════════
    //  UI FACTORY HELPERS
    // ═══════════════════════════════════════════════════════════

    static T AddManager<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        return go.AddComponent<T>();
    }

    static void AddEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    static GameObject MakePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
        int size, TextAlignmentOptions align)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = align;
        tmp.color     = Color.white;
        var font = DefaultFont();
        if (font != null) tmp.font = font;
        return tmp;
    }

    static Button MakeButton(Transform parent, string name, string label, Sprite sprite = null)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 55);
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        else img.color = new Color(0.2f, 0.1f, 0.05f, 0.9f);
        var btn = go.AddComponent<Button>();
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one; lblRT.sizeDelta = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 20; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.9f, 0.7f);
        var font = DefaultFont();
        if (font != null) tmp.font = font;
        return btn;
    }

    static Slider MakeSlider(Transform parent, string name, string label, float min, float max, float val)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
        var slider = go.AddComponent<Slider>();
        slider.minValue = min; slider.maxValue = max; slider.value = val;
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        lblGO.AddComponent<RectTransform>();
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18; tmp.color = new Color(0.9f, 0.85f, 0.75f);
        return slider;
    }

    static Toggle MakeToggle(Transform parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 35);
        var toggle = go.AddComponent<Toggle>();
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        lblGO.AddComponent<RectTransform>();
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18; tmp.color = new Color(0.9f, 0.85f, 0.75f);
        return toggle;
    }

    static RectTransform MakeRhythmTrack(Transform parent, string name, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(80, 400);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        return rt;
    }

    static RectTransform MakeHitZone(Transform parent, string name, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(80, 20);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.7f, 0.2f, 0.9f);
        return rt;
    }

    static GameObject MakeWorldCanvas(string name, int sortOrder)
    {
        var go = new GameObject(name);
        var c = go.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // ── Layout helpers ──────────────────────────────────────────

    static void RectAt(Transform t, float xNorm, float yNorm, float wNorm, float hNorm)
    {
        var rt = t.GetComponent<RectTransform>() ?? t.gameObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xNorm, yNorm);
        rt.anchorMax = new Vector2(xNorm + wNorm, yNorm + hNorm);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetRect(Transform t, Vector2 anchoredPos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rt = t.GetComponent<RectTransform>() ?? t.gameObject.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
    }

    static void LayoutVertical(Transform parent, float spacing)
    {
        float y = 0;
        foreach (Transform child in parent)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, y);
            y -= rt.sizeDelta.y + spacing;
        }
    }

    // ─── SerializedObject setters ───────────────────────────────

    static void SetSO(Component c, string f, UnityEngine.Object v)
    {
        var so = new SerializedObject(c); var p = so.FindProperty(f);
        if (p != null) { p.objectReferenceValue = v; so.ApplyModifiedProperties(); }
    }
    static void SetSOf(Component c, string f, float v)
    {
        var so = new SerializedObject(c); var p = so.FindProperty(f);
        if (p != null) { p.floatValue = v; so.ApplyModifiedProperties(); }
    }
    static void SetSOi(Component c, string f, int v)
    {
        var so = new SerializedObject(c); var p = so.FindProperty(f);
        if (p != null) { p.intValue = v; so.ApplyModifiedProperties(); }
    }
    static void SetSOb(Component c, string f, bool v)
    {
        var so = new SerializedObject(c); var p = so.FindProperty(f);
        if (p != null) { p.boolValue = v; so.ApplyModifiedProperties(); }
    }
    static void SetSOs(Component c, string f, string v)
    {
        var so = new SerializedObject(c); var p = so.FindProperty(f);
        if (p != null) { p.stringValue = v; so.ApplyModifiedProperties(); }
    }

    static void SetSOP(Component c, string f, Transform[] arr)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(f);
        if (p == null) return;
        p.arraySize = arr.Length;
        for (int i = 0; i < arr.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
        so.ApplyModifiedProperties();
    }

    // ─── Asset helpers ──────────────────────────────────────────

    static Sprite Spr(string relPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ART}/{relPath}");
        if (sprite == null)
            Debug.LogWarning($"[SceneBuilder] Sprite not found: {ART}/{relPath}");
        return sprite;
    }

    const string RESOURCES_AUDIO = "Assets/_Project/Resources/Audio";

    static AudioClip Clip(string relPath)
    {
        // Try with common extensions
        string[] exts = { ".wav", ".mp3", ".ogg" };
        foreach (var ext in exts)
        {
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>($"{RESOURCES_AUDIO}/{relPath}{ext}");
            if (c != null) return c;
        }
        Debug.LogWarning($"[SceneBuilder] AudioClip not found: {RESOURCES_AUDIO}/{relPath}");
        return null;
    }

    static Sprite CreateColoredSprite(Color color)
    {
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    static void Progress(float t, string msg) =>
        EditorUtility.DisplayProgressBar("Building Tejimola Scenes", msg, t);

    // ── Build Settings ──────────────────────────────────────────

    static void UpdateBuildSettings()
    {
        var sceneNames = new[]
        {
            "MainMenu", "Act1_HappyHome", "Act1_Funeral", "Act2_Descent", "Act2_Dheki",
            "Act2_Burial", "Act3_DomArrival", "Act3_DualTimeline", "Act4_Confrontation", "Epilogue"
        };

        var scenes = new EditorBuildSettingsScene[sceneNames.Length];
        for (int i = 0; i < sceneNames.Length; i++)
        {
            string path = $"{SCENES}/{sceneNames[i]}.unity";
            scenes[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[SceneBuilder] Build Settings updated with all 10 scenes.");
    }

}
