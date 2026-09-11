#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Cria o prefab Casa_SafeZone a partir da geometria solta da cena Alagamento_EmCasa (pisos, paredes, teto,
    /// telhados, muros, pilares, gramados, portas, móveis e Kombi) e abre o vão da janela da cozinha.
    /// A cena do Alagamento passa a usar a instância do prefab.
    /// </summary>
    public static class HousePrefabBuilder
    {
        const string k_AlagamentoScene = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_PrefabFolder = "Assets/SafeZoneVR/Prefabs";

        // Raízes que NÃO fazem parte da casa.
        static readonly HashSet<string> k_Exclude = new HashSet<string>
        {
            "Directional Light", "XR Origin (XR Rig)", "XR Interaction Manager", "EventSystem",
            "Disjuntor_PowerOff", "SafeZone_Gameplay", "Registro_Knob", "XR Interaction Simulator", "Global Volume"
        };

        // Vão da janela da cozinha (parede oeste): 1,0 × 1,0 m, centro em y 1,4, z 11.
        static readonly Vector2 k_WindowZ = new Vector2(10.5f, 11.5f);
        static readonly Vector2 k_WindowY = new Vector2(0.9f, 1.9f);

        [MenuItem("SafeZone VR/Build/1b. Prefab da casa (Casa_SafeZone)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Prefab Casa_SafeZone criado/atualizado.", "OK");
        }

        public static GameObject Build()
        {
            var scene = ScenarioSceneKit.OpenOrCreateScene(k_AlagamentoScene);
            UIBuilderUtil.EnsureFolder(k_PrefabFolder);

            GameObject houseRoot = null;
            foreach (var rootGo in scene.GetRootGameObjects())
                if (rootGo.name == ScenarioSceneKit.k_HouseRootName) { houseRoot = rootGo; break; }

            if (houseRoot == null)
            {
                houseRoot = new GameObject(ScenarioSceneKit.k_HouseRootName);
                SceneManager.MoveGameObjectToScene(houseRoot, scene);
                houseRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(houseRoot))
            {
                // Já é instância: desconecta para reconstruir o asset com o conteúdo atual da cena.
                PrefabUtility.UnpackPrefabInstance(houseRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            var toMove = new List<GameObject>();
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (rootGo == houseRoot || k_Exclude.Contains(rootGo.name)) continue;
                if (rootGo.GetComponent<Light>() != null || rootGo.GetComponent<Camera>() != null) continue;
                if (rootGo.GetComponentInChildren<Unity.XR.CoreUtils.XROrigin>(true) != null) continue;
                toMove.Add(rootGo);
            }
            foreach (var go in toMove)
                go.transform.SetParent(houseRoot.transform, true);

            OpenKitchenWindow(houseRoot.transform);

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(houseRoot, ScenarioSceneKit.k_HousePrefabPath, InteractionMode.AutomatedAction, out var ok);
            if (!ok)
                throw new System.Exception("Falha ao salvar o prefab da casa.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[SafeZone VR] Prefab da casa salvo em {ScenarioSceneKit.k_HousePrefabPath} ({toMove.Count} raízes movidas).");
            return prefab;
        }

        /// <summary>Substitui a parede oeste da cozinha por 4 peças com um vão de janela real.</summary>
        static void OpenKitchenWindow(Transform house)
        {
            if (house.Find("ParedeGaragem_JanelaCozinha") != null) return;   // já feito

            // Parede oeste: x −36.01..−35.81, z 6.17..14.97 (raiz "ParedeGaragem" com essa extensão).
            Transform wall = null;
            foreach (Transform t in house)
            {
                if (!t.name.StartsWith("ParedeGaragem")) continue;
                var r = t.GetComponent<Renderer>();
                if (r == null) continue;
                var b = r.bounds;
                if (b.max.x < -35.7f && b.size.z > 8f && b.size.y > 1.5f) { wall = t; break; }
            }
            if (wall == null)
            {
                Debug.LogWarning("[SafeZone VR] Parede oeste da cozinha não encontrada: vão da janela não aberto.");
                return;
            }

            var wr = wall.GetComponent<Renderer>();
            var wb = wr.bounds;
            var mat = wr.sharedMaterial;

            var group = new GameObject("ParedeGaragem_JanelaCozinha");
            group.transform.SetParent(house, false);

            void Piece(string name, float z0, float z1, float y0, float y1)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.SetParent(group.transform, false);
                go.transform.position = new Vector3(wb.center.x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f);
                go.transform.localScale = new Vector3(wb.size.x, y1 - y0, z1 - z0);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                UIBuilderUtil.MarkStatic(go);
            }

            Piece("Trecho_Sul", wb.min.z, k_WindowZ.x, wb.min.y, wb.max.y);
            Piece("Trecho_Norte", k_WindowZ.y, wb.max.z, wb.min.y, wb.max.y);
            Piece("Peitoril", k_WindowZ.x, k_WindowZ.y, wb.min.y, k_WindowY.x);
            Piece("Verga", k_WindowZ.x, k_WindowZ.y, k_WindowY.y, wb.max.y);

            Object.DestroyImmediate(wall.gameObject);
        }
    }
}
#endif
