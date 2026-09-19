using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefesaCivil.Itens.EditorTools
{
    public class DCItensImportador : AssetPostprocessor
    {
        const string PASTA = "/Itens/";
        const string ATLAS = "Texturas/DC_T_Itens_Atlas.png";
        const float LUMENS_POR_UNIDADE = 700f;

        static readonly string[] CENARIO = { "UBS", "PontoOnibus", "Placa", "Plataforma", "Vaga" };

        bool EhItem => assetPath.Replace('\\', '/').Contains(PASTA) &&
                       assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);

        bool EhCenario => System.Array.IndexOf(CENARIO, Path.GetFileNameWithoutExtension(assetPath)) >= 0;

        void OnPreprocessModel()
        {
            if (!EhItem) return;
            var mi = (ModelImporter)assetImporter;
            mi.globalScale = 1f;
            mi.useFileScale = true;
            mi.bakeAxisConversion = false;
            mi.importCameras = false;
            mi.importLights = false;
            mi.importVisibility = false;
            mi.importBlendShapes = false;
            mi.importAnimation = false;
            mi.animationType = ModelImporterAnimationType.None;
            mi.importNormals = ModelImporterNormals.Import;
            mi.meshCompression = ModelImporterMeshCompression.Off;
            mi.isReadable = false;
            mi.addCollider = false;
            mi.generateSecondaryUV = EhCenario;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            context.DependsOnSourceAsset(CaminhoAtlas());
        }

        void OnPreprocessTexture()
        {
            var p = assetPath.Replace('\\', '/');
            if (!p.Contains(PASTA) || !p.EndsWith("DC_T_Itens_Atlas.png")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = 2048;
        }

        string CaminhoAtlas()
        {
            var dir = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            return dir + "/" + ATLAS;
        }

        void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] nomes, object[] valores)
        {
            if (!EhItem) return;
            bool luz = false;
            float lumens = 1000f, kelvin = 4000f, alcance = 6f;
            string modo = "Baked";
            for (int i = 0; i < nomes.Length; i++)
            {
                switch (nomes[i])
                {
                    case "unity_luz": luz = true; break;
                    case "unity_modo": modo = valores[i] as string ?? modo; break;
                    case "lumens": lumens = System.Convert.ToSingle(valores[i]); break;
                    case "temperatura_k": kelvin = System.Convert.ToSingle(valores[i]); break;
                    case "alcance_m": alcance = System.Convert.ToSingle(valores[i]); break;
                }
            }
            if (!luz) return;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = alcance;
            l.color = Mathf.CorrelatedColorTemperatureToRGB(kelvin);
            l.intensity = Mathf.Clamp(lumens / LUMENS_POR_UNIDADE, 0.2f, 5f);
            l.shadows = LightShadows.Soft;
            l.lightmapBakeType = modo == "Mixed" ? LightmapBakeType.Mixed
                : modo == "Realtime" ? LightmapBakeType.Realtime : LightmapBakeType.Baked;
        }

        void OnPostprocessModel(GameObject raiz)
        {
            if (!EhItem || !EhCenario) return;
            const StaticEditorFlags estatico =
                StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ReflectionProbeStatic;
            foreach (var r in raiz.GetComponentsInChildren<MeshRenderer>(true))
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, estatico);
        }

        void OnPostprocessMaterial(Material m)
        {
            if (!EhItem || !m.name.Contains("DC_M_Itens")) return;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(CaminhoAtlas());
            if (tex == null)
            {
                Debug.LogWarning($"[Itens] atlas nao encontrado em {CaminhoAtlas()}: copie a pasta Texturas junto e reimporte.");
                return;
            }
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);

            bool translucido = m.name.Contains("Translucido");
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", translucido ? 0.75f : 0.2f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (!translucido) return;

            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
