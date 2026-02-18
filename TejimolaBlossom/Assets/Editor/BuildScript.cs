using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Windows x64")]
    public static void BuildWindows()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Builds", "Windows");
        Directory.CreateDirectory(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = Path.Combine(buildPath, "TejimolaBlossom.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize / 1024 / 1024} MB at {buildPath}");
        }
        else
        {
            Debug.LogError($"Build FAILED: {summary.totalErrors} errors");
            EditorApplication.Exit(1);
        }
    }

    // Called from command line via -executeMethod BuildScript.BatchBuild
    public static void BatchBuild()
    {
        Debug.Log("Starting batch build for Windows x64...");
        BuildWindows();
        EditorApplication.Exit(0);
    }

    // Called from command line via -executeMethod BuildScript.BatchBuildAll
    // Step 1: Build all scenes (includes animation setup)
    // Step 2: Validate scenes
    // Step 3: Compile Windows .exe
    public static void BatchBuildAll()
    {
        try
        {
            Debug.Log("=== Step 1/3: Building all game scenes (+ animations) ===");
            SceneBuilder.BuildAllScenesBatch();

            Debug.Log("=== Step 2/3: Validating scenes ===");
            int validationErrors = SceneValidator.ValidateAllScenesBatch();
            if (validationErrors > 0)
                Debug.LogWarning($"Validation found {validationErrors} error(s) — build continues.");

            Debug.Log("=== Step 3/3: Compiling Windows .exe ===");
            BuildWindows();
            EditorApplication.Exit(0);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BatchBuildAll FAILED: {e}");
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }
}
