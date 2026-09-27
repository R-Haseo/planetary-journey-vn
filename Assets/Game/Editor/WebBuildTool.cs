using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebBuildTool
{
    private const string DevelopmentBuildPath = "Builds/Web/Development";
    private const string ReleaseBuildPath = "Builds/Web/Release";

    [MenuItem("Build/Web/Development")]
    public static void BuildDevelopment()
    {
        if (!Application.isBatchMode && !ConfirmVersion(false))
        {
            return;
        }

        Build(
            DevelopmentBuildPath,
            WebGLCompressionFormat.Disabled,
            BuildOptions.Development);
    }

    [MenuItem("Build/Web/Release")]
    public static void BuildRelease()
    {
        if (!Application.isBatchMode && !ConfirmVersion(true))
        {
            return;
        }

        Build(
            ReleaseBuildPath,
            WebGLCompressionFormat.Brotli,
            BuildOptions.None);
    }

    private static bool ConfirmVersion(bool isRelease)
    {
        string version = PlayerSettings.bundleVersion;

        string message = isRelease
            ? $"Current application version: {version}\n\n" +
            "Have you updated the application version for this release?"
            : $"Current application version: {version}\n\n" +
            "Please confirm that this version is correct for the next release.";

        return EditorUtility.DisplayDialog(
            isRelease ? "Release Build" : "Development Build",
            message,
            "Build",
            "Cancel");
    }

    private static void Build(
        string outputPath,
        WebGLCompressionFormat compressionFormat,
        BuildOptions buildOptions)
    {
        var originalCompressionFormat = PlayerSettings.WebGL.compressionFormat;

        try
        {
            PlayerSettings.WebGL.compressionFormat = compressionFormat;

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            Directory.CreateDirectory(outputPath);

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = buildOptions
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Web build failed: {summary.result}");
            }

            Debug.Log(
                $"Web build succeeded: {outputPath} " +
                $"({summary.totalSize / 1024 / 1024} MB)");
        }
        finally
        {
            // Editor上の設定を勝手に変更したままにしない
            PlayerSettings.WebGL.compressionFormat = originalCompressionFormat;
        }
    }

    private static string[] GetEnabledScenes()
    {
        return Array.ConvertAll(
            Array.FindAll(
                EditorBuildSettings.scenes,
                scene => scene.enabled),
            scene => scene.path);
    }
}
