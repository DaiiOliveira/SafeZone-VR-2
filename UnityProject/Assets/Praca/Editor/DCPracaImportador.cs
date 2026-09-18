using UnityEditor;
using UnityEngine;

namespace DefesaCivil.Praca.EditorTools
{
    public class DCPracaImportador : AssetPostprocessor
    {
        const string ARQUIVO = "DC_Praca.fbx";

        static readonly (string chave, float forca)[] MATERIAIS_EMISSIVOS =
            { ("vidro_lampiao", 1.2f), ("nuvem", 0.6f) };

        bool EhPraca => assetPath.EndsWith(ARQUIVO);

        void OnPreprocessModel()
        {
            if (!EhPraca) return;
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
            mi.generateSecondaryUV = true;
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        }

        void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] nomes, object[] valores)
        {
            if (!EhPraca) return;
            for (int i = 0; i < nomes.Length; i++)
            {
                if (nomes[i] != "dc_interacao") continue;
                var c = go.AddComponent<DefesaCivil.Casa.DCInteracao>();
                c.id = valores[i] as string;
                c.estado = "";
            }
        }

        void OnPostprocessModel(GameObject raiz)
        {
            if (!EhPraca) return;
            const StaticEditorFlags estatico =
                StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ReflectionProbeStatic;
            foreach (var r in raiz.GetComponentsInChildren<MeshRenderer>(true))
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, estatico);
        }

        void OnPostprocessMaterial(Material m)
        {
            if (!EhPraca) return;
            foreach (var (chave, forca) in MATERIAIS_EMISSIVOS)
            {
                if (!m.name.Contains(chave)) continue;
                Color cor = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor")
                    : m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", cor * forca);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                break;
            }
        }
    }
}
