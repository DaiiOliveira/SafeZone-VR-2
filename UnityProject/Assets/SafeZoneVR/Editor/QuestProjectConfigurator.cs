#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SafeZoneVR.Editor
{
    /// <summary>Aplica configurações de projeto recomendadas para Meta Quest 3/3S (RNF01) e registra as cenas no build.</summary>
    public static class QuestProjectConfigurator
    {
        public const string k_MenuScenePath = "Assets/Scenes/MainMenu.unity";
        public const string k_AlagamentoScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_PerformanceUrpPath = "Assets/Settings/Project Configuration/Performance URP Config.asset";
        const string k_QualityUrpPath = "Assets/Settings/Project Configuration/Quality URP Config.asset";

        [MenuItem("SafeZone VR/Build/4. Configurações do projeto (Quest 3)")]
        public static void ApplyMenu()
        {
            Apply();
            EditorUtility.DisplayDialog("SafeZone VR", "Configurações do projeto aplicadas (nome, Android/Quest, URP, cenas no build).", "OK");
        }

        public static void Apply()
        {
            PlayerSettings.productName = "SafeZone VR";
            PlayerSettings.companyName = "SafeZone VR";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.safezonevr.app");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // Quest 3/3S: API 32 é o mínimo aceito pela Meta Store; evita excluir headsets não atualizados.
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)32;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.Vulkan });

            TuneUrpAsset(k_PerformanceUrpPath, 2048, 12f);
            TuneUrpAsset(k_QualityUrpPath, 2048, 15f);

            // Colisores de malha dos móveis são "cozidos" no build: carregamento mais rápido no Quest e
            // não exige Read/Write nas malhas importadas.
            PlayerSettings.bakeCollisionMeshes = true;

            RegisterScenes();
            AssetDatabase.SaveAssets();
        }

        public static void RegisterScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            if (System.IO.File.Exists(k_MenuScenePath))
                scenes.Add(new EditorBuildSettingsScene(k_MenuScenePath, true));
            if (System.IO.File.Exists(k_AlagamentoScenePath))
                scenes.Add(new EditorBuildSettingsScene(k_AlagamentoScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void TuneUrpAsset(string path, int shadowRes, float shadowDistance)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null)
                return;
            var so = new SerializedObject(asset);
            var res = so.FindProperty("m_MainLightShadowmapResolution");
            if (res != null) res.intValue = shadowRes;
            var dist = so.FindProperty("m_ShadowDistance");
            if (dist != null) dist.floatValue = shadowDistance;
            var hdr = so.FindProperty("m_SupportsHDR");
            if (hdr != null) hdr.boolValue = false;
            var msaa = so.FindProperty("m_MSAA");
            if (msaa != null) msaa.intValue = 4;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
