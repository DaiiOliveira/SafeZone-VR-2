#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using Unity.VRTemplate;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    /// Helpers compartilhados pelos builders de fase (Alagamento, Incêndio, Granizo): XR Origin, materiais,
    /// casa reaproveitada (prefab), teleporte, colisores, interagíveis genéricos, UI de pulso, painéis e NPCs.
    /// Comportamento idêntico ao que vivia no AlagamentoGameplayBuilder.
    /// </summary>
    public static class ScenarioSceneKit
    {
        public const string k_RootName = "SafeZone_Gameplay";
        public const string k_XROriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        public const string k_VignettePrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";
        public const string k_CasaModelPath = "Assets/Casa/DC_Casa.fbx";
        public const string k_PracaModelPath = "Assets/Praca/DC_Praca.fbx";
        public const string k_HouseRootName = "DC_Casa";
        public const string k_PracaRootName = "DC_Praca";
        public const int k_TeleportLayerBit = 31;
        public const int k_MaxMeshColliderTris = 2000;

        public static Material MatWater, MatMarker, MatSocket, MatFade, MatChevron, MatStreet, MatPlatform,
            MatSign, MatDocs, MatPhoto, MatBottle, MatMeds, MatFlashlight, MatBackpack, MatIndicator, MatHint, MatLever,
            MatSkin, MatCloth, MatMetal, MatWood, MatConcrete, MatIce, MatFlame, MatSmoke, MatGlass, MatFire;

        // ------------------------------------------------------------------ Scene

        public static Scene OpenOrCreateScene(string scenePath)
        {
            var active = SceneManager.GetActiveScene();
            if (active.path == scenePath)
                return active;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new System.Exception("Operação cancelada pelo usuário.");
            if (System.IO.File.Exists(scenePath))
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            UIBuilderUtil.EnsureFolder(System.IO.Path.GetDirectoryName(scenePath).Replace('\\', '/'));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
            return scene;
        }

        public static void DestroyRoot(Scene scene, string name)
        {
            foreach (var rootGo in scene.GetRootGameObjects())
                if (rootGo != null && rootGo.name == name)
                    Object.DestroyImmediate(rootGo);
        }

        public static GameObject CreateRoot(Scene scene)
        {
            var root = new GameObject(k_RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        public static void SetupDirectionalLight(Scene scene, Color color, float intensity, Vector3 euler)
        {
            Light light = null;
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                var l = rootGo.GetComponent<Light>();
                if (l != null && l.type == LightType.Directional) { light = l; break; }
            }
            if (light == null)
            {
                var go = new GameObject("Directional Light");
                SceneManager.MoveGameObjectToScene(go, scene);
                light = go.AddComponent<Light>();
                light.type = LightType.Directional;
            }
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(euler);
        }

        // ------------------------------------------------------------------ Modelos (casa e praça)

        // Os FBX exportados do Blender chegam deitados (altura no eixo Z). Girando a raiz, casa e praça ficam de pé
        // e continuam no mesmo referencial: as duas na origem já se encaixam (praça do outro lado da rua).
        public static readonly Quaternion k_ModelUpright = Quaternion.Euler(-90f, 0f, 0f);

        // Raízes da casa antiga (prefab Casa_SafeZone e sobras da cena original do Alagamento).
        static readonly string[] k_LegacyHouseRoots = { "Casa_SafeZone", "Disjuntor_PowerOff", "Fase_AlagamentoEmCasa" };

        /// <summary>Casa DC_Casa recriada do zero na cena (desempacotada, para os builders mexerem em portas e peças).</summary>
        public static GameObject EnsureHouseInstance(Scene scene)
        {
            // O nome pode vir com sufixo ("Casa_SafeZone (Missing Prefab with guid: ...)") quando o prefab antigo já não existe.
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (rootGo == null) continue;
                foreach (var legacy in k_LegacyHouseRoots)
                {
                    if (!rootGo.name.StartsWith(legacy)) continue;
                    Object.DestroyImmediate(rootGo);
                    break;
                }
            }
            var house = InstantiateModel(scene, k_CasaModelPath, k_HouseRootName);
            DimFalseEmission(house);
            return house;
        }

        /// <summary>Praça DC_Praca recriada do zero na cena, no mesmo referencial da casa.</summary>
        public static GameObject EnsurePracaInstance(Scene scene)
        {
            return InstantiateModel(scene, k_PracaModelPath, k_PracaRootName);
        }

        static GameObject InstantiateModel(Scene scene, string path, string rootName)
        {
            DestroyRoot(scene, rootName);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
                throw new System.Exception($"Modelo não encontrado em {path}.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
            // Desempacotado: portas ganham pivôs de dobradiça e peças são escondidas; a cena é regerada a cada build.
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = rootName;
            instance.transform.SetPositionAndRotation(Vector3.zero, k_ModelUpright);
            return instance;
        }

        // O importador da casa acende qualquer material com "Luz" no nome; o quadro de energia não é luminária.
        static readonly string[] k_NotEmissive = { "QuadroLuz" };

        /// <summary>Apaga a emissão indevida de algumas peças, sem mexer nos materiais do FBX (property block por renderer).</summary>
        static void DimFalseEmission(GameObject model)
        {
            var block = new MaterialPropertyBlock();
            foreach (var r in model.GetComponentsInChildren<Renderer>(true))
            {
                var wrong = false;
                foreach (var m in r.sharedMaterials)
                    if (m != null && ContainsAny(m.name, k_NotEmissive)) { wrong = true; break; }
                if (!wrong) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", Color.black);
                r.SetPropertyBlock(block);
            }
        }

        /// <summary>Peça do modelo pelo nome (null se não existir).</summary>
        public static Transform ModelPart(GameObject model, string name)
        {
            return model != null ? FindChildRecursive(model.transform, name) : null;
        }

        /// <summary>Bounds de mundo de uma peça do modelo (todas as malhas dela e das filhas).</summary>
        public static Bounds PartBounds(Transform part)
        {
            var renderers = part.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(part.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (var r in renderers)
                b.Encapsulate(r.bounds);
            return b;
        }

        /// <summary>Desliga o visual e os colisores de uma peça (ex.: panela do modelo, trocada pela da gameplay).</summary>
        public static void HidePart(GameObject model, string name)
        {
            var part = ModelPart(model, name);
            if (part == null)
                return;
            foreach (var r in part.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            foreach (var c in part.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }

        // ------------------------------------------------------------------ XR Origin

        public static XROrigin SetupXROrigin(Scene scene, Vector3 startPosition, Quaternion startRotation)
        {
            var origin = Object.FindAnyObjectByType<XROrigin>();
            if (origin == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_XROriginPrefab);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                origin = instance.GetComponent<XROrigin>();
            }

            origin.transform.SetPositionAndRotation(startPosition, startRotation);
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
                fader.fadeMaterial = MatFade;
            }

            EnsureXRInteractionManager(scene);
            RemoveStrayCameras(scene, origin);
            return origin;
        }

        /// <summary>Remove a "Main Camera" padrão de cena nova (fora do XR Origin): 2 AudioListeners e Camera.main errado.</summary>
        static void RemoveStrayCameras(Scene scene, XROrigin origin)
        {
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (rootGo == null || rootGo.GetComponentInParent<XROrigin>() != null) continue;
                if (rootGo.GetComponent<Camera>() != null && rootGo != origin.gameObject)
                    Object.DestroyImmediate(rootGo);
            }
        }

        static void EnsureXRInteractionManager(Scene scene)
        {
            if (Object.FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>() == null)
            {
                var go = new GameObject("XR Interaction Manager");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            }
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var go = new GameObject("EventSystem");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                go.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            }
        }

        static void AddVignetteProvider(List<LocomotionVignetteProvider> list, LocomotionProvider provider)
        {
            if (provider == null) return;
            list.Add(new LocomotionVignetteProvider { locomotionProvider = provider, enabled = true });
        }

        public static Transform FindChildRecursive(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform c in root)
            {
                var r = FindChildRecursive(c, name);
                if (r != null) return r;
            }
            return null;
        }

        public static PlayerSpawnPoint CreatePlayerSpawn(Transform parent, Vector3 position, Quaternion rotation)
        {
            var spawn = UIBuilderUtil.Child(parent, "PlayerSpawn");
            spawn.transform.SetPositionAndRotation(position, rotation);
            return spawn.AddComponent<PlayerSpawnPoint>();
        }

        /// <summary>ScenarioManager (autoStart = false), FeedbackPlayer, ComfortSettingsApplier e ObjectiveBeaconSystem.</summary>
        public static ScenarioManager CreateCore(Transform root, XROrigin origin, ScenarioSO scenario, bool rainAmbience, out FeedbackPlayer feedback)
        {
            var managerGo = UIBuilderUtil.Child(root, "ScenarioManager");
            var manager = managerGo.AddComponent<ScenarioManager>();
            manager.scenario = scenario;
            manager.autoStart = false;

            var systems = UIBuilderUtil.Child(root, "Systems");
            feedback = systems.AddComponent<FeedbackPlayer>();
            feedback.scenarioManager = manager;
            feedback.playRainAmbience = rainAmbience;
            var comfort = systems.AddComponent<ComfortSettingsApplier>();
            comfort.origin = origin;
            comfort.vignette = origin.GetComponentInChildren<TunnelingVignetteController>(true);

            var beaconGo = UIBuilderUtil.Child(root, "ObjectiveBeacons");
            var beacons = beaconGo.AddComponent<ObjectiveBeaconSystem>();
            beacons.scenarioManager = manager;
            beacons.markerTemplate = CreateMarkerTemplate(beaconGo.transform);
            return manager;
        }

        // ------------------------------------------------------------------ Materials

        public static void CreateMaterials()
        {
            MatWater = UIBuilderUtil.GetOrCreateMaterial("Mat_Agua", new Color(0.18f, 0.42f, 0.62f, 0.55f), true, false, 0.9f);
            MatMarker = UIBuilderUtil.GetOrCreateMaterial("Mat_Marcador", new Color(1f, 0.85f, 0.15f, 0.9f), true, true);
            MatSocket = UIBuilderUtil.GetOrCreateMaterial("Mat_Socket", new Color(0.2f, 1f, 0.45f, 0.4f), true, true);
            MatFade = UIBuilderUtil.GetOrCreateMaterial("Mat_Fade", new Color(0f, 0f, 0f, 1f), true, true, 0f, 4000);
            MatChevron = UIBuilderUtil.GetOrCreateMaterial("Mat_Seta", new Color(0.15f, 0.95f, 0.35f, 0.95f), true, true);
            MatStreet = UIBuilderUtil.GetOrCreateMaterial("Mat_Rua", new Color(0.28f, 0.28f, 0.3f), false, false, 0.1f);
            MatPlatform = UIBuilderUtil.GetOrCreateMaterial("Mat_PontoEncontro", new Color(0.22f, 0.6f, 0.32f), false, false, 0.2f);
            MatSign = UIBuilderUtil.GetOrCreateMaterial("Mat_Placa", new Color(0.05f, 0.45f, 0.2f), false, false, 0.3f);
            MatDocs = UIBuilderUtil.GetOrCreateMaterial("Mat_Documentos", new Color(0.88f, 0.78f, 0.52f), false, false, 0.1f);
            MatPhoto = UIBuilderUtil.GetOrCreateMaterial("Mat_PortaRetrato", new Color(0.42f, 0.26f, 0.15f), false, false, 0.4f);
            MatBottle = UIBuilderUtil.GetOrCreateMaterial("Mat_Garrafa", new Color(0.35f, 0.65f, 0.95f, 0.75f), true, false, 0.85f);
            MatMeds = UIBuilderUtil.GetOrCreateMaterial("Mat_Remedios", new Color(0.95f, 0.95f, 0.97f), false, false, 0.3f);
            MatFlashlight = UIBuilderUtil.GetOrCreateMaterial("Mat_Lanterna", new Color(0.2f, 0.2f, 0.22f), false, false, 0.6f);
            MatBackpack = UIBuilderUtil.GetOrCreateMaterial("Mat_Mochila", new Color(0.16f, 0.36f, 0.22f), false, false, 0.15f);
            MatIndicator = UIBuilderUtil.GetOrCreateMaterial("Mat_Indicador", Color.white, false, true);
            MatHint = UIBuilderUtil.GetOrCreateMaterial("Mat_Dica", new Color(1f, 0.55f, 0.2f, 0.25f), true, true);
            MatLever = AssetDatabase.LoadAssetAtPath<Material>("Assets/SafeZoneVR/Materials/Mat_DisjuntorAlavanca.mat");
            if (MatLever == null)
                MatLever = UIBuilderUtil.GetOrCreateMaterial("Mat_Alavanca", new Color(0.85f, 0.1f, 0.1f), false, false, 0.5f);

            // Compartilhados pelas fases novas (NPCs, cenário urbano, granizo, fogo).
            MatSkin = UIBuilderUtil.GetOrCreateMaterial("Mat_Pele", new Color(0.86f, 0.66f, 0.52f), false, false, 0.2f);
            MatCloth = UIBuilderUtil.GetOrCreateMaterial("Mat_Roupa", new Color(0.25f, 0.35f, 0.6f), false, false, 0.1f);
            MatMetal = UIBuilderUtil.GetOrCreateMaterial("Mat_Metal", new Color(0.6f, 0.62f, 0.66f), false, false, 0.75f);
            MatWood = UIBuilderUtil.GetOrCreateMaterial("Mat_Madeira", new Color(0.5f, 0.35f, 0.2f), false, false, 0.2f);
            MatConcrete = UIBuilderUtil.GetOrCreateMaterial("Mat_Concreto", new Color(0.78f, 0.77f, 0.74f), false, false, 0.1f);
            MatIce = UIBuilderUtil.GetOrCreateMaterial("Mat_Gelo", new Color(0.92f, 0.95f, 1f, 0f), true, true);
            MatFlame = UIBuilderUtil.GetOrCreateMaterial("Mat_Chama", new Color(1f, 0.55f, 0.15f, 0.85f), true, true);
            MatSmoke = UIBuilderUtil.GetOrCreateMaterial("Mat_Fumaca", new Color(0.35f, 0.35f, 0.37f, 0.45f), true, true);
            MatGlass = UIBuilderUtil.GetOrCreateMaterial("Mat_Vidro", new Color(0.7f, 0.85f, 0.95f, 0.35f), true, false, 0.95f);
            MatFire = UIBuilderUtil.GetOrCreateMaterial("Mat_Fogo", new Color(1f, 0.35f, 0.05f, 0.9f), true, true);
            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------------ Environment

        public static TeleportationArea AddTeleportArea(GameObject go)
        {
            var area = go.GetComponent<TeleportationArea>();
            if (area == null)
                area = go.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << k_TeleportLayerBit;
            area.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            return area;
        }

        // Peças sem colisor: céu e entorno distante, fiação, plantas miúdas e o que fica acima da cabeça.
        static readonly string[] k_NoColliderKeys =
        {
            "Montanha", "Nuvem", "PosteRede", "Fio", "FloresJardim", "Plantas", "Arbusto", "CercaViva", "Forro", "Telhado",
            "Oitao", "Tabeira", "Calha", "Luminaria", "Plafon", "Pendente", "Cortina", "Varal", "LedAereo",
        };

        /// <summary>
        /// Física do modelo (casa ou praça): pisos, tapetes, calçadas, rua e gramas viram MeshCollider + área de teleporte;
        /// paredes, móveis e estruturas ganham colisor estático; árvores só no tronco. Nada disso vem do importador.
        /// </summary>
        public static void SetupModelPhysics(GameObject model)
        {
            foreach (var mf in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null || mf.GetComponent<Collider>() != null)
                    continue;
                if (IsUnder(mf.transform, "DC_GRUPO_Luzes") || ContainsAny(mf.name, k_NoColliderKeys))
                    continue;

                var renderer = mf.GetComponent<Renderer>();
                if (renderer == null)
                    continue;
                var b = renderer.bounds;

                if (mf.name.Contains("Arvore"))
                {
                    AddTrunkCollider(mf.transform, b);
                    continue;
                }

                if (IsFloorLike(b))
                {
                    var floor = mf.gameObject.AddComponent<MeshCollider>();
                    floor.sharedMesh = mf.sharedMesh;
                    AddTeleportArea(mf.gameObject);
                    continue;
                }

                AddPieceCollider(mf, b);
            }
            Physics.SyncTransforms();
        }

        /// <summary>Superfície baixa e fina (piso, tapete, soleira, calçada, rua, grama, degrau): dá para teleportar em cima.</summary>
        static bool IsFloorLike(Bounds b)
        {
            return b.size.y <= 0.35f && b.max.y <= 0.25f && b.size.x * b.size.z >= 0.2f;
        }

        static void AddTrunkCollider(Transform tree, Bounds b)
        {
            var trunk = new GameObject("Tronco_Colisor");
            trunk.transform.SetParent(tree, false);
            trunk.transform.SetPositionAndRotation(new Vector3(b.center.x, b.min.y + 1.2f, b.center.z), Quaternion.identity);
            var capsule = trunk.AddComponent<CapsuleCollider>();
            capsule.radius = 0.22f;
            capsule.height = 2.4f;
        }

        /// <summary>
        /// Peças leves (ou grandes, como paredes com vão de porta) usam MeshCollider; peças pequenas e pesadas, como o
        /// carro, usam caixa.
        /// </summary>
        static void AddPieceCollider(MeshFilter mf, Bounds worldBounds)
        {
            var mesh = mf.sharedMesh;
            var tris = 0L;
            for (var i = 0; i < mesh.subMeshCount; i++)
                tris += mesh.GetIndexCount(i) / 3;

            var small = Mathf.Max(worldBounds.size.x, worldBounds.size.z) < 5f;
            if (tris > k_MaxMeshColliderTris && small)
            {
                var box = mf.gameObject.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
                return;
            }

            var mc = mf.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = false;
        }

        static bool ContainsAny(string name, string[] keys)
        {
            foreach (var k in keys)
                if (name.Contains(k)) return true;
            return false;
        }

        static bool IsUnder(Transform t, string ancestorName)
        {
            for (var p = t; p != null; p = p.parent)
                if (p.name == ancestorName) return true;
            return false;
        }

        /// <summary>Ponto da superfície embaixo de (x, z), por raycast nos colisores já criados. Sem acerto: y = 0.</summary>
        public static Vector3 OnSurface(float x, float z, float fromY = 2.5f, float lift = 0f)
        {
            Physics.SyncTransforms();
            if (Physics.Raycast(new Vector3(x, fromY, z), Vector3.down, out var hit, fromY + 10f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point + Vector3.up * lift;
            return new Vector3(x, lift, z);
        }

        // ------------------------------------------------------------------ Portas do modelo

        /// <summary>
        /// Coloca uma folha (porta, janela, portão) sob um pivô na dobradiça, com rotação de mundo zero:
        /// o eixo Y local do pivô é o vertical do mundo, como o <see cref="ToggleOpening"/> espera.
        /// </summary>
        public static Transform HingePivot(Transform leaf, Vector3 hinge)
        {
            var pivot = new GameObject(leaf.name + "_Dobradica").transform;
            pivot.SetParent(leaf.parent, false);
            pivot.SetPositionAndRotation(hinge, Quaternion.identity);
            leaf.SetParent(pivot, true);
            foreach (var t in pivot.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            return pivot;
        }

        /// <summary>Ângulo (±graus em Y) que leva o centro da folha para o lado <paramref name="towards"/>.</summary>
        public static float SwingAngle(Transform pivot, Vector3 towards, float degrees)
        {
            var offset = PartBounds(pivot).center - pivot.position;
            offset.y = 0f;
            var plus = Quaternion.AngleAxis(degrees, Vector3.up) * offset;
            return Vector3.Dot(plus, towards) >= 0f ? degrees : -degrees;
        }

        /// <summary>Folha de porta do modelo (origem já na dobradiça) aberta para o lado indicado. Devolve o pivô.</summary>
        public static Transform OpenModelDoor(GameObject model, string leafName, Vector3 towards, float degrees = 95f)
        {
            var leaf = ModelPart(model, leafName);
            if (leaf == null)
            {
                Debug.LogWarning($"[SafeZone VR] Porta {leafName} não existe no modelo.");
                return null;
            }
            var pivot = HingePivot(leaf, leaf.position);
            pivot.rotation = Quaternion.AngleAxis(SwingAngle(pivot, towards, degrees), Vector3.up);
            return pivot;
        }

        /// <summary>
        /// Portas internas abertas (cômodos acessíveis) e portão de pedestres aberto para dentro do lote. A porta da frente
        /// ganha pivô na dobradiça e só abre se <paramref name="openFrontDoor"/>; o pivô é devolvido para quem precisar dela.
        /// </summary>
        public static Transform OpenHouseDoors(GameObject house, bool openFrontDoor)
        {
            OpenModelDoor(house, "DC_CASA_Porta_FrenteCorredor0", Vector3.forward);   // corredor → sala
            OpenModelDoor(house, "DC_CASA_Porta_FrenteCorredor1", Vector3.forward);   // corredor → cozinha
            OpenModelDoor(house, "DC_CASA_Porta_CorredorFundo0", Vector3.back);       // quarto de casal
            OpenModelDoor(house, "DC_CASA_Porta_CorredorFundo1", Vector3.back);       // quarto 2
            OpenModelDoor(house, "DC_CASA_Porta_CorredorFundo2", Vector3.back);       // banheiro
            OpenModelDoor(house, "DC_CASA_Porta_Direita1", Vector3.left);             // cozinha → quintal

            var gate = ModelPart(house, "DC_CASA_PortaoPedestre");
            if (gate != null)
            {
                var gb = PartBounds(gate);
                var pivot = HingePivot(gate, new Vector3(gb.min.x, 0f, gb.center.z));
                pivot.rotation = Quaternion.AngleAxis(SwingAngle(pivot, Vector3.back, 100f), Vector3.up);
            }

            var front = ModelPart(house, "DC_CASA_Porta_NichoFundo0");
            if (front == null)
                return null;
            var frontPivot = HingePivot(front, front.position);
            if (openFrontDoor)
                frontPivot.rotation = Quaternion.AngleAxis(SwingAngle(frontPivot, Vector3.back, 95f), Vector3.up);
            return frontPivot;
        }

        public static GameObject CreateTriggerZone(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var zone = UIBuilderUtil.Child(parent, name);
            zone.transform.position = center;
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            return zone;
        }

        public static WrongActionZone CreateWrongZone(Transform parent, string name, Vector3 center, Vector3 size, string mistakeId, string message)
        {
            var zone = CreateTriggerZone(parent, name, center, size);
            var wrong = zone.AddComponent<WrongActionZone>();
            wrong.mistakeId = mistakeId;
            wrong.message = message;
            return wrong;
        }

        public static WrongActionInteractable CreateWrongInteractable(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat,
            string mistakeId, string message, string labelText)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, name, parent, pos, Vector3.zero, scale, mat);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = mistakeId;
            wrong.message = message;
            if (!string.IsNullOrEmpty(labelText))
            {
                var label = UIBuilderUtil.WorldText(parent, name + "_Label", labelText, 0.7f, new Color(1f, 0.75f, 0.35f),
                    pos + new Vector3(0f, scale.y * 0.5f + 0.25f, 0f), Vector3.zero, new Vector2(1.2f, 0.2f));
                label.gameObject.AddComponent<FacePlayer>().yawOnly = true;
            }
            return wrong;
        }

        /// <summary>Cria (ou reaproveita) uma layer no TagManager.</summary>
        public static int EnsureLayer(string name)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = 8; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == name) return i;
            for (var i = 8; i < layers.arraySize; i++)
            {
                var p = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(p.stringValue)) continue;
                p.stringValue = name;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
            throw new System.Exception("Sem layer livre para " + name);
        }

        // ------------------------------------------------------------------ Interactables

        /// <summary>
        /// Alavanca do quadro de energia na face frontal <paramref name="frontCenter"/>; <paramref name="facing"/> aponta
        /// para fora da parede (+Z local). Com <paramref name="withBox"/> falso, aproveita a caixa do modelo.
        /// </summary>
        public static BreakerLever SetupBreaker(Transform parent, MissionStepSO step, Vector3 frontCenter, Quaternion facing, bool withBox)
        {
            var box = UIBuilderUtil.Child(parent, "Disjuntor_PowerOff");
            box.transform.SetPositionAndRotation(frontCenter, facing);
            if (withBox)
            {
                var caixa = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Caixa", box.transform, Vector3.zero, Vector3.zero,
                    new Vector3(0.26f, 0.34f, 0.10f), MatFlashlight);
                caixa.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                caixa.transform.localRotation = Quaternion.identity;
            }

            var pivotGo = UIBuilderUtil.Child(box.transform, "Alavanca_Pivot");
            pivotGo.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            pivotGo.transform.localRotation = Quaternion.identity;

            var handle = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Alavanca", pivotGo.transform, Vector3.zero, Vector3.zero,
                new Vector3(0.035f, 0.025f, 0.075f), MatLever, false);
            handle.transform.localPosition = new Vector3(0f, 0f, 0.035f);
            handle.transform.localRotation = Quaternion.identity;

            var col = pivotGo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0f, 0.04f);
            col.size = new Vector3(0.10f, 0.12f, 0.12f);
            pivotGo.AddComponent<XRSimpleInteractable>();

            var indicator = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Indicador", box.transform, Vector3.zero, Vector3.zero,
                new Vector3(0.025f, 0.025f, 0.01f), MatIndicator, false);
            indicator.transform.localPosition = new Vector3(0.09f, 0.13f, 0.01f);
            indicator.transform.localRotation = Quaternion.identity;

            var lever = pivotGo.AddComponent<BreakerLever>();
            lever.pivot = pivotGo.transform;
            lever.axis = Vector3.right;
            lever.onAngle = 35f;
            lever.offAngle = -35f;
            lever.startsOn = true;
            lever.indicatorRenderer = indicator.GetComponent<Renderer>();

            // Legível para quem está de frente para o quadro (lado +Z local).
            UIBuilderUtil.WorldText(box.transform, "Label", "QUADRO DE ENERGIA", 0.35f, Color.white,
                frontCenter + facing * new Vector3(0f, 0.36f, 0.03f), (facing * Quaternion.Euler(0f, 180f, 0f)).eulerAngles, new Vector2(0.7f, 0.16f));

            var target = pivotGo.AddComponent<ObjectiveTarget>();
            target.step = step;
            target.markerHeightOffset = 0.2f;
            return lever;
        }

        /// <summary>
        /// Luzes da casa acesas sem bake: os marcadores DC_LUZ_* do modelo viram luzes Realtime sem sombra, num
        /// <see cref="HouseLights"/> que a alavanca do quadro de energia apaga.
        /// </summary>
        public static HouseLights SetupHouseLights(GameObject house, Transform parent, BreakerLever breaker)
        {
            var go = UIBuilderUtil.Child(parent, "LuzesDaCasa");
            var control = go.AddComponent<HouseLights>();
            foreach (var light in house.GetComponentsInChildren<Light>(true))
            {
                if (light.type == LightType.Directional)
                    continue;
                light.lightmapBakeType = LightmapBakeType.Realtime;
                light.shadows = LightShadows.None;   // 20+ luzes em tempo real: sombra sai caro no Quest
                light.enabled = true;
                control.lights.Add(light);
            }

            if (breaker != null)
                UnityEditor.Events.UnityEventTools.AddPersistentListener(breaker.onStateChanged, control.SetPowered);
            return control;
        }

        /// <summary>Alavanca sobre o quadro de energia da casa DC_Casa (corredor), no lugar do disjuntor geral do FBX.</summary>
        public static BreakerLever SetupHouseBreaker(GameObject house, Transform parent, MissionStepSO step)
        {
            HidePart(house, "DC_CASA_QDL_DisjuntorGeral");
            var panel = PartBounds(ModelPart(house, "DC_CASA_QuadroLuz"));
            // O quadro fica na parede do corredor e olha para −Z.
            var front = new Vector3(panel.center.x, panel.center.y, panel.min.z);
            return SetupBreaker(parent, step, front, Quaternion.Euler(0f, 180f, 0f), false);
        }

        /// <summary>Registro/botão giratório genérico (XRKnob) com volante cilíndrico e etiqueta.</summary>
        public static XRKnob CreateKnob(Transform parent, string name, Vector3 center, Quaternion rotation, float radius, float thickness,
            Material mat, string label, Vector3 labelOffset, Vector3 labelEuler, MissionStepSO step)
        {
            var knobGo = UIBuilderUtil.Child(parent, name);
            knobGo.transform.SetPositionAndRotation(center, rotation);
            var handle = UIBuilderUtil.Child(knobGo.transform, "Handle");
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localRotation = Quaternion.identity;

            var wheel = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Volante", handle.transform, Vector3.zero, Vector3.zero,
                new Vector3(radius * 2f, thickness, radius * 2f), mat, false);
            wheel.transform.localPosition = Vector3.zero;
            wheel.transform.localRotation = Quaternion.identity;
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Marca", handle.transform, Vector3.zero, Vector3.zero,
                new Vector3(radius * 0.25f, thickness * 1.4f, radius * 0.9f), MatIndicator, false).transform.localPosition = new Vector3(0f, 0f, radius * 0.5f);

            var sphere = handle.AddComponent<SphereCollider>();
            sphere.radius = Mathf.Max(radius * 1.3f, 0.06f);

            var knob = knobGo.AddComponent<XRKnob>();
            knob.handle = handle.transform;
            knob.clampedMotion = true;
            knob.minAngle = -180f;
            knob.maxAngle = 180f;
            knob.value = 0.5f;

            if (!string.IsNullOrEmpty(label))
                UIBuilderUtil.WorldText(knobGo.transform, "Label", label, 0.5f, Color.white, center + labelOffset, labelEuler, new Vector2(0.7f, 0.2f));

            if (step != null)
            {
                var target = knobGo.AddComponent<ObjectiveTarget>();
                target.step = step;
                target.markerHeightOffset = 0.2f;
            }
            return knob;
        }

        public static GameObject CreateItem(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 euler, Vector3 scale, Material mat,
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

        public static XRSocketInteractor CreateSocket(Transform parent, string name, Vector3 pos, List<string> allowedIds, float radius)
        {
            var go = UIBuilderUtil.Child(parent, name);
            go.transform.position = pos;
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            var socket = SetupSocket(go, allowedIds, radius);

            var disc = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Disco", go.transform, pos + new Vector3(0f, -radius * 0.6f, 0f), Vector3.zero,
                new Vector3(radius * 2.2f, 0.004f, radius * 2.2f), MatSocket, false);
            var r = disc.GetComponent<MeshRenderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return socket;
        }

        public static XRSocketInteractor SetupSocket(GameObject go, List<string> allowedIds, float snapRadius)
        {
            var socket = go.AddComponent<XRSocketInteractor>();
            socket.showInteractableHoverMeshes = true;
            socket.hoverSocketSnapping = false;
            socket.socketSnappingRadius = snapRadius;

            var filter = go.AddComponent<SocketItemFilter>();
            filter.allowedItemIds.AddRange(allowedIds);
            return socket;
        }

        public static XRSocketInteractor CreateBackpackSlot(Transform parent, string name, Vector3 slotPos, Vector3 zoneCenter, Vector3 zoneSize,
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

        public static GameObject CreateMarkerTemplate(Transform parent)
        {
            var template = UIBuilderUtil.Child(parent, "MarkerTemplate");
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Diamond", template.transform, Vector3.zero, new Vector3(45f, 0f, 45f),
                new Vector3(0.09f, 0.09f, 0.09f), MatMarker, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Ring", template.transform, new Vector3(0f, -0.12f, 0f), Vector3.zero,
                new Vector3(0.22f, 0.003f, 0.22f), MatMarker, false);
            foreach (var r in template.GetComponentsInChildren<MeshRenderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            template.SetActive(false);
            return template;
        }

        /// <summary>Setas verdes no chão (rota segura) ao longo de pontos; o grupo começa desligado.</summary>
        public static void CreateRouteChevrons(Transform parent, Vector3[] points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var dir = i + 1 < points.Length ? points[i + 1] - points[i] : (i > 0 ? points[i] - points[i - 1] : Vector3.forward);
                dir.y = 0f;
                var yaw = dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir).eulerAngles.y : 0f;
                var chevron = UIBuilderUtil.Child(parent, "Seta_" + (i + 1));
                chevron.transform.SetPositionAndRotation(points[i], Quaternion.Euler(0f, yaw, 0f));
                var l = UIBuilderUtil.Primitive(PrimitiveType.Cube, "L", chevron.transform, Vector3.zero, Vector3.zero, new Vector3(0.06f, 0.01f, 0.36f), MatChevron, false);
                l.transform.localPosition = new Vector3(-0.12f, 0f, -0.1f); l.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
                var r = UIBuilderUtil.Primitive(PrimitiveType.Cube, "R", chevron.transform, Vector3.zero, Vector3.zero, new Vector3(0.06f, 0.01f, 0.36f), MatChevron, false);
                r.transform.localPosition = new Vector3(0.12f, 0f, -0.1f); r.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            }
            parent.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ NPCs

        /// <summary>NPC low-poly: corpo (cápsula), cabeça, opcionalmente andador. Collider na layer Ignore Raycast + XRSimpleInteractable.</summary>
        public static XRSimpleInteractable CreateNpc(Transform parent, string name, Vector3 feetPos, float yaw, Color clothColor, bool walker,
            string speech, out TextMeshPro speechBubble)
        {
            var npc = UIBuilderUtil.Child(parent, name);
            npc.transform.SetPositionAndRotation(feetPos, Quaternion.Euler(0f, yaw, 0f));
            var cloth = UIBuilderUtil.GetOrCreateMaterial("Mat_Roupa_" + name, clothColor, false, false, 0.1f);

            var body = UIBuilderUtil.Primitive(PrimitiveType.Capsule, "Corpo", npc.transform, Vector3.zero, Vector3.zero, new Vector3(0.42f, 0.62f, 0.42f), cloth, false);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            var head = UIBuilderUtil.Primitive(PrimitiveType.Sphere, "Cabeca", npc.transform, Vector3.zero, Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), MatSkin, false);
            head.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            if (walker)
            {
                var w = UIBuilderUtil.Child(npc.transform, "Andador");
                w.transform.localPosition = new Vector3(0f, 0f, 0.35f);
                for (var i = 0; i < 4; i++)
                {
                    var leg = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Perna" + i, w.transform, Vector3.zero, Vector3.zero, new Vector3(0.02f, 0.4f, 0.02f), MatMetal, false);
                    leg.transform.localPosition = new Vector3(i % 2 == 0 ? -0.22f : 0.22f, 0.4f, i < 2 ? 0f : 0.25f);
                }
                var bar = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Barra", w.transform, Vector3.zero, Vector3.zero, new Vector3(0.48f, 0.03f, 0.03f), MatMetal, false);
                bar.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            }

            var col = npc.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.9f, 0f);
            col.height = 1.8f;
            col.radius = 0.3f;
            npc.layer = LayerMask.NameToLayer("Ignore Raycast");
            var interactable = npc.AddComponent<XRSimpleInteractable>();

            speechBubble = UIBuilderUtil.WorldText(npc.transform, "Fala", speech, 0.55f, Color.white, Vector3.zero, Vector3.zero, new Vector2(1.6f, 0.4f));
            speechBubble.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            speechBubble.gameObject.AddComponent<FacePlayer>().yawOnly = true;
            speechBubble.gameObject.SetActive(false);
            return interactable;
        }

        public static List<Transform> CreateWaypoints(Transform parent, string prefix, params Vector3[] points)
        {
            var list = new List<Transform>();
            for (var i = 0; i < points.Length; i++)
            {
                var wp = UIBuilderUtil.Child(parent, $"{prefix}_{i + 1}");
                wp.transform.position = points[i];
                list.Add(wp.transform);
            }
            return list;
        }

        // ------------------------------------------------------------------ UI

        /// <summary>Relógio de pulso. Com <paramref name="withCallButton"/>, a faixa inferior vira Opções + Ligar e o botão "Ligar" é retornado.</summary>
        public static WristUIController CreateWristUI(Transform hand, ScenarioManager manager, FloodWaterController water, OptionsPanelController options,
            bool withCallButton, out Button callButton)
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

            // Botão largo (~2,3 cm de altura): fácil de acertar com o raio ou com o dedo (poke).
            callButton = null;
            if (withCallButton)
            {
                ctrl.optionsButton = UIBuilderUtil.BottomButton(panel, "OptionsButton", "Opções", 18f, UIBuilderUtil.Accent, 8f, 52f, 0f, 0.5f, 8f, out _);
                callButton = UIBuilderUtil.BottomButton(panel, "CallButton", "Ligar  <size=70%>199/193</size>", 18f, new Color(0.75f, 0.25f, 0.2f), 8f, 52f, 0.5f, 1f, 8f, out _);
            }
            else
            {
                ctrl.optionsButton = UIBuilderUtil.BottomButton(panel, "OptionsButton", "Opções  <size=70%>(botão Menu)</size>", 18f, UIBuilderUtil.Accent,
                    8f, 52f, 0f, 1f, 8f, out _);
            }

            // Bússola: seta 3D acima do painel apontando para o objetivo atual.
            var compass = UIBuilderUtil.Child(wrist.transform, "Compass");
            compass.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Body", compass.transform, Vector3.zero, Vector3.zero, new Vector3(0.012f, 0.004f, 0.035f), MatMarker, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Head", compass.transform, Vector3.zero, new Vector3(0f, 45f, 0f), new Vector3(0.02f, 0.004f, 0.02f), MatMarker, false);
            compass.transform.GetChild(1).localPosition = new Vector3(0f, 0f, 0.02f);
            ctrl.compassArrow = compass.transform;
            return ctrl;
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

        public static IntroBriefingPanel CreateIntroPanel(Transform parent, Transform camera, ScenarioManager manager, string title, string introText)
        {
            var go = UIBuilderUtil.Child(parent, "IntroBriefingPanel");
            UIBuilderUtil.AddLazyFollow(go, camera, new Vector3(0f, 0.06f, 1.5f), 3f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(760f, 620f), 0.001f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.Accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), Vector2.zero);

            var ctrl = go.AddComponent<IntroBriefingPanel>();
            ctrl.scenarioManager = manager;
            ctrl.root = go;
            ctrl.titleText = UIBuilderUtil.TopText(panel, "Title", title, 32f, Color.white, TextAlignmentOptions.Center, 12f, 48f, 24f, FontStyles.Bold);
            ctrl.bodyText = UIBuilderUtil.TopText(panel, "Body", "", 22f, UIBuilderUtil.TextColor, TextAlignmentOptions.TopLeft, 90f, 430f, 32f);
            ctrl.introText = introText;
            ctrl.startButton = UIBuilderUtil.BottomButton(panel, "Start", "Começar", 26f, UIBuilderUtil.AccentGreen, 24f, 64f, 0.25f, 0.75f, 0f, out _);
            return ctrl;
        }

        public static EndReportPanelController CreateEndReportPanel(Transform parent, Transform camera, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "EndReportPanel");
            UIBuilderUtil.AddLazyFollow(go, camera, new Vector3(0f, 0.02f, 1.95f), 3f);
            // 0,0016 m por pixel: o painel fica com 1,38 m x 1,22 m, bem mais legível no headset.
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", go.transform, new Vector2(860f, 760f), 0.0016f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.AccentGreen, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -70f), Vector2.zero);

            var ctrl = go.AddComponent<EndReportPanelController>();
            ctrl.scenarioManager = manager;
            ctrl.root = go;
            ctrl.titleText = UIBuilderUtil.TopText(panel, "Title", "Fase concluída!", 32f, Color.white, TextAlignmentOptions.Center, 12f, 48f, 24f, FontStyles.Bold);

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
            UIBuilderUtil.TopText(panel, "CardHeader", "Orientações oficiais", 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.Left, 398f, 22f, 40f);
            ctrl.cardTitleText = UIBuilderUtil.TopText(panel, "CardTitle", "", 22f, UIBuilderUtil.AccentGreen, TextAlignmentOptions.Left, 422f, 34f, 40f, FontStyles.Bold);
            ctrl.cardBodyText = UIBuilderUtil.TopText(panel, "CardBody", "", 18f, Color.white, TextAlignmentOptions.TopLeft, 458f, 108f, 40f);
            ctrl.cardCounterText = UIBuilderUtil.TopText(panel, "CardCounter", "", 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.Center, 572f, 24f, 40f);
            ctrl.prevCardButton = UIBuilderUtil.Button(panel, "PrevCard", "<", 24f, UIBuilderUtil.Accent, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -596f), new Vector2(92f, -548f), out _);
            ctrl.nextCardButton = UIBuilderUtil.Button(panel, "NextCard", ">", 24f, UIBuilderUtil.Accent, Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -596f), new Vector2(-36f, -548f), out _);

            ctrl.replayButton = UIBuilderUtil.BottomButton(panel, "Replay", "Jogar de novo", 22f, UIBuilderUtil.AccentGreen, 24f, 60f, 0.06f, 0.48f, 8f, out _);
            ctrl.menuButton = UIBuilderUtil.BottomButton(panel, "Menu", "Menu principal", 22f, UIBuilderUtil.Accent, 24f, 60f, 0.52f, 0.94f, 8f, out _);
            return ctrl;
        }

        /// <summary>Painel de ligação de emergência (199/193/190/192) aberto pelo botão "Ligar" do relógio.</summary>
        public static EmergencyCallPanel CreateCallPanel(Transform parent, Transform camera, Button openButton,
            string reply199, string reply193, string reply190, string reply192, params string[] acceptedNumbers)
        {
            var go = UIBuilderUtil.Child(parent, "EmergencyCallPanel");
            var ctrl = go.AddComponent<EmergencyCallPanel>();

            var rootGo = UIBuilderUtil.Child(go.transform, "Root");
            UIBuilderUtil.AddLazyFollow(rootGo, camera, new Vector3(0f, -0.1f, 1.1f), 4f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", rootGo.transform, new Vector2(520f, 640f), 0.001f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", new Color(0.75f, 0.25f, 0.2f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -56f), Vector2.zero);
            UIBuilderUtil.TopText(panel, "Title", "Ligar para emergência", 26f, Color.white, TextAlignmentOptions.Center, 8f, 40f, 20f, FontStyles.Bold);

            ctrl.root = rootGo;
            ctrl.openButton = openButton;

            var y = 76f;
            const float h = 56f;
            const float gap = 10f;
            var lines = new[]
            {
                ("199", "Defesa Civil", reply199),
                ("193", "Bombeiros", reply193),
                ("190", "Polícia Militar", reply190),
                ("192", "SAMU", reply192),
            };
            foreach (var (number, label, reply) in lines)
            {
                var b = UIBuilderUtil.TopButton(panel, "Call_" + number, $"{number}  —  {label}", 20f, UIBuilderUtil.PanelLight, y, h, 24f, out _);
                ctrl.lines.Add(new EmergencyCallPanel.Line { number = number, label = label, reply = reply, button = b });
                y += h + gap;
            }
            ctrl.replyText = UIBuilderUtil.TopText(panel, "Reply", "", 18f, UIBuilderUtil.TextColor, TextAlignmentOptions.TopLeft, y + 6f, 190f, 24f);
            ctrl.closeButton = UIBuilderUtil.BottomButton(panel, "Close", "Fechar", 20f, UIBuilderUtil.Accent, 16f, 50f, 0.25f, 0.75f, 0f, out _);

            ctrl.acceptedNumbers.Clear();
            ctrl.acceptedNumbers.AddRange(acceptedNumbers);
            return ctrl;
        }

        /// <summary>Legenda de fase presa à câmera (a 0,6 m), renderizada na frente do FadeQuad. Começa desligada.</summary>
        public static TextMeshProUGUI CreatePhaseCaption(Transform camera)
        {
            var existing = camera.Find("PhaseCaption");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var go = UIBuilderUtil.Child(camera, "PhaseCaption");
            go.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            go.transform.localRotation = Quaternion.identity;
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800f, 200f);
            rt.localScale = Vector3.one * 0.0006f;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            var text = UIBuilderUtil.Text(go.transform, "Caption", "", 40f, Color.white, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyles.Italic);

            // Material do TMP na frente do fade (renderQueue 4001).
            var font = UIBuilderUtil.Font;
            if (font != null && font.material != null)
            {
                UIBuilderUtil.EnsureFolder(UIBuilderUtil.k_GeneratedMaterialsFolder);
                var path = $"{UIBuilderUtil.k_GeneratedMaterialsFolder}/Mat_TMP_Legenda.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(font.material);
                    AssetDatabase.CreateAsset(mat, path);
                }
                mat.renderQueue = 4001;
                EditorUtility.SetDirty(mat);
                text.fontSharedMaterial = mat;
            }
            go.SetActive(false);
            return text;
        }
    }
}
#endif
