using UnityEditor;
using UnityEngine;

namespace DefesaCivil.Casa.EditorTools
{
    public class DCCasaImportador : AssetPostprocessor
    {
        const string ARQUIVO = "DC_Casa.fbx";

        const float LUMENS_POR_UNIDADE = 700f;

        static readonly string[] MATERIAIS_EMISSIVOS =
            { "Luz", "Cupula", "Tela", "Led", "Visor" };

        bool EhCasa => assetPath.EndsWith(ARQUIVO);

        void OnPreprocessModel()
        {
            if (!EhCasa) return;
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
            if (!EhCasa) return;

            bool luz = false;
            float lumens = 500f, kelvin = 3000f, alcance = 3f;
            string modo = "Baked", interacao = null, estado = "";

            for (int i = 0; i < nomes.Length; i++)
            {
                switch (nomes[i])
                {
                    case "unity_luz": luz = true; break;
                    case "unity_modo": modo = valores[i] as string ?? modo; break;
                    case "lumens": lumens = System.Convert.ToSingle(valores[i]); break;
                    case "temperatura_k": kelvin = System.Convert.ToSingle(valores[i]); break;
                    case "alcance_m": alcance = System.Convert.ToSingle(valores[i]); break;
                    case "dc_interacao": interacao = valores[i] as string; break;
                    case "dc_estado": estado = valores[i] as string ?? ""; break;
                }
            }

            if (luz)
            {
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = alcance;
                l.color = Mathf.CorrelatedColorTemperatureToRGB(kelvin);
                l.intensity = Mathf.Clamp(lumens / LUMENS_POR_UNIDADE, 0.2f, 4f);
                l.shadows = LightShadows.Soft;
                l.lightmapBakeType = modo == "Mixed" ? LightmapBakeType.Mixed
                    : modo == "Realtime" ? LightmapBakeType.Realtime
                    : LightmapBakeType.Baked;
            }

            if (!string.IsNullOrEmpty(interacao))
            {
                var c = go.AddComponent<DefesaCivil.Casa.DCInteracao>();
                c.id = interacao;
                c.estado = estado;
            }
        }

        void OnPostprocessModel(GameObject raiz)
        {
            if (!EhCasa) return;

            var todos = raiz.GetComponentsInChildren<Transform>(true);

            foreach (var t in todos)
            {
                if (!t.name.EndsWith("_Macaneta")) continue;
                string folha = t.name.Substring(0, t.name.Length - "_Macaneta".Length);
                foreach (var f in todos)
                {
                    if (f.name == folha) { t.SetParent(f, true); break; }
                }
            }

            const StaticEditorFlags estatico =
                StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ReflectionProbeStatic;

            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (t.GetComponent<MeshRenderer>() == null) continue;
                if (Movel(t)) continue;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, estatico);
            }
        }

        static bool Movel(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
            {
                string n = p.name;
                if (n.Contains("QDL_Porta") || n.Contains("DisjuntorGeral")) return true;
                if (n.StartsWith("DC_CASA_Porta_") && !n.EndsWith("_Batente")) return true;
            }
            return false;
        }

        void OnPostprocessMaterial(Material m)
        {
            if (!EhCasa) return;
            foreach (var chave in MATERIAIS_EMISSIVOS)
            {
                if (!m.name.Contains(chave)) continue;
                Color cor = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor")
                    : m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", cor * 2f);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                break;
            }
        }
    }
}
