#if UNITY_EDITOR
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Gera a cena MainMenu (RF01): ambiente simples, XR Origin, painel diegético com a lista de cenários,
    /// opções de conforto e créditos. Sobrescreve a cena existente.
    /// </summary>
    public static class MainMenuBuilder
    {
        const string k_ScenePath = "Assets/Scenes/MainMenu.unity";
        const string k_XROriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        const string k_VignettePrefab = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";

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

            var data = SafeZoneDataBuilder.Build();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var matGround = UIBuilderUtil.GetOrCreateMaterial("Mat_MenuChao", new Color(0.32f, 0.36f, 0.4f), false, false, 0.1f);
            var matWall = UIBuilderUtil.GetOrCreateMaterial("Mat_MenuParede", new Color(0.55f, 0.6f, 0.66f), false, false, 0.05f);
            var matFade = UIBuilderUtil.GetOrCreateMaterial("Mat_Fade", new Color(0f, 0f, 0f, 1f), true, true, 0f, 4000);
            var matAccent = UIBuilderUtil.GetOrCreateMaterial("Mat_MenuDestaque", new Color(0.12f, 0.62f, 0.85f), false, false, 0.4f);

            // Luz
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambiente mínimo: chão + paredes baixas
            var ground = UIBuilderUtil.Primitive(PrimitiveType.Plane, "Chao", null, Vector3.zero, Vector3.zero, new Vector3(1.2f, 1f, 1.2f), matGround);
            var area = ground.AddComponent<TeleportationArea>();
            area.interactionLayers = 1 << 31;
            UIBuilderUtil.MarkStatic(ground);

            // Chão externo amplo: andando no quarto real ou com WASD no simulador a cabeça pode sair da sala;
            // sem chão embaixo, a gravidade do rig derrubaria o jogador no vazio.
            var matOutside = UIBuilderUtil.GetOrCreateMaterial("Mat_MenuGrama", new Color(0.30f, 0.45f, 0.28f), false, false, 0.05f);
            var outside = UIBuilderUtil.Primitive(PrimitiveType.Plane, "ChaoExterno", null, new Vector3(0f, -0.02f, 0f), Vector3.zero,
                new Vector3(6f, 1f, 6f), matOutside);
            UIBuilderUtil.MarkStatic(outside);
            for (var i = 0; i < 4; i++)
            {
                var angle = i * 90f;
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var wall = UIBuilderUtil.Primitive(PrimitiveType.Cube, "Parede_" + i, null, dir * 6f + Vector3.up * 1.25f, new Vector3(0f, angle, 0f),
                    new Vector3(12f, 2.5f, 0.2f), matWall);
                UIBuilderUtil.MarkStatic(wall);
            }
            UIBuilderUtil.Primitive(PrimitiveType.Cube, "Balcao", null, new Vector3(0f, 0.45f, 1.6f), Vector3.zero, new Vector3(2.2f, 0.9f, 0.5f), matAccent);

            // XR
            var im = new GameObject("XR Interaction Manager");
            im.AddComponent<XRInteractionManager>();
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<XRUIInputModule>();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_XROriginPrefab);
            var originGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var origin = originGo.GetComponent<XROrigin>();
            origin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            origin.tag = "Player";
            var cam = origin.Camera;

            var vignettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_VignettePrefab);
            TunnelingVignetteController vignette = null;
            if (vignettePrefab != null)
            {
                var v = (GameObject)PrefabUtility.InstantiatePrefab(vignettePrefab, cam.transform);
                v.transform.localPosition = Vector3.zero;
                v.transform.localRotation = Quaternion.identity;
                vignette = v.GetComponent<TunnelingVignetteController>();
            }
            var fader = cam.gameObject.AddComponent<ScreenFader>();
            fader.fadeMaterial = matFade;

            // Ponto de início: cabeça no centro da sala, de frente para o painel.
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            spawn.AddComponent<PlayerSpawnPoint>();

            var systems = new GameObject("Systems");
            var comfort = systems.AddComponent<ComfortSettingsApplier>();
            comfort.origin = origin;
            comfort.vignette = vignette;

            // Painel do menu
            var ui = new GameObject("UI");
            var options = ScenarioSceneKit.CreateOptionsPanel(ui.transform, cam.transform, false);

            var menuGo = UIBuilderUtil.Child(ui.transform, "MainMenuPanel");
            menuGo.transform.position = new Vector3(0f, 1.45f, 1.9f);
            var canvas = UIBuilderUtil.CreateWorldCanvas("Canvas", menuGo.transform, new Vector2(900f, 720f), 0.0012f);
            var panel = canvas.transform;
            UIBuilderUtil.FullPanel(panel, "Background", UIBuilderUtil.PanelColor);
            UIBuilderUtil.Panel(panel, "HeaderBar", UIBuilderUtil.Accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -96f), Vector2.zero);
            UIBuilderUtil.TopText(panel, "Title", "SafeZone VR", 44f, Color.white, TextAlignmentOptions.Center, 10f, 54f, 24f, FontStyles.Bold);
            UIBuilderUtil.TopText(panel, "Subtitle", "Aprenda a agir em desastres naturais com as orientações da Defesa Civil", 18f, Color.white, TextAlignmentOptions.Center, 62f, 28f, 24f);
            UIBuilderUtil.TopText(panel, "ChooseLabel", "Escolha um cenário", 22f, UIBuilderUtil.MutedText, TextAlignmentOptions.Left, 112f, 30f, 40f, FontStyles.Bold);

            var listRt = UIBuilderUtil.Rect(panel, "ScenarioList", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -470f), new Vector2(-40f, -148f));
            var layout = listRt.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            var template = UIBuilderUtil.Button(listRt, "ScenarioButtonTemplate", "Cenário", 22f, UIBuilderUtil.PanelLight, Color.white,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, out _);
            var le = template.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 68f;
            le.minHeight = 68f;
            template.gameObject.SetActive(false);

            var ctrl = menuGo.AddComponent<MainMenuController>();
            ctrl.catalog = data.catalog;
            ctrl.buttonContainer = listRt;
            ctrl.buttonTemplate = template;
            ctrl.optionsPanel = options;
            ctrl.descriptionText = UIBuilderUtil.TopText(panel, "Description", "", 17f, UIBuilderUtil.MutedText, TextAlignmentOptions.TopLeft, 480f, 90f, 40f);
            ctrl.optionsButton = UIBuilderUtil.BottomButton(panel, "Options", "Conforto e acessibilidade", 20f, UIBuilderUtil.PanelLight, 70f, 54f, 0.05f, 0.62f, 8f, out _);
            ctrl.quitButton = UIBuilderUtil.BottomButton(panel, "Quit", "Sair", 20f, new Color(0.45f, 0.2f, 0.2f), 70f, 54f, 0.66f, 0.95f, 8f, out _);
            UIBuilderUtil.Text(panel, "Credits", "Emergência: Defesa Civil 199 · Bombeiros 193 · SAMU 192", 15f, UIBuilderUtil.MutedText, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 18f), new Vector2(-24f, 48f));

            EditorSceneManager.SaveScene(scene, k_ScenePath);
            QuestProjectConfigurator.RegisterScenes();
            Debug.Log("[SafeZone VR] Cena MainMenu gerada em " + k_ScenePath);
        }
    }
}
#endif
