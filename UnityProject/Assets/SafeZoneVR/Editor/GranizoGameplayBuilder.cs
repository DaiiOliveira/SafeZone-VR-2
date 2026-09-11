#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Monta (idempotente) a fase Granizo: praça do bairro, UBS de alvenaria, abrigos errados, vagas, NPCs,
    /// granizo por partículas, gelo no chão, fase "Depois" na casa (prefab Casa_SafeZone), validadores e UI.
    /// </summary>
    public static class GranizoGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Granizo_Praca.unity";
        const string k_HailLayer = "HailBlockers";

        const float k_PlazaY = 0.15f;
        static readonly Vector3 k_PlayerStart = new Vector3(-52f, k_PlazaY, 30f);
        static readonly Vector3 k_JoaquimBench = new Vector3(-49f, k_PlazaY, 33f);
        static readonly Vector3 k_Ubs = new Vector3(-55f, 0f, 43f);          // centro; x −60..−50, z 40..46
        static readonly Vector3 k_BigTree = new Vector3(-56f, k_PlazaY, 36f);
        static readonly Vector3 k_BusStop = new Vector3(-47f, k_PlazaY, 29.5f);
        static readonly Vector3 k_Shack = new Vector3(-66f, k_PlazaY, 33f);
        static readonly Vector3 k_CarStart = new Vector3(-62f, 0f, 24f);
        static readonly Vector3 k_Carlos = new Vector3(-60.3f, 0f, 25.6f);
        static readonly Vector3 k_SpotA = new Vector3(-70f, 0f, 22f);       // placa de propaganda
        static readonly Vector3 k_SpotB = new Vector3(-76f, 0f, 24.5f);     // torre de transmissão (torre em x −76, z 30)
        static readonly Vector3 k_SpotC = new Vector3(-40f, 0f, 24f);       // estacionamento do mercado
        static readonly Vector3 k_Billboard = new Vector3(-70f, 0f, 19.2f);
        static readonly Vector3 k_Tower = new Vector3(-76f, 0f, 30f);
        static readonly Vector3 k_RampCenter = new Vector3(-44f, 0.08f, 26f);
        static readonly Vector3 k_Ladder = new Vector3(-34.8f, 0f, 15.25f);
        static readonly Vector3 k_Ceiling = new Vector3(-30.9f, 1.98f, 7.8f);
        static readonly Vector3 k_GateZone = new Vector3(-33.0f, 1.2f, 17.7f);

        const string k_IntroText =
            "<b>Alerta da Defesa Civil (SMS 40199):</b> tempestade com possibilidade de <b>granizo</b> no seu bairro nos próximos minutos.\n" +
            "Você está na praça, a duas quadras de casa. Procure um local seguro e ajude quem estiver por perto.\n\n" +
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
            var hailLayer = ScenarioSceneKit.EnsureLayer(k_HailLayer);

            var house = ScenarioSceneKit.EnsureHouseInstance(scene);
            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_PlayerStart, Quaternion.identity);
            var camera = origin.Camera.transform;
            var leftHand = ScenarioSceneKit.FindChildRecursive(origin.transform, "Left Controller");

            var root = ScenarioSceneKit.CreateRoot(scene);
            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_PlayerStart, Quaternion.identity);
            var manager = ScenarioSceneKit.CreateCore(root.transform, origin, data.scenario, true, out var feedback);

            // ---- Ambiente
            var env = UIBuilderUtil.Child(root.transform, "Environment");
            ScenarioSceneKit.SetupTeleportFloors();
            ScenarioSceneKit.AddFurnitureColliders();
            ScenarioSceneKit.MarkHouseStatic();
            ScenarioSceneKit.CreateStreet(env.transform, -50f, 60f);          // rua x −80..−20
            var iceLayers = new List<Renderer>();
            CreatePlaza(env.transform, iceLayers);
            CreateNorthSidewalk(env.transform, iceLayers);
            CreateRamp(env.transform, iceLayers);
            CreateMarketLot(env.transform);
            var ubsZoneBounds = CreateUbs(env.transform, out var ubsRoof);
            var busRoof = CreateBusStop(env.transform);
            var shackRoof = CreateShack(env.transform);
            CreateTrees(env.transform);
            CreateBillboardAndTower(env.transform);
            CreateHailBlockers(env.transform, hailLayer, house, ubsRoof, busRoof, shackRoof);
            var hail = CreateHail(env.transform, hailLayer, iceLayers, data.aguardar, k_Ubs + new Vector3(0f, 3.2f, 0f), k_SpotC + new Vector3(0f, 1.2f, 0f));

            // ---- NPCs e carro
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

            // ---- Vagas (missão 1)
            var spots = UIBuilderUtil.Child(root.transform, "Vagas");
            var optA = CreateSpot(spots.transform, "Vaga_A_Placa", k_SpotA, false, "vaga_placa",
                "Não estacione perto de placas de propaganda: elas podem cair com o vento e o granizo.", data.vaga);
            var optB = CreateSpot(spots.transform, "Vaga_B_Torre", k_SpotB, false, "vaga_torre",
                "Não estacione perto de torres de transmissão: há risco de queda e de danos à rede elétrica.", data.vaga);
            var optC = CreateSpot(spots.transform, "Vaga_C_Mercado", k_SpotC, true, "", "Carlos: boa! Longe da placa e da torre.", data.vaga);

            // ---- Zonas de abrigo errado (fase Granizo) e praça aberta
            var wrongShelters = UIBuilderUtil.Child(root.transform, "ZonasAbrigoErrado");
            ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_Arvore", k_BigTree + new Vector3(0f, 1.5f, 0f), new Vector3(7f, 3f, 7f),
                "abrigo_arvore", "Não fique debaixo de árvores: galhos podem quebrar com o granizo e o vento.");
            ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_PontoOnibus", k_BusStop + new Vector3(0f, 1.5f, 0f), new Vector3(3.2f, 3f, 2f),
                "abrigo_metalico", "Evite estruturas metálicas: elas podem cair ou ser danificadas pelo granizo.");
            ScenarioSceneKit.CreateWrongZone(wrongShelters.transform, "Zona_Barraca", k_Shack + new Vector3(0f, 1.5f, 0f), new Vector3(2.8f, 3f, 2.8f),
                "abrigo_precario", "Não se abrigue em construções precárias: o telhado pode ser arrancado.");
            wrongShelters.SetActive(false);

            var openPlaza = ScenarioSceneKit.CreateWrongZone(root.transform, "ZonaPracaAberta", new Vector3(-58f, 1.5f, 34f), new Vector3(28f, 3f, 12f),
                "sair_durante_granizo", "Espere o granizo passar dentro do abrigo.");
            openPlaza.gameObject.SetActive(false);

            // ---- Fase Depois (casa)
            var after = UIBuilderUtil.Child(root.transform, "Depois");
            var ceilingSign = CreateDamagedCeiling(after.transform, data.forro, out var ceilingInteractable);
            CreateLadder(after.transform);
            CreateRoofDamage(after.transform, house);
            var rampZone = ScenarioSceneKit.CreateWrongZone(after.transform, "Zona_RampaGelada", k_RampCenter + new Vector3(0f, 1.2f, 0f), new Vector3(2.4f, 2.4f, 4.4f),
                "atalho_escorregadio", "Cuidado: o gelo deixa o piso escorregadio. Use o caminho com corrimão.");
            after.SetActive(false);

            var bedroomZone = ScenarioSceneKit.CreateWrongZone(root.transform, "Zona_VoltarAoQuarto", new Vector3(-30.9f, 1.2f, 7.8f), new Vector3(3.6f, 2.4f, 3.0f),
                "voltar_ao_quarto", "Não volte ao cômodo com risco de desabamento até a vistoria das autoridades.");
            bedroomZone.gameObject.SetActive(false);

            // ---- Rota segura (missão 5)
            var gateZone = ScenarioSceneKit.CreateTriggerZone(env.transform, "Zona_Portao", k_GateZone, new Vector3(1.6f, 2.4f, 1.4f));
            var routeGroup = UIBuilderUtil.Child(env.transform, "RotaSegura");
            ScenarioSceneKit.CreateRouteChevrons(routeGroup.transform, new[]
            {
                new Vector3(-55f, k_PlazaY + 0.02f, 38.5f), new Vector3(-53f, k_PlazaY + 0.02f, 34f), new Vector3(-50f, k_PlazaY + 0.02f, 30.5f),
                new Vector3(-48f, 0.1f, 27.5f), new Vector3(-44f, 0.1f, 27.5f), new Vector3(-40f, 0.1f, 27.5f), new Vector3(-37f, 0.1f, 27.5f),
                new Vector3(-35.5f, 0.02f, 24f), new Vector3(-34f, 0.02f, 21f), new Vector3(-33.2f, 0.1f, 19f),
            });

            // ---- Validadores
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
            v4.scenarioManager = manager; v4.step = data.aguardar; v4.requiredSeconds = 30f;

            var v5 = gateZone.AddComponent<StepCompleteOnTriggerZone>();
            v5.scenarioManager = manager; v5.step = data.voltar;
            var gateTarget = gateZone.AddComponent<ObjectiveTarget>();
            gateTarget.step = data.voltar; gateTarget.markerHeightOffset = 0.6f;
            gateTarget.showWhileActive.Add(routeGroup);

            var v6 = UIBuilderUtil.Child(validators.transform, "Validator_06_Forro").AddComponent<StepCompleteOnInteractCount>();
            v6.scenarioManager = manager; v6.step = data.forro;
            v6.interactables.Add(ceilingInteractable); v6.requiredCount = 1;

            var livingZone = ScenarioSceneKit.CreateTriggerZone(validators.transform, "Validator_07_Sair", new Vector3(-31f, 1.2f, 12.1f), new Vector3(3.6f, 2.4f, 5f));
            var v7 = livingZone.AddComponent<StepCompleteOnTriggerZone>();
            v7.scenarioManager = manager; v7.step = data.sair;
            var livingTarget = livingZone.AddComponent<ObjectiveTarget>();
            livingTarget.step = data.sair; livingTarget.markerHeightOffset = 0.3f;
            livingZone.SetActive(false);

            var v8 = UIBuilderUtil.Child(validators.transform, "Validator_08_Comunicar").AddComponent<StepCompleteOnButtonPress>();
            v8.scenarioManager = manager; v8.step = data.comunicar;

            // ---- Fases
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

            // ---- UI
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
            ScenarioSceneKit.CreateIntroPanel(ui.transform, camera, manager, "Granizo", k_IntroText);
            ScenarioSceneKit.CreateEndReportPanel(ui.transform, camera, manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Gameplay do Granizo montado e cena salva.");
        }

        // ------------------------------------------------------------------ Helpers

        static void AddActivate(Transform parent, string name, MissionStepSO step, params GameObject[] activate)
        {
            var a = UIBuilderUtil.Child(parent, name).AddComponent<ActivateOnStepCompleted>();
            a.step = step;
            a.activate.AddRange(activate);
            foreach (var go in activate) if (go != null) go.SetActive(false);
        }

        static GameObject Floor(Transform parent, string name, Vector3 center, Vector3 size, Material mat, bool teleport = true)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Cube, name, parent, center, Vector3.zero, size, mat);
            if (teleport) ScenarioSceneKit.AddTeleportArea(go);
            UIBuilderUtil.MarkStatic(go);
            return go;
        }

        static Renderer IceLayer(Transform parent, string name, Vector3 center, Vector2 size)
        {
            var go = UIBuilderUtil.Primitive(PrimitiveType.Quad, name, parent, center, new Vector3(90f, 0f, 0f), new Vector3(size.x, size.y, 1f), ScenarioSceneKit.MatIce, false);
            var r = go.GetComponent<MeshRenderer>();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return r;
        }

        static void CreatePlaza(Transform parent, List<Renderer> ice)
        {
            var g = UIBuilderUtil.Child(parent, "Praca");
            var stone = UIBuilderUtil.GetOrCreateMaterial("Mat_PisoPraca", new Color(0.62f, 0.6f, 0.56f), false, false, 0.15f);
            var grass = UIBuilderUtil.GetOrCreateMaterial("Mat_GramaPraca", new Color(0.3f, 0.5f, 0.25f), false, false, 0.05f);
            Floor(g.transform, "Piso", new Vector3(-58f, k_PlazaY * 0.5f, 37f), new Vector3(28f, k_PlazaY, 18f), stone);
            ice.Add(IceLayer(g.transform, "Gelo_Praca", new Vector3(-58f, k_PlazaY + 0.01f, 37f), new Vector2(28f, 18f)));
            // Canteiros (visuais, sem teleporte por cima).
            foreach (var (x, z) in new[] { (-66f, 42f), (-48f, 42f), (-62f, 30f) })
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Canteiro", g.transform, new Vector3(x, k_PlazaY + 0.15f, z), Vector3.zero, new Vector3(3f, 0.3f, 2f), grass);
            // Bancos.
            foreach (var (x, z, yaw) in new[] { (-49f, 33.6f, 0f), (-58f, 33f, 0f), (-66f, 38f, 90f) })
            {
                var bench = UIBuilderUtil.Child(g.transform, "Banco");
                bench.transform.SetPositionAndRotation(new Vector3(x, k_PlazaY, z), Quaternion.Euler(0f, yaw, 0f));
                var seat = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Assento", bench.transform, Vector3.zero, Vector3.zero, new Vector3(1.6f, 0.06f, 0.45f), ScenarioSceneKit.MatWood);
                seat.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                var back = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Encosto", bench.transform, Vector3.zero, Vector3.zero, new Vector3(1.6f, 0.45f, 0.05f), ScenarioSceneKit.MatWood);
                back.transform.localPosition = new Vector3(0f, 0.7f, 0.22f);
                UIBuilderUtil.MarkStatic(bench);
            }
        }

        static void CreateNorthSidewalk(Transform parent, List<Renderer> ice)
        {
            var g = UIBuilderUtil.Child(parent, "CalcadaNorte");
            Floor(g.transform, "Calcada", new Vector3(-56f, 0.04f, 27.5f), new Vector3(44f, 0.08f, 1.2f), ScenarioSceneKit.MatStreet);
            ice.Add(IceLayer(g.transform, "Gelo_Calcada", new Vector3(-56f, 0.09f, 27.5f), new Vector2(44f, 1.2f)));
            // Corrimão entre a praça e a casa (x −56..−36), lado da rua.
            var rail = UIBuilderUtil.Child(g.transform, "Corrimao");
            for (var x = -56f; x <= -36f; x += 2f)
                UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", rail.transform, new Vector3(x, 0.55f, 26.95f), Vector3.zero, new Vector3(0.05f, 0.5f, 0.05f), ScenarioSceneKit.MatMetal, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Barra", rail.transform, new Vector3(-46f, 1.0f, 26.95f), new Vector3(0f, 0f, 90f), new Vector3(0.05f, 10.2f, 0.05f), ScenarioSceneKit.MatMetal, false);
            UIBuilderUtil.MarkStatic(rail);
            // Faixa de pedestres até a calçada sul (em frente à casa).
            var stripe = UIBuilderUtil.GetOrCreateMaterial("Mat_Faixa", new Color(0.9f, 0.9f, 0.85f), false, false, 0.1f);
            for (var z = 19.5f; z <= 26.5f; z += 1f)
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Faixa", g.transform, new Vector3(-36f, 0.005f, z), Vector3.zero, new Vector3(2.4f, 0.01f, 0.4f), stripe, false);
        }

        static void CreateRamp(Transform parent, List<Renderer> ice)
        {
            var g = UIBuilderUtil.Child(parent, "RampaAtalho");
            var ramp = Floor(g.transform, "Rampa", k_RampCenter, new Vector3(2.2f, 0.06f, 4.2f), ScenarioSceneKit.MatConcrete);
            ramp.transform.rotation = Quaternion.Euler(-2f, 0f, 0f);
            ice.Add(IceLayer(g.transform, "Gelo_Rampa", k_RampCenter + new Vector3(0f, 0.045f, 0f), new Vector2(2.2f, 4.2f)));
            UIBuilderUtil.WorldText(g.transform, "Label", "ATALHO", 0.8f, Color.white, k_RampCenter + new Vector3(0f, 1.6f, 1.5f), Vector3.zero, new Vector2(1.5f, 0.3f));
        }

        static void CreateMarketLot(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "Mercado");
            Floor(g.transform, "Estacionamento", new Vector3(-40f, 0.005f, 24f), new Vector3(8f, 0.01f, 6f), UIBuilderUtil.GetOrCreateMaterial("Mat_Estacionamento", new Color(0.35f, 0.35f, 0.36f), false, false, 0.1f));
            var wall = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Fachada", g.transform, new Vector3(-40f, 2f, 29.5f), Vector3.zero, new Vector3(9f, 4f, 3f), ScenarioSceneKit.MatConcrete);
            UIBuilderUtil.WorldText(wall.transform, "Texto", "MERCADO", 3f, Color.white, new Vector3(-40f, 3.2f, 27.95f), Vector3.zero, new Vector2(6f, 1f));
            UIBuilderUtil.MarkStatic(g);
        }

        /// <summary>UBS de alvenaria: paredes, laje, piso interno com teleporte, vão da porta em x −55 (z 40) e placa.</summary>
        static Bounds CreateUbs(Transform parent, out GameObject roof)
        {
            var g = UIBuilderUtil.Child(parent, "UBS");
            var wallMat = UIBuilderUtil.GetOrCreateMaterial("Mat_UBS", new Color(0.93f, 0.9f, 0.82f), false, false, 0.1f);
            const float h = 3f;
            // Piso interno (nível da praça).
            Floor(g.transform, "PisoUBS", new Vector3(-55f, k_PlazaY * 0.5f, 43f), new Vector3(10f, k_PlazaY, 6f), ScenarioSceneKit.MatConcrete);
            // Paredes (sul com vão de 1,8 m em x −55).
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeSul_Esq", g.transform, new Vector3(-58f, h * 0.5f, 40f), Vector3.zero, new Vector3(4.1f, h, 0.25f), wallMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeSul_Dir", g.transform, new Vector3(-52f, h * 0.5f, 40f), Vector3.zero, new Vector3(4.1f, h, 0.25f), wallMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeSul_Verga", g.transform, new Vector3(-55f, 2.6f, 40f), Vector3.zero, new Vector3(1.9f, 0.8f, 0.25f), wallMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeNorte", g.transform, new Vector3(-55f, h * 0.5f, 46f), Vector3.zero, new Vector3(10.25f, h, 0.25f), wallMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeOeste", g.transform, new Vector3(-60f, h * 0.5f, 43f), Vector3.zero, new Vector3(0.25f, h, 6f), wallMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "ParedeLeste", g.transform, new Vector3(-50f, h * 0.5f, 43f), Vector3.zero, new Vector3(0.25f, h, 6f), wallMat);
            roof = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Laje", g.transform, new Vector3(-55f, h + 0.1f, 43f), Vector3.zero, new Vector3(10.6f, 0.2f, 6.6f), ScenarioSceneKit.MatConcrete);
            // Bancos internos.
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "BancoInterno", g.transform, new Vector3(-58f, k_PlazaY + 0.45f, 44.5f), Vector3.zero, new Vector3(2.5f, 0.06f, 0.45f), ScenarioSceneKit.MatWood);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "BancoInterno", g.transform, new Vector3(-52f, k_PlazaY + 0.45f, 44.5f), Vector3.zero, new Vector3(2.5f, 0.06f, 0.45f), ScenarioSceneKit.MatWood);
            // Luz interna e placa.
            var lightGo = UIBuilderUtil.Child(g.transform, "Luz");
            lightGo.transform.position = new Vector3(-55f, 2.7f, 43f);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Point; light.range = 8f; light.intensity = 1.2f; light.shadows = LightShadows.None;
            var sign = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", g.transform, new Vector3(-55f, 3.5f, 39.9f), Vector3.zero, new Vector3(3f, 0.6f, 0.05f), ScenarioSceneKit.MatSign);
            UIBuilderUtil.WorldText(sign.transform, "Texto", "UBS — ABRIGO\n<size=60%>alvenaria com laje</size>", 1.8f, Color.white, new Vector3(-55f, 3.5f, 39.86f), Vector3.zero, new Vector2(2.9f, 0.6f));
            UIBuilderUtil.MarkStatic(g);
            return new Bounds(new Vector3(-55f, 1.5f, 43f), new Vector3(9.6f, 3f, 5.6f));
        }

        static GameObject CreateBusStop(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "PontoOnibus");
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "PosteA", g.transform, k_BusStop + new Vector3(-1.3f, 1.2f, 0.6f), Vector3.zero, new Vector3(0.08f, 1.2f, 0.08f), ScenarioSceneKit.MatMetal);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "PosteB", g.transform, k_BusStop + new Vector3(1.3f, 1.2f, 0.6f), Vector3.zero, new Vector3(0.08f, 1.2f, 0.08f), ScenarioSceneKit.MatMetal);
            var roof = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Cobertura", g.transform, k_BusStop + new Vector3(0f, 2.45f, 0f), Vector3.zero, new Vector3(3f, 0.06f, 1.6f), ScenarioSceneKit.MatMetal);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Fundo", g.transform, k_BusStop + new Vector3(0f, 1.2f, 0.75f), Vector3.zero, new Vector3(3f, 2.4f, 0.05f), ScenarioSceneKit.MatGlass);
            UIBuilderUtil.WorldText(g.transform, "Label", "PONTO DE ÔNIBUS\n<size=60%>cobertura metálica</size>", 0.8f, Color.white, k_BusStop + new Vector3(0f, 2.75f, -0.2f), Vector3.zero, new Vector2(2.5f, 0.4f));
            UIBuilderUtil.MarkStatic(g);
            return roof;
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

        static void CreateTrees(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "Arvores");
            var leaves = UIBuilderUtil.GetOrCreateMaterial("Mat_Copa", new Color(0.2f, 0.45f, 0.2f), false, false, 0.05f);
            void Tree(Vector3 feet, float trunkH, float canopy, string name)
            {
                var t = UIBuilderUtil.Child(g.transform, name);
                t.transform.position = feet;
                UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Tronco", t.transform, feet + new Vector3(0f, trunkH * 0.5f, 0f), Vector3.zero, new Vector3(0.35f, trunkH * 0.5f, 0.35f), ScenarioSceneKit.MatWood);
                UIBuilderUtil.Primitive(PrimitiveType.Sphere, "Copa", t.transform, feet + new Vector3(0f, trunkH + canopy * 0.35f, 0f), Vector3.zero, Vector3.one * canopy, leaves, false);
                UIBuilderUtil.MarkStatic(t);
            }
            Tree(k_BigTree, 3.5f, 8f, "ArvoreGrande");
            Tree(new Vector3(-70f, k_PlazaY, 44f), 2.5f, 4f, "Arvore");
            Tree(new Vector3(-46f, k_PlazaY, 44f), 2.5f, 4f, "Arvore");
            Tree(new Vector3(-64f, k_PlazaY, 40f), 2.2f, 3.5f, "Arvore");
            // Árvore doente na calçada em frente à casa (etiqueta de prevenção).
            var sick = UIBuilderUtil.Child(g.transform, "ArvoreDoente");
            sick.transform.position = new Vector3(-29.5f, 0.08f, 19.3f);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Tronco", sick.transform, sick.transform.position + new Vector3(0f, 1.5f, 0f), Vector3.zero, new Vector3(0.3f, 1.5f, 0.3f), ScenarioSceneKit.MatWood);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "GalhoSeco", sick.transform, sick.transform.position + new Vector3(0.7f, 2.9f, 0f), new Vector3(0f, 0f, 60f), new Vector3(0.12f, 0.9f, 0.12f), ScenarioSceneKit.MatWood, false);
            UIBuilderUtil.Primitive(PrimitiveType.Sphere, "Copa", sick.transform, sick.transform.position + new Vector3(0f, 3.6f, 0f), Vector3.zero, Vector3.one * 2.4f, leaves, false);
            UIBuilderUtil.WorldText(sick.transform, "Etiqueta", "<size=70%>galho seco:\navise a Prefeitura</size>", 0.7f, new Color(1f, 0.85f, 0.4f),
                sick.transform.position + new Vector3(0f, 2.2f, -0.5f), Vector3.zero, new Vector2(1.6f, 0.4f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static void CreateBillboardAndTower(Transform parent)
        {
            var g = UIBuilderUtil.Child(parent, "PlacaETorre");
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "PostePlaca", g.transform, k_Billboard + new Vector3(0f, 3f, 0f), Vector3.zero, new Vector3(0.25f, 3f, 0.25f), ScenarioSceneKit.MatMetal);
            var board = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", g.transform, k_Billboard + new Vector3(0f, 7f, 1.2f), new Vector3(15f, 0f, 0f), new Vector3(6f, 2.5f, 0.1f), ScenarioSceneKit.MatSign);
            UIBuilderUtil.WorldText(board.transform, "Texto", "PROPAGANDA", 4f, Color.white, k_Billboard + new Vector3(0f, 7f, 1.14f), new Vector3(15f, 0f, 0f), new Vector2(5.8f, 2f));
            // Torre de transmissão (treliça simplificada).
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

        static void CreateHailBlockers(Transform parent, int layer, GameObject house, GameObject ubsRoof, GameObject busRoof, GameObject shackRoof)
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
            // Colliders grossos (topo na superfície visível): partículas rápidas não atravessam com colisão "Low".
            Block("Chao", new Vector3(-52f, -0.5f, 30f), new Vector3(80f, 1f, 50f));
            var laje = ubsRoof.transform.position; var lajeSize = ubsRoof.transform.localScale;
            Block("LajeUBS", new Vector3(laje.x, laje.y + lajeSize.y * 0.5f - 0.6f, laje.z), new Vector3(lajeSize.x, 1.2f, lajeSize.z));
            var bus = busRoof.transform.position; var busSize = busRoof.transform.localScale;
            Block("CoberturaPonto", new Vector3(bus.x, bus.y + busSize.y * 0.5f - 0.5f, bus.z), new Vector3(busSize.x, 1f, busSize.z));
            Block("TelhadoBarraca", shackRoof.transform.position + new Vector3(0f, -0.4f, 0f), new Vector3(3.2f, 1.2f, 3.2f));
            // Telhados da casa (bounds medidos).
            Block("TelhadoCasa", new Vector3(-32.47f, 3.0f, 10.6f), new Vector3(7.8f, 2.1f, 9.5f));
        }

        static HailController CreateHail(Transform parent, int layer, List<Renderer> iceLayers, MissionStepSO stopStep, params Vector3[] soundPositions)
        {
            var g = UIBuilderUtil.Child(parent, "Granizo");
            var ctrl = g.AddComponent<HailController>();

            var psGo = UIBuilderUtil.Child(g.transform, "Particulas");
            psGo.transform.position = k_PlayerStart + Vector3.up * 6f;
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 400;
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
            shape.scale = new Vector3(20f, 0.5f, 20f);
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
                loop.source = src; loop.kind = SimpleLoopSound.ClipKind.Hail;
                ctrl.roofSounds.Add(src);
            }
            return ctrl;
        }

        static CompanionFollower CreateJoaquim(Transform parent, MissionStepSO step, out XRSimpleInteractable interactable)
        {
            interactable = ScenarioSceneKit.CreateNpc(parent, "SeuJoaquim", k_JoaquimBench, 180f, new Color(0.45f, 0.4f, 0.35f), true, "Obrigado, meu filho! Vamos.", out var speech);
            var npc = interactable.gameObject;
            // Boina.
            var cap = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Boina", npc.transform, Vector3.zero, Vector3.zero, new Vector3(0.28f, 0.02f, 0.28f), ScenarioSceneKit.MatFlashlight, false);
            cap.transform.localPosition = new Vector3(0f, 1.71f, 0f);
            var follower = npc.AddComponent<CompanionFollower>();
            follower.talkInteractable = interactable;
            follower.speed = 0.8f;
            follower.maxLeadDistance = 3f;
            follower.speechBubble = speech;
            var routes = UIBuilderUtil.Child(parent, "Rota_Joaquim");
            follower.waypoints.AddRange(ScenarioSceneKit.CreateWaypoints(routes.transform, "WP",
                new Vector3(-49f, 0f, 33f), new Vector3(-52f, 0f, 37f), new Vector3(-55f, 0f, 39.5f), new Vector3(-55f, 0f, 42f)));
            var target = npc.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.5f;
            return follower;
        }

        /// <summary>Carro do Carlos: cópia do prefab da Kombi da casa, ou caixa simples se não existir.</summary>
        static GameObject CreateCar(Transform parent, GameObject house)
        {
            GameObject car = null;
            var kombi = ScenarioSceneKit.FindChildRecursive(house.transform, "Kombi");
            if (kombi != null)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(kombi.gameObject);
                if (source != null)
                {
                    var asset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(kombi.gameObject);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    if (prefab != null)
                    {
                        car = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                        car.name = "Carro_Carlos";
                        car.transform.SetPositionAndRotation(k_CarStart, Quaternion.Euler(0f, 90f, 0f));
                        car.transform.localScale = kombi.localScale;
                        foreach (var mf in car.GetComponentsInChildren<MeshFilter>(true))
                        {
                            if (mf.GetComponent<Collider>() != null) continue;
                            var box = mf.gameObject.AddComponent<BoxCollider>();
                            box.center = mf.sharedMesh.bounds.center; box.size = mf.sharedMesh.bounds.size;
                        }
                    }
                }
            }
            if (car == null)
            {
                car = UIBuilderUtil.Child(parent, "Carro_Carlos");
                car.transform.SetPositionAndRotation(k_CarStart, Quaternion.Euler(0f, 90f, 0f));
                var body = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Carroceria", car.transform, Vector3.zero, Vector3.zero, new Vector3(4.2f, 1.5f, 1.9f), ScenarioSceneKit.MatCloth);
                body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            }
            // Vidro que racha em silêncio caso o carro fique sob a placa (detalhe visual só na vaga A: mantido simples).
            return car;
        }

        static DecisionOption CreateSpot(Transform parent, string name, Vector3 pos, bool correct, string mistakeId, string feedback, MissionStepSO step)
        {
            var g = UIBuilderUtil.Child(parent, name);
            g.transform.position = pos;
            var mark = UIBuilderUtil.GetOrCreateMaterial("Mat_Vaga", new Color(0.95f, 0.95f, 0.9f), false, false, 0.05f);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "LinhaEsq", g.transform, pos + new Vector3(0f, 0.01f, -1.1f), Vector3.zero, new Vector3(4.6f, 0.01f, 0.1f), mark, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "LinhaDir", g.transform, pos + new Vector3(0f, 0.01f, 1.1f), Vector3.zero, new Vector3(4.6f, 0.01f, 0.1f), mark, false);
            var sign = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", g.transform, pos + new Vector3(0f, 1.3f, 0f), Vector3.zero, new Vector3(0.7f, 0.4f, 0.05f), ScenarioSceneKit.MatHint);
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", g.transform, pos + new Vector3(0f, 0.55f, 0f), Vector3.zero, new Vector3(0.04f, 0.55f, 0.04f), ScenarioSceneKit.MatMetal, false);
            UIBuilderUtil.WorldText(sign.transform, "Texto", "Aqui?", 1.6f, Color.white, pos + new Vector3(0f, 1.3f, 0f), Vector3.zero, new Vector2(0.7f, 0.4f))
                .gameObject.AddComponent<FacePlayer>().yawOnly = true;
            sign.AddComponent<XRSimpleInteractable>();
            var opt = sign.AddComponent<DecisionOption>();
            opt.isCorrect = correct;
            opt.mistakeId = mistakeId;
            opt.feedback = feedback;
            var target = sign.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.3f;
            return opt;
        }

        static InspectableSign CreateDamagedCeiling(Transform parent, MissionStepSO step, out XRSimpleInteractable interactable)
        {
            var g = UIBuilderUtil.Child(parent, "ForroCedendo");
            var forroMat = UIBuilderUtil.GetOrCreateMaterial("Mat_ForroDanificado", new Color(0.72f, 0.66f, 0.55f), false, false, 0.05f);
            // "Barriga" no forro: caixa fina levemente inclinada abaixo do teto.
            var sag = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Barriga", g.transform, k_Ceiling + new Vector3(0f, -0.08f, 0f), new Vector3(4f, 0f, 3f), new Vector3(1.8f, 0.05f, 1.3f), forroMat);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Trinca", sag.transform, k_Ceiling + new Vector3(0.2f, -0.115f, 0.1f), new Vector3(0f, 25f, 0f), new Vector3(0.9f, 0.005f, 0.02f), ScenarioSceneKit.MatFlashlight, false);
            interactable = sag.AddComponent<XRSimpleInteractable>();
            var sign = sag.AddComponent<InspectableSign>();
            sign.explanation = "Forro cedendo e goteira: o telhado pode estar comprometido. Saia do quarto.";
            sign.highlight = sag.GetComponent<Renderer>();
            var label = UIBuilderUtil.WorldText(g.transform, "Etiqueta", "", 0.6f, new Color(1f, 0.8f, 0.4f), k_Ceiling + new Vector3(0f, -0.5f, 0f), Vector3.zero, new Vector2(1.8f, 0.5f));
            label.gameObject.AddComponent<FacePlayer>().yawOnly = true;
            label.gameObject.SetActive(false);
            sign.label = label;
            var target = sag.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = -0.6f;

            // Goteira (partículas) + poça.
            var dripGo = UIBuilderUtil.Child(g.transform, "Goteira");
            dripGo.transform.position = k_Ceiling + new Vector3(0.3f, -0.12f, 0f);
            var ps = dripGo.AddComponent<ParticleSystem>();
            var main = ps.main; main.startSize = 0.03f; main.startLifetime = 0.7f; main.startSpeed = 0f; main.gravityModifier = 1f; main.maxParticles = 20;
            main.startColor = new Color(0.6f, 0.75f, 0.95f, 0.9f); main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 2.5f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.01f;
            var pr = dripGo.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = ScenarioSceneKit.MatBottle;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poca", g.transform, new Vector3(k_Ceiling.x + 0.3f, 0.005f, k_Ceiling.z), Vector3.zero, new Vector3(0.6f, 0.003f, 0.5f), ScenarioSceneKit.MatWater, false);
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
                var step = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Degrau" + i, g.transform, Vector3.zero, Vector3.zero, new Vector3(0.5f, 0.04f, 0.04f), ScenarioSceneKit.MatMetal, false);
                step.transform.localPosition = new Vector3(0f, 0.3f + i * 0.36f, 0f);
            }
            var col = g.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.7f, 0f);
            col.size = new Vector3(0.6f, 3.4f, 0.15f);
            g.AddComponent<XRSimpleInteractable>();
            var wrong = g.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "subir_telhado";
            wrong.message = "Não suba em telhados molhados: há risco de queda. Peça a vistoria às autoridades.";
            UIBuilderUtil.WorldText(g.transform, "Label", "Subir no telhado?", 0.7f, new Color(1f, 0.75f, 0.35f), k_Ladder + new Vector3(0f, 2.2f, -0.6f), Vector3.zero, new Vector2(1.4f, 0.2f))
                .gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static void CreateRoofDamage(Transform parent, GameObject house)
        {
            var g = UIBuilderUtil.Child(parent, "TelhasQuebradas");
            var dark = UIBuilderUtil.GetOrCreateMaterial("Mat_TelhaQuebrada", new Color(0.25f, 0.15f, 0.1f), false, false, 0.05f);
            foreach (var (x, z) in new[] { (-31f, 8f), (-30f, 12f), (-34f, 9.5f), (-33.5f, 13f), (-29.8f, 10f) })
            {
                // Aproximação da altura do telhado (duas águas, cumeeira em x −32.47, y 4.0).
                var y = 4.0f - Mathf.Abs(x + 32.47f) * 0.52f + 0.05f;
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Telha", g.transform, new Vector3(x, y, z), new Vector3(0f, 0f, x < -32.47f ? 27f : -27f), new Vector3(0.4f, 0.03f, 0.5f), dark, false);
            }
            UIBuilderUtil.WorldText(g.transform, "Etiqueta", "<size=70%>telhas quebradas:\nmantenha o madeiramento em dia</size>", 0.8f, new Color(1f, 0.85f, 0.4f),
                new Vector3(-32.5f, 3.2f, 16.2f), Vector3.zero, new Vector2(2.4f, 0.5f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }
    }
}
#endif
