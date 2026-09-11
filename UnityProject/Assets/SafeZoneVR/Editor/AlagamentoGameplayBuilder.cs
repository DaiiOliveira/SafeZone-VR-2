#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.VRTemplate;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        const string k_IntroText = "<b>Alerta da Defesa Civil:</b> chuva muito forte na sua região e a água já começou a subir na rua.\n" +
            "Você está em casa e tem alguns minutos para agir com calma e segurança.\n\n" +
            "Use o relógio no seu pulso esquerdo para ver a missão atual. Aponte para o chão e aperte o gatilho para se teleportar. " +
            "Para ajustar o conforto, aperte \"Opções\" no relógio ou o botão Menu do controle esquerdo.";

        static readonly Vector3 k_KombiDoor = new Vector3(-26.8f, 1.0f, 15.75f);

        [MenuItem("SafeZone VR/Build/2. Fase Alagamento (montar gameplay na casa)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Gameplay da fase Alagamento montado e cena salva.", "OK");
        }

        public static void Build()
        {
            var scene = ScenarioSceneKit.OpenOrCreateScene(k_ScenePath);
            var data = SafeZoneDataBuilder.Build();
            ScenarioSceneKit.CreateMaterials();

            CleanupLegacy(scene);

            ScenarioSceneKit.EnsureHouseInstance(scene);
            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_PlayerStart, Quaternion.identity);
            var camera = origin.Camera.transform;
            var leftHand = ScenarioSceneKit.FindChildRecursive(origin.transform, "Left Controller");

            var root = new GameObject(k_RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            // Ponto de início: alinha a cabeça do jogador aqui ao carregar e o devolve se cair para fora do mundo.
            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_PlayerStart, Quaternion.identity);

            // ---- Núcleo
            var manager = ScenarioSceneKit.CreateCore(root.transform, origin, data.scenario, true, out _);

            // ---- Ambiente extra: água, rua, ponto de encontro, rota
            var env = UIBuilderUtil.Child(root.transform, "Environment");
            ScenarioSceneKit.SetupTeleportFloors();
            ScenarioSceneKit.AddFurnitureColliders();
            ScenarioSceneKit.MarkHouseStatic();
            var water = CreateWater(env.transform, manager);
            ScenarioSceneKit.CreateStreet(env.transform);
            var evacZone = CreateMeetingPoint(env.transform, out var routeGroup);
            CreateRouteChevrons(routeGroup.transform);

            // ---- Interagíveis
            var interactables = UIBuilderUtil.Child(root.transform, "Interactables");
            var lever = ScenarioSceneKit.SetupBreaker(interactables.transform, data.stepPower, k_BreakerBox);
            var knob = SetupValve(interactables.transform, data.stepWater);

            var docs = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Documentos", PrimitiveType.Cube, k_SofaDocs, new Vector3(0f, 90f, 0f),
                new Vector3(0.24f, 0.03f, 0.32f), ScenarioSceneKit.MatDocs, "documentos", "Documentos", data.stepValuables);
            var photo = ScenarioSceneKit.CreateItem(interactables.transform, "Item_PortaRetrato", PrimitiveType.Cube, k_SofaPhoto, new Vector3(0f, 90f, 0f),
                new Vector3(0.16f, 0.02f, 0.20f), ScenarioSceneKit.MatPhoto, "porta_retrato", "Porta-retrato", data.stepValuables);
            var bottle = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Agua", PrimitiveType.Cylinder, k_KitchenWater, Vector3.zero,
                new Vector3(0.07f, 0.11f, 0.07f), ScenarioSceneKit.MatBottle, "agua", "Água potável", data.stepKit);
            var meds = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Remedios", PrimitiveType.Cube, k_BathroomMeds, Vector3.zero,
                new Vector3(0.10f, 0.05f, 0.06f), ScenarioSceneKit.MatMeds, "remedios", "Remédios", data.stepKit);
            var flashlight = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Lanterna", PrimitiveType.Cylinder, k_BedFlashlight, new Vector3(0f, 0f, 90f),
                new Vector3(0.045f, 0.09f, 0.045f), ScenarioSceneKit.MatFlashlight, "lanterna", "Lanterna", data.stepKit);

            var shelfTop = UIBuilderUtil.Child(interactables.transform, "ArmarioTopo");
            shelfTop.transform.position = (k_ShelfSocketA + k_ShelfSocketB) * 0.5f;
            var shelfTarget = shelfTop.AddComponent<ObjectiveTarget>();
            shelfTarget.step = data.stepValuables;
            shelfTarget.markerHeightOffset = 0.3f;
            UIBuilderUtil.WorldText(shelfTop.transform, "Label", "LOCAL ALTO E SEGURO", 0.9f, UIBuilderUtil.AccentGreen,
                shelfTop.transform.position + new Vector3(0f, 0.12f, 0.25f), new Vector3(0f, 180f, 0f), new Vector2(1.6f, 0.3f));
            var valuableIds = new List<string> { "documentos", "porta_retrato" };
            var shelfSocketA = ScenarioSceneKit.CreateSocket(shelfTop.transform, "Socket_Valores_A", k_ShelfSocketA, valuableIds, 0.16f);
            var shelfSocketB = ScenarioSceneKit.CreateSocket(shelfTop.transform, "Socket_Valores_B", k_ShelfSocketB, valuableIds, 0.16f);

            var bench = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Banco_Mochila", interactables.transform, k_BackpackBench, Vector3.zero,
                new Vector3(0.5f, 0.45f, 0.36f), ScenarioSceneKit.MatPhoto);
            UIBuilderUtil.MarkStatic(bench, false);
            var backpack = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Mochila_Kit", interactables.transform, k_Backpack, Vector3.zero,
                new Vector3(0.34f, 0.44f, 0.22f), ScenarioSceneKit.MatBackpack);
            var backpackTarget = backpack.AddComponent<ObjectiveTarget>();
            backpackTarget.step = data.stepKit;
            backpackTarget.markerHeightOffset = 0.35f;
            UIBuilderUtil.WorldText(backpack.transform, "Label", "MOCHILA DE EMERGÊNCIA\n<size=60%>solte os itens perto dela</size>", 0.8f, UIBuilderUtil.AccentGreen,
                k_Backpack + new Vector3(0f, 0.55f, -0.05f), new Vector3(0f, 180f, 0f), new Vector2(1.6f, 0.4f));
            var opening = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Abertura", backpack.transform, k_Backpack + new Vector3(0f, 0.222f, 0f), Vector3.zero,
                Vector3.one, ScenarioSceneKit.MatSocket, false);
            opening.transform.localScale = new Vector3(0.9f, 0.006f, 0.8f); // relativo à escala da mochila
            opening.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Os 3 espaços dividem uma área de captura que envolve mochila + banco com folga (BackpackStorage).
            var kitIds = new List<string> { "agua", "remedios", "lanterna" };
            var zoneCenter = k_Backpack + new Vector3(0f, 0.15f, 0f);
            var zoneSize = new Vector3(0.8f, 0.9f, 0.64f);
            var kitSockets = new List<XRSocketInteractor>
            {
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_A", k_Backpack + new Vector3(-0.105f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_B", k_Backpack + new Vector3(0f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_C", k_Backpack + new Vector3(0.105f, 0.22f, 0f), zoneCenter, zoneSize, kitIds),
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
            var options = ScenarioSceneKit.CreateOptionsPanel(ui.transform, camera, true);
            ScenarioSceneKit.CreateWristUI(leftHand != null ? leftHand : camera, manager, water, options, false, out _);
            ScenarioSceneKit.CreateIntroPanel(ui.transform, camera, manager, "Alagamento em Casa", k_IntroText);
            ScenarioSceneKit.CreateEndReportPanel(ui.transform, camera, manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Gameplay do Alagamento montado e cena salva.");
        }

        // ------------------------------------------------------------------ Scene / cleanup

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

        // ------------------------------------------------------------------ Materials

        // ------------------------------------------------------------------ Environment

        static FloodWaterController CreateWater(Transform parent, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "Flood");
            var surface = UIBuilderUtil.Primitive(PrimitiveType.Plane, "WaterSurface", go.transform, new Vector3(-31f, -0.04f, 14f), Vector3.zero,
                new Vector3(1.5f, 1f, 2.6f), ScenarioSceneKit.MatWater, false);
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

        static GameObject CreateMeetingPoint(Transform parent, out GameObject routeGroup)
        {
            var group = UIBuilderUtil.Child(parent, "PontoDeEncontro");

            var platform = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Plataforma", group.transform, k_MeetingPoint, Vector3.zero,
                new Vector3(3f, 0.3f, 3f), ScenarioSceneKit.MatPlatform);
            ScenarioSceneKit.AddTeleportArea(platform);
            UIBuilderUtil.MarkStatic(platform);

            var signPos = k_MeetingPoint + new Vector3(1.8f, 0f, 0.4f);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", group.transform, signPos + new Vector3(0f, 0.9f, 0f), Vector3.zero,
                new Vector3(0.06f, 0.9f, 0.06f), ScenarioSceneKit.MatFlashlight);
            var board = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", group.transform, signPos + new Vector3(0f, 1.95f, 0f), Vector3.zero,
                new Vector3(1.1f, 0.55f, 0.04f), ScenarioSceneKit.MatSign);
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
                    new Vector3(0.06f, 0.01f, 0.36f), ScenarioSceneKit.MatChevron, false);
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "R", chevron.transform, points[i] + new Vector3(0.12f, 0f, -0.1f), new Vector3(0f, 45f, 0f),
                    new Vector3(0.06f, 0.01f, 0.36f), ScenarioSceneKit.MatChevron, false);
            }
            parent.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Interactables

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
                    new Vector3(0.2f, 0.02f, 0.2f), ScenarioSceneKit.MatLever);
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

        static void CreateKombiWrongAction(Transform parent)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Kombi_PortaTraseira", parent, k_KombiDoor, Vector3.zero,
                new Vector3(0.5f, 0.6f, 0.1f), ScenarioSceneKit.MatHint);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "sair_de_carro";
            wrong.message = "Nunca saia de carro em alagamento: a força da água arrasta veículos. Siga a pé até o local alto.";
            UIBuilderUtil.WorldText(parent, "Kombi_Label", "Sair de carro?", 0.7f, new Color(1f, 0.75f, 0.35f),
                k_KombiDoor + new Vector3(0f, 0.85f, 0.15f), Vector3.zero, new Vector2(1.2f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        // ------------------------------------------------------------------ UI

    }
}
#endif
