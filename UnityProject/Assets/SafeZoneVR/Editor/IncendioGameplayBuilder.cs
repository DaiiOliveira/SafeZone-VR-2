#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR.Editor
{
    public static class IncendioGameplayBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Incendio_EmCasa.unity";

        static readonly Vector3 k_PlayerStart = new Vector3(-2.8f, 0f, -2.6f);
        static readonly Quaternion k_PlayerRot = Quaternion.Euler(0f, -90f, 0f);
        static readonly Vector2 k_PanXZ = new Vector2(-9.36f, -0.62f);
        static readonly Vector3 k_GasCylinder = new Vector3(-8.45f, 0.28f, -0.45f);
        static readonly Vector2 k_LidXZ = new Vector2(-10.36f, -3.05f);
        static readonly Vector2 k_JugXZ = new Vector2(-8.35f, -2.75f);
        static readonly Vector3 k_PowerStrip = new Vector3(-5.05f, 0.03f, -1.05f);
        static readonly Vector3 k_RackFire = new Vector3(-5.15f, 0.55f, -1.40f);
        static readonly Vector2 k_NotebookXZ = new Vector2(-0.85f, -1.70f);
        static readonly Vector2 k_BlanketXZ = new Vector2(-0.85f, -2.45f);
        static readonly Vector3 k_FrontYardCenter = new Vector3(-4.5f, 1.2f, 1.3f);
        static readonly Vector3 k_FrontYardSize = new Vector3(3.0f, 2.4f, 3.6f);
        static readonly Vector3 k_MeetingSign = new Vector3(-9.0f, 0f, 6.3f);
        static readonly Vector3 k_Truck = new Vector3(-13.0f, -0.06f, 9.7f);

        const string k_IntroText =
            "Fim de tarde. Você esquentou óleo para fritar e foi até a sala por um instante. Ao voltar, a panela <b>pegou fogo</b>. " +
            "O fogo ainda é pequeno: é um <b>princípio de incêndio</b>.\n" +
            "Mantenha a calma. Faça o que os Bombeiros orientam e, se o fogo sair do controle, saia de casa.\n\n" +
            "O relógio no pulso esquerdo mostra cada missão e permite ligar para o 193. Aponte para o chão e aperte o gatilho para se teleportar.";

        [MenuItem("SafeZone VR/Build/2b. Fase Incêndio (montar gameplay na casa)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Gameplay da fase Incêndio montado e cena salva.", "OK");
        }

        public static void Build()
        {
            var scene = ScenarioSceneKit.OpenOrCreateScene(k_ScenePath);
            var data = SafeZoneDataBuilder.BuildIncendio();
            AssetDatabase.SaveAssets();
            ScenarioSceneKit.CreateMaterials();
            ScenarioSceneKit.DestroyRoot(scene, ScenarioSceneKit.k_RootName);
            ScenarioSceneKit.SetupDirectionalLight(scene, new Color(1f, 0.85f, 0.7f), 0.9f, new Vector3(28f, -120f, 0f));

            var house = ScenarioSceneKit.EnsureHouseInstance(scene);
            var frontDoorPivot = ScenarioSceneKit.OpenHouseDoors(house, false);
            ScenarioSceneKit.HidePart(house, "DC_CASA_PanelaChaleira");
            ScenarioSceneKit.SetupModelPhysics(house);
            var origin = ScenarioSceneKit.SetupXROrigin(scene, k_PlayerStart, k_PlayerRot);
            var camera = origin.Camera.transform;
            var leftHand = ScenarioSceneKit.FindChildRecursive(origin.transform, "Left Controller");

            var root = ScenarioSceneKit.CreateRoot(scene);
            ScenarioSceneKit.CreatePlayerSpawn(root.transform, k_PlayerStart, k_PlayerRot);
            var manager = ScenarioSceneKit.CreateCore(root.transform, origin, data.scenario, false, out _);

            var env = UIBuilderUtil.Child(root.transform, "Environment");
            var frontYard = ScenarioSceneKit.CreateTriggerZone(env.transform, "Zona_QuintalFrente", k_FrontYardCenter, k_FrontYardSize);
            CreateMeetingSign(env.transform);
            var truck = CreateFireTruck(env.transform);
            var siren = CreateSiren(env.transform);

            var matGas = UIBuilderUtil.GetOrCreateMaterial("Mat_Botijao", new Color(0.93f, 0.93f, 0.9f), false, false, 0.5f);
            var matPan = ScenarioSceneKit.MatMetal;

            var kitchen = UIBuilderUtil.Child(root.transform, "Cozinha");
            var stove = ScenarioSceneKit.PartBounds(ScenarioSceneKit.ModelPart(house, "DC_CASA_Fogao"));

            var panPos = ScenarioSceneKit.OnSurface(k_PanXZ.x, k_PanXZ.y, 1.5f, 0.03f);
            var stoveKnobPos = new Vector3(stove.center.x + 0.2f, stove.max.y - 0.12f, stove.min.z - 0.01f);
            var pan = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Panela", kitchen.transform, panPos, Vector3.zero, new Vector3(0.28f, 0.06f, 0.28f), matPan);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Cabo", pan.transform, panPos + new Vector3(0.22f, 0.02f, 0f), Vector3.zero, new Vector3(0.18f, 0.02f, 0.03f), ScenarioSceneKit.MatFlashlight, false);
            var panFire = CreateFire(kitchen.transform, "Fogo_Panela", panPos + new Vector3(0f, 0.08f, 0f), FireController.Size.Small, new float[0], false, 20f);

            var lidSocket = ScenarioSceneKit.CreateSocket(pan.transform, "Socket_Tampa", panPos + new Vector3(0f, 0.09f, 0f), new List<string> { "tampa_panela" }, 0.16f);
            var lidCooldown = lidSocket.gameObject.AddComponent<LidCooldown>();
            lidCooldown.lidSocket = lidSocket;
            lidCooldown.panFire = panFire;
            lidCooldown.coolSeconds = 45f;

            ScenarioSceneKit.CreateItem(kitchen.transform, "Item_Tampa", PrimitiveType.Cylinder, ScenarioSceneKit.OnSurface(k_LidXZ.x, k_LidXZ.y, 2.5f, 0.02f), Vector3.zero, new Vector3(0.30f, 0.012f, 0.30f), matPan,
                "tampa_panela", "Tampa da panela", data.tampa);

            var stoveKnob = ScenarioSceneKit.CreateKnob(kitchen.transform, "Botao_Fogao", stoveKnobPos, Quaternion.Euler(-90f, 0f, 0f), 0.03f, 0.02f,
                ScenarioSceneKit.MatFlashlight, "BOTÃO DO FOGÃO\n<size=70%>gire para desligar</size>", new Vector3(0f, 0.2f, -0.05f), Vector3.zero, data.gas);

            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Botijao", kitchen.transform, k_GasCylinder, Vector3.zero, new Vector3(0.30f, 0.27f, 0.30f), matGas);
            var gasKnob = ScenarioSceneKit.CreateKnob(kitchen.transform, "Registro_Botijao", k_GasCylinder + new Vector3(0f, 0.32f, 0f), Quaternion.identity, 0.05f, 0.02f,
                UIBuilderUtil.GetOrCreateMaterial("Mat_RegistroGas", new Color(0.95f, 0.75f, 0.1f), false, false, 0.4f),
                "REGISTRO DO GÁS\n<size=70%>gire para fechar</size>", new Vector3(0f, 0.25f, -0.25f), Vector3.zero, data.gas);

            var jugPos = ScenarioSceneKit.OnSurface(k_JugXZ.x, k_JugXZ.y, 2.0f, 0.11f);
            var jug = UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Jarra_Agua", kitchen.transform, jugPos, Vector3.zero, new Vector3(0.12f, 0.11f, 0.12f), ScenarioSceneKit.MatBottle);
            jug.AddComponent<XRSimpleInteractable>();
            var jugWrong = jug.AddComponent<WrongActionInteractable>();
            jugWrong.mistakeId = "agua_no_oleo";
            jugWrong.message = "Nunca jogue água em óleo quente: a água vira vapor na hora e espalha o óleo em chamas. Abafe com a tampa.";
            jug.AddComponent<FlareOnSelect>().fire = panFire;
            UIBuilderUtil.WorldText(kitchen.transform, "Jarra_Label", "Jogar água?", 0.6f, new Color(1f, 0.75f, 0.35f),
                jugPos + new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector2(1f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;

            var window = CreateWindow(house, kitchen.transform, data.ventilar);

            var living = UIBuilderUtil.Child(root.transform, "Sala");
            CreatePowerStrip(living.transform);
            var rackFire = CreateFire(living.transform, "Fogo_Rack", k_RackFire, FireController.Size.Out, new[] { 20f, 25f }, true, 0f);
            var notebook = ScenarioSceneKit.CreateWrongInteractable(living.transform, "Notebook", ScenarioSceneKit.OnSurface(k_NotebookXZ.x, k_NotebookXZ.y, 1.5f, 0.02f), new Vector3(0.32f, 0.02f, 0.22f), ScenarioSceneKit.MatFlashlight,
                "salvar_objetos", "Não perca tempo tentando salvar objetos. Sua vida vale mais: saia de casa.", "Salvar o notebook?");
            var tvWrong = CreateTvWrongAction(house, living.transform);
            var blanket = ScenarioSceneKit.CreateWrongInteractable(living.transform, "Cobertor", ScenarioSceneKit.OnSurface(k_BlanketXZ.x, k_BlanketXZ.y, 1.5f, 0.05f), new Vector3(0.45f, 0.08f, 0.35f), ScenarioSceneKit.MatCloth,
                "combater_sozinho", "Não tente combater sozinho um incêndio que já cresceu. Saia de casa e ligue 193.", "Abafar o fogo da TV?");
            blanket.gameObject.SetActive(false);
            var blanketLabel = living.transform.Find("Cobertor_Label");
            if (blanketLabel != null) blanketLabel.SetParent(blanket.transform, true);

            var smoke = CreateSmokeLayer(living.transform);
            var detector = CreateDetector(house, living.transform);

            var hall = UIBuilderUtil.Child(root.transform, "Corredor");
            var lever = ScenarioSceneKit.SetupHouseBreaker(house, hall.transform, data.energia);

            ScenarioSceneKit.SetupHouseLights(house, root.transform, lever);

            var door = SetupFrontDoor(frontDoorPivot, root.transform);

            var outside = UIBuilderUtil.Child(root.transform, "Exterior");
            var neighbor = CreateNeighbor(outside.transform, data.vizinho, out var neighborInteractable);

            var insideZone = ScenarioSceneKit.CreateWrongZone(outside.transform, "Zona_VoltarParaDentro", new Vector3(-5.5f, 1.2f, -5.3f), new Vector3(10.6f, 2.4f, 10.0f),
                "voltar_para_dentro", "Não volte para dentro de uma casa em chamas. Espere os Bombeiros.");
            insideZone.gameObject.SetActive(false);

            var validators = UIBuilderUtil.Child(root.transform, "Validators");

            var v1 = UIBuilderUtil.Child(validators.transform, "Validator_01_Gas").AddComponent<StepCompleteOnAllConditions>();
            v1.scenarioManager = manager; v1.step = data.gas;
            v1.knobsTurned.Add(stoveKnob); v1.knobsTurned.Add(gasKnob);
            v1.requiredDelta = 0.25f;

            var v2 = UIBuilderUtil.Child(validators.transform, "Validator_02_Tampa").AddComponent<StepCompleteOnSocket>();
            v2.scenarioManager = manager; v2.step = data.tampa;
            v2.sockets.Add(lidSocket); v2.requiredItemIds.Add("tampa_panela");

            var v3 = UIBuilderUtil.Child(validators.transform, "Validator_03_Ventilar").AddComponent<StepCompleteOnOpenings>();
            v3.scenarioManager = manager; v3.step = data.ventilar;
            v3.openings.Add(window); v3.targetOpen = true;

            var v4 = UIBuilderUtil.Child(validators.transform, "Validator_04_Energia").AddComponent<StepCompleteOnBreaker>();
            v4.scenarioManager = manager; v4.step = data.energia; v4.lever = lever;

            var v5 = frontYard.AddComponent<StepCompleteOnTriggerZone>();
            v5.scenarioManager = manager; v5.step = data.sair;
            var yardTarget = frontYard.AddComponent<ObjectiveTarget>();
            yardTarget.step = data.sair; yardTarget.markerHeightOffset = 0.3f;

            var v6 = UIBuilderUtil.Child(validators.transform, "Validator_06_Porta").AddComponent<StepCompleteOnOpenings>();
            v6.scenarioManager = manager; v6.step = data.porta;
            v6.openings.Add(door); v6.targetOpen = false; v6.requirePlayerInZone = frontYard.GetComponent<Collider>();
            var doorTarget = door.gameObject.AddComponent<ObjectiveTarget>();
            doorTarget.step = data.porta; doorTarget.markerHeightOffset = 0.3f;

            var v7 = UIBuilderUtil.Child(validators.transform, "Validator_07_Ligar").AddComponent<StepCompleteOnButtonPress>();
            v7.scenarioManager = manager; v7.step = data.ligar;

            var v8 = UIBuilderUtil.Child(validators.transform, "Validator_08_Vizinho").AddComponent<StepCompleteOnInteractCount>();
            v8.scenarioManager = manager; v8.step = data.vizinho;
            v8.interactables.Add(neighborInteractable); v8.requiredCount = 1;

            var phases = UIBuilderUtil.Child(root.transform, "Phases");
            var caption = ScenarioSceneKit.CreatePhaseCaption(camera);
            var switcher = phases.AddComponent<ScenarioPhaseSwitcher>();
            switcher.caption = caption;

            var rack = new ScenarioPhaseSwitcher.Phase { name = "Rack", fadeSeconds = 0f,
                info = "Faíscas no benjamim atrás da TV! Não use água em aparelhos ligados." };
            rack.triggerSteps.AddRange(new[] { data.gas, data.tampa, data.ventilar });
            var setId = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), jugWrong, "set_mistakeId");
            var setMsg = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), jugWrong, "set_message");
            UnityEventTools.AddStringPersistentListener(rack.onEnter, setId, "agua_no_aparelho");
            UnityEventTools.AddStringPersistentListener(rack.onEnter, setMsg, "Nunca jogue água em aparelho elétrico ligado: risco de choque. Desligue a energia.");
            UnityEventTools.AddPersistentListener(rack.onEnter, rackFire.SparkThenIgnite);
            UnityEventTools.AddPersistentListener(rack.onEnter, smoke.Begin);
            UnityEventTools.AddPersistentListener(rack.onEnter, detector.Play);
            switcher.phases.Add(rack);

            var evac = new ScenarioPhaseSwitcher.Phase { name = "Evacuar", fadeSeconds = 0f,
                info = "O fogo cresceu. Não tente apagar sozinho: abaixe-se e saia de casa." };
            evac.triggerSteps.Add(data.energia);
            evac.activate.Add(blanket.gameObject);
            switcher.phases.Add(evac);

            AddActivate(phases.transform, "Ativar_ZonaVoltar", data.sair, insideZone.gameObject);
            AddActivate(phases.transform, "Ativar_Vizinho", data.ligar, neighbor, siren);
            AddActivate(phases.transform, "Ativar_Caminhao", data.vizinho, truck);

            UnityEventTools.AddFloatPersistentListener(lidCooldown.onCovered, detector.StopAfter, 20f);

            var ui = UIBuilderUtil.Child(root.transform, "UI");
            var options = ScenarioSceneKit.CreateOptionsPanel(ui.transform, camera, true);
            ScenarioSceneKit.CreateWristUI(leftHand != null ? leftHand : camera, manager, null, options, true, out var callButton);
            var callPanel = ScenarioSceneKit.CreateCallPanel(ui.transform, camera, callButton,
                "Defesa Civil: vamos acionar os Bombeiros, mas em caso de incêndio ligue direto para o 193.",
                "Corpo de Bombeiros. Endereço anotado, uma viatura está a caminho. Todos saíram? Não entre na casa e mantenha as pessoas afastadas.",
                "Polícia Militar: vamos repassar. Para incêndio, o número é 193.",
                "SAMU: se alguém estiver passando mal, estamos à disposição. Para o incêndio, ligue 193.",
                "193");
            UnityEventTools.AddPersistentListener(callPanel.onAcceptedCall, v7.OnButtonPressed);
            ScenarioSceneKit.CreateIntroPanel(ui.transform, camera, manager, "Incêndio em Casa", k_IntroText);
            ScenarioSceneKit.CreateEndReportPanel(ui.transform, camera, manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Gameplay do Incêndio montado e cena salva.");
        }

        static void AddActivate(Transform parent, string name, MissionStepSO step, params GameObject[] activate)
        {
            var a = UIBuilderUtil.Child(parent, name).AddComponent<ActivateOnStepCompleted>();
            a.step = step;
            a.activate.AddRange(activate);
            foreach (var go in activate) if (go != null) go.SetActive(false);
        }

        public static FireController CreateFire(Transform parent, string name, Vector3 pos, FireController.Size start, float[] growSteps, bool sparks, float residualSmoke)
        {
            var go = UIBuilderUtil.Child(parent, name);
            go.transform.position = pos;
            var fire = go.AddComponent<FireController>();

            fire.flames = CreateParticles(go.transform, "Chamas", ScenarioSceneKit.MatFlame, new Color(1f, 0.6f, 0.15f, 0.9f), new Color(1f, 0.2f, 0.05f, 0f),
                0.18f, 0.7f, 0.9f, 0.12f, 60, 0f);
            fire.smoke = CreateParticles(go.transform, "Fumaca", ScenarioSceneKit.MatSmoke, new Color(0.4f, 0.4f, 0.42f, 0.45f), new Color(0.3f, 0.3f, 0.32f, 0f),
                0.35f, 2.2f, 0.7f, 0.25f, 30, 0.6f);
            fire.smoke.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            if (sparks)
            {
                fire.sparks = CreateParticles(go.transform, "Faiscas", ScenarioSceneKit.MatMarker, new Color(1f, 0.95f, 0.6f, 1f), new Color(1f, 0.6f, 0.2f, 0f),
                    0.03f, 0.5f, 2.5f, 0.4f, 60, 0f);
                var m = fire.sparks.main; m.gravityModifier = 1f;
                var e = fire.sparks.emission; e.rateOverTime = 40f;
                fire.sparks.Stop();
            }

            var lightGo = UIBuilderUtil.Child(go.transform, "Luz");
            lightGo.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.25f);
            light.range = 4f;
            light.shadows = LightShadows.None;
            fire.fireLight = light;

            var audioGo = UIBuilderUtil.Child(go.transform, "Crepitar");
            var src = audioGo.AddComponent<AudioSource>();
            src.spatialBlend = 1f; src.minDistance = 0.5f; src.maxDistance = 12f; src.loop = true; src.playOnAwake = false;
            var loop = audioGo.AddComponent<SimpleLoopSound>();
            loop.source = src; loop.kind = SimpleLoopSound.ClipKind.Crackle;
            fire.crackle = src;

            fire.startSize = start;
            fire.growSteps = growSteps;
            fire.residualSmokeSeconds = residualSmoke;
            return fire;
        }

        static ParticleSystem CreateParticles(Transform parent, string name, Material mat, Color c0, Color c1, float size, float lifetime, float speed, float radius, int max, float spreadNoise)
        {
            var go = UIBuilderUtil.Child(parent, name);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.3f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.7f, lifetime * 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.7f, speed * 1.2f);
            main.startColor = c0;
            main.maxParticles = max;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = radius;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(c0, 0f), new GradientColorKey(c1, 1f) },
                new[] { new GradientAlphaKey(c0.a, 0f), new GradientAlphaKey(c0.a, 0.4f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.4f));
            if (spreadNoise > 0f)
            {
                var noise = ps.noise; noise.enabled = true; noise.strength = spreadNoise; noise.frequency = 0.5f;
            }
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        static ToggleOpening CreateWindow(GameObject house, Transform parent, MissionStepSO step)
        {
            var glass = ScenarioSceneKit.ModelPart(house, "DC_CASA_Janela_Direita0_Vidro");
            var mullion = ScenarioSceneKit.ModelPart(house, "DC_CASA_Janela_Direita0_Montante");
            var gb = ScenarioSceneKit.PartBounds(glass);
            var hinge = ScenarioSceneKit.HingePivot(glass, new Vector3(gb.center.x, gb.center.y, gb.min.z));
            if (mullion != null)
                mullion.SetParent(hinge, true);
            hinge.name = "Janela_Cozinha";
            var glassCollider = glass.GetComponent<Collider>();
            if (glassCollider == null)
                glassCollider = glass.gameObject.AddComponent<BoxCollider>();

            var interactable = hinge.gameObject.AddComponent<XRSimpleInteractable>();
            interactable.colliders.Add(glassCollider);
            var toggle = hinge.gameObject.AddComponent<ToggleOpening>();
            toggle.hinge = hinge;
            toggle.axis = Vector3.up;
            toggle.openAngle = ScenarioSceneKit.SwingAngle(hinge, Vector3.left, 80f);
            toggle.startsOpen = false;

            var target = hinge.gameObject.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.3f;

            UIBuilderUtil.WorldText(parent, "Janela_Label", "JANELA\n<size=70%>aponte e aperte para abrir</size>", 0.5f, Color.white,
                new Vector3(gb.center.x + 0.15f, gb.max.y + 0.2f, gb.center.z), new Vector3(0f, -90f, 0f), new Vector2(0.9f, 0.2f));
            return toggle;
        }

        static void CreatePowerStrip(Transform parent)
        {
            var strip = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Benjamim", parent, k_PowerStrip, Vector3.zero, new Vector3(0.22f, 0.05f, 0.08f), ScenarioSceneKit.MatMeds);
            for (var i = 0; i < 5; i++)
                UIBuilderUtil.Primitive(PrimitiveType.Cube, "Plugue" + i, strip.transform, k_PowerStrip + new Vector3(-0.08f + i * 0.04f, 0.045f, 0f), Vector3.zero,
                    new Vector3(0.025f, 0.04f, 0.03f), ScenarioSceneKit.MatFlashlight, false);
            UIBuilderUtil.WorldText(parent, "Benjamim_Label", "<size=70%>benjamim lotado:\nevite \"T\" e gambiarras</size>", 0.6f, new Color(1f, 0.75f, 0.35f),
                k_PowerStrip + new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector2(1.2f, 0.3f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
        }

        static WrongActionInteractable CreateTvWrongAction(GameObject house, Transform parent)
        {
            var tv = ScenarioSceneKit.ModelPart(house, "DC_CASA_TV");
            var pos = tv != null ? ScenarioSceneKit.PartBounds(tv).center : new Vector3(-5.19f, 0.99f, -2.0f);
            var go = UIBuilderUtil.Child(parent, "TV_Salvar");
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(0.45f, 1.1f, 1.3f);
            go.AddComponent<XRSimpleInteractable>();
            var wrong = go.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "salvar_objetos";
            wrong.message = "Não perca tempo tentando salvar objetos. Sua vida vale mais: saia de casa.";
            UIBuilderUtil.WorldText(go.transform, "Label", "Tirar a TV?", 0.7f, new Color(1f, 0.75f, 0.35f),
                pos + new Vector3(0.3f, 0.75f, 0f), Vector3.zero, new Vector2(1.2f, 0.2f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
            return wrong;
        }

        static SmokeLayer CreateSmokeLayer(Transform parent)
        {
            var go = UIBuilderUtil.Child(parent, "Fumaca_Sala");
            var zone = go.AddComponent<BoxCollider>();
            zone.isTrigger = true;
            go.transform.position = new Vector3(-2.8f, 1.05f, -2.5f);
            zone.size = new Vector3(5.2f, 2.1f, 4.6f);

            var visual = UIBuilderUtil.Child(go.transform, "Camadas");
            visual.transform.position = new Vector3(-2.8f, 2.0f, -2.5f);
            for (var i = 0; i < 3; i++)
            {
                var q = UIBuilderUtil.Primitive(PrimitiveType.Quad, "Camada" + i, visual.transform, visual.transform.position + new Vector3(0f, 0.3f * i, 0f),
                    new Vector3(90f, 0f, 0f), new Vector3(5.2f, 4.6f, 1f), ScenarioSceneKit.MatSmoke, false);
                var r = q.GetComponent<MeshRenderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;

                var q2 = UIBuilderUtil.Primitive(PrimitiveType.Quad, "Camada" + i + "_b", visual.transform, visual.transform.position + new Vector3(0f, 0.3f * i + 0.001f, 0f),
                    new Vector3(-90f, 0f, 0f), new Vector3(5.2f, 4.6f, 1f), ScenarioSceneKit.MatSmoke, false);
                q2.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            visual.SetActive(false);

            var smoke = go.AddComponent<SmokeLayer>();
            smoke.zone = zone;
            smoke.layerVisual = visual.transform;
            return smoke;
        }

        static SimpleLoopSound CreateDetector(GameObject house, Transform parent)
        {
            var model = ScenarioSceneKit.ModelPart(house, "DC_CASA_DetectorFumaca");
            var go = model != null
                ? model.gameObject
                : UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Detector_Fumaca", parent, new Vector3(-3f, 2.65f, -2.5f), Vector3.zero, new Vector3(0.12f, 0.015f, 0.12f), ScenarioSceneKit.MatMeds, false);
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f; src.minDistance = 1f; src.maxDistance = 15f;
            var loop = go.AddComponent<SimpleLoopSound>();
            loop.source = src; loop.kind = SimpleLoopSound.ClipKind.SmokeAlarm; loop.playOnStart = true;
            return loop;
        }

        static ToggleOpening SetupFrontDoor(Transform hinge, Transform labelParent)
        {
            if (hinge == null)
                throw new System.Exception("Porta da frente (DC_CASA_Porta_NichoFundo0) não encontrada na casa.");
            hinge.name = "PortaFrente_Operavel";
            var leaf = hinge.GetChild(0);
            var leafCollider = leaf.GetComponent<Collider>();
            if (leafCollider == null)
                leafCollider = leaf.gameObject.AddComponent<BoxCollider>();

            var doorInteractable = hinge.gameObject.AddComponent<XRSimpleInteractable>();
            doorInteractable.colliders.Add(leafCollider);
            var toggle = hinge.gameObject.AddComponent<ToggleOpening>();
            toggle.hinge = hinge;
            toggle.axis = Vector3.up;
            toggle.openAngle = ScenarioSceneKit.SwingAngle(hinge, Vector3.back, 95f);
            toggle.startsOpen = false;

            var lb = ScenarioSceneKit.PartBounds(leaf);
            var keyPos = new Vector3(lb.min.x + 0.12f, 0.88f, lb.min.z - 0.03f);
            var key = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Chave", hinge, keyPos, Vector3.zero, new Vector3(0.02f, 0.05f, 0.04f), ScenarioSceneKit.MatMarker);
            key.AddComponent<XRSimpleInteractable>();
            var wrong = key.AddComponent<WrongActionInteractable>();
            wrong.mistakeId = "trancar_porta";
            wrong.message = "Não tranque a porta: os Bombeiros e outras pessoas podem precisar passar.";
            UIBuilderUtil.WorldText(key.transform, "Label", "Trancar?", 0.5f, new Color(1f, 0.75f, 0.35f),
                keyPos + new Vector3(0f, -0.16f, -0.06f), Vector3.zero, new Vector2(0.6f, 0.15f)).gameObject.AddComponent<FacePlayer>().yawOnly = true;
            key.transform.GetChild(0).localScale = new Vector3(50f, 20f, 25f);

            UIBuilderUtil.WorldText(labelParent, "PortaFrente_Label", "PORTA DA FRENTE\n<size=70%>aponte e aperte para abrir/fechar</size>", 0.5f, Color.white,
                new Vector3(lb.center.x, 2.25f, -0.45f), new Vector3(0f, 180f, 0f), new Vector2(1.2f, 0.2f));
            return toggle;
        }

        static void CreateMeetingSign(Transform parent)
        {
            var group = UIBuilderUtil.Child(parent, "PontoDeEncontro");
            UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Poste", group.transform, k_MeetingSign + new Vector3(0f, 0.9f, 0f), Vector3.zero, new Vector3(0.06f, 0.9f, 0.06f), ScenarioSceneKit.MatFlashlight);
            var board = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Placa", group.transform, k_MeetingSign + new Vector3(0f, 1.95f, 0f), Vector3.zero, new Vector3(1.1f, 0.5f, 0.04f), ScenarioSceneKit.MatSign);
            UIBuilderUtil.WorldText(board.transform, "Texto", "PONTO DE ENCONTRO\n<size=60%>espere os Bombeiros aqui</size>", 1.6f, Color.white,
                k_MeetingSign + new Vector3(0f, 1.95f, -0.03f), Vector3.zero, new Vector2(1.05f, 0.5f));
            UIBuilderUtil.MarkStatic(group);
        }

        static GameObject CreateFireTruck(Transform parent)
        {
            var truck = UIBuilderUtil.Child(parent, "Caminhao_Bombeiros");
            var red = UIBuilderUtil.GetOrCreateMaterial("Mat_Caminhao", new Color(0.8f, 0.1f, 0.08f), false, false, 0.5f);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Carroceria", truck.transform, k_Truck + new Vector3(-1.2f, 1.2f, 0f), Vector3.zero, new Vector3(4.5f, 2.2f, 2.2f), red);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Cabine", truck.transform, k_Truck + new Vector3(2.0f, 1.0f, 0f), Vector3.zero, new Vector3(1.8f, 1.8f, 2.2f), red);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Vidro", truck.transform, k_Truck + new Vector3(2.95f, 1.3f, 0f), Vector3.zero, new Vector3(0.1f, 0.8f, 2.0f), ScenarioSceneKit.MatGlass, false);
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Giroflex", truck.transform, k_Truck + new Vector3(2.0f, 2.0f, 0f), Vector3.zero, new Vector3(0.6f, 0.2f, 0.3f), ScenarioSceneKit.MatMarker, false);
            for (var i = 0; i < 4; i++)
                UIBuilderUtil.Primitive(PrimitiveType.Cylinder, "Roda" + i, truck.transform, k_Truck + new Vector3(i < 2 ? -2.2f : 2.0f, 0.45f, i % 2 == 0 ? -1.1f : 1.1f), new Vector3(90f, 0f, 0f),
                    new Vector3(0.9f, 0.15f, 0.9f), ScenarioSceneKit.MatFlashlight, false);
            UIBuilderUtil.WorldText(truck.transform, "Texto", "BOMBEIROS 193", 1.2f, Color.white, k_Truck + new Vector3(-1.2f, 1.3f, -1.12f), Vector3.zero, new Vector2(3f, 0.5f));
            truck.SetActive(false);
            return truck;
        }

        static GameObject CreateSiren(Transform parent)
        {
            var go = UIBuilderUtil.Child(parent, "Sirene_Distante");
            go.transform.position = new Vector3(-40f, 2f, 25f);
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f; src.minDistance = 5f; src.maxDistance = 60f; src.volume = 0.1f;
            var loop = go.AddComponent<SimpleLoopSound>();
            loop.source = src; loop.kind = SimpleLoopSound.ClipKind.DistantSiren; loop.playOnStart = true;
            go.SetActive(false);
            return go;
        }

        static GameObject CreateNeighbor(Transform parent, MissionStepSO step, out XRSimpleInteractable interactable)
        {
            var routes = UIBuilderUtil.Child(parent, "Rotas_Vizinho");
            var toGate = ScenarioSceneKit.CreateWaypoints(routes.transform, "AoPortao", new Vector3(10f, 0f, 6.3f), new Vector3(-4.9f, 0f, 6.3f));
            var toMeeting = ScenarioSceneKit.CreateWaypoints(routes.transform, "AoEncontro", new Vector3(-7.0f, 0f, 6.4f), new Vector3(-8.3f, 0f, 6.4f));

            interactable = ScenarioSceneKit.CreateNpc(parent, "Vizinho", new Vector3(10f, 0f, 6.3f), -90f, new Color(0.55f, 0.3f, 0.2f), false, "", out var speech);
            var npc = interactable.gameObject;
            var follower = npc.AddComponent<CompanionFollower>();
            follower.talkInteractable = null;
            follower.maxLeadDistance = 100f;
            follower.speed = 0.9f;
            follower.speechBubble = speech;
            var intruder = npc.AddComponent<NeighborIntruder>();
            intruder.follower = follower;
            intruder.interactable = interactable;
            intruder.toGate.AddRange(toGate);
            intruder.toMeeting.AddRange(toMeeting);
            intruder.speech = speech;
            var target = npc.AddComponent<ObjectiveTarget>();
            target.step = step; target.markerHeightOffset = 0.5f;
            return npc;
        }
    }
}
#endif
