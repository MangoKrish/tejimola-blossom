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
