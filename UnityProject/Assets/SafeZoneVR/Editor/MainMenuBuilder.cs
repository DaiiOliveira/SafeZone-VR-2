#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

namespace SafeZoneVR.Editor
{
    public static class MainMenuBuilder
    {
        const string k_ScenePath = "Assets/Scenes/MainMenu.unity";
        public const string k_HubModelPath = "Assets/SafeZoneVR/Models/DC_Hub.fbx";
        const string k_HubRootName = "DC_Hub";
        const string k_CatalogPath = SafeZoneDataBuilder.k_ResourcesFolder + "/ScenarioCatalog.asset";

        static readonly Vector3 k_SpawnPosition = new Vector3(0f, 0f, 5.9f);
        const float k_SpawnYaw = 180f;

        const float k_BoardFontSize = 1.05f;

        static readonly Quaternion k_FacingEntrance = Quaternion.Euler(0f, 180f, 0f);

        struct KioskMap
        {
            public string objectName;
            public int catalogIndex;
            public Color accent;
            public string boardTitle;
            public string boardSubtitle;
        }

        static readonly KioskMap[] k_Kiosks =
        {
            new KioskMap { objectName = "DC_PANEL_Rain", catalogIndex = 0, accent = new Color(0.24f, 0.55f, 0.84f) },
            new KioskMap { objectName = "DC_PANEL_Fire", catalogIndex = 1, accent = new Color(0.88f, 0.57f, 0.2f) },

            new KioskMap
            {
                objectName = "DC_PANEL_Quake", catalogIndex = 2, accent = new Color(0.13f, 0.62f, 0.32f),
                boardTitle = "GRANIZO", boardSubtitle = "Chuva de pedras e abrigo",
            },
        };

        static readonly string[] k_NoColliderParts =
        {
            "DC_ENV_Backdrop", "DC_ENV_GroundFar", "DC_ENV_Mountains_Far", "DC_ENV_Mountains_Mid", "DC_ENV_Mountains_Near",
            "DC_ENV_FloorInlay", "DC_ENV_FloorLines", "DC_SIGN_FloorDecal",
            "DC_PANEL_Fire_Art", "DC_PANEL_Rain_Art", "DC_PANEL_Quake_Art",
        };

        [MenuItem("SafeZone VR/Build/3. Menu principal (gerar cena)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Cena MainMenu gerada e salva.", "OK");
        }

        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SafeZoneDataBuilder.Build();
            ConfigureHubImporter();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ScenarioSceneKit.CreateMaterials();
            SetupLighting(scene);

            var hub = InstantiateHub(scene);
            StripUnwantedColliders(hub);
            CreateTeleportArea(scene);
            CreateBounds(scene);

            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_SpawnPosition, Quaternion.Euler(0f, k_SpawnYaw, 0f));
            var cam = origin.Camera;

            var root = new GameObject("HubMenu");
            SceneManager.MoveGameObjectToScene(root, scene);
            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_SpawnPosition, Quaternion.Euler(0f, k_SpawnYaw, 0f));

            var systems = UIBuilderUtil.Child(root.transform, "Systems");
            var comfort = systems.AddComponent<ComfortSettingsApplier>();
            comfort.origin = origin;
            comfort.vignette = cam.GetComponentInChildren<TunnelingVignetteController>(true);

            var options = ScenarioSceneKit.CreateOptionsPanel(root.transform, cam.transform, false);

            var catalog = AssetDatabase.LoadAssetAtPath<ScenarioCatalogSO>(k_CatalogPath);
            if (catalog == null)
                throw new System.Exception($"Catálogo não encontrado em {k_CatalogPath}. Rode SafeZoneDataBuilder.Build() primeiro.");

            var ctrl = root.AddComponent<MainMenuController>();
            ctrl.catalog = catalog;
            ctrl.optionsPanel = options;
            ctrl.kiosks = CreateKiosks(root.transform, hub, catalog);
            ctrl.optionsButton = CreateDiscButton(root.transform, hub, "DC_UI_ButtonSettings", "BotaoConfiguracoes");
            ctrl.quitButton = CreateDiscButton(root.transform, hub, "DC_UI_ButtonExit", "BotaoSair");

            ScenarioSceneKit.CreateLoadingPanel(root.transform, cam.transform);
            var notice = ScenarioSceneKit.CreateSafetyNotice(root.transform, cam.transform);
            notice.blockedButtons.Add(ctrl.optionsButton);
            notice.blockedButtons.Add(ctrl.quitButton);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(notice.onAccepted, ctrl.RefreshKiosks);

            UIBuilderUtil.MarkStatic(hub);

            EditorSceneManager.SaveScene(scene, k_ScenePath);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Cena MainMenu (hub DC_Hub) gerada em " + k_ScenePath);
        }

        static void ConfigureHubImporter()
        {
            var importer = AssetImporter.GetAtPath(k_HubModelPath) as ModelImporter;
            if (importer == null)
                throw new System.Exception($"Modelo do hub não encontrado em {k_HubModelPath}.");

            var changed = false;
            if (!importer.addCollider) { importer.addCollider = true; changed = true; }
            if (importer.importCameras) { importer.importCameras = false; changed = true; }
            if (importer.importLights) { importer.importLights = false; changed = true; }
            if (importer.importAnimation) { importer.importAnimation = false; changed = true; }
            if (importer.animationType != ModelImporterAnimationType.None) { importer.animationType = ModelImporterAnimationType.None; changed = true; }
            if (changed)
                importer.SaveAndReimport();
        }

        static GameObject InstantiateHub(Scene scene)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_HubModelPath);
            if (prefab == null)
                throw new System.Exception($"Modelo do hub não encontrado em {k_HubModelPath}.");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = k_HubRootName;
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return instance;
        }

        static void StripUnwantedColliders(GameObject hub)
        {
            foreach (var name in k_NoColliderParts)
            {
                var part = ScenarioSceneKit.FindChildRecursive(hub.transform, name);
                if (part == null)
                    continue;
                foreach (var collider in part.GetComponents<Collider>())
                    Object.DestroyImmediate(collider);
            }
        }

        static void CreateTeleportArea(Scene scene)
        {
            var area = GameObject.CreatePrimitive(PrimitiveType.Plane);
            area.name = "AreaTeleporte";
            SceneManager.MoveGameObjectToScene(area, scene);
            Object.DestroyImmediate(area.GetComponent<MeshRenderer>());
            area.transform.SetPositionAndRotation(new Vector3(0f, 0.02f, -0.9f), Quaternion.identity);
            area.transform.localScale = new Vector3(0.96f, 1f, 1.5f);
            ScenarioSceneKit.AddTeleportArea(area);
        }

        static void CreateBounds(Scene scene)
        {
            var root = new GameObject("Limites");
            SceneManager.MoveGameObjectToScene(root, scene);
            AddWall(root.transform, "Oeste", new Vector3(-5.1f, 1.3f, -1f), new Vector3(0.2f, 2.6f, 16f));
            AddWall(root.transform, "Leste", new Vector3(5.1f, 1.3f, -1f), new Vector3(0.2f, 2.6f, 16f));
            AddWall(root.transform, "Fundo", new Vector3(0f, 1.3f, -8.9f), new Vector3(10.4f, 2.6f, 0.2f));
            AddWall(root.transform, "Entrada", new Vector3(0f, 1.3f, 6.9f), new Vector3(10.4f, 2.6f, 0.2f));
        }

        static void AddWall(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var go = UIBuilderUtil.Child(parent, name);
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
        }

        static void SetupLighting(Scene scene)
        {
            ScenarioSceneKit.SetupDirectionalLight(scene, new Color(1f, 0.96f, 0.9f), 1.15f, new Vector3(48f, -32f, 0f));
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1f;
            DynamicGI.UpdateEnvironment();
        }

        static List<HubKiosk> CreateKiosks(Transform parent, GameObject hub, ScenarioCatalogSO catalog)
        {
            var kiosks = new List<HubKiosk>();
            var hidden = UIBuilderUtil.GetOrCreateMaterial("Mat_HubTextoOculto", new Color(0f, 0f, 0f, 0f), true, true, 0f, 3000);

            foreach (var map in k_Kiosks)
            {
                var panel = ScenarioSceneKit.FindChildRecursive(hub.transform, map.objectName);
                if (panel == null)
                {
                    Debug.LogWarning($"[SafeZone VR] Totem {map.objectName} não existe no modelo do hub.");
                    continue;
                }

                var scenarios = catalog != null ? catalog.scenarios : null;
                var scenario = scenarios != null && map.catalogIndex < scenarios.Count ? scenarios[map.catalogIndex] : null;

                var go = UIBuilderUtil.Child(parent, "Totem_" + map.objectName.Replace("DC_PANEL_", string.Empty));
                var kiosk = go.AddComponent<HubKiosk>();
                kiosk.scenario = scenario;

                CreateKioskCanvas(kiosk, go.transform, hub, map, panel);

                if (!string.IsNullOrEmpty(map.boardTitle))
                    CreateBoardLabel(kiosk, go.transform, panel, map, hidden);

                kiosk.Refresh();
                kiosks.Add(kiosk);
            }

            return kiosks;
        }

        static void CreateKioskCanvas(HubKiosk kiosk, Transform parent, GameObject hub, KioskMap map, Transform panel)
        {
            var art = ScenarioSceneKit.FindChildRecursive(hub.transform, map.objectName + "_Art");
            var artRenderer = art != null ? art.GetComponent<Renderer>() : null;
            var center = artRenderer != null ? artRenderer.bounds.center : panel.position + new Vector3(0f, 2.5f, 0.08f);

            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", parent, new Vector2(440f, 372f), 0.005f);
            canvas.transform.SetPositionAndRotation(center + new Vector3(0f, 0f, 0.03f), k_FacingEntrance);

            var face = canvas.transform;
            UIBuilderUtil.Panel(face, "Background", UIBuilderUtil.PanelColor, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));
            UIBuilderUtil.Panel(face, "HeaderBar", map.accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -74f), new Vector2(-12f, -12f));
            kiosk.titleLabel = UIBuilderUtil.TopText(face, "Title", "Cenário", 28f, Color.white, TextAlignmentOptions.Center, 24f, 42f, 26f, FontStyles.Bold);
            kiosk.descriptionLabel = UIBuilderUtil.TopText(face, "Description", string.Empty, 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.TopLeft, 92f, 142f, 32f);
            kiosk.statusLabel = UIBuilderUtil.TopText(face, "Status", string.Empty, 16f, UIBuilderUtil.AccentGreen, TextAlignmentOptions.Center, 240f, 26f, 26f);
            kiosk.playButton = UIBuilderUtil.BottomButton(face, "Play", "Jogar", 24f, map.accent, 26f, 62f, 0.14f, 0.86f, 0f, out var playLabel);
            kiosk.playLabel = playLabel;
        }

        static void CreateBoardLabel(HubKiosk kiosk, Transform parent, Transform panel, KioskMap map, Material hidden)
        {
            var renderer = panel.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var materials = renderer.sharedMaterials;
                var changed = false;
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null || materials[i].name != "DC_M_White")
                        continue;
                    materials[i] = hidden;
                    changed = true;
                }
                if (changed)
                    renderer.sharedMaterials = materials;
                else
                    Debug.LogWarning($"[SafeZone VR] Texto gravado do totem {map.objectName} não encontrado (submalha DC_M_White).");
            }

            var label = UIBuilderUtil.WorldText(parent, "Placa", map.boardTitle, k_BoardFontSize, new Color(0.93f, 0.91f, 0.87f),
                panel.position + new Vector3(0f, 0.84f, 0.13f), new Vector3(0f, 180f, 0f), new Vector2(1.7f, 0.5f));
            label.fontStyle = FontStyles.Bold;
            label.characterSpacing = 6f;
            kiosk.boardLabel = label;
            kiosk.boardTitle = map.boardTitle;
            kiosk.boardSubtitle = map.boardSubtitle;
        }

        static Button CreateDiscButton(Transform parent, GameObject hub, string objectName, string name)
        {
            var disc = ScenarioSceneKit.FindChildRecursive(hub.transform, objectName);
            if (disc == null)
            {
                Debug.LogWarning($"[SafeZone VR] Botão {objectName} não existe no modelo do hub.");
                return null;
            }

            var bounds = disc.GetComponent<Renderer>().bounds;
            var go = UIBuilderUtil.Child(parent, name);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(118f, 118f), 0.005f);
            canvas.transform.SetPositionAndRotation(new Vector3(bounds.center.x, 0.62f, bounds.max.z + 0.03f), k_FacingEntrance);

            var button = UIBuilderUtil.Button(canvas.transform, "Alvo", string.Empty, 10f, new Color(1f, 1f, 1f, 0.05f), Color.clear,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, out var label);
            label.gameObject.SetActive(false);

            var image = button.targetGraphic as Image;
            if (image != null)
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var colors = button.colors;
            colors.colorMultiplier = 3f;
            colors.normalColor = new Color(1f, 1f, 1f, 0.3f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(1f, 1f, 1f, 0.6f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.3f);
            button.colors = colors;
            return button;
        }
    }
}
#endif
