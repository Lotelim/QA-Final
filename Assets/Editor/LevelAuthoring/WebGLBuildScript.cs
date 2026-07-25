using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLBuildScript
{
    [MenuItem("Tools/QA Final/Build WebGL (Level_1 + Level_2)")]
    public static void BuildWebGL()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Space Shooter Template FREE/Scenes/Level_1.unity",
                "Assets/Space Shooter Template FREE/Scenes/Level_2.unity",
            },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        UnityEngine.Debug.Log($"[WebGLBuildScript] Build result: {report.summary.result}, " +
            $"total size: {report.summary.totalSize} bytes, errors: {report.summary.totalErrors}");

        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
