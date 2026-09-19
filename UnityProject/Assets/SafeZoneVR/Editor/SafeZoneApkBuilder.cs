#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SafeZoneVR.Editor
{
    public static class SafeZoneApkBuilder
    {
        const string k_OutputFolder = "Builds";

        [MenuItem("SafeZone VR/Build/5. Gerar APK (release)")]
        public static void BuildReleaseMenu()
        {
            var apk = Build(false);
            if (apk != null && EditorUtility.DisplayDialog("SafeZone VR", "APK gerado em:\n" + apk + "\n\nInstale com:\nadb install -r \"" + apk + "\"", "Abrir pasta", "OK"))
                EditorUtility.RevealInFinder(apk);
        }

        [MenuItem("SafeZone VR/Build/5b. Gerar APK de desenvolvimento (profiler)")]
        public static void BuildDevelopmentMenu()
        {
            var apk = Build(true);
            if (apk != null)
                EditorUtility.DisplayDialog("SafeZone VR", "APK de desenvolvimento gerado em:\n" + apk, "OK");
        }

        public static string Build(bool development)
        {
            QuestProjectConfigurator.Apply();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new Exception("Não foi possível trocar o alvo do build para Android. Instale o Android Build Support pelo Unity Hub.");

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
                throw new Exception("Nenhuma cena habilitada no Build Settings.");

            Directory.CreateDirectory(k_OutputFolder);
            var suffix = development ? "dev" : "release";
            var apk = Path.Combine(k_OutputFolder, $"SafeZoneVR-{PlayerSettings.bundleVersion}-{suffix}.apk");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apk,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = development
                    ? BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging
                    : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[SafeZone VR] Build falhou ({summary.result}): {summary.totalErrors} erro(s). Veja o Console e o Editor.log.");
                return null;
            }

            Debug.Log($"[SafeZone VR] APK gerado: {apk} ({summary.totalSize / 1048576f:F1} MB em {summary.totalTime.TotalMinutes:F1} min)\n" +
                      $"Instalar no headset: adb install -r \"{apk}\"");
            return apk;
        }

        public static void BuildFromCommandLine()
        {
            var development = Environment.GetCommandLineArgs().Contains("-development");
            var apk = Build(development);
            if (apk == null)
                EditorApplication.Exit(1);
        }
    }
}
#endif
