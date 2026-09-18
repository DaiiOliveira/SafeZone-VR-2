using System.IO;
using UnityEditor;
using UnityEngine;

namespace DefesaCivil.NPC.EditorTools
{
    public class DCNpcImportador : AssetPostprocessor
    {
        const string PASTA = "/NPC/";
        const string ATLAS = "Texturas/DC_T_NPC_Atlas.png";

        string Caminho => assetPath.Replace('\\', '/');
        bool EhNpcPasta => Caminho.Contains(PASTA) && Caminho.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);
        bool EhAndador => EhNpcPasta && Path.GetFileNameWithoutExtension(Caminho) == "Andador";
        bool EhPersonagem => EhNpcPasta && !EhAndador;

        void OnPreprocessModel()
        {
            if (!EhNpcPasta) return;
            var mi = (ModelImporter)assetImporter;
            mi.globalScale = 1f;
            mi.useFileScale = true;
            mi.bakeAxisConversion = true;
            mi.importCameras = false;
            mi.importLights = false;
            mi.importVisibility = false;
            mi.importBlendShapes = false;
            mi.importNormals = ModelImporterNormals.Import;
            mi.meshCompression = ModelImporterMeshCompression.Off;
            mi.addCollider = false;
            mi.generateSecondaryUV = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            context.DependsOnSourceAsset(Path.GetDirectoryName(Caminho).Replace('\\', '/') + "/" + ATLAS);

            if (EhAndador)
            {
                mi.importAnimation = false;
                mi.animationType = ModelImporterAnimationType.None;
                return;
            }
            mi.importAnimation = true;
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            mi.optimizeGameObjects = false;
            mi.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
        }

        void OnPreprocessAnimation()
        {
            if (!EhPersonagem) return;
            var mi = (ModelImporter)assetImporter;
            var clipes = mi.defaultClipAnimations;
            if (clipes == null || clipes.Length == 0) return;
            foreach (var c in clipes)
            {
                var nome = c.takeName.Substring(c.takeName.LastIndexOf('|') + 1);
                c.name = nome;
                bool loop = nome == "Idle" || nome == "Andar";
                c.loopTime = loop;
                c.loopPose = loop;
                c.lockRootRotation = true;
                c.lockRootHeightY = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = true;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            mi.clipAnimations = clipes;
        }

        void OnPreprocessTexture()
        {
            if (!Caminho.Contains(PASTA) || !Caminho.EndsWith("DC_T_NPC_Atlas.png")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Point;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = 256;
        }

        void OnPostprocessMaterial(Material m)
        {
            if (!EhNpcPasta || !m.name.Contains("DC_M_NPC")) return;
            var arq = Path.GetDirectoryName(Caminho).Replace('\\', '/') + "/" + ATLAS;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(arq);
            if (tex == null)
            {
                Debug.LogWarning($"[NPC] atlas nao encontrado em {arq}: copie a pasta Texturas junto e reimporte.");
                return;
            }
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        }
    }
}
