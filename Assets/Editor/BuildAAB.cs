using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAAB
{
    private const string BuildsFolder = "C:/Users/zirow/source/repos/IMRtecnologygroup/polixgamestudio/Paper Hangman Matatiempo/Paper Hangman Matatiempo/Builds";

    [MenuItem("Build/Build AAB")]
    public static void Build()
    {
        BuildAndroid(true);
    }

    [MenuItem("Build/Build APK")]
    public static void BuildAPK()
    {
        BuildAndroid(false);
    }

    private static void BuildAndroid(bool buildAppBundle)
    {
        PlayerSettings.Android.keystorePass = "Derowin.22";
        PlayerSettings.Android.keyaliasPass = "Derowin.22";
        PlayerSettings.Android.useCustomKeystore = true;
        var scenes = EditorBuildSettings.scenes;
        var scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled)
                scenePaths[i] = scenes[i].path;
        }

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = buildAppBundle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        string fileName = "Matatiempo" + (buildAppBundle ? ".aab" : ".apk");
        string outputPath = System.IO.Path.Combine(BuildsFolder, fileName).Replace('\\', '/');

        System.IO.Directory.CreateDirectory(BuildsFolder);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"Build failed: {summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
