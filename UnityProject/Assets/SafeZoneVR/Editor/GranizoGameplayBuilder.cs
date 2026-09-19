#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR.Editor
{
    public static class GranizoGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Granizo_Praca.unity";
        const string k_HailLayer = "HailBlockers";

        static readonly Vector3 k_PlayerStart = new Vector3(-4.9f, 0f, 13.4f);
        static readonly Vector3 k_JoaquimBench = new Vector3(-7.9f, 0f, 16.1f);

        static readonly Vector3 k_Ubs = new Vector3(15.8f, 0f, 19f);
        static readonly Vector3 k_UbsSize = new Vector3(8f, 3f, 8.4f);
        const float k_UbsDoorZ = 19.45f;
        const float k_UbsFloorY = 0.15f;
        const float k_UbsYaw = 180f;
        static readonly Vector3 k_BusStop = new Vector3(-10f, 0f, 6.3f);
        static readonly Vector3 k_Shack = new Vector3(-22.4f, 0.12f, 20f);
        const float k_StreetLaneZ = 10.9f;
        static readonly Vector3 k_CarStart = new Vector3(-36f, -0.06f, k_StreetLaneZ);
        static readonly Vector3 k_Carlos = new Vector3(-34.5f, 0f, 12.8f);
        static readonly Vector3 k_SpotA = new Vector3(-30.5f, -0.06f, k_StreetLaneZ);
        static readonly Vector3 k_SpotB = new Vector3(24.5f, -0.06f, k_StreetLaneZ);
        static readonly Vector3 k_SpotC = new Vector3(-12.5f, -0.06f, k_StreetLaneZ);
        static readonly Vector3 k_Billboard = new Vector3(-30.5f, 0f, 13.3f);
        static readonly Vector3 k_Tower = new Vector3(27.5f, 0f, 16f);
        const float k_BusStopYaw = 0f;
        const float k_BillboardYaw = 0f;
        const float k_SpotYaw = 90f;
        static readonly Vector3 k_Ladder = new Vector3(-10f, 0f, 1.25f);
        static readonly Vector3 k_Ceiling = new Vector3(-2.9f, 2.68f, -8.9f);
        static readonly Vector3 k_GateZone = new Vector3(-4.9f, 1.2f, 4.4f);
        static readonly Color k_StormSky = new Color(0.55f, 0.58f, 0.63f);

        const string k_IntroText =
            "<b>Alerta da Defesa Civil (SMS 40199):</b> tempestade com possibilidade de <b>granizo</b> no seu bairro nos próximos minutos.\n" +
            "Você está na praça, do outro lado da rua de casa. Procure um local seguro e ajude quem estiver por perto.\n\n" +
            "O relógio no pulso esquerdo mostra cada missão e permite ligar para a Defesa Civil e os Bombeiros. " +
            "Aponte para o chão e aperte o gatilho para se teleportar.";

        [MenuItem("SafeZone VR/Build/2c. Fase Granizo (montar praça e casa)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Gameplay da fase Granizo montado e cena salva.", "OK");
        }

        public static void Build()
        {
            var scene = ScenarioSceneKit.OpenOrCreateScene(k_ScenePath);
            var data = SafeZoneDataBuilder.BuildGranizo();
            AssetDatabase.SaveAssets();
            ScenarioSceneKit.CreateMaterials();
            ScenarioSceneKit.DestroyRoot(scene, ScenarioSceneKit.k_RootName);
            ScenarioSceneKit.SetupDirectionalLight(scene, new Color(0.75f, 0.78f, 0.85f), 0.65f, new Vector3(40f, -30f, 0f));
            SetupStormSky();
            var hailLayer = ScenarioSceneKit.EnsureLayer(k_HailLayer);

            var house = ScenarioSceneKit.EnsureHouseInstance(scene);
            var praca = ScenarioSceneKit.EnsurePracaInstance(scene);
            ScenarioSceneKit.OpenHouseDoors(house, true);
            HideNeighborForUbs(praca);
            ScenarioSceneKit.SetupModelPhysics(house);
            ScenarioSceneKit.SetupModelPhysics(praca);

            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_PlayerStart, Quaternion.identity);
            var camera = origin.Camera.transform;

            origin.Camera.farClipPlane = Mathf.Max(origin.Camera.farClipPlane, 600f);
            SetupStormCamera(origin.Camera);
            var leftHand = ScenarioSceneKit.FindChildRecursive(origin.transform, "Left Controller");

            var root = ScenarioSceneKit.CreateRoot(scene);
            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_PlayerStart, Quaternion.identity);
            var manager = ScenarioSceneKit.CreateCore(root.transform, origin, data.scenario, true, out var feedback);

            var env = UIBuilderUtil.Child(root.transform, "Environment");
            var iceLayers = CreateIce(env.transform);
            var ubsZoneBounds = CreateUbs(env.transform, out var ubsRoof);
            var busRoof = CreateBusStop(env.transform);
            var shackRoof = CreateShack(env.transform);
            CreateBillboardAndTower(env.transform);
            CreateHailBlockers(env.transform, hailLayer, ubsRoof, busRoof, shackRoof);
            var hail = CreateHail(env.transform, hailLayer, iceLayers, data.aguardar, k_Ubs + new Vector3(0f, 3.2f, 0f), k_SpotC + new Vector3(0f, 1.2f, 0f));

            ScenarioSceneKit.SetupHouseLights(house, root.transform, null);

            var actors = UIBuilderUtil.Child(root.transform, "Atores");
            var joaquim = CreateJoaquim(actors.transform, data.joaquim, out var joaquimInteractable);
            var car = CreateCar(actors.transform, house);
            var mover = car.AddComponent<SimpleMover>();
            var spotC = UIBuilderUtil.Child(actors.transform, "Alvo_VagaC");
            spotC.transform.SetPositionAndRotation(k_SpotC, Quaternion.Euler(0f, 90f, 0f));
            mover.target = spotC.transform;
            mover.seconds = 6f;
            ScenarioSceneKit.CreateNpc(actors.transform, "Carlos", k_Carlos, 180f, new Color(0.2f, 0.45f, 0.3f), false, "Onde eu deixo o carro?", out var carlosSpeech);
            carlosSpeech.gameObject.SetActive(true);

            var spots = UIBuilderUtil.Child(root.transform, "Vagas");
            var optA = CreateSpot(spots.transform, "Vaga_A_Placa", k_SpotA, false, "vaga_placa",
                "Não estacione perto de placas de propaganda: elas podem cair com o vento e o granizo.", data.vaga,
                "Estacione longe de placas de propaganda e torres de transmissão.");
            var optB = CreateSpot(spots.transform, "Vaga_B_Torre", k_SpotB, false, "vaga_torre",
                "Não estacione perto de torres de transmissão: há risco de queda e de danos à rede elétrica.", data.vaga,
                "Estacione longe de torres de transmissão e de placas de propaganda.");
            var optC = CreateSpot(spots.transform, "Vaga_C_Rua", k_SpotC, true, "", "Carlos: boa! Longe da placa e da torre.", data.vaga);

            var wrongShelters = UIBuilderUtil.Child(root.transform, "ZonasAbrigoErrado");
            CreateTreeZones(wrongShelters.transform, praca);
            var pergola = ScenarioSceneKit.ModelPart(praca, "DC_PRACA_PergoladoMadeira");
            if (pergola != null)
            {
                var pb = ScenarioSceneKit.PartBounds(pergola);
                ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_Pergolado", new Vector3(pb.center.x, 1.5f, pb.center.z), new Vector3(pb.size.x - 0.6f, 3f, pb.size.z - 0.5f),
                    "abrigo_pergolado", "O pergolado tem ripas abertas: não protege do granizo. Procure uma construção resistente.",
                    ActionKind.ErroGrave, "Abrigue-se em construção de alvenaria, sem risco de destelhamento.");
            }
            ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_PontoOnibus", k_BusStop + new Vector3(0f, 1.5f, 0f), new Vector3(3.2f, 3f, 2f),
                "abrigo_metalico", "Evite estruturas metálicas: elas podem cair ou ser danificadas pelo granizo.",
                ActionKind.ErroGrave, "Procure uma construção resistente, longe de estruturas metálicas e de árvores.");
            ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_Barraca", k_Shack + new Vector3(0f, 1.5f, 0f), new Vector3(2.8f, 3f, 2.8f),
                "abrigo_precario", "Não se abrigue em construções precárias: o telhado pode ser arrancado.",
                ActionKind.ErroGrave, "Procure uma construção resistente, sem risco de destelhamento.");
            wrongShelters.SetActive(false);

            var openPlaza = ScenarioSceneKit.CreateWrongZone(root.transform, "ZonaPracaAberta", new Vector3(-4.4f, 1.5f, 18f), new Vector3(24.4f, 3f, 14f),
                "sair_durante_granizo", "Espere o granizo passar dentro do abrigo.",
                ActionKind.ErroGrave, "Fique no abrigo até a chuva de granizo terminar.");
            openPlaza.gameObject.SetActive(false);

            var after = UIBuilderUtil.Child(root.transform, "Depois");
            CreateDamagedCeiling(after.transform, data.forro, out var ceilingInteractable);
            CreateLadder(after.transform);
            CreateRoofDamage(after.transform);

            ScenarioSceneKit.CreateWrongZone(after.transform, "Zona_RuaForaDaFaixa", new Vector3(2.6f, 1.2f, 9.7f), new Vector3(11f, 2.4f, 4.4f),
                "atalho_escorregadio", "Cuidado: com gelo, o meio-fio e o asfalto ficam escorregadios. Atravesse pela faixa de pedestres.",
                ActionKind.ErroLeve, "Volte pela calçada e atravesse na faixa de pedestres.");
            after.SetActive(false);

            var bedroomZone = ScenarioSceneKit.CreateWrongZone(root.transform, "Zona_VoltarAoQuarto", new Vector3(-2.2f, 1.2f, -8.47f), new Vector3(3.9f, 2.4f, 3.8f),
                "voltar_ao_quarto", "Não volte ao cômodo com risco de desabamento até a vistoria das autoridades.",
                ActionKind.ErroGrave, "Fique fora do cômodo e espere a vistoria da Defesa Civil.");
            bedroomZone.gameObject.SetActive(false);

            var gateZone = ScenarioSceneKit.CreateTriggerZone(env.transform, "Zona_Portao", k_GateZone, new Vector3(1.8f, 2.4f, 1.4f));
            var routeGroup = UIBuilderUtil.Child(env.transform, "RotaSegura");
            ScenarioSceneKit.CreateRouteChevrons(routeGroup.transform, Snap(0.03f,
                new Vector2(9.5f, 16f), new Vector2(7f, 15.2f), new Vector2(6.6f, 13f), new Vector2(3.5f, 12.5f), new Vector2(0f, 12.5f),
                new Vector2(-3.2f, 12.5f), new Vector2(-4.9f, 11.4f), new Vector2(-4.9f, 9.6f), new Vector2(-4.9f, 7.8f), new Vector2(-4.9f, 6f)));

            var validators = UIBuilderUtil.Child(root.transform, "Validators");

            var v1 = UIBuilderUtil.Child(validators.transform, "Validator_01_Vaga").AddComponent<StepCompleteOnChoice>();
            v1.scenarioManager = manager; v1.step = data.vaga;
            v1.options.AddRange(new[] { optA, optB, optC });
            UnityEventTools.AddPersistentListener(v1.onCorrect, mover.MoveToTarget);

            var v2 = UIBuilderUtil.Child(validators.transform, "Validator_02_Joaquim").AddComponent<StepCompleteOnInteractCount>();
            v2.scenarioManager = manager; v2.step = data.joaquim;
            v2.interactables.Add(joaquimInteractable); v2.requiredCount = 1;

            var ubsZone3 = ScenarioSceneKit.CreateTriggerZone(validators.transform, "Validator_03_Abrigo", ubsZoneBounds.center, ubsZoneBounds.size);
            var v3 = ubsZone3.AddComponent<StepCompleteOnCompanionInZone>();
            v3.scenarioManager = manager; v3.step = data.abrigo; v3.companion = joaquim;
            var ubsTarget = ubsZone3.AddComponent<ObjectiveTarget>();
            ubsTarget.step = data.abrigo; ubsTarget.markerHeightOffset = 0.5f;

            var ubsZone4 = ScenarioSceneKit.CreateTriggerZone(validators.transform, "Validator_04_Aguardar", ubsZoneBounds.center, ubsZoneBounds.size);
            var v4 = ubsZone4.AddComponent<StepCompleteOnStayInZone>();
            v4.scenarioManager = manager; v4.step = data.aguardar; v4.requiredSeconds = 15f;

            var v5 = gateZone.AddComponent<StepCompleteOnTriggerZone>();
            v5.scenarioManager = manager; v5.step = data.voltar;
            var gateTarget = gateZone.AddComponent<ObjectiveTarget>();
            gateTarget.step = data.voltar; gateTarget.markerHeightOffset = 0.6f;
            gateTarget.showWhileActive.Add(routeGroup);

            var v6 = UIBuilderUtil.Child(validators.transform, "Validator_06_Forro").AddComponent<StepCompleteOnInteractCount>();
            v6.scenarioManager = manager; v6.step = data.forro;
            v6.interactables.Add(ceilingInteractable); v6.requiredCount = 1;

            var livingZone = ScenarioSceneKit.CreateTriggerZone(validators.transform, "Validator_07_Sair", new Vector3(-2.8f, 1.2f, -2.5f), new Vector3(5f, 2.4f, 4.4f));
            var v7 = livingZone.AddComponent<StepCompleteOnTriggerZone>();
            v7.scenarioManager = manager; v7.step = data.sair;
            var livingTarget = livingZone.AddComponent<ObjectiveTarget>();
            livingTarget.step = data.sair; livingTarget.markerHeightOffset = 0.3f;
            livingZone.SetActive(false);

            var v8 = UIBuilderUtil.Child(validators.transform, "Validator_08_Comunicar").AddComponent<StepCompleteOnButtonPress>();
            v8.scenarioManager = manager; v8.step = data.comunicar;

            var phases = UIBuilderUtil.Child(root.transform, "Phases");
            var caption = ScenarioSceneKit.CreatePhaseCaption(camera);
            var switcher = phases.AddComponent<ScenarioPhaseSwitcher>();
            switcher.caption = caption;

            var hailPhase = new ScenarioPhaseSwitcher.Phase { name = "Granizo", fadeSeconds = 0f,
                info = "Defesa Civil: granizo caindo no bairro. Procure um local seguro e resistente." };
            hailPhase.triggerSteps.AddRange(new[] { data.vaga, data.joaquim });
            hailPhase.activate.Add(wrongShelters);
            UnityEventTools.AddPersistentListener(hailPhase.onEnter, hail.StartHail);
            UnityEventTools.AddFloatPersistentListener(hailPhase.onEnter, feedback.SetRainVolume, 0.4f);
            switcher.phases.Add(hailPhase);

            var afterPhase = new ScenarioPhaseSwitcher.Phase { name = "Depois", fadeSeconds = 1f, caption = "Alguns minutos depois…",
                info = "Seu Joaquim: obrigado! Minha filha já está vindo me buscar." };
            afterPhase.triggerSteps.Add(data.aguardar);
            afterPhase.activate.Add(after);
            afterPhase.deactivate.Add(wrongShelters);
            afterPhase.deactivate.Add(openPlaza.gameObject);
            UnityEventTools.AddFloatPersistentListener(afterPhase.onEnter, feedback.SetRainVolume, 0.05f);
            switcher.phases.Add(afterPhase);

            AddActivate(phases.transform, "Ativar_PracaAberta", data.abrigo, openPlaza.gameObject);
            AddActivate(phases.transform, "Ativar_ZonaSala", data.forro, livingZone);
            AddActivate(phases.transform, "Ativar_ZonaQuarto", data.sair, bedroomZone.gameObject);

            var ui = UIBuilderUtil.Child(root.transform, "UI");
            var options = ScenarioSceneKit.CreateOptionsPanel(ui.transform, camera, true);
            ScenarioSceneKit.CreateWristUI(leftHand != null ? leftHand : camera, manager, null, options, true, out var callButton);
            var callPanel = ScenarioSceneKit.CreateCallPanel(ui.transform, camera, callButton,
                "Defesa Civil, obrigado pelo aviso. Não entre no quarto e não suba no telhado. Uma equipe vai vistoriar a casa.",
                "Corpo de Bombeiros, ocorrência registrada. Mantenha todos fora do quarto e longe do telhado.",
                "Vamos repassar, mas para risco de desabamento de telhado ligue para a Defesa Civil (199) ou para os Bombeiros (193).",
                "Vamos repassar, mas para risco de desabamento de telhado ligue para a Defesa Civil (199) ou para os Bombeiros (193).",
                "199", "193");
            UnityEventTools.AddPersistentListener(callPanel.onAcceptedCall, v8.OnButtonPressed);
            ScenarioSceneKit.CreateLoadingPanel(ui.transform, camera);
            ScenarioSceneKit.CreateIntroPanel(ui.transform, camera, manager, "Granizo", k_IntroText);
            ScenarioSceneKit.CreateEndReportPanel(ui.transform, camera, manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Gameplay do Granizo montado e cena salva.");
        }

        static void SetupStormSky()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.45f, 0.5f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.37f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.21f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = k_StormSky;
            RenderSettings.fogDensity = 0.012f;
        }

        static void SetupStormCamera(Camera camera)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = k_StormSky;
        }

        static void AddActivate(Transform parent, string name, MissionStepSO step, params GameObject[] activate)
        {
            var a = UIBuilderUtil.Child(parent, name).AddComponent<ActivateOnStepCompleted>();
            a.step = step;
            a.activate.AddRange(activate);
            foreach (var go in activate) if (go != null) go.SetActive(false);
        }

        static Vector3[] Snap(float lift, params Vector2[] xz)
        {
            var points = new Vector3[xz.Length];
            for (var i = 0; i < xz.Length; i++)
                points[i] = ScenarioSceneKit.OnSurface(xz[i].x, xz[i].y, 0.3f, lift);
            return points;
        }

        static Renderer IceLayer(Transform parent, string name, Vector3 center, Vector2 size)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Quad, name, parent, center, new Vector3(90f, 0f, 0f), new Vector3(size.x, size.y, 1f), ScenarioSceneKit.MatIce, false);
            var r = go.GetComponent<MeshRenderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return r;
        }

        static List<Renderer> CreateIce(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "Gelo");
            return new List<Renderer>
            {
                IceLayer(g.transform, "Gelo_PracaECalcadas", new Vector3(-5.4f, 0.035f, 27.05f), new Vector2(67.2f, 29.9f)),
                IceLayer(g.transform, "Gelo_CalcadaCasa", new Vector3(-5.4f, 0.0f, 6.2f), new Vector2(67.2f, 2.0f)),
                IceLayer(g.transform, "Gelo_Rua", new Vector3(-5.4f, -0.045f, 9.65f), new Vector2(67.2f, 4.9f)),
            };
        }

        static void HideNeighborForUbs(GameObject praca)
        {
            foreach (var part in new[] { "DC_PRACA_VizinhoOeste0_Corpo", "DC_PRACA_VizinhoOeste0_Fachada", "DC_PRACA_VizinhoOeste0_Rodape", "DC_PRACA_VizinhoOeste0_Telhado" })
            {
                var t = ScenarioSceneKit.ModelPart(praca, part);
                if (t != null)
                    Object.DestroyImmediate(t.gameObject);
            }
        }

        static Bounds CreateUbs(Transform parent, out GameObject roof)
        {
            var g = ScenarioSceneKit.CreateModelVisual(parent, "UBS",
                $"{ScenarioSceneKit.k_ItemsFolder}/UBS.fbx", new Vector3(k_Ubs.x, 0f, k_Ubs.z), new Vector3(0f, k_UbsYaw, 0f));

            roof = null;
            var interior = new Bounds();
            var achouInterior = false;
            foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                UIBuilderUtil.MarkStatic(mf.gameObject);

                if (mf.name.StartsWith("Laje")) roof = mf.gameObject;
                if (mf.name.StartsWith("PisoUBS") || mf.name.StartsWith("SalaEspera")
                    || mf.name.StartsWith("Corredor") || mf.name.StartsWith("Consultorio"))
                {
                    ScenarioSceneKit.AddTeleportArea(mf.gameObject);
                    var b = mf.GetComponent<Renderer>().bounds;
                    if (!achouInterior) { interior = b; achouInterior = true; } else interior.Encapsulate(b);
                }
            }

            foreach (var luz in g.GetComponentsInChildren<Light>(true))
            {
                luz.lightmapBakeType = LightmapBakeType.Realtime;
                luz.shadows = LightShadows.None;
            }

            if (roof == null) roof = g;
            if (!achouInterior) interior = new Bounds(new Vector3(k_Ubs.x, 1.5f, k_Ubs.z), k_UbsSize);
            return new Bounds(new Vector3(interior.center.x, 1.5f, interior.center.z),
                new Vector3(Mathf.Max(1f, interior.size.x - 0.6f), 3f, Mathf.Max(1f, interior.size.z - 0.6f)));
        }

        static GameObject CreateBusStop(Transform parent)
        {
            var g = ScenarioSceneKit.CreateModelVisual(parent, "PontoOnibus",
                $"{ScenarioSceneKit.k_ItemsFolder}/PontoOnibus.fbx",
                ScenarioSceneKit.OnSurface(k_BusStop.x, k_BusStop.z, 2f, 0f), new Vector3(0f, k_BusStopYaw, 0f));

            GameObject roof = null;
            foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                if (mf.name.StartsWith("Cobertura")) roof = mf.gameObject;
                UIBuilderUtil.MarkStatic(mf.gameObject);
            }
            return roof != null ? roof : g;
        }

        static GameObject CreateShack(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "Barraca");
            for (var i = 0; i < 4; i++)
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Poste" + i, g.transform, k_Shack + new Vector3(i % 2 == 0 ? -1.2f : 1.2f, 1.1f, i < 2 ? -1.2f : 1.2f), Vector3.zero, new Vector3(0.1f, 2.2f, 0.1f), ScenarioSceneKit.MatWood);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Balcao", g.transform, k_Shack + new Vector3(0f, 0.5f, -1.1f), Vector3.zero, new Vector3(2.4f, 1f, 0.5f), ScenarioSceneKit.MatWood);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Fundo", g.transform, k_Shack + new Vector3(0f, 1.1f, 1.25f), Vector3.zero, new Vector3(2.4f, 2.2f, 0.05f), ScenarioSceneKit.MatWood);
            var roof = UIBuilderUtil.Primitive(PrimitiveType.Cube, "TelhadoZinco", g.transform, k_Shack + new Vector3(0.2f, 2.3f, 0f), new Vector3(0f, 0f, 6f), new Vector3(3f, 0.04f, 3f), ScenarioSceneKit.MatMetal);
            UIBuilderUtil.WorldText(g.transform, "Label", "BARRACA\n<size=60%>madeira e zinco solto</size>", 0.8f, Color.white, k_Shack + new Vector3(0f, 2.7f, -1.4f), Vector3.zero, new Vector2(2.5f, 0.4f));
            UIBuilderUtil.MarkStatic(g);
            return roof;
        }

        static void CreateTreeZones(Transform parent, GameObject praca)
        {
            foreach (var r in praca.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.name.StartsWith("DC_PRACA_Arvore"))
                    continue;
                var b = r.bounds;
                ScenarioSceneKit.CreateWrongZone(parent, "Zona_" + r.name.Replace("DC_PRACA_", ""), new Vector3(b.center.x, 1.5f, b.center.z),
                    new Vector3(Mathf.Min(b.size.x, 3.4f), 3f, Mathf.Min(b.size.z, 3.4f)),
                    "abrigo_arvore", "Não fique debaixo de árvores: galhos podem quebrar com o granizo e o vento.",
                    ActionKind.ErroGrave, "Nunca se abrigue sob árvores: procure uma construção de alvenaria.");
            }
        }

        static void CreateBillboardAndTower(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "PlacaETorre");
            var billboard = ScenarioSceneKit.CreateModelVisual(g.transform, "Placa",
                $"{ScenarioSceneKit.k_ItemsFolder}/Placa.fbx",
                ScenarioSceneKit.OnSurface(k_Billboard.x, k_Billboard.z, 2f, 0f), new Vector3(0f, k_BillboardYaw, 0f));
            foreach (var mf in billboard.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }

            var tower = UIBuilderUtil.Child(g.transform, "TorreTransmissao");
            tower.transform.position = k_Tower;
            for (var i = 0; i < 4; i++)
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Perna" + i, tower.transform, k_Tower + new Vector3(i % 2 == 0 ? -1.2f : 1.2f, 7f, i < 2 ? -1.2f : 1.2f), new Vector3(i < 2 ? 4f : -4f, 0f, i % 2 == 0 ? -4f : 4f), new Vector3(0.15f, 14f, 0.15f), ScenarioSceneKit.MatMetal);
            for (var y = 3f; y <= 13f; y += 3f)
            {
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Travessa", tower.transform, k_Tower + new Vector3(0f, y, -1f), Vector3.zero, new Vector3(2.4f, 0.1f, 0.1f), ScenarioSceneKit.MatMetal, false);
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Travessa", tower.transform, k_Tower + new Vector3(0f, y, 1f), Vector3.zero, new Vector3(2.4f, 0.1f, 0.1f), ScenarioSceneKit.MatMetal, false);
            }
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Braco", tower.transform, k_Tower + new Vector3(0f, 12f, 0f), Vector3.zero, new Vector3(6f, 0.15f, 0.15f), ScenarioSceneKit.MatMetal, false);
            UIBuilderUtil.WorldText(tower.transform, "Label", "TORRE DE TRANSMISSÃO", 1.5f, Color.white, k_Tower + new Vector3(0f, 5f, -2f), Vector3.zero, new Vector2(5f, 0.6f));
            UIBuilderUtil.MarkStatic(g);
        }

        static Bounds RoofBounds(GameObject roof)
        {
            var b = new Bounds(roof.transform.position, roof.transform.localScale);
            var first = true;
            foreach (var r in roof.GetComponentsInChildren<Renderer>(true))
            {
                if (first) { b = r.bounds; first = false; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        static void CreateHailBlockers(Transform parent, int layer, GameObject ubsRoof, GameObject busRoof, GameObject shackRoof)
        {
            var g = UIBuilderUtil.Child(parent, "HailBlockers");
            void Block(string name, Vector3 center, Vector3 size)
            {
                var go = UIBuilderUtil.Child(g.transform, name);
                go.layer = layer;
                go.transform.position = center;
                var b = go.AddComponent<BoxCollider>();
                b.size = size;
            }

            Block("Chao", new Vector3(-5f, -0.5f, 10f), new Vector3(90f, 1f, 60f));
            var laje = RoofBounds(ubsRoof);
            Block("LajeUBS", new Vector3(laje.center.x, laje.max.y - 0.6f, laje.center.z), new Vector3(laje.size.x, 1.2f, laje.size.z));
            var bus = RoofBounds(busRoof);
            Block("CoberturaPonto", new Vector3(bus.center.x, bus.max.y - 0.5f, bus.center.z), new Vector3(bus.size.x, 1f, bus.size.z));
            Block("TelhadoBarraca", shackRoof.transform.position + new Vector3(0f, -0.4f, 0f), new Vector3(3.2f, 1.2f, 3.2f));

            Block("TelhadoCasa", new Vector3(-5.5f, 3.6f, -5.3f), new Vector3(12.5f, 1.6f, 12.1f));
            Block("CoberturaGaragem", new Vector3(1.73f, 2.2f, -2.58f), new Vector3(3.7f, 0.6f, 5.3f));
        }

        static HailController CreateHail(Transform parent, int layer, List<Renderer> iceLayers, MissionStepSO stopStep, params Vector3[] soundPositions)
        {
            var g = UIBuilderUtil.Child(parent, "Granizo");
            var ctrl = g.AddComponent<HailController>();

            var psGo = UIBuilderUtil.Child(g.transform, "Particulas");

            psGo.transform.position = new Vector3(-2f, 7f, 17f);
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 700;
            main.startLifetime = 3f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.95f, 0.98f, 1f, 0.95f);
            main.playOnAwake = false;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 0.5f, 24f);
            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.quality = ParticleSystemCollisionQuality.Low;
            collision.collidesWith = 1 << layer;
            collision.bounce = 0.3f;
            collision.lifetimeLoss = 0.5f;
            collision.maxCollisionShapes = 64;
            var r = psGo.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = UIBuilderUtil.GetOrCreateMaterial("Mat_PedraGelo", new Color(0.95f, 0.98f, 1f, 0.95f), true, true);
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            ctrl.hail = ps;
            ctrl.iceLayers.AddRange(iceLayers);
            ctrl.stopStep = stopStep;

            for (var i = 0; i < soundPositions.Length; i++)
            {
                var sGo = UIBuilderUtil.Child(g.transform, "SomTelhado_" + i);
                sGo.transform.position = soundPositions[i];
                var src = sGo.AddComponent<AudioSource>();
                src.spatialBlend = 1f; src.minDistance = 2f; src.maxDistance = 30f; src.volume = 0f; src.loop = true; src.playOnAwake = false;
                var loop = sGo.AddComponent<SimpleLoopSound>();
                loop.source = src; loop.kind = SimpleLoopSound.ClipKind.Hail; loop.manageVolume = false;
                ctrl.roofSounds.Add(src);
            }
            return ctrl;
        }

        static CompanionFollower CreateJoaquim(Transform parent, MissionStepSO step, out XRSimpleInteractable interactable)
        {
            interactable = ScenarioSceneKit.CreateNpc(parent, "SeuJoaquim", k_JoaquimBench, 132f, new Color(0.45f, 0.4f, 0.35f), true, "Obrigado, meu filho! Vamos.", out var speech);
            var npc = interactable.gameObject;
            if (npc.transform.Find("Modelo") == null)
            {
                var cap = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Boina", npc.transform, Vector3.zero, Vector3.zero, new Vector3(0.28f, 0.02f, 0.28f), ScenarioSceneKit.MatFlashlight, false);
                cap.transform.localPosition = new Vector3(0f, 1.71f, 0f);
            }
            var follower = npc.AddComponent<CompanionFollower>();
            follower.talkInteractable = interactable;
            follower.speed = 0.8f;
            follower.maxLeadDistance = 3f;
            follower.speechBubble = speech;
            var routes = UIBuilderUtil.Child(parent, "Rota_Joaquim");

            follower.waypoints.AddRange(ScenarioSceneKit.CreateWaypoints(routes.transform, "WP",
                k_JoaquimBench, new Vector3(-5f, 0f, 16.5f), new Vector3(-5f, 0f, 12.35f), new Vector3(5.9f, 0f, 12.35f),
                new Vector3(6.9f, 0f, 13.5f), new Vector3(6.9f, 0f, k_UbsDoorZ), new Vector3(9.5f, k_UbsFloorY, k_UbsDoorZ), new Vector3(13f, k_UbsFloorY, 19f)));
            var target = npc.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.5f;
            return follower;
        }

        static GameObject CreateCar(Transform parent, GameObject house)
        {
            var car = UIBuilderUtil.Child(parent, "Carro_Carlos");
            car.transform.SetPositionAndRotation(k_CarStart, Quaternion.Euler(0f, 90f, 0f));

            var source = ScenarioSceneKit.ModelPart(house, "DC_CASA_Carro");
            if (source == null)
            {
                var body = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Carroceria", car.transform, Vector3.zero, Vector3.zero, new Vector3(1.9f, 1.5f, 4.2f), ScenarioSceneKit.MatCloth);
                body.transform.localPosition = new Vector3(0f, 0.75f, 0f);
                return car;
            }

            var sb = ScenarioSceneKit.PartBounds(source);
            var copy = Object.Instantiate(source.gameObject);
            copy.name = "Modelo";
            foreach (var c in copy.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(c);
            foreach (var t in copy.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            copy.transform.SetParent(car.transform, false);

            copy.transform.localRotation = source.rotation;
            copy.transform.localPosition = source.position - new Vector3(sb.center.x, sb.min.y, sb.center.z);

            var box = car.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, sb.size.y * 0.5f, 0f);
            box.size = new Vector3(sb.size.x, sb.size.y, sb.size.z);
            return car;
        }

        static DecisionOption CreateSpot(Transform parent, string name, Vector3 pos, bool correct, string mistakeId, string feedback, MissionStepSO step, string advice = "")
        {
            var g = ScenarioSceneKit.CreateModelVisual(parent, name,
                $"{ScenarioSceneKit.k_ItemsFolder}/Vaga.fbx", pos, new Vector3(0f, k_SpotYaw, 0f));

            var sign = ScenarioSceneKit.ModelPart(g, "Placa").gameObject;
            var signBounds = ScenarioSceneKit.PartBounds(sign.transform);
            var local = sign.transform.InverseTransformVector(signBounds.size + new Vector3(0.1f, 0.1f, 0.1f));
            var box = sign.AddComponent<BoxCollider>();
            box.center = sign.transform.InverseTransformPoint(signBounds.center);
            box.size = new Vector3(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z));

            UIBuilderUtil.WorldText(g.transform, "Texto", "Aqui?", 1.6f, Color.white,
                signBounds.center + new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector2(0.7f, 0.4f))
                .gameObject.AddComponent<FacePlayer>().yawOnly = true;
            sign.AddComponent<XRSimpleInteractable>();
            var opt = sign.AddComponent<DecisionOption>();
            opt.isCorrect = correct;
            opt.mistakeId = mistakeId;
            opt.feedback = feedback;
            opt.advice = advice;
            var target = sign.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.3f;
            return opt;
        }

        static InspectableSign CreateDamagedCeiling(Transform parent, MissionStepSO step, out XRSimpleInteractable interactable)
        {
            var g = ScenarioSceneKit.CreateModelVisual(parent, "ForroCedendo",
                $"{ScenarioSceneKit.k_ItemsFolder}/Forro.fbx", k_Ceiling, Vector3.zero);

            ScenarioSceneKit.ModelPart(g, "Barriga").gameObject.SetActive(false);

            var sag = ScenarioSceneKit.ModelPart(g, "Modelo").gameObject;
            var sagLocal = sag.GetComponent<MeshFilter>().sharedMesh.bounds;
            var sagBox = sag.AddComponent<BoxCollider>();
            sagBox.center = sagLocal.center;
            sagBox.size = sagLocal.size;

            var sagCenter = sag.transform.TransformPoint(sagLocal.center);
            var sagBottom = sagCenter.y - sagLocal.extents.y;

            interactable = sag.AddComponent<XRSimpleInteractable>();
            var sign = sag.AddComponent<InspectableSign>();
            sign.explanation = "Forro cedendo e goteira: o telhado pode estar comprometido. Saia do quarto.";
            sign.highlight = sag.GetComponent<Renderer>();
            var label = UIBuilderUtil.WorldText(g.transform, "Etiqueta", "", 0.6f, new Color(1f, 0.8f, 0.4f), new Vector3(sagCenter.x, sagBottom - 0.32f, sagCenter.z), Vector3.zero, new Vector2(1.8f, 0.5f));
            label.gameObject.AddComponent<FacePlayer>().yawOnly = true;
            label.gameObject.SetActive(false);
            sign.label = label;
            var target = sag.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = -0.6f;

            var dripGo = UIBuilderUtil.Child(g.transform, "Goteira");
            dripGo.transform.position = new Vector3(sagCenter.x, sagBottom - 0.02f, sagCenter.z);
            var ps = dripGo.AddComponent<ParticleSystem>();
            var main = ps.main; main.startSize = 0.03f; main.startLifetime = 0.7f; main.startSpeed = 0f; main.gravityModifier = 1f; main.maxParticles = 20;
            main.startColor = new Color(0.6f, 0.75f, 0.95f, 0.9f); main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 2.5f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.01f;
            var pr = dripGo.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = ScenarioSceneKit.MatBottle;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poca", g.transform, new Vector3(sagCenter.x, 0.02f, sagCenter.z), Vector3.zero, new Vector3(0.6f, 0.003f, 0.5f), ScenarioSceneKit.MatWater, false);
            return sign;
        }

        static void CreateLadder(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "EscadaTelhado");
            g.transform.SetPositionAndRotation(k_Ladder, Quaternion.Euler(-20f, 0f, 0f));
            var rail1 = UIBuilderUtil.Primitive(PrimitiveType.Cube, "LateralA", g.transform, Vector3.zero, Vector3.zero, new Vector3(0.06f, 3.4f, 0.06f), ScenarioSceneKit.MatMetal, false);
            rail1.transform.localPosition = new Vector3(-0.25f, 1.7f, 0f);
            var rail2 = UIBuilderUtil.Primitive(PrimitiveType.Cube, "LateralB", g.transform, Vector3.zero, Vector3.zero, new Vector3(0.06f, 3.4f, 0.06f), ScenarioSceneKit.MatMetal, false);
            rail2.transform.localPosition = new Vector3(0.25f, 1.7f, 0f);
            for (var i = 0; i < 9; i++)
            {
                var rung = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Degrau" + i, g.transform, Vector3.zero, Vector3.zero, new Vector3(0.5f, 0.04f, 0.04f), ScenarioSceneKit.MatMetal, false);
                rung.transform.localPosition = new Vector3(0f, 0.3f + i * 0.36f, 0f);
            }
            var col = g.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.7f, 0f);
            col.size = new Vector3(0.6f, 3.4f, 0.15f);
            g.AddComponent<XRSimpleInteractable>();
            var wrong = g.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "subir_telhado";
            wrong.message = "Não suba em telhados molhados: há risco de queda. Peça a vistoria às autoridades.";
            wrong.kind = ActionKind.ErroGrave;
            wrong.advice = "Não suba no telhado: ligue 199 ou 193 e peça vistoria.";
            UIBuilderUtil.WorldText(g.transform, "Label", "Subir no telhado?", 0.7f, new Color(1f, 0.75f, 0.35f), k_Ladder + new Vector3(0f, 2.2f, 0.6f), Vector3.zero, new Vector2(1.4f, 0.2f))
                .gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static void CreateRoofDamage(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "TelhasQuebradas");
            var dark = UIBuilderUtil.GetOrCreateMaterial("Mat_TelhaQuebrada", new Color(0.25f, 0.15f, 0.1f), false, false, 0.05f);

            const float ridgeX = -5.5f, ridgeY = 5.17f, slope = 0.379f, angle = 20.8f;
            foreach (var (x, z) in new[] { (-8f, -3f), (-3f, -6f), (-9.5f, -8f), (-2f, -2f), (-7f, -9f) })
            {
                var y = ridgeY - Mathf.Abs(x - ridgeX) * slope + 0.05f;
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Telha", g.transform, new Vector3(x, y, z), new Vector3(0f, 0f, x < ridgeX ? angle : -angle), new Vector3(0.4f, 0.03f, 0.5f), dark, false);
            }
            UIBuilderUtil.WorldText(g.transform, "Etiqueta", "<size=70%>telhas quebradas:\nmantenha o madeiramento em dia</size>", 0.8f, new Color(1f, 0.85f, 0.4f),
                new Vector3(-5.5f, 3.4f, 2f), Vector3.zero, new Vector2(2.4f, 0.5f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }
    }
}
#endif
