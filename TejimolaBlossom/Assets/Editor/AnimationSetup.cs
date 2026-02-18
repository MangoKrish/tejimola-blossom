using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Slices character spritesheets into individual frames and creates AnimationClip
/// assets for each animator state. Wires clips into TejimolaAnimator and DomAnimator.
///
/// Menu: Tejimola > Setup Animations
/// Also called automatically from BuildAllScenesBatch().
/// </summary>
public static class AnimationSetup
{
    const string CHARS   = "Assets/_Project/Art/Sprites/Characters";
    const string PREFABS = "Assets/_Project/Prefabs";
    const string ANIMS   = "Assets/_Project/Animations";

    // How many columns each spritesheet has (rows are computed from height)
    const int DEFAULT_COLS = 4;

    // Frames per second for each animation state
    const float FPS_IDLE   = 6f;
    const float FPS_WALK   = 10f;
    const float FPS_CROUCH = 6f;
    const float FPS_HIDE   = 4f;

    // ── Entry points ──────────────────────────────────────────────────────────

    [MenuItem("Tejimola/Setup Animations", false, 10)]
    public static void SetupAnimations()
    {
        SetupAnimationsBatch();
        EditorUtility.DisplayDialog("Animations",
            "Character animations configured!\n\nCheck Console for details.", "OK");
    }

    /// <summary>Batch-mode safe — no UI dialogs, throws on failure.</summary>
    public static void SetupAnimationsBatch()
    {
        Directory.CreateDirectory(ANIMS);

        // ── 1. Import spritesheets as Multiple sprites ────────────────────────
        Debug.Log("[AnimationSetup] Configuring spritesheets…");
        ConfigureSheet($"{CHARS}/tejimola_child_spritesheet.png",   DEFAULT_COLS);
        ConfigureSheet($"{CHARS}/tejimola_spirit_spritesheet.png",  DEFAULT_COLS);
        ConfigureSheet($"{CHARS}/dom_spritesheet.png",              DEFAULT_COLS);
        ConfigureSheet($"{CHARS}/ranima_spritesheet.png",           DEFAULT_COLS);
        ConfigureSheet($"{CHARS}/ranima_corrupted_spritesheet.png", DEFAULT_COLS);
        ConfigureSheet($"{CHARS}/father_spritesheet.png",           DEFAULT_COLS);

        AssetDatabase.Refresh();

        // ── 2. Build Tejimola animator ────────────────────────────────────────
        Debug.Log("[AnimationSetup] Building Tejimola clips…");
        var tejiCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{PREFABS}/TejimolaAnimator.controller");
        if (tejiCtrl != null)
        {
            // Primary: child form sprites
            var sprites = LoadSprites($"{CHARS}/tejimola_child_spritesheet.png");
            if (sprites.Length == 0)
            {
                // Fallback: spirit form
                sprites = LoadSprites($"{CHARS}/tejimola_spirit_spritesheet.png");
            }

            if (sprites.Length > 0)
            {
                WireClip(tejiCtrl, "Idle",   MakeClip("Tejimola_Idle",   sprites, 0,                    4, FPS_IDLE));
                WireClip(tejiCtrl, "Walk",   MakeClip("Tejimola_Walk",   sprites, Offset(sprites, 1, 4), 4, FPS_WALK));
                WireClip(tejiCtrl, "Crouch", MakeClip("Tejimola_Crouch", sprites, Offset(sprites, 2, 4), 4, FPS_CROUCH));
                WireClip(tejiCtrl, "Hide",   MakeClip("Tejimola_Hide",   sprites, Offset(sprites, 3, 4), 2, FPS_HIDE));
                EditorUtility.SetDirty(tejiCtrl);
            }
            else
            {
                Debug.LogWarning("[AnimationSetup] No Tejimola sprites loaded — skipping clips.");
            }
        }
        else
        {
            Debug.LogWarning("[AnimationSetup] TejimolaAnimator.controller not found — run Build All Scenes first.");
        }

        // ── 3. Build Dom animator ─────────────────────────────────────────────
        Debug.Log("[AnimationSetup] Building Dom clips…");
        var domCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{PREFABS}/DomAnimator.controller");
        if (domCtrl != null)
        {
            var sprites = LoadSprites($"{CHARS}/dom_spritesheet.png");
            if (sprites.Length > 0)
            {
                WireClip(domCtrl, "Idle", MakeClip("Dom_Idle", sprites, 0,                    4, FPS_IDLE));
                WireClip(domCtrl, "Walk", MakeClip("Dom_Walk", sprites, Offset(sprites, 1, 4), 4, FPS_WALK));
                EditorUtility.SetDirty(domCtrl);
            }
            else
            {
                Debug.LogWarning("[AnimationSetup] No Dom sprites loaded — skipping clips.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[AnimationSetup] Done — animation clips created and wired.");
    }

    // ── Implementation ────────────────────────────────────────────────────────

    /// <summary>
    /// Reconfigures a spritesheet PNG for Multiple-sprite mode and slices it
    /// into a cols×rows grid. Skips if already configured.
    /// </summary>
    static void ConfigureSheet(string path, int cols)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[AnimationSetup] Spritesheet not found: {path}");
            return;
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[AnimationSetup] No TextureImporter for: {path}");
            return;
        }

        // Only reconfigure if not yet set up as Multiple sprites
#pragma warning disable 618
        bool alreadySliced = importer.spriteImportMode == SpriteImportMode.Multiple &&
                             importer.spritesheet != null && importer.spritesheet.Length > 0;
#pragma warning restore 618
        if (alreadySliced)
        {
            Debug.Log($"[AnimationSetup] Already configured: {Path.GetFileName(path)}");
            return;
        }

        // Load the texture to get source dimensions (Unity 6 removed GetSourceTextureInformation)
        var tex  = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int texW = tex != null ? tex.width  : 0;
        int texH = tex != null ? tex.height : 0;

        if (texW <= 0 || texH <= 0)
        {
            Debug.LogWarning($"[AnimationSetup] Could not read dimensions for: {Path.GetFileName(path)} — skipping");
            return;
        }

        // Compute grid — rows derived from height assuming square frames
        int frameW = texW / cols;
        int frameH = (frameW > 0 && texH >= frameW) ? frameW : texH;
        int rows   = frameH > 0 ? Mathf.Max(1, texH / frameH) : 1;

        string baseName = Path.GetFileNameWithoutExtension(path);
        var metas = new List<SpriteMetaData>();
        int idx = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++, idx++)
            {
                // Unity sprite coords: y=0 is bottom-left
                float x = c * frameW;
                float y = texH - (r + 1) * frameH;

                metas.Add(new SpriteMetaData
                {
                    name      = $"{baseName}_{idx:D2}",
                    rect      = new Rect(x, y, frameW, frameH),
                    pivot     = new Vector2(0.5f, 0f),
                    alignment = (int)SpriteAlignment.BottomCenter
                });
            }
        }

        importer.textureType      = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode       = FilterMode.Point;
        importer.mipmapEnabled    = false;
#pragma warning disable 618  // TextureImporter.spritesheet is obsolete but still functional in Unity 6
        importer.spritesheet = metas.ToArray();
#pragma warning restore 618
        importer.SaveAndReimport();

        Debug.Log($"[AnimationSetup] Sliced {baseName}: {cols}×{rows} = {idx} frames ({texW}×{texH}px)");
    }

    /// <summary>Loads all sub-sprites from a Multiple-sprite texture, sorted by name.</summary>
    static Sprite[] LoadSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    /// <summary>
    /// Safely computes the start frame for animation row <paramref name="row"/>,
    /// clamped to available sprites.
    /// </summary>
    static int Offset(Sprite[] sprites, int row, int framesPerRow)
    {
        int idx = row * framesPerRow;
        return idx < sprites.Length ? idx : 0;
    }

    /// <summary>
    /// Creates (or reloads) an AnimationClip asset at Assets/_Project/Animations/{clipName}.anim
    /// with sprite keyframes drawn from <paramref name="sprites"/>[startFrame … startFrame+count].
    /// </summary>
    static AnimationClip MakeClip(string clipName, Sprite[] sprites, int startFrame, int count, float fps)
    {
        string clipPath = $"{ANIMS}/{clipName}.anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (existing != null)
        {
            Debug.Log($"[AnimationSetup] Reusing existing clip: {clipName}");
            return existing;
        }

        var clip = new AnimationClip { frameRate = fps };
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Clamp frame range to available sprites
        int end    = Mathf.Min(startFrame + count, sprites.Length);
        int actual = Mathf.Max(1, end - startFrame);

        var keyframes = new ObjectReferenceKeyframe[actual];
        for (int i = 0; i < actual; i++)
        {
            int si = startFrame + i < sprites.Length ? startFrame + i : 0;
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i / fps,
                value = sprites[si]
            };
        }

        var binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AssetDatabase.CreateAsset(clip, clipPath);
        Debug.Log($"[AnimationSetup] Created {clipName} ({actual} frames @ {fps}fps)");
        return clip;
    }

    /// <summary>Assigns <paramref name="clip"/> to the named state in <paramref name="ctrl"/>.</summary>
    static void WireClip(AnimatorController ctrl, string stateName, AnimationClip clip)
    {
        if (clip == null) return;
        var root = ctrl.layers[0].stateMachine;
        foreach (var cs in root.states)
        {
            if (cs.state.name == stateName)
            {
                cs.state.motion = clip;
                Debug.Log($"[AnimationSetup] {ctrl.name}/{stateName} ← {clip.name}");
                return;
            }
        }
        Debug.LogWarning($"[AnimationSetup] State '{stateName}' not found in {ctrl.name}");
    }
}
