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
    public static class AlagamentoGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_RootName = "SafeZone_Gameplay";

        static readonly Vector3 k_PlayerStart = new Vector3(-2.6f, 0f, -2.4f);
        static readonly Quaternion k_PlayerRot = Quaternion.Euler(0f, 90f, 0f);
        static readonly Vector2 k_SofaDocs = new Vector2(-0.98f, -1.30f);
        static readonly Vector2 k_SofaPhoto = new Vector2(-0.80f, -2.40f);
        static readonly Vector2 k_KitchenWater = new Vector2(-10.40f, -3.05f);
        static readonly Vector2 k_BathroomMeds = new Vector2(-10.55f, -6.95f);
        static readonly Vector2 k_BedFlashlight = new Vector2(-1.50f, -8.40f);

        static readonly Vector3 k_ValveCenter = new Vector3(0.07f, 0.35f, -0.45f);

        static readonly Vector3 k_BackpackBench = new Vector3(-4.0f, 0.225f, -4.30f);
        static readonly Vector3 k_Backpack = new Vector3(-4.0f, 0.67f, -4.30f);
        static readonly Vector3 k_BackpackBase = new Vector3(-4.0f, 0.45f, -4.30f);

        static readonly Vector3 k_MeetingPoint = new Vector3(-9.5f, 0.2f, 6.2f);
        static readonly Vector3 k_CarDoor = new Vector3(0.42f, 1.0f, -2.3f);
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
            water.gameObject.AddComponent<WaterWadingSlowdown>().water = water;
            var evacZone = CreateMeetingPoint(env.transform, out var routeGroup);
            CreateRouteChevrons(routeGroup.transform);

            var interactables = UIBuilderUtil.Child(root.transform, "Interactables");
            var lever = ScenarioSceneKit.SetupHouseBreaker(house, interactables.transform, data.stepPower);
            var knob = SetupValve(interactables.transform, data.stepWater);
            ScenarioSceneKit.SetupHouseLights(house, root.transform, lever);

            var docs = ScenarioSceneKit.CreateModelItem(interactables.transform, "Item_Documentos", OnSurface(k_SofaDocs, 0.004f), Vector3.zero,
                "documentos", "Documentos", data.stepKit);
            var photo = ScenarioSceneKit.CreateModelItem(interactables.transform, "Item_PortaRetrato", OnSurface(k_SofaPhoto, 0.004f), Vector3.zero,
                "porta_retrato", "Porta-retrato", data.stepValuables);
            var bottle = ScenarioSceneKit.CreateModelItem(interactables.transform, "Item_Agua", OnSurface(k_KitchenWater, 0.004f), Vector3.zero,
                "agua", "Água potável", data.stepKit);
            var meds = ScenarioSceneKit.CreateModelItem(interactables.transform, "Item_Remedios", OnSurface(k_BathroomMeds, 0.004f), Vector3.zero,
                "remedios", "Remédios", data.stepKit);
            var flashlight = ScenarioSceneKit.CreateModelItem(interactables.transform, "Item_Lanterna", OnSurface(k_BedFlashlight, 0.004f, 2.0f), Vector3.zero,
                "lanterna", "Lanterna", data.stepKit);

            var wardrobe = ScenarioSceneKit.PartBounds(ScenarioSceneKit.ModelPart(house, "DC_CASA_GuardaRoupa1"));
            var shelfPos = new Vector3(wardrobe.center.x, wardrobe.max.y + 0.05f, wardrobe.center.z);
            var shelfTop = UIBuilderUtil.Child(interactables.transform, "ArmarioTopo");
            shelfTop.transform.position = shelfPos;
            var shelfTarget = shelfTop.AddComponent<ObjectiveTarget>();
            shelfTarget.step = data.stepValuables;
            shelfTarget.markerHeightOffset = 0.3f;
            UIBuilderUtil.WorldText(shelfTop.transform, "Label", "LOCAL ALTO E SEGURO", 0.9f, UIBuilderUtil.AccentGreen,
                shelfTop.transform.position + new Vector3(0.35f, 0.15f, 0f), new Vector3(0f, -90f, 0f), new Vector2(1.6f, 0.3f));
            var valuableIds = new List<string> { "porta_retrato" };
            var shelfSocket = ScenarioSceneKit.CreateSocket(shelfTop.transform, "Socket_Valores", shelfPos, valuableIds, 0.18f);

            var bench = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Banco_Mochila", interactables.transform, k_BackpackBench, Vector3.zero,
                new Vector3(0.5f, 0.45f, 0.36f), ScenarioSceneKit.MatPhoto);
            UIBuilderUtil.MarkStatic(bench, false);
            var backpack = ScenarioSceneKit.CreateModelVisual(interactables.transform, "Mochila_Kit", $"{ScenarioSceneKit.k_ItemsFolder}/Mochila_Kit.fbx", k_BackpackBase, Vector3.zero);
            ScenarioSceneKit.AddFittedBox(backpack, Vector3.zero);
            var backpackTarget = backpack.AddComponent<ObjectiveTarget>();
            backpackTarget.step = data.stepKit;
            backpackTarget.markerHeightOffset = 0.35f;
            UIBuilderUtil.WorldText(backpack.transform, "Label", "MOCHILA DE EMERGÊNCIA\n<size=60%>solte os itens perto dela</size>", 0.8f, UIBuilderUtil.AccentGreen,
                k_Backpack + new Vector3(0f, 0.55f, -0.05f), new Vector3(0f, 180f, 0f), new Vector2(1.6f, 0.4f));

            var kitIds = new List<string> { "documentos", "agua", "remedios", "lanterna" };
            var zoneCenter = k_Backpack + new Vector3(0f, 0.15f, 0f);
            var zoneSize = new Vector3(0.8f, 0.9f, 0.64f);
            var kitSockets = new List<XRSocketInteractor>
            {
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_A", k_BackpackBase + new Vector3(-0.12f, 0.3f, 0f), zoneCenter, zoneSize, kitIds),
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_B", k_BackpackBase + new Vector3(-0.04f, 0.3f, 0f), zoneCenter, zoneSize, kitIds),
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_C", k_BackpackBase + new Vector3(0.04f, 0.3f, 0f), zoneCenter, zoneSize, kitIds),
                ScenarioSceneKit.CreateBackpackSlot(interactables.transform, "Socket_Kit_D", k_BackpackBase + new Vector3(0.12f, 0.3f, 0f), zoneCenter, zoneSize, kitIds),
            };
            backpack.AddComponent<BackpackStorage>().slots.AddRange(kitSockets);

            CreateCarWrongAction(interactables.transform);
            CreateElectricHazards(interactables.transform, house, lever, water);

            var validators = UIBuilderUtil.Child(root.transform, "Validators");

            var vValuables = UIBuilderUtil.Child(validators.transform, "Validator_01_Valores").AddComponent<StepCompleteOnSocket>();
            vValuables.scenarioManager = manager;
            vValuables.step = data.stepValuables;
            vValuables.sockets.Add(shelfSocket);
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
            ScenarioSceneKit.CreateLoadingPanel(ui.transform, camera);
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

            ScenarioSceneKit.CreateMeetingPlatform(group.transform,
                new Vector3(k_MeetingPoint.x, k_MeetingPoint.y - size.y * 0.5f, k_MeetingPoint.z));

            var zone = UIBuilderUtil.Child(group.transform, "EvacuationZone");
            zone.transform.position = k_MeetingPoint + new Vector3(0f, 1.2f, 0f);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(size.x, 2.4f, size.z);

            routeGroup = UIBuilderUtil.Child(group.transform, "RotaDeFuga");
            return zone;
        }

        static void CreateRouteChevrons(Transform parent)
        {
            var path = new[]
            {
                new Vector2(-4.5f, 0.4f), new Vector2(-4.6f, 1.8f), new Vector2(-4.7f, 3.2f), new Vector2(-4.9f, 4.4f),
                new Vector2(-4.9f, 6.0f), new Vector2(-6.6f, 6.2f), new Vector2(-7.7f, 6.2f),
            };
            var points = new Vector3[path.Length];

            for (var i = 0; i < path.Length; i++)
                points[i] = OnSurface(path[i], 0.03f, 0.3f);
            ScenarioSceneKit.CreateRouteChevrons(parent, points);
        }

        static XRKnob SetupValve(Transform parent, MissionStepSO step)
        {
            var knobGo = ScenarioSceneKit.CreateModelVisual(parent, "Registro_Knob",
                $"{ScenarioSceneKit.k_ItemsFolder}/Registro_Knob.fbx", k_ValveCenter, new Vector3(0f, 0f, -90f));
            var wheel = ScenarioSceneKit.ModelPart(knobGo, "Volante");
            knobGo.transform.position += k_ValveCenter - wheel.position;

            var knob = ScenarioSceneKit.ModelKnob(knobGo, wheel, 0.14f, null, Vector3.zero, Vector3.zero, null);

            UIBuilderUtil.WorldText(knobGo.transform, "Label", "REGISTRO DE ÁGUA\n<size=70%>gire para fechar</size>", 0.6f, Color.white,
                k_ValveCenter + new Vector3(0.05f, 0.42f, 0f), new Vector3(0f, -90f, 0f), new Vector2(0.7f, 0.2f));

            var target = knobGo.AddComponent<ObjectiveTarget>();
            target.step = step;
            target.markerHeightOffset = 0.25f;
            return knob;
        }

        static void CreateElectricHazards(Transform parent, GameObject house, BreakerLever lever, FloodWaterController water)
        {
            var deadline = UIBuilderUtil.Child(parent, "PrazoDoQuadro").AddComponent<BreakerDeadline>();
            deadline.lever = lever;
            deadline.water = water;
            deadline.outletLevel = 0.12f;

            var tv = ScenarioSceneKit.ModelPart(house, "DC_CASA_TV");
            var pos = tv != null ? ScenarioSceneKit.PartBounds(tv).center : new Vector3(-5.19f, 0.99f, -2.0f);
            var go = UIBuilderUtil.Child(parent, "TV_Energizada");
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(0.45f, 1.1f, 1.3f);
            go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            var hazard = go.AddComponent<EnergizedAppliance>();
            hazard.lever = lever;
            hazard.water = water;
            UIBuilderUtil.WorldText(go.transform, "Label", "Mexer na TV ligada?", 0.7f, new Color(1f, 0.75f, 0.35f),
                pos + new Vector3(0.3f, 0.75f, 0f), Vector3.zero, new Vector2(1.4f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static void CreateCarWrongAction(Transform parent)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Carro_PortaMotorista", parent, k_CarDoor, Vector3.zero,
                new Vector3(0.1f, 0.6f, 0.5f), ScenarioSceneKit.MatHint);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "sair_de_carro";
            wrong.message = "Nunca saia de carro em alagamento: a força da água arrasta veículos. Siga a pé até o local alto.";
            wrong.kind = ActionKind.ErroGrave;
            wrong.advice = "Vá a pé até um ponto alto e seguro. Nunca atravesse ruas alagadas, nem de carro.";
            UIBuilderUtil.WorldText(parent, "Carro_Label", "Sair de carro?", 0.7f, new Color(1f, 0.75f, 0.35f),
                k_CarDoor + new Vector3(-0.1f, 0.85f, 0f), Vector3.zero, new Vector2(1.2f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }
    }
}
#endif
