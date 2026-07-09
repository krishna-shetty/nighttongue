using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public static class BuildScript
{
    // CLI entry point:
    //   -executeMethod BuildScript.BuildFromCLI -buildTarget Win64 -outDir "/path/to/output" -product "Aftertaste"
    public static void BuildFromCLI()
    {
        string buildTargetArg = GetArg("-buildTarget") ?? "Win64"; 
        string outDir = GetArg("-outDir") ?? "../../Builds/Aftertaste";
        string product = GetArg("-product") ?? PlayerSettings.productName;

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("No enabled scenes found in Build Settings (File > Build Settings...).");
            EditorApplication.Exit(1);
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string resolvedOutDir = Path.GetFullPath(Path.Combine(projectRoot, outDir));
        Directory.CreateDirectory(resolvedOutDir);

        BuildTarget target;
        string locationPath;

        if (buildTargetArg.Equals("Win64", StringComparison.OrdinalIgnoreCase) ||
            buildTargetArg.Equals("Windows", StringComparison.OrdinalIgnoreCase))
        {
            target = BuildTarget.StandaloneWindows64;
            locationPath = Path.Combine(resolvedOutDir, $"{product}.exe");
        }
        else if (buildTargetArg.Equals("Mac", StringComparison.OrdinalIgnoreCase) ||
                 buildTargetArg.Equals("OSX", StringComparison.OrdinalIgnoreCase) ||
                 buildTargetArg.Equals("MacOS", StringComparison.OrdinalIgnoreCase))
        {
            target = BuildTarget.StandaloneOSX;

            locationPath = Path.Combine(resolvedOutDir, $"{product}.app");
        }
        else
        {
            Debug.LogError($"Unknown -buildTarget '{buildTargetArg}'. Use Win64 or Mac.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Build target: {target}");
        Debug.Log($"Output dir:   {resolvedOutDir}");
        Debug.Log($"Output path:  {locationPath}");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPath,
            target = target,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"Build FAILED: {report.summary.result} (errors: {report.summary.totalErrors})");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Build SUCCEEDED: {report.summary.totalSize} bytes");
        EditorApplication.Exit(0);
    }

    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
                return args[i + 1];
        }
        return null;
    }
}