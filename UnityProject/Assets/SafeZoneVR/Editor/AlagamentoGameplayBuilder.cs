#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using Unity.VRTemplate;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Monta (e remonta, de forma idempotente) toda a camada de gameplay da cena Alagamento_EmCasa
    /// sobre a casa modelada: XR Origin, teleporte, vinheta, disjuntor, registro, itens, sockets,
    /// validadores, UI de pulso, briefing, relatório, água, rota de fuga e ponto de encontro.
    /// Tudo que é gerado fica sob o objeto raiz "SafeZone_Gameplay" (exceto ajustes na casa).
    /// </summary>
    public static class AlagamentoGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_RootName = "SafeZone_Gameplay";
        const string k_XROriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string k_VignettePrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";
        const int k_TeleportLayerBit = 31;

        // Posições medidas na casa modelada (metros, espaço de mundo).
        static readonly Vector3 k_PlayerStart = new Vector3(-33.2f, 0f, 11.2f);
        static readonly Vector3 k_BreakerBox = new Vector3(-29.68f, 1.50f, 6.22f);
        static readonly Vector3 k_ValveCenter = new Vector3(-28.80f, 0.30f, 14.68f);
        static readonly Vector3 k_SofaDocs = new Vector3(-31.57f, 0.93f, 11.55f);
        static readonly Vector3 k_SofaPhoto = new Vector3(-31.57f, 0.92f, 12.45f);
        static readonly Vector3 k_ShelfSocketA = new Vector3(-32.2f, 1.92f, 6.47f);
        static readonly Vector3 k_ShelfSocketB = new Vector3(-31.0f, 1.92f, 6.47f);
        static readonly Vector3 k_KitchenWater = new Vector3(-35.45f, 1.02f, 10.2f);
        // Tampa da caixa acoplada do vaso (plana, y 0.98): a pia é de coluna, sem bancada, e o item caía na cuba.
        static readonly Vector3 k_BathroomMeds = new Vector3(-35.70f, 1.03f, 6.60f);
        static readonly Vector3 k_BedFlashlight = new Vector3(-29.6f, 0.86f, 8.9f);
        // Mochila sobre um banco perto da porta: na altura da mão (conforto sentado/em pé) e acima da água.
        static readonly Vector3 k_BackpackBench = new Vector3(-34.3f, 0.225f, 14.35f);
        static readonly Vector3 k_Backpack = new Vector3(-34.3f, 0.67f, 14.35f);
        static readonly Vector3 k_MeetingPoint = new Vector3(-33.0f, 0.15f, 23.0f);
        static readonly Vector3 k_KombiDoor = new Vector3(-26.8f, 1.0f, 15.75f);

        static Material s_MatWater, s_MatMarker, s_MatSocket, s_MatFade, s_MatChevron, s_MatStreet, s_MatPlatform,
            s_MatSign, s_MatDocs, s_MatPhoto, s_MatBottle, s_MatMeds, s_MatFlashlight, s_MatBackpack, s_MatIndicator, s_MatHint, s_MatLever;

        [MenuItem("SafeZone VR/Build/2. Fase Alagamento (montar gameplay na casa)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Gameplay da fase Alagamento montado e cena salva.", "OK");
        }

        public static void Build()
        {
            var scene = OpenScene();
            var data = SafeZoneDataBuilder.Build();
            CreateMaterials();

            CleanupLegacy(scene);

            var origin = SetupXROrigin(scene);
            var camera = origin.Camera.transform;
            var leftHand = FindChildRecursive(origin.transform, "Left Controller");

            var root = new GameObject(k_RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            // Ponto de início: alinha a cabeça do jogador aqui ao carregar e o devolve se cair para fora do mundo.
            var spawn = UIBuilderUtil.Child(root.transform, "PlayerSpawn");
            spawn.transform.SetPositionAndRotation(k_PlayerStart, Quaternion.identity);
            spawn.AddComponent<PlayerSpawnPoint>();

            // ---- Núcleo
            var managerGo = UIBuilderUtil.Child(root.transform, "ScenarioManager");
            var manager = managerGo.AddComponent<ScenarioManager>();
            manager.scenario = data.scenario;
            manager.autoStart = false;

            var systems = UIBuilderUtil.Child(root.transform, "Systems");
            var feedback = systems.AddComponent<FeedbackPlayer>();
            feedback.scenarioManager = manager;
            var comfort = systems.AddComponent<ComfortSettingsApplier>();
            comfort.origin = origin;
            comfort.vignette = origin.GetComponentInChildren<TunnelingVignetteController>(true);

            var beaconGo = UIBuilderUtil.Child(root.transform, "ObjectiveBeacons");
            var beacons = beaconGo.AddComponent<ObjectiveBeaconSystem>();
            beacons.scenarioManager = manager;
            beacons.markerTemplate = CreateMarkerTemplate(beaconGo.transform);

            // ---- Ambiente extra: água, rua, ponto de encontro, rota
            var env = UIBuilderUtil.Child(root.transform, "Environment");
            SetupTeleportFloors();
            AddFurnitureColliders();
            MarkHouseStatic();
            var water = CreateWater(env.transform, manager);
            CreateStreet(env.transform);
            var evacZone = CreateMeetingPoint(env.transform, out var routeGroup);
            CreateRouteChevrons(routeGroup.transform);

            // ---- Interagíveis
            var interactables = UIBuilderUtil.Child(root.transform, "Interactables");
            var lever = SetupBreaker(interactables.transform, data.stepPower);
            var knob = SetupValve(interactables.transform, data.stepWater);

            var docs = CreateItem(interactables.transform, "Item_Documentos", PrimitiveType.Cube, k_SofaDocs, new Vector3(0f, 90f, 0f),
                new Vector3(0.24f, 0.03f, 0.32f), s_MatDocs, "documentos", "Documentos", data.stepValuables);
            var photo = CreateItem(interactables.transform, "Item_PortaRetrato", PrimitiveType.Cube, k_SofaPhoto, new Vector3(0f, 90f, 0f),
                new Vector3(0.16f, 0.02f, 0.20f), s_MatPhoto, "porta_retrato", "Porta-retrato", data.stepValuables);
            var bottle = CreateItem(interactables.transform, "Item_Agua", PrimitiveType.Cylinder, k_KitchenWater, Vector3.zero,
                new Vector3(0.07f, 0.11f, 0.07f), s_MatBottle, "agua", "Água potável", data.stepKit);
            var meds = CreateItem(interactables.transform, "Item_Remedios", PrimitiveType.Cube, k_BathroomMeds, Vector3.zero,
                new Vector3(0.10f, 0.05f, 0.06f), s_MatMeds, "remedios", "Remédios", data.stepKit);
            var flashlight = CreateItem(interactables.transform, "Item_Lanterna", PrimitiveType.Cylinder, k_BedFlashlight, new Vector3(0f, 0f, 90f),
                new Vector3(0.045f, 0.09f, 0.045f), s_MatFlashlight, "lanterna", "Lanterna", data.stepKit);

            var shelfTop = UIBuilderUtil.Child(interactables.transform, "ArmarioTopo");
            shelfTop.transform.position = (k_ShelfSocketA + k_ShelfSocketB) * 0.5f;
            var shelfTarget = shelfTop.AddComponent<ObjectiveTarget>();
            shelfTarget.step = data.stepValuables;
            shelfTarget.markerHeightOffset = 0.3f;
            UIBuilderUtil.WorldText(shelfTop.transform, "Label", "LOCAL ALTO E SEGURO", 0.9f, UIBuilderUtil.AccentGreen,
                shelfTop.transform.position + new Vector3(0f, 0.12f, 0.25f), new Vector3(0f, 180f, 0f), new Vector2(1.6f, 0.3f));
            var valuableIds = new List<string> { "documentos", "porta_retrato" };
            var shelfSocketA = CreateSocket(shelfTop.transform, "Socket_Valores_A", k_ShelfSocketA, valuableIds, 0.16f);
            var shelfSocketB = CreateSocket(shelfTop.transform, "Socket_Valores_B", k_ShelfSocketB, valuableIds, 0.16f);

            var bench = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Banco_Mochila", interactables.transform, k_BackpackBench, Vector3.zero,
                new Vector3(0.5f, 0.45f, 0.36f), s_MatPhoto);
            UIBuilderUtil.MarkStatic(bench, false);
            var backpack = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Mochila_Kit", interactables.transform, k_Backpack, Vector3.zero,
                new Vector3(0.34f, 0.44f, 0.22f), s_MatBackpack);
            var backpackTarget = backpack.AddComponent<ObjectiveTarget>();
            backpackTarget.step = data.stepKit;
            backpackTarget.markerHeightOffset = 0.35f;
            UIBuilderUtil.WorldText(backpack.transform, "Label", "MOCHILA DE EMERGÊNCIA\n<size=60%>solte os itens perto dela</size>", 0.8f, UIBuilderUtil.AccentGreen,
                k_Backpack + new Vector3(0f, 0.55f, -0.05f), new Vector3(0f, 180f, 0f), new Vector2(1.6f, 0.4f));
            var opening = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Abertura", backpack.transform, k_Backpack + new Vector3(0f, 0.222f, 0f), Vector3.zero,
                Vector3.one, s_MatSocket, false);
            opening.transform.localScale = new Vector3(0.9f, 0.006f, 0.8f); // relativo à escala da mochila
            opening.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Os 3 espaços dividem uma área de captura que envolve mochila + banco com folga (BackpackStorage).
            var kitIds = new List<string> { "agua", "remedios", "lanterna" };
            var zoneCenter = k_Backpack + new Vector3(0f, 0.15f, 0f);
            var zoneSize = new Vector3(0.8f, 0.9f, 0.64f);
            var kitSockets = new List<XRSocketInteractor>
            {
                CreateBackpackSlot(interactables.transform, "Socket_Kit_A", k_Backpack + new Vector3(-0.105f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
                CreateBackpackSlot(interactables.transform, "Socket_Kit_B", k_Backpack + new Vector3(0f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
                CreateBackpackSlot(interactables.transform, "Socket_Kit_C", k_Backpack + new Vector3(0.105f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
            };
            backpack.AddComponent<BackpackStorage>().slots.AddRange(kitSockets);

            CreateKombiWrongAction(interactables.transform);

            // ---- Validadores
            var validators = UIBuilderUtil.Child(root.transform, "Validators");

            var vValuables = UIBuilderUtil.Child(validators.transform, "Validator_01_Valores").AddComponent<StepCompleteOnSocket>();
            vValuables.scenarioManager = manager;
            vValuables.step = data.stepValuables;
            vValuables.sockets.Add(shelfSocketA);
            vValuables.sockets.Add(shelfSocketB);
            vValuables.requiredItemIds.AddRange(valuableIds);

            var vPower = UIBuilderUtil.Child(validators.transform, "Validator_02_Energia").AddComponent<StepCompleteOnBreaker>();
            vPower.scenarioManager = manager;
            vPower.step = data.stepPower;
            vPower.lever = lever;

            var vWater = UIBuilderUtil.Child(validators.transform, "Validator_03_Registro").AddComponent<StepCompleteOnKnob>();
            vWater.scenarioManager = manager;
            vWater.step = data.stepWater;
            vWater.knob = knob;
            vWater.requiredDelta = 0.25f;

            var vKit = UIBuilderUtil.Child(validators.transform, "Validator_04_Kit").AddComponent<StepCompleteOnSocket>();
            vKit.scenarioManager = manager;
            vKit.step = data.stepKit;
            vKit.sockets.AddRange(kitSockets);
            vKit.requiredItemIds.AddRange(kitIds);

            var vEvac = evacZone.AddComponent<StepCompleteOnTriggerZone>();
            vEvac.scenarioManager = manager;
            vEvac.step = data.stepEvacuate;
            var evacTarget = evacZone.AddComponent<ObjectiveTarget>();
            evacTarget.step = data.stepEvacuate;
            evacTarget.markerHeightOffset = 0.6f;
            evacTarget.showWhileActive.Add(routeGroup);

            // ---- UI
            var ui = UIBuilderUtil.Child(root.transform, "UI");
            var options = CreateOptionsPanel(ui.transform, camera, true);
            CreateWristUI(leftHand != null ? leftHand : camera, manager, water, options);
            CreateIntroPanel(ui.transform, camera, manager);
            CreateEndReportPanel(ui.transform, camera, manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Gameplay do Alagamento montado e cena salva.");
        }

        // ------------------------------------------------------------------ Scene / cleanup

        static Scene OpenScene()
        {
            var active = SceneManager.GetActiveScene();
            if (active.path == k_ScenePath)
                return active;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new System.Exception("Operação cancelada pelo usuário.");
            return EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
        }

        static void CleanupLegacy(Scene scene)
        {
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (rootGo == null) continue;
                switch (rootGo.name)
                {
                    case k_RootName:
                    case "EvacuationTriggerZone":
                    case "Validator_Valuables":
                    case "Validator_PowerOff":
                    case "Validator_WaterValve":
                    case "Validator_Kit":
                    case "ScenarioManager":
                    case "Disjuntor":
                        Object.DestroyImmediate(rootGo);
                        continue;
                }

                if (rootGo.name == "Fase_AlagamentoEmCasa")
                {
                    var toDelete = new List<GameObject>();
                    foreach (Transform c in rootGo.transform)
                    {
                        if (c.name == "Casa" || c.name.StartsWith("Validator_") || c.name == "EvacuationTriggerZone" || c.name == "ScenarioManager")
                            toDelete.Add(c.gameObject);
                    }
                    foreach (var go in toDelete)
                        Object.DestroyImmediate(go);
                }
            }
        }

        static XROrigin SetupXROrigin(Scene scene)
        {
            var origin = Object.FindAnyObjectByType<XROrigin>();
            if (origin == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_XROriginPrefab);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                origin = instance.GetComponent<XROrigin>();
            }

            origin.transform.SetPositionAndRotation(k_PlayerStart, Quaternion.identity);
            origin.tag = "Player";

            var cam = origin.Camera;
            if (cam.GetComponentInChildren<TunnelingVignetteController>(true) == null)
            {
                var vignettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_VignettePrefab);
                if (vignettePrefab != null)
                {
                    var v = (GameObject)PrefabUtility.InstantiatePrefab(vignettePrefab, cam.transform);
                    v.transform.localPosition = Vector3.zero;
                    v.transform.localRotation = Quaternion.identity;
                    var controller = v.GetComponent<TunnelingVignetteController>();
                    var providers = controller.locomotionVignetteProviders;
                    providers.Clear();
                    AddVignetteProvider(providers, origin.GetComponentInChildren<ContinuousMoveProvider>(true));
                    AddVignetteProvider(providers, origin.GetComponentInChildren<ContinuousTurnProvider>(true));
                    AddVignetteProvider(providers, origin.GetComponentInChildren<SnapTurnProvider>(true));
                    AddVignetteProvider(providers, origin.GetComponentInChildren<TeleportationProvider>(true));
                    EditorUtility.SetDirty(controller);
                }
            }

            if (cam.GetComponent<ScreenFader>() == null)
            {
                var fader = cam.gameObject.AddComponent<ScreenFader>();
                fader.fadeMaterial = s_MatFade;
            }

            return origin;
        }

        static void AddVignetteProvider(List<LocomotionVignetteProvider> list, LocomotionProvider provider)
        {
            if (provider == null) return;
            list.Add(new LocomotionVignetteProvider { locomotionProvider = provider, enabled = true });
        }

        static Transform FindChildRecursive(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform c in root)
            {
                var r = FindChildRecursive(c, name);
                if (r != null) return r;
            }
            return null;
        }

        // ------------------------------------------------------------------ Materials

        static void CreateMaterials()
        {
            s_MatWater = UIBuilderUtil.GetOrCreateMaterial("Mat_Agua", new Color(0.18f, 0.42f, 0.62f, 0.55f), true, false, 0.9f);
            s_MatMarker = UIBuilderUtil.GetOrCreateMaterial("Mat_Marcador", new Color(1f, 0.85f, 0.15f, 0.9f), true, true);
            s_MatSocket = UIBuilderUtil.GetOrCreateMaterial("Mat_Socket", new Color(0.2f, 1f, 0.45f, 0.4f), true, true);
            s_MatFade = UIBuilderUtil.GetOrCreateMaterial("Mat_Fade", new Color(0f, 0f, 0f, 1f), true, true, 0f, 4000);
            s_MatChevron = UIBuilderUtil.GetOrCreateMaterial("Mat_Seta", new Color(0.15f, 0.95f, 0.35f, 0.95f), true, true);
            s_MatStreet = UIBuilderUtil.GetOrCreateMaterial("Mat_Rua", new Color(0.28f, 0.28f, 0.3f), false, false, 0.1f);
            s_MatPlatform = UIBuilderUtil.GetOrCreateMaterial("Mat_PontoEncontro", new Color(0.22f, 0.6f, 0.32f), false, false, 0.2f);
            s_MatSign = UIBuilderUtil.GetOrCreateMaterial("Mat_Placa", new Color(0.05f, 0.45f, 0.2f), false, false, 0.3f);
            s_MatDocs = UIBuilderUtil.GetOrCreateMaterial("Mat_Documentos", new Color(0.88f, 0.78f, 0.52f), false, false, 0.1f);
            s_MatPhoto = UIBuilderUtil.GetOrCreateMaterial("Mat_PortaRetrato", new Color(0.42f, 0.26f, 0.15f), false, false, 0.4f);
            s_MatBottle = UIBuilderUtil.GetOrCreateMaterial("Mat_Garrafa", new Color(0.35f, 0.65f, 0.95f, 0.75f), true, false, 0.85f);
            s_MatMeds = UIBuilderUtil.GetOrCreateMaterial("Mat_Remedios", new Color(0.95f, 0.95f, 0.97f), false, false, 0.3f);
            s_MatFlashlight = UIBuilderUtil.GetOrCreateMaterial("Mat_Lanterna", new Color(0.2f, 0.2f, 0.22f), false, false, 0.6f);
            s_MatBackpack = UIBuilderUtil.GetOrCreateMaterial("Mat_Mochila", new Color(0.16f, 0.36f, 0.22f), false, false, 0.15f);
            s_MatIndicator = UIBuilderUtil.GetOrCreateMaterial("Mat_Indicador", Color.white, false, true);
            s_MatHint = UIBuilderUtil.GetOrCreateMaterial("Mat_Dica", new Color(1f, 0.55f, 0.2f, 0.25f), true, true);
            s_MatLever = AssetDatabase.LoadAssetAtPath<Material>("Assets/SafeZoneVR/Materials/Mat_DisjuntorAlavanca.mat");
            if (s_MatLever == null)
                s_MatLever = UIBuilderUtil.GetOrCreateMaterial("Mat_Alavanca", new Color(0.85f, 0.1f, 0.1f), false, false, 0.5f);
            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------------ Environment

        static void SetupTeleportFloors()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                // Pisos da casa, garagem e gramados (os gramados ficam na mesma altura do terreno e
                // bloqueariam o raio de teleporte se não fossem áreas válidas).
                if (rootGo.name == "ChaoCasa" || rootGo.name == "Plane" || rootGo.name.StartsWith("Grama"))
                    AddTeleportArea(rootGo);
            }
            var terreno = GameObject.Find("Terreno");
            if (terreno != null)
                AddTeleportArea(terreno);
        }

        static TeleportationArea AddTeleportArea(GameObject go)
        {
            var area = go.GetComponent<TeleportationArea>();
            if (area == null)
                area = go.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << k_TeleportLayerBit;
            area.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            return area;
        }

        const int k_MaxMeshColliderTris = 2000;

        /// <summary>
        /// Colisor estático direto em cada peça de móvel, porta e acessório da casa (sem Rigidbody: nada ali se
        /// move, e é o mais barato no Quest). Jogador (CharacterController), itens pegáveis e raio de teleporte
        /// passam a respeitar os objetos. Peças leves usam MeshCollider (formato fiel: portas abertas não fecham o
        /// vão e itens apoiam na superfície real); peças pesadas, como a Kombi, usam caixa.
        /// </summary>
        static void AddFurnitureColliders()
        {
            var names = new HashSet<string>
            {
                "Sofa", "Tapete", "Estante", "TV", "PS5", "AltoFalantes", "Livros", "Geladeira", "Balcao", "Fogao", "Microondas",
                "LavaLoucas", "Chuveiro", "Pia", "Privada", "Toalheiro", "Papel", "Espelho", "Cama", "Armario",
                "PortaQuarto", "PortaFrente", "PortaBanheiro", "Kombi"
            };
            var scene = SceneManager.GetActiveScene();
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (!names.Contains(rootGo.name)) continue;
                foreach (var mf in rootGo.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null || mf.GetComponent<Collider>() != null) continue;
                    AddPieceCollider(mf);
                }

                // O tapete fica sobre o piso: sem isto o colisor dele bloquearia o teleporte na sala.
                if (rootGo.name == "Tapete")
                    AddTeleportArea(rootGo);
            }
        }

        static void AddPieceCollider(MeshFilter mf)
        {
            var mesh = mf.sharedMesh;
            var tris = 0L;
            for (var i = 0; i < mesh.subMeshCount; i++)
                tris += mesh.GetIndexCount(i) / 3;

            if (tris <= k_MaxMeshColliderTris)
            {
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                mc.convex = false;
                return;
            }

            var box = mf.gameObject.AddComponent<BoxCollider>();
            box.center = mesh.bounds.center;
            box.size = mesh.bounds.size;
        }

        static void MarkHouseStatic()
        {
            var prefixes = new[] { "ParedeGaragem", "PilarMuro", "Grama", "Muro", "Telhado", "Teto", "ChaoCasa", "Plane" };
            var scene = SceneManager.GetActiveScene();
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                foreach (var p in prefixes)
                {
                    if (rootGo.name.StartsWith(p))
                    {
                        UIBuilderUtil.MarkStatic(rootGo);
                        break;
                    }
                }
                if (rootGo.name == "Fase_AlagamentoEmCasa")
                    UIBuilderUtil.MarkStatic(rootGo);
            }
        }

        static FloodWaterController CreateWater(Transform parent, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "Flood");
            var surface = UIBuilderUtil.Primitive(PrimitiveType.Plane, "WaterSurface", go.transform, new Vector3(-31f, -0.04f, 14f), Vector3.zero,
                new Vector3(1.5f, 1f, 2.6f), s_MatWater, false);
            var r = surface.GetComponent<MeshRenderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;

            var ctrl = go.AddComponent<FloodWaterController>();
            ctrl.scenarioManager = manager;
            ctrl.waterSurface = surface.transform;
            ctrl.startLevel = -0.04f;
            ctrl.maxLevel = 0.32f;
            ctrl.riseDuration = 360f;
            return ctrl;
        }

        static void CreateStreet(Transform parent)
        {
            var street = UIBuilderUtil.Primitive(PrimitiveType.Plane, "Rua", parent, new Vector3(-31f, -0.005f, 22.5f), Vector3.zero,
                new Vector3(1.6f, 1f, 0.9f), s_MatStreet);
            AddTeleportArea(street);
            UIBuilderUtil.MarkStatic(street);

            var curb = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Calcada", parent, new Vector3(-31f, 0.04f, 18.6f), Vector3.zero,
                new Vector3(16f, 0.08f, 1.0f), s_MatStreet);
            AddTeleportArea(curb);
            UIBuilderUtil.MarkStatic(curb);
        }

        static GameObject CreateMeetingPoint(Transform parent, out GameObject routeGroup)
        {
            var group = UIBuilderUtil.Child(parent, "PontoDeEncontro");

            var platform = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Plataforma", group.transform, k_MeetingPoint, Vector3.zero,
                new Vector3(3f, 0.3f, 3f), s_MatPlatform);
            AddTeleportArea(platform);
            UIBuilderUtil.MarkStatic(platform);

            var signPos = k_MeetingPoint + new Vector3(1.8f, 0f, 0.4f);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", group.transform, signPos + new Vector3(0f, 0.9f, 0f), Vector3.zero,
                new Vector3(0.06f, 0.9f, 0.06f), s_MatFlashlight);
            var board = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", group.transform, signPos + new Vector3(0f, 1.95f, 0f), Vector3.zero,
                new Vector3(1.1f, 0.55f, 0.04f), s_MatSign);
            // O jogador chega pelo sul (z menor) olhando para +Z: texto com rotação zero fica legível.
            UIBuilderUtil.WorldText(board.transform, "Texto", "PONTO DE ENCONTRO\n<size=60%>local alto e seguro</size>", 1.6f, Color.white,
                signPos + new Vector3(0f, 1.95f, -0.03f), Vector3.zero, new Vector2(1.05f, 0.5f));

            var zone = UIBuilderUtil.Child(group.transform, "EvacuationZone");
            zone.transform.position = k_MeetingPoint + new Vector3(0f, 1.2f, 0f);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(3f, 2.4f, 3f);

            routeGroup = UIBuilderUtil.Child(group.transform, "RotaDeFuga");
            return zone;
        }

        static void CreateRouteChevrons(Transform parent)
        {
            var points = new[]
            {
                new Vector3(-32.4f, 0.03f, 13.6f),
                new Vector3(-32.4f, 0.03f, 15.7f),
                new Vector3(-32.7f, 0.03f, 16.9f),
                new Vector3(-33.0f, 0.03f, 18.7f),
                new Vector3(-33.0f, 0.03f, 20.3f),
                new Vector3(-33.0f, 0.03f, 21.4f),
            };
            for (var i = 0; i < points.Length; i++)
            {
                var chevron = UIBuilderUtil.Child(parent, "Seta_" + (i + 1));
                chevron.transform.position = points[i];
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "L", chevron.transform, points[i] + new Vector3(-0.12f, 0f, -0.1f), new Vector3(0f, -45f, 0f),
                    new Vector3(0.06f, 0.01f, 0.36f), s_MatChevron, false);
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "R", chevron.transform, points[i] + new Vector3(0.12f, 0f, -0.1f), new Vector3(0f, 45f, 0f),
                    new Vector3(0.06f, 0.01f, 0.36f), s_MatChevron, false);
            }
            parent.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Interactables

        static BreakerLever SetupBreaker(Transform parent, MissionStepSO step)
        {
            var box = GameObject.Find("Disjuntor_PowerOff");
            if (box == null)
            {
                box = UIBuilderUtil.Child(parent, "Disjuntor_PowerOff");
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Caixa", box.transform, k_BreakerBox, Vector3.zero, new Vector3(0.26f, 0.34f, 0.10f), s_MatFlashlight);
            }

            // Reposiciona a raiz sobre a caixa mantendo os filhos existentes no lugar.
            var keep = new List<(Transform t, Vector3 pos, Quaternion rot)>();
            foreach (Transform c in box.transform)
                keep.Add((c, c.position, c.rotation));
            box.transform.SetPositionAndRotation(k_BreakerBox, Quaternion.identity);
            foreach (var k in keep)
                k.t.SetPositionAndRotation(k.pos, k.rot);

            // Idempotente: remove o que este builder criou em execuções anteriores.
            foreach (var childName in new[] { "Alavanca_Pivot", "Indicador", "Label" })
            {
                var previous = box.transform.Find(childName);
                if (previous != null)
                    Object.DestroyImmediate(previous.gameObject);
            }
            var pivotGo = UIBuilderUtil.Child(box.transform, "Alavanca_Pivot");
            pivotGo.transform.SetPositionAndRotation(k_BreakerBox + new Vector3(0f, 0f, 0.085f), Quaternion.identity);

            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Alavanca", pivotGo.transform, pivotGo.transform.position + new Vector3(0f, 0f, 0.035f), Vector3.zero,
                new Vector3(0.035f, 0.025f, 0.075f), s_MatLever, false);

            var col = pivotGo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0f, 0.04f);
            col.size = new Vector3(0.10f, 0.12f, 0.12f);
            pivotGo.AddComponent<XRSimpleInteractable>();

            var indicator = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Indicador", box.transform, k_BreakerBox + new Vector3(0.09f, 0.13f, 0.055f), Vector3.zero,
                new Vector3(0.025f, 0.025f, 0.01f), s_MatIndicator, false);

            var lever = pivotGo.AddComponent<BreakerLever>();
            lever.pivot = pivotGo.transform;
            lever.axis = Vector3.right;
            lever.onAngle = 35f;
            lever.offAngle = -35f;
            lever.startsOn = true;
            lever.indicatorRenderer = indicator.GetComponent<Renderer>();

            UIBuilderUtil.WorldText(box.transform, "Label", "QUADRO DE ENERGIA", 0.7f, Color.white,
                k_BreakerBox + new Vector3(0f, 0.22f, 0.06f), new Vector3(0f, 180f, 0f), new Vector2(0.6f, 0.12f));

            var target = pivotGo.AddComponent<ObjectiveTarget>();
            target.step = step;
            target.markerHeightOffset = 0.2f;
            return lever;
        }

        static XRKnob SetupValve(Transform parent, MissionStepSO step)
        {
            var existing = GameObject.Find("Registro_Knob");
            XRKnob knob;
            if (existing != null)
            {
                knob = existing.GetComponent<XRKnob>();
                if (existing.GetComponent<ObjectiveTarget>() == null)
                {
                    var t = existing.AddComponent<ObjectiveTarget>();
                    t.step = step;
                }
                return knob;
            }

            var knobGo = UIBuilderUtil.Child(parent, "Registro_Knob");
            // Eixo Y local do knob = eixo X do mundo (o volante do registro é fino em X).
            knobGo.transform.SetPositionAndRotation(k_ValveCenter, Quaternion.Euler(0f, 0f, -90f));
            var handle = UIBuilderUtil.Child(knobGo.transform, "Handle");
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localRotation = Quaternion.identity;

            var old = GameObject.Find("RegistroGeral");
            if (old != null)
            {
                var oldKnob = old.GetComponent<XRKnob>();
                if (oldKnob != null) Object.DestroyImmediate(oldKnob);
                var children = new List<Transform>();
                foreach (Transform c in old.transform) children.Add(c);
                foreach (var c in children) c.SetParent(handle.transform, true);
                if (old.transform.childCount == 0)
                    Object.DestroyImmediate(old);
            }
            else
            {
                UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Volante", handle.transform, k_ValveCenter, new Vector3(0f, 0f, 90f),
                    new Vector3(0.2f, 0.02f, 0.2f), s_MatLever);
            }

            var sphere = handle.AddComponent<SphereCollider>();
            sphere.radius = 0.14f;

            knob = knobGo.AddComponent<XRKnob>();
            knob.handle = handle.transform;
            knob.clampedMotion = true;
            knob.minAngle = -180f;
            knob.maxAngle = 180f;
            knob.value = 0.5f;

            UIBuilderUtil.WorldText(knobGo.transform, "Label", "REGISTRO DE ÁGUA\n<size=70%>gire para fechar</size>", 0.6f, Color.white,
                k_ValveCenter + new Vector3(0.05f, 0.42f, 0f), new Vector3(0f, -90f, 0f), new Vector2(0.7f, 0.2f));

            var target = knobGo.AddComponent<ObjectiveTarget>();
            target.step = step;
            target.markerHeightOffset = 0.25f;
            return knob;
        }

        static GameObject CreateItem(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 euler, Vector3 scale, Material mat,
            string itemId, string displayName, MissionStepSO step)
        {
            var go = UIBuilderUtil.Primitive(type, name, parent, pos, euler, scale, mat);
            if (type == PrimitiveType.Cylinder)
            {
                // Cápsula/mesh do cilindro: troca por BoxCollider para física estável e barata.
                var c = go.GetComponent<Collider>();
                if (c != null) Object.DestroyImmediate(c);
                var box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(1f, 2f, 1f);
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.4f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var grab = go.AddComponent<XRGrabInteractable>();
            grab.useDynamicAttach = true;
            grab.throwOnDetach = true;
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;

            var id = go.AddComponent<ObjectiveItemId>();
            id.itemId = itemId;
            id.displayName = displayName;

            var target = go.AddComponent<ObjectiveTarget>();
            target.step = step;
            target.markerHeightOffset = 0.18f;

            go.AddComponent<RespawnIfFallen>();

            var label = UIBuilderUtil.WorldText(go.transform, "Label", displayName, 0.5f, Color.white, pos + Vector3.up * 0.12f, Vector3.zero, new Vector2(0.6f, 0.12f));
            var face = label.gameObject.AddComponent<FacePlayer>();
            face.yawOnly = true;
            // Etiqueta não deve herdar a escala do primitivo.
            label.transform.localScale = new Vector3(1f / Mathf.Max(scale.x, 0.001f), 1f / Mathf.Max(scale.y, 0.001f), 1f / Mathf.Max(scale.z, 0.001f));
            return go;
        }

        static XRSocketInteractor CreateSocket(Transform parent, string name, Vector3 pos, List<string> allowedIds, float radius)
        {
            var go = UIBuilderUtil.Child(parent, name);
            go.transform.position = pos;
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            var socket = SetupSocket(go, allowedIds, radius);

            var disc = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Disco", go.transform, pos + new Vector3(0f, -radius * 0.6f, 0f), Vector3.zero,
                new Vector3(radius * 2.2f, 0.004f, radius * 2.2f), s_MatSocket, false);
            var r = disc.GetComponent<MeshRenderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return socket;
        }

        static XRSocketInteractor SetupSocket(GameObject go, List<string> allowedIds, float snapRadius)
        {
            var socket = go.AddComponent<XRSocketInteractor>();
            socket.showInteractableHoverMeshes = true;
            socket.hoverSocketSnapping = false;
            socket.socketSnappingRadius = snapRadius;

            var filter = go.AddComponent<SocketItemFilter>();
            filter.allowedItemIds.AddRange(allowedIds);
            return socket;
        }

        /// <summary>
        /// Espaço na boca da mochila cuja área de captura (caixa grande em <paramref name="zoneCenter"/>) é a mesma
        /// dos outros espaços: basta soltar ou jogar o item perto da mochila, sem mirar num encaixe pequeno.
        /// <see cref="BackpackStorage"/> deixa só um espaço vazio ativo por vez.
        /// </summary>
        static XRSocketInteractor CreateBackpackSlot(Transform parent, string name, Vector3 slotPos, Vector3 zoneCenter, Vector3 zoneSize,
            List<string> allowedIds)
        {
            var go = UIBuilderUtil.Child(parent, name);
            go.transform.position = slotPos;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = go.transform.InverseTransformPoint(zoneCenter);
            box.size = zoneSize;
            return SetupSocket(go, allowedIds, 0.1f);
        }

        static void CreateKombiWrongAction(Transform parent)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Kombi_PortaTraseira", parent, k_KombiDoor, Vector3.zero,
                new Vector3(0.5f, 0.6f, 0.1f), s_MatHint);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "sair_de_carro";
            wrong.message = "Nunca saia de carro em alagamento: a força da água arrasta veículos. Siga a pé até o local alto.";
            UIBuilderUtil.WorldText(parent, "Kombi_Label", "Sair de carro?", 0.7f, new Color(1f, 0.75f, 0.35f),
                k_KombiDoor + new Vector3(0f, 0.85f, 0.15f), Vector3.zero, new Vector2(1.2f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static GameObject CreateMarkerTemplate(Transform parent)
        {
            var template = UIBuilderUtil.Child(parent, "MarkerTemplate");
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Diamond", template.transform, Vector3.zero, new Vector3(45f, 0f, 45f),
                new Vector3(0.09f, 0.09f, 0.09f), s_MatMarker, false);
            var ring = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Ring", template.transform, new Vector3(0f, -0.12f, 0f), Vector3.zero,
                new Vector3(0.22f, 0.003f, 0.22f), s_MatMarker, false);
            foreach (var r in template.GetComponentsInChildren<MeshRenderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            template.SetActive(false);
            return template;
        }

        // ------------------------------------------------------------------ UI

        static void CreateWristUI(Transform hand, ScenarioManager manager, FloodWaterController water, OptionsPanelController options)
        {
            var existing = hand.Find("WristUI");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var wrist = UIBuilderUtil.Child(hand, "WristUI");
            wrist.transform.localPosition = new Vector3(0f, 0.09f, -0.07f);

            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", wrist.transform, new Vector2(320f, 300f), 0.00045f);
            var face = canvas.gameObject.AddComponent<FacePlayer>();
            face.yawOnly = false;
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.Accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), Vector2.zero);

            var ctrl = wrist.AddComponent<WristUIController>();
            ctrl.scenarioManager = manager;
            ctrl.water = water;
            ctrl.optionsPanel = options;
            ctrl.panelRoot = canvas.gameObject;

            UIBuilderUtil.Text(panel, "Brand", "SafeZone VR", 16f, Color.white, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0.6f, 1f), new Vector2(10f, -32f), new Vector2(0f, -2f), FontStyles.Bold);
            ctrl.timerText = UIBuilderUtil.Text(panel, "Timer", "--:--", 16f, Color.white, TextAlignmentOptions.Right,
                new Vector2(0.6f, 1f), new Vector2(1f, 1f), new Vector2(0f, -32f), new Vector2(-10f, -2f), FontStyles.Bold);

            ctrl.progressText = UIBuilderUtil.TopText(panel, "Progress", "Passo 1 de 5", 13f, UIBuilderUtil.MutedText, TextAlignmentOptions.Left, 40f, 18f, 10f);
            ctrl.titleText = UIBuilderUtil.TopText(panel, "Title", "", 18f, Color.white, TextAlignmentOptions.Left, 58f, 44f, 10f, FontStyles.Bold);
            ctrl.instructionText = UIBuilderUtil.TopText(panel, "Instruction", "", 13f, UIBuilderUtil.TextColor, TextAlignmentOptions.TopLeft, 102f, 62f, 10f);
            ctrl.locationText = UIBuilderUtil.TopText(panel, "Location", "", 12f, UIBuilderUtil.AccentGreen, TextAlignmentOptions.Left, 164f, 18f, 10f);
            ctrl.subProgressText = UIBuilderUtil.Text(panel, "SubProgress", "", 13f, UIBuilderUtil.AccentGreen, TextAlignmentOptions.Right,
                new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -182f), new Vector2(-10f, -164f), FontStyles.Bold);
            ctrl.waterText = UIBuilderUtil.TopText(panel, "Water", "", 12f, new Color(0.55f, 0.8f, 1f), TextAlignmentOptions.Left, 182f, 18f, 10f);
            ctrl.warningText = UIBuilderUtil.TopText(panel, "Warning", "", 12f, new Color(1f, 0.8f, 0.4f), TextAlignmentOptions.TopLeft, 200f, 22f, 10f);

            // Botão largo (toda a largura, ~2,3 cm de altura): fácil de acertar com o raio ou com o dedo (poke).
            ctrl.optionsButton = UIBuilderUtil.BottomButton(panel, "OptionsButton", "Opções  <size=70%>(botão Menu)</size>", 18f, UIBuilderUtil.Accent,
                8f, 52f, 0f, 1f, 8f, out _);

            // Bússola: seta 3D acima do painel apontando para o objetivo atual.
            var compass = UIBuilderUtil.Child(wrist.transform, "Compass");
            compass.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Body", compass.transform, Vector3.zero, Vector3.zero, new Vector3(0.012f, 0.004f, 0.035f), s_MatMarker, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Head", compass.transform, Vector3.zero, new Vector3(0f, 45f, 0f), new Vector3(0.02f, 0.004f, 0.02f), s_MatMarker, false);
            compass.transform.GetChild(1).localPosition = new Vector3(0f, 0f, 0.02f);
            ctrl.compassArrow = compass.transform;
        }

        public static OptionsPanelController CreateOptionsPanel(Transform parent, Transform camera, bool showRestart)
        {
            var go = UIBuilderUtil.Child(parent, "OptionsPanel");
            UIBuilderUtil.AddLazyFollow(go, camera, new Vector3(0f, -0.1f, 1.1f), 4f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(520f, 600f), 0.001f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.Accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -56f), Vector2.zero);
            UIBuilderUtil.TopText(panel, "Title", "Conforto e acessibilidade", 26f, Color.white, TextAlignmentOptions.Center, 8f, 40f, 20f, FontStyles.Bold);

            var ctrl = go.AddComponent<OptionsPanelController>();
            ctrl.root = go;
            ctrl.startHidden = true;

            var y = 76f;
            const float h = 56f;
            const float gap = 10f;
            ctrl.locomotionButton = UIBuilderUtil.TopButton(panel, "Locomotion", "Locomoção", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out var l1); ctrl.locomotionLabel = l1; y += h + gap;
            ctrl.turnButton = UIBuilderUtil.TopButton(panel, "Turn", "Giro", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out var l2); ctrl.turnLabel = l2; y += h + gap;
            ctrl.vignetteButton = UIBuilderUtil.TopButton(panel, "Vignette", "Vinheta", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out var l3); ctrl.vignetteLabel = l3; y += h + gap;
            ctrl.seatedButton = UIBuilderUtil.TopButton(panel, "Seated", "Posição", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out var l4); ctrl.seatedLabel = l4; y += h + gap;
            ctrl.recenterButton = UIBuilderUtil.TopButton(panel, "Recenter", "Recentrar visão", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out _); y += h + gap;
            if (showRestart)
            {
                ctrl.restartButton = UIBuilderUtil.TopButton(panel, "Restart", "Reiniciar fase", 20f, new Color(0.55f, 0.35f, 0.15f), y, h, 24f, out _); y += h + gap;
                ctrl.menuButton = UIBuilderUtil.TopButton(panel, "Menu", "Voltar ao menu", 20f, new Color(0.45f, 0.2f, 0.2f), y, h, 24f, out _); y += h + gap;
            }
            ctrl.closeButton = UIBuilderUtil.BottomButton(panel, "Close", "Fechar", 20f, UIBuilderUtil.Accent, 16f, 50f, 0.25f, 0.75f, 0f, out _);
            return ctrl;
        }

        static void CreateIntroPanel(Transform parent, Transform camera, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "IntroBriefingPanel");
            UIBuilderUtil.AddLazyFollow(go, camera, new Vector3(0f, -0.05f, 1.5f), 3f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(760f, 620f), 0.001f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.Accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), Vector2.zero);

            var ctrl = go.AddComponent<IntroBriefingPanel>();
            ctrl.scenarioManager = manager;
            ctrl.root = go;
            ctrl.titleText = UIBuilderUtil.TopText(panel, "Title", "Alagamento em Casa", 32f, Color.white, TextAlignmentOptions.Center, 12f, 48f, 24f, FontStyles.Bold);
            ctrl.bodyText = UIBuilderUtil.TopText(panel, "Body", "", 22f, UIBuilderUtil.TextColor, TextAlignmentOptions.TopLeft, 90f, 430f, 32f);
            ctrl.introText = "<b>Alerta da Defesa Civil:</b> chuva muito forte na sua região e a água já começou a subir na rua.\n" +
                             "Você está em casa e tem alguns minutos para agir com calma e segurança.\n\n" +
                             "Use o relógio no seu pulso esquerdo para ver a missão atual. Aponte para o chão e aperte o gatilho para se teleportar. " +
                             "Para ajustar o conforto, aperte \"Opções\" no relógio ou o botão Menu do controle esquerdo.";
            ctrl.startButton = UIBuilderUtil.BottomButton(panel, "Start", "Começar", 26f, UIBuilderUtil.AccentGreen, 24f, 64f, 0.25f, 0.75f, 0f, out _);
        }

        static void CreateEndReportPanel(Transform parent, Transform camera, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "EndReportPanel");
            UIBuilderUtil.AddLazyFollow(go, camera, new Vector3(0f, -0.05f, 1.6f), 3f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(860f, 760f), 0.001f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.AccentGreen, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), Vector2.zero);

            var ctrl = go.AddComponent<EndReportPanelController>();
            ctrl.scenarioManager = manager;
            ctrl.root = go;
            ctrl.titleText = UIBuilderUtil.TopText(panel, "Title", "Fase concluída!", 32f, Color.white, TextAlignmentOptions.Center, 12f, 48f, 24f, FontStyles.Bold);

            // Estrelas (losangos) centralizadas.
            for (var i = 0; i < 3; i++)
            {
                var x = 430f + (i - 1) * 70f;
                var star = UIBuilderUtil.Panel(panel, "Star" + (i + 1), new Color(0.35f, 0.35f, 0.4f),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x - 22f, -134f), new Vector2(x + 22f, -90f));
                star.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                ctrl.starImages.Add(star);
            }

            ctrl.summaryText = UIBuilderUtil.TopText(panel, "Summary", "", 20f, Color.white, TextAlignmentOptions.Center, 148f, 60f, 24f, FontStyles.Bold);
            ctrl.detailsText = UIBuilderUtil.TopText(panel, "Details", "", 18f, UIBuilderUtil.TextColor, TextAlignmentOptions.TopLeft, 214f, 170f, 32f);

            UIBuilderUtil.Panel(panel, "CardBackground", UIBuilderUtil.PanelLight, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -610f), new Vector2(-24f, -392f));
            UIBuilderUtil.TopText(panel, "CardHeader", "Orientações da Defesa Civil", 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.Left, 398f, 22f, 40f);
            ctrl.cardTitleText = UIBuilderUtil.TopText(panel, "CardTitle", "", 22f, UIBuilderUtil.AccentGreen, TextAlignmentOptions.Left, 422f, 34f, 40f, FontStyles.Bold);
            ctrl.cardBodyText = UIBuilderUtil.TopText(panel, "CardBody", "", 18f, Color.white, TextAlignmentOptions.TopLeft, 458f, 108f, 40f);
            ctrl.cardCounterText = UIBuilderUtil.TopText(panel, "CardCounter", "", 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.Center, 572f, 24f, 40f);
            ctrl.prevCardButton = UIBuilderUtil.Button(panel, "PrevCard", "<", 24f, UIBuilderUtil.Accent, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -596f), new Vector2(92f, -548f), out _);
            ctrl.nextCardButton = UIBuilderUtil.Button(panel, "NextCard", ">", 24f, UIBuilderUtil.Accent, Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -596f), new Vector2(-36f, -548f), out _);

            ctrl.replayButton = UIBuilderUtil.BottomButton(panel, "Replay", "Jogar de novo", 22f, UIBuilderUtil.AccentGreen, 24f, 60f, 0.06f, 0.48f, 8f, out _);
            ctrl.menuButton = UIBuilderUtil.BottomButton(panel, "Menu", "Menu principal", 22f, UIBuilderUtil.Accent, 24f, 60f, 0.52f, 0.94f, 8f, out _);
        }
    }
}
#endif
