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
    /// sobre a casa DC_Casa: XR Origin, teleporte, vinheta, disjuntor, registro, itens, sockets,
    /// validadores, UI de pulso, briefing, relatório, água, rota de fuga e ponto de encontro.
    /// Tudo que é gerado fica sob o objeto raiz "SafeZone_Gameplay" (exceto ajustes na casa).
    /// </summary>
    public static class AlagamentoGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_RootName = "SafeZone_Gameplay";

        // Posições na casa DC_Casa (metros, mundo; frente da casa para +Z, rua e calçada em z 5..12).
        static readonly Vector3 k_PlayerStart = new Vector3(-2.6f, 0f, -2.4f);                 // sala
        static readonly Quaternion k_PlayerRot = Quaternion.Euler(0f, 90f, 0f);                // de frente para o sofá
        static readonly Vector2 k_SofaDocs = new Vector2(-0.80f, -1.60f);                      // assento do sofá (x, z)
        static readonly Vector2 k_SofaPhoto = new Vector2(-0.80f, -2.40f);
        static readonly Vector2 k_KitchenWater = new Vector2(-10.40f, -3.05f);                 // bancada da cozinha
        static readonly Vector2 k_BathroomMeds = new Vector2(-10.55f, -6.95f);                 // bancada do banheiro, ao lado da cuba
        static readonly Vector2 k_BedFlashlight = new Vector2(-1.50f, -8.40f);                 // cama de casal
        // Registro na parede externa da casa, do lado da garagem, entre a parede e o carro.
        static readonly Vector3 k_ValveCenter = new Vector3(0.07f, 0.35f, -0.45f);
        // Mochila sobre um banco na sala, perto da porta do corredor: na altura da mão e acima da água.
        static readonly Vector3 k_BackpackBench = new Vector3(-4.0f, 0.225f, -4.30f);
        static readonly Vector3 k_Backpack = new Vector3(-4.0f, 0.67f, -4.30f);
        // Ponto de encontro elevado na calçada, à esquerda do portão de pedestres.
        static readonly Vector3 k_MeetingPoint = new Vector3(-9.5f, 0.2f, 6.2f);
        static readonly Vector3 k_CarDoor = new Vector3(0.42f, 1.0f, -2.3f);                   // porta do motorista do carro da garagem
        const string k_IntroText = "<b>Alerta da Defesa Civil:</b> chuva muito forte na sua região e a água já começou a subir na rua.\n" +
            "Você está em casa e tem alguns minutos para agir com calma e segurança.\n\n" +
            "Use o relógio no seu pulso esquerdo para ver a missão atual. Aponte para o chão e aperte o gatilho para se teleportar. " +
            "Para ajustar o conforto, aperte \"Opções\" no relógio ou o botão Menu do controle esquerdo.";

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

            var house = ScenarioSceneKit.EnsureHouseInstance(scene);
            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_PlayerStart, k_PlayerRot);
            var camera = origin.Camera.transform;
            var leftHand = ScenarioSceneKit.FindChildRecursive(origin.transform, "Left Controller");

            var root = new GameObject(k_RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_PlayerStart, k_PlayerRot);

            var manager = ScenarioSceneKit.CreateCore(root.transform, origin, data.scenario, true, out _);

            var env = UIBuilderUtil.Child(root.transform, "Environment");
            ScenarioSceneKit.OpenHouseDoors(house, true);
            ScenarioSceneKit.SetupModelPhysics(house);
            var water = CreateWater(env.transform, manager);
            var evacZone = CreateMeetingPoint(env.transform, out var routeGroup);
            CreateRouteChevrons(routeGroup.transform);

            var interactables = UIBuilderUtil.Child(root.transform, "Interactables");
            var lever = ScenarioSceneKit.SetupHouseBreaker(house, interactables.transform, data.stepPower);
            var knob = SetupValve(interactables.transform, data.stepWater);
            ScenarioSceneKit.SetupHouseLights(house, root.transform, lever);

            var docs = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Documentos", PrimitiveType.Cube, OnSurface(k_SofaDocs, 0.03f), new Vector3(0f, 90f, 0f),
                new Vector3(0.24f, 0.03f, 0.32f), ScenarioSceneKit.MatDocs, "documentos", "Documentos", data.stepValuables);
            var photo = ScenarioSceneKit.CreateItem(interactables.transform, "Item_PortaRetrato", PrimitiveType.Cube, OnSurface(k_SofaPhoto, 0.03f), new Vector3(0f, 90f, 0f),
                new Vector3(0.16f, 0.02f, 0.20f), ScenarioSceneKit.MatPhoto, "porta_retrato", "Porta-retrato", data.stepValuables);
            var bottle = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Agua", PrimitiveType.Cylinder, OnSurface(k_KitchenWater, 0.12f), Vector3.zero,
                new Vector3(0.07f, 0.11f, 0.07f), ScenarioSceneKit.MatBottle, "agua", "Água potável", data.stepKit);
            var meds = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Remedios", PrimitiveType.Cube, OnSurface(k_BathroomMeds, 0.04f), Vector3.zero,
                new Vector3(0.10f, 0.05f, 0.06f), ScenarioSceneKit.MatMeds, "remedios", "Remédios", data.stepKit);
            var flashlight = ScenarioSceneKit.CreateItem(interactables.transform, "Item_Lanterna", PrimitiveType.Cylinder, OnSurface(k_BedFlashlight, 0.06f, 2.0f), new Vector3(0f, 0f, 90f),
                new Vector3(0.045f, 0.09f, 0.045f), ScenarioSceneKit.MatFlashlight, "lanterna", "Lanterna", data.stepKit);

            // Local alto: topo do guarda-roupa do quarto de casal.
            var wardrobe = ScenarioSceneKit.PartBounds(ScenarioSceneKit.ModelPart(house, "DC_CASA_GuardaRoupa1"));
            var shelfPosA = new Vector3(wardrobe.center.x, wardrobe.max.y + 0.05f, wardrobe.center.z + 0.45f);
            var shelfPosB = new Vector3(wardrobe.center.x, wardrobe.max.y + 0.05f, wardrobe.center.z - 0.45f);
            var shelfTop = UIBuilderUtil.Child(interactables.transform, "ArmarioTopo");
            shelfTop.transform.position = (shelfPosA + shelfPosB) * 0.5f;
            var shelfTarget = shelfTop.AddComponent<ObjectiveTarget>();
            shelfTarget.step = data.stepValuables;
            shelfTarget.markerHeightOffset = 0.3f;
            UIBuilderUtil.WorldText(shelfTop.transform, "Label", "LOCAL ALTO E SEGURO", 0.9f, UIBuilderUtil.AccentGreen,
                shelfTop.transform.position + new Vector3(0.35f, 0.15f, 0f), new Vector3(0f, -90f, 0f), new Vector2(1.6f, 0.3f));
            var valuableIds = new List<string> { "documentos", "porta_retrato" };
            var shelfSocketA = ScenarioSceneKit.CreateSocket(shelfTop.transform, "Socket_Valores_A", shelfPosA, valuableIds, 0.16f);
            var shelfSocketB = ScenarioSceneKit.CreateSocket(shelfTop.transform, "Socket_Valores_B", shelfPosB, valuableIds, 0.16f);

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

            CreateCarWrongAction(interactables.transform);

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

        static Vector3 OnSurface(Vector2 xz, float lift, float fromY = 2.5f)
        {
            return ScenarioSceneKit.OnSurface(xz.x, xz.y, fromY, lift);
        }

        static FloodWaterController CreateWater(Transform parent, ScenarioManager manager)
        {
            var go = UIBuilderUtil.Child(parent, "Flood");
            // Cobre a casa, o lote, a calçada e a rua (40 × 30 m).
            var surface = UIBuilderUtil.Primitive(PrimitiveType.Plane, "WaterSurface", go.transform, new Vector3(-5.4f, -0.04f, -3f), Vector3.zero,
                new Vector3(4f, 1f, 3f), ScenarioSceneKit.MatWater, false);
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
            var size = new Vector3(3f, 0.44f, 1.8f);

            var platform = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Plataforma", group.transform, k_MeetingPoint, Vector3.zero, size, ScenarioSceneKit.MatPlatform);
            ScenarioSceneKit.AddTeleportArea(platform);
            UIBuilderUtil.MarkStatic(platform);

            var signPos = new Vector3(k_MeetingPoint.x - 1.8f, 0f, k_MeetingPoint.z);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", group.transform, signPos + new Vector3(0f, 0.9f, 0f), Vector3.zero,
                new Vector3(0.06f, 0.9f, 0.06f), ScenarioSceneKit.MatFlashlight);
            var board = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", group.transform, signPos + new Vector3(0f, 1.95f, 0f), Vector3.zero,
                new Vector3(1.1f, 0.55f, 0.04f), ScenarioSceneKit.MatSign);
            // O jogador chega do portão (z menor) olhando para +Z: texto com rotação zero fica legível.
            UIBuilderUtil.WorldText(board.transform, "Texto", "PONTO DE ENCONTRO\n<size=60%>local alto e seguro</size>", 1.6f, Color.white,
                signPos + new Vector3(0f, 1.95f, -0.03f), Vector3.zero, new Vector2(1.05f, 0.5f));

            var zone = UIBuilderUtil.Child(group.transform, "EvacuationZone");
            zone.transform.position = k_MeetingPoint + new Vector3(0f, 1.2f, 0f);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(size.x, 2.4f, size.z);

            routeGroup = UIBuilderUtil.Child(group.transform, "RotaDeFuga");
            return zone;
        }

        /// <summary>Setas da porta da frente pelo caminho de pedras e pelo portão até o ponto de encontro.</summary>
        static void CreateRouteChevrons(Transform parent)
        {
            var path = new[]
            {
                new Vector2(-4.5f, 0.4f), new Vector2(-4.6f, 1.8f), new Vector2(-4.7f, 3.2f), new Vector2(-4.9f, 4.4f),
                new Vector2(-4.9f, 6.0f), new Vector2(-6.6f, 6.2f), new Vector2(-7.7f, 6.2f),
            };
            var points = new Vector3[path.Length];
            // Raycast baixo: começa abaixo do portão aberto e das plantas, só enxerga o chão.
            for (var i = 0; i < path.Length; i++)
                points[i] = OnSurface(path[i], 0.03f, 0.3f);
            ScenarioSceneKit.CreateRouteChevrons(parent, points);
        }

        static XRKnob SetupValve(Transform parent, MissionStepSO step)
        {
            var knobGo = UIBuilderUtil.Child(parent, "Registro_Knob");
            // Eixo Y local do knob = eixo X do mundo (volante encostado na parede, que é perpendicular a X).
            knobGo.transform.SetPositionAndRotation(k_ValveCenter, Quaternion.Euler(0f, 0f, -90f));
            var handle = UIBuilderUtil.Child(knobGo.transform, "Handle");
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localRotation = Quaternion.identity;

            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Cano", knobGo.transform, k_ValveCenter + new Vector3(-0.03f, 0f, 0f), new Vector3(0f, 0f, 90f),
                new Vector3(0.05f, 0.05f, 0.05f), ScenarioSceneKit.MatMetal, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Volante", handle.transform, k_ValveCenter, new Vector3(0f, 0f, 90f),
                new Vector3(0.2f, 0.02f, 0.2f), ScenarioSceneKit.MatLever);

            var sphere = handle.AddComponent<SphereCollider>();
            sphere.radius = 0.14f;

            var knob = knobGo.AddComponent<XRKnob>();
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

        static void CreateCarWrongAction(Transform parent)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Carro_PortaMotorista", parent, k_CarDoor, Vector3.zero,
                new Vector3(0.1f, 0.6f, 0.5f), ScenarioSceneKit.MatHint);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "sair_de_carro";
            wrong.message = "Nunca saia de carro em alagamento: a força da água arrasta veículos. Siga a pé até o local alto.";
            UIBuilderUtil.WorldText(parent, "Carro_Label", "Sair de carro?", 0.7f, new Color(1f, 0.75f, 0.35f),
                k_CarDoor + new Vector3(-0.1f, 0.85f, 0f), Vector3.zero, new Vector2(1.2f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

    }
}
#endif
