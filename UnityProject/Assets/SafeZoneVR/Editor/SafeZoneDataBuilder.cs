#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Cria/atualiza os ScriptableObjects do jogo: passos e cards do Alagamento (conteúdo baseado nas
    /// cartilhas "Como agir? Alagamentos/Inundações" do MDR/Defesa Civil), cenários futuros (bloqueados)
    /// e o catálogo usado pelo menu.
    /// </summary>
    public static class SafeZoneDataBuilder
    {
        public const string k_DataFolder = "Assets/SafeZoneVR/Data";
        public const string k_AlagamentoFolder = "Assets/SafeZoneVR/Data/Alagamento";
        public const string k_CardsFolder = "Assets/SafeZoneVR/Data/Alagamento/Cards";
        public const string k_ResourcesFolder = "Assets/SafeZoneVR/Resources";
        public const string k_AlagamentoSceneName = "Alagamento_EmCasa";
        public const string k_MenuSceneName = "MainMenu";

        public class AlagamentoData
        {
            public ScenarioSO scenario;
            public ScenarioCatalogSO catalog;
            public MissionStepSO stepValuables;
            public MissionStepSO stepPower;
            public MissionStepSO stepWater;
            public MissionStepSO stepKit;
            public MissionStepSO stepEvacuate;
        }

        [MenuItem("SafeZone VR/Build/1. Dados (passos, cards, cenários, catálogo)")]
        public static void BuildMenu()
        {
            Build();
            EditorUtility.DisplayDialog("SafeZone VR", "Dados criados/atualizados em Assets/SafeZoneVR/Data e Resources.", "OK");
        }

        public static AlagamentoData Build()
        {
            UIBuilderUtil.EnsureFolder(k_AlagamentoFolder);
            UIBuilderUtil.EnsureFolder(k_CardsFolder);
            UIBuilderUtil.EnsureFolder(k_ResourcesFolder);

            // Passos antigos (nomes/ordem obsoletos) são removidos; os validadores são recriados pelo builder da cena.
            foreach (var old in new[] { "Step_01_SafeArea", "Step_02_PowerOff", "Step_03_WaterValve", "Step_04_EmergencyKit", "Step_05_Evacuate" })
            {
                var p = $"{k_AlagamentoFolder}/{old}.asset";
                if (AssetDatabase.LoadAssetAtPath<MissionStepSO>(p) != null)
                    AssetDatabase.DeleteAsset(p);
            }

            var data = new AlagamentoData();

            data.stepValuables = Step("Step_01_ProtegerValores", "step_01_valores", 0,
                "Proteja documentos e objetos de valor",
                "Pegue a pasta de documentos e o porta-retrato que estão no sofá e coloque-os no alto do armário do quarto.",
                "Sala → Quarto",
                "A Defesa Civil orienta guardar documentos e objetos de valor em local alto e protegido, de fácil acesso para uma evacuação.");

            data.stepPower = Step("Step_02_DesligarEnergia", "step_02_energia", 1,
                "Desligue o quadro geral de energia",
                "Abaixe a alavanca do quadro de energia na parede do quarto.",
                "Quarto",
                "Água e eletricidade juntas causam choque e curto-circuito. Desligar o quadro geral evita acidentes.");

            data.stepWater = Step("Step_03_FecharRegistro", "step_03_registro", 2,
                "Feche o registro de entrada de água",
                "Saia pela porta da frente, vá até a garagem e gire o registro geral até fechar.",
                "Garagem (junto à parede da casa)",
                "Fechar o registro impede que a água contaminada do alagamento entre na rede interna da casa.");

            data.stepKit = Step("Step_04_KitEmergencia", "step_04_kit", 3,
                "Monte o kit de emergência",
                "Coloque na mochila ao lado da porta: água potável (cozinha), remédios (banheiro) e lanterna (quarto).",
                "Cozinha, banheiro e quarto → mochila na entrada",
                "Um kit com água, remédios, roupas e lanterna permite sair rápido de casa. Não use velas: risco de incêndio.");

            data.stepEvacuate = Step("Step_05_Evacuar", "step_05_evacuar", 4,
                "Vá para o ponto de encontro seguro",
                "Saia pela porta da frente, siga as setas verdes até o portão e suba no ponto de encontro. Nunca use o carro.",
                "Rua, em frente à casa",
                "Procure um local alto e espere a água baixar. Nunca atravesse ruas alagadas, nem mesmo de carro.");

            var cards = new List<EducationCardSO>
            {
                Card("Card_01_RuasAlagadas", "Nunca atravesse ruas alagadas",
                    "Nunca atravesse pontes, ruas ou avenidas alagadas, mesmo de carro, moto ou bicicleta: a força da água pode arrastá-lo. Prefira um local alto e espere o nível baixar."),
                Card("Card_02_Calma", "Mantenha a calma e siga a Defesa Civil",
                    "Siga rigorosamente as orientações da Defesa Civil e das autoridades da sua cidade. Acompanhe as informações oficiais por rádio, sites, SMS e carros de som."),
                Card("Card_03_FakeNews", "Desconfie de mensagens não oficiais",
                    "Mensagens em redes sociais podem ser falsas e causar pânico. Não repasse. Confirme sempre em canais oficiais."),
                Card("Card_04_Telefones", "Telefones úteis",
                    "Defesa Civil: 199\nBombeiros: 193\nSAMU: 192\nPolícia Militar: 190"),
                Card("Card_05_Depois", "Depois que a água baixar",
                    "Não use aparelhos elétricos que foram molhados. Descarte alimentos e bebidas que tiveram contato com a água. Lave e desinfete a casa usando luvas e botas. Volte para casa durante o dia."),
                Card("Card_06_Cuidar", "Cuide de quem precisa",
                    "Auxilie crianças, idosos e pessoas com dificuldade de locomoção. Animais de estimação também sofrem com a água: garanta a segurança deles."),
            };

            data.scenario = CreateOrLoad<ScenarioSO>($"{k_AlagamentoFolder}/Scenario_Alagamento.asset");
            data.scenario.EditorInitialize(
                "alagamento_em_casa",
                "Alagamento em Casa",
                "Chuva forte, alerta da Defesa Civil e a água começando a subir na rua. Pratique as decisões certas dentro de casa: proteger documentos, desligar energia, fechar o registro, montar o kit e evacuar pela rota segura.",
                k_AlagamentoSceneName,
                true,
                240f,
                420f,
                new List<MissionStepSO> { data.stepValuables, data.stepPower, data.stepWater, data.stepKit, data.stepEvacuate },
                cards);
            EditorUtility.SetDirty(data.scenario);

            var deslizamento = LockedScenario("Scenario_Deslizamento", "deslizamento", "Deslizamento de Encosta",
                "Sinais de risco em morros e encostas: trincas, árvores inclinadas, água barrenta. Em breve.");
            var enxurrada = LockedScenario("Scenario_Enxurrada", "enxurrada", "Enxurrada",
                "Água com força em ruas íngremes e córregos. Em breve.");
            var vendaval = LockedScenario("Scenario_Vendaval", "vendaval", "Vendaval",
                "Ventos fortes, telhados e fiação. Em breve.");

            data.catalog = CreateOrLoad<ScenarioCatalogSO>($"{k_ResourcesFolder}/ScenarioCatalog.asset");
            data.catalog.EditorInitialize(k_MenuSceneName, new List<ScenarioSO> { data.scenario, deslizamento, enxurrada, vendaval });
            EditorUtility.SetDirty(data.catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return data;
        }

        static MissionStepSO Step(string fileName, string id, int order, string title, string instruction, string location, string why)
        {
            var step = CreateOrLoad<MissionStepSO>($"{k_AlagamentoFolder}/{fileName}.asset");
            step.EditorInitialize(id, order, title, instruction, location, why);
            EditorUtility.SetDirty(step);
            return step;
        }

        static EducationCardSO Card(string fileName, string title, string tip)
        {
            var card = CreateOrLoad<EducationCardSO>($"{k_CardsFolder}/{fileName}.asset");
            card.EditorInitialize(title, tip, "Defesa Civil / MDR — Como agir? Alagamentos e Inundações");
            EditorUtility.SetDirty(card);
            return card;
        }

        static ScenarioSO LockedScenario(string fileName, string id, string displayName, string description)
        {
            var s = CreateOrLoad<ScenarioSO>($"{k_DataFolder}/{fileName}.asset");
            s.EditorInitialize(id, displayName, description, "", false, 240f, 420f, new List<MissionStepSO>(), new List<EducationCardSO>());
            EditorUtility.SetDirty(s);
            return s;
        }

        public static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
