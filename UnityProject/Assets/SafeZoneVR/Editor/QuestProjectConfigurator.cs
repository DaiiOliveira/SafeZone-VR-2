#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.OpenXR;

namespace SafeZoneVR.Editor
{
    public static class QuestProjectConfigurator
    {
        public const string k_MenuScenePath = "Assets/Scenes/MainMenu.unity";
        public const string k_AlagamentoScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        public const string k_IncendioScenePath = "Assets/Scenes/Fases/Incendio_EmCasa.unity";
        public const string k_GranizoScenePath = "Assets/Scenes/Fases/Granizo_Praca.unity";
        const string k_PerformanceUrpPath = "Assets/Settings/Project Configuration/Performance URP Config.asset";
        const string k_QualityUrpPath = "Assets/Settings/Project Configuration/Quality URP Config.asset";
        const string k_AndroidRendererPath = "Assets/Settings/Project Configuration/Android Preset.asset";

        const string k_AndroidQualityLevel = "Low";

        const int k_AdditionalLightsPerObject = 8;

        [MenuItem("SafeZone VR/Build/4. Configurações do projeto (Quest 3)")]
        public static void ApplyMenu()
        {
            Apply();
            EditorUtility.DisplayDialog("SafeZone VR", "Configurações do projeto aplicadas (app, Android/Quest, OpenXR, URP e cenas).", "OK");
        }

        [MenuItem("SafeZone VR/Build/4b. Conferir configurações do APK")]
        public static void ValidateMenu()
        {
            Debug.Log(Validate());
        }

        public static void Apply()
        {
            ApplyIdentity();
            ApplyAndroidPlayer();
            ApplyOpenXr();
            ApplyRendering();
            RegisterScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[SafeZone VR] Configurações do projeto aplicadas.\n" + Validate());
        }

        static void ApplyIdentity()
        {
            PlayerSettings.productName = "SafeZone VR";
            PlayerSettings.companyName = "SafeZone VR";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.safezonevr.app");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        static void ApplyAndroidPlayer()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSpeed);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.gcIncremental = true;

            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;

            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)32;
            if ((int)PlayerSettings.Android.targetSdkVersion < 32)
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetMobileMTRendering(NamedBuildTarget.Android, true);
            PlayerSettings.MTRendering = true;
            PlayerSettings.gpuSkinning = true;

            PlayerSettings.Android.blitType = AndroidBlitType.Never;

            PlayerSettings.Android.textureCompressionFormats = new[] { TextureCompressionFormat.ASTC };
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            EditorUserBuildSettings.buildAppBundle = false;

            PlayerSettings.bakeCollisionMeshes = true;
        }

        static void ApplyOpenXr()
        {
            var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (general != null)
                general.InitManagerOnStart = true;

            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (settings == null)
            {
                Debug.LogWarning("[SafeZone VR] OpenXR não configurado para Android; abra Project Settings > XR Plug-in Management.");
                return;
            }

            var so = new SerializedObject(settings);
            SetInt(so, "m_renderMode", 1);
            SetBool(so, "m_optimizeBufferDiscards", true);
            SetBool(so, "m_symmetricProjection", true);
            SetInt(so, "m_foveatedRenderingApi", 1);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);

            foreach (var feature in settings.GetFeatures())
            {
                switch (feature.GetType().Name)
                {
                    case "MetaQuestFeature":
                    case "OculusTouchControllerProfile":
                    case "MetaQuestTouchPlusControllerProfile":
                    case "MetaQuestTouchProControllerProfile":
                    case "FoveatedRenderingFeature":
                        if (!feature.enabled)
                        {
                            feature.enabled = true;
                            EditorUtility.SetDirty(feature);
                        }
                        break;
                }
            }
        }

        static void SetInt(SerializedObject so, string path, int value)
        {
            var p = so.FindProperty(path);
            if (p != null) p.intValue = value;
        }

        static void SetBool(SerializedObject so, string path, bool value)
        {
            var p = so.FindProperty(path);
            if (p != null) p.boolValue = value;
        }

        static void ApplyRendering()
        {
            TuneUrpAsset(k_PerformanceUrpPath, 2048, 12f);
            TuneUrpAsset(k_QualityUrpPath, 2048, 15f);

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(k_AndroidRendererPath);
            if (renderer != null)
            {
                var so = new SerializedObject(renderer);
                SetInt(so, "m_RenderingMode", 2);
                SetInt(so, "m_IntermediateTextureMode", 0);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(renderer);
            }

            SetAndroidQualityLevel();
        }

        static void TuneUrpAsset(string path, int shadowRes, float shadowDistance)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset == null)
                return;

            var so = new SerializedObject(asset);
            SetInt(so, "m_MainLightShadowmapResolution", shadowRes);
            var dist = so.FindProperty("m_ShadowDistance");
            if (dist != null) dist.floatValue = shadowDistance;
            SetBool(so, "m_SupportsHDR", false);
            SetInt(so, "m_MSAA", 4);
            SetInt(so, "m_ShadowCascadeCount", 1);
            SetBool(so, "m_RequireDepthTexture", false);
            SetBool(so, "m_RequireOpaqueTexture", false);
            SetBool(so, "m_UseSRPBatcher", true);

            SetInt(so, "m_AdditionalLightsRenderingMode", 1);
            SetInt(so, "m_AdditionalLightsPerObjectLimit", k_AdditionalLightsPerObject);
            SetBool(so, "m_AdditionalLightShadowsSupported", false);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        static void SetAndroidQualityLevel()
        {
            var index = System.Array.IndexOf(QualitySettings.names, k_AndroidQualityLevel);
            if (index < 0)
                return;

            var quality = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset");
            if (quality.Length == 0)
                return;

            var so = new SerializedObject(quality[0]);
            var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
            if (perPlatform == null)
                return;

            for (var i = 0; i < perPlatform.arraySize; i++)
            {
                var entry = perPlatform.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue != "Android")
                    continue;
                entry.FindPropertyRelative("second").intValue = index;
                so.ApplyModifiedPropertiesWithoutUndo();
                return;
            }
        }

        public static void RegisterScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            if (System.IO.File.Exists(k_MenuScenePath))
                scenes.Add(new EditorBuildSettingsScene(k_MenuScenePath, true));
            foreach (var path in new[] { k_AlagamentoScenePath, k_IncendioScenePath, k_GranizoScenePath })
                if (System.IO.File.Exists(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static string Validate()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[SafeZone VR] Configurações do APK (Quest):");
            sb.AppendLine("  app: " + PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) + " v" + PlayerSettings.bundleVersion + " (code " + PlayerSettings.Android.bundleVersionCode + ")");
            sb.AppendLine("  suporte Android instalado: " + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android) + " | alvo ativo: " + EditorUserBuildSettings.activeBuildTarget);
            sb.AppendLine("  entrada: " + PlayerSettings.Android.applicationEntry + " (o MetaQuestFeature exige GameActivity no Unity 6)");
            sb.AppendLine("  backend: " + PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) + " / " + PlayerSettings.Android.targetArchitectures + " / " + PlayerSettings.GetIl2CppCodeGeneration(NamedBuildTarget.Android));
            sb.AppendLine("  SDK: min " + PlayerSettings.Android.minSdkVersion + " alvo " + PlayerSettings.Android.targetSdkVersion);
            sb.AppendLine("  gráficos: " + string.Join(",", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)) + " | blit " + PlayerSettings.Android.blitType + " | texturas " + string.Join(",", PlayerSettings.Android.textureCompressionFormats));
            sb.AppendLine("  cor: " + PlayerSettings.colorSpace + " | splash " + PlayerSettings.SplashScreen.show);

            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (settings != null)
            {
                var so = new SerializedObject(settings);
                sb.AppendLine("  OpenXR: renderMode=" + so.FindProperty("m_renderMode").intValue + " (1 = Single Pass Instanced)"
                    + " bufferDiscards=" + so.FindProperty("m_optimizeBufferDiscards").boolValue
                    + " symmetric=" + so.FindProperty("m_symmetricProjection").boolValue
                    + " foveation=" + so.FindProperty("m_foveatedRenderingApi").intValue);
                var features = new List<string>();
                foreach (var f in settings.GetFeatures())
                    if (f.enabled) features.Add(f.GetType().Name);
                sb.AppendLine("  OpenXR ligados: " + string.Join(", ", features));
            }

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(k_PerformanceUrpPath);
            if (urp != null)
                sb.AppendLine("  URP headset: luzes " + urp.additionalLightsRenderingMode + " (limite " + urp.maxAdditionalLightsCount + ") MSAA " + urp.msaaSampleCount + " HDR " + urp.supportsHDR);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(k_AndroidRendererPath);
            if (renderer != null)
                sb.AppendLine("  caminho de render: " + new SerializedObject(renderer).FindProperty("m_RenderingMode").intValue + " (2 = Forward+)");

            sb.AppendLine("  cenas no build: " + EditorBuildSettings.scenes.Length);
            sb.AppendLine("  assinatura: " + (PlayerSettings.Android.useCustomKeystore ? PlayerSettings.Android.keystoreName : "keystore de debug (serve para sideload; a Meta Store exige keystore próprio)"));
            return sb.ToString();
        }
    }
}
#endif
