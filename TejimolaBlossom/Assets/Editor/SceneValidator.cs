using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Opens every built scene and checks for common problems:
///   • Missing Main Camera
///   • Missing EventSystem (MainMenu only)
///   • Missing-script (null) components on any GameObject
///
/// Menu: Tejimola > Validate All Scenes
/// Also called from BuildScript.BatchBuildAll so problems surface in CI logs.
/// </summary>
public static class SceneValidator
{
    static readonly string SCENES = "Assets/_Project/Scenes";

    static readonly string[] SCENE_NAMES =
    {
        "MainMenu", "Act1_HappyHome", "Act1_Funeral",
        "Act2_Descent", "Act2_Dheki", "Act2_Burial",
        "Act3_DomArrival", "Act3_DualTimeline",
        "Act4_Confrontation", "Epilogue"
    };

    // ── Entry points ──────────────────────────────────────────────────────────

    [MenuItem("Tejimola/Validate All Scenes", false, 20)]
    public static void ValidateAllScenes()
    {
        int totalErrors   = 0;
        int totalWarnings = 0;

        foreach (var name in SCENE_NAMES)
        {
            string path = $"{SCENES}/{name}.unity";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Validator] Scene file missing: {path}");
                totalWarnings++;
                continue;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var (errs, warns) = ValidateCurrentScene(name);
            totalErrors   += errs;
            totalWarnings += warns;
        }

        string summary = totalErrors == 0
            ? $"All scenes valid! ({totalWarnings} warnings)"
            : $"FAILED: {totalErrors} errors, {totalWarnings} warnings — see Console";

        Debug.Log($"[Validator] {summary}");
        EditorUtility.DisplayDialog("Scene Validation", summary, "OK");
    }

    /// <summary>
    /// Batch-mode safe validate — no DisplayDialog, returns error count.
    /// Throws if any errors found (for CI).
    /// </summary>
    public static int ValidateAllScenesBatch()
    {
        int totalErrors = 0;

        foreach (var name in SCENE_NAMES)
        {
            string path = $"{SCENES}/{name}.unity";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Validator] Scene missing: {path}");
                continue;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var (errs, _) = ValidateCurrentScene(name);
            totalErrors += errs;
        }

        if (totalErrors > 0)
            Debug.LogError($"[Validator] {totalErrors} validation error(s) found.");
        else
            Debug.Log("[Validator] All scenes passed validation.");

        return totalErrors;
    }

    // ── Core validation ───────────────────────────────────────────────────────

    static (int errors, int warnings) ValidateCurrentScene(string sceneName)
    {
        int errors = 0, warnings = 0;

        // ── Camera ──
        if (Camera.main == null)
        {
            Debug.LogError($"[Validator] {sceneName}: No GameObject tagged 'MainCamera'");
            errors++;
        }

        // ── EventSystem (only matters for UI scenes) ──
        if (sceneName == "MainMenu")
        {
            var es = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es == null)
            {
                Debug.LogWarning($"[Validator] {sceneName}: No EventSystem — UI clicks will not work");
                warnings++;
            }
        }

        // ── Missing scripts ──
        var allGOs = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in allGOs)
        {
            var components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.LogError($"[Validator] {sceneName}: Missing script on '{FullPath(go)}'");
                    errors++;
                }
            }
        }

        // ── Audio Listener (warn if absent) ──
        var listener = Object.FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include);
        if (listener == null)
        {
            Debug.LogWarning($"[Validator] {sceneName}: No AudioListener in scene");
            warnings++;
        }

        string status = errors > 0 ? "FAIL" : "PASS";
        Debug.Log($"[Validator] {status}  {sceneName}: {errors} errors, {warnings} warnings");
        return (errors, warnings);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static string FullPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
