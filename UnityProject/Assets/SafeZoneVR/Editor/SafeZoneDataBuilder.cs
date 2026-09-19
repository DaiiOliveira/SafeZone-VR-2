#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SafeZoneVR.Editor
{
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

            foreach (var old in new[] { "Step_01_SafeArea", "Step_02_PowerOff", "Step_03_WaterValve", "Step_04_EmergencyKit", "Step_05_Evacuate" })
            {
                var p = $"{k_AlagamentoFolder}/{old}.asset";
                if (AssetDatabase.LoadAssetAtPath<MissionStepSO>(p) != null)
                    AssetDatabase.DeleteAsset(p);
            }

            var data = new AlagamentoData();

            data.stepValuables = Step("Step_01_ProtegerValores", "step_01_valores", 0,
                "Proteja os objetos de valor",
                "Pegue o porta-retrato que está no sofá e coloque-o em cima do guarda-roupa do quarto de casal.",
                "Sala → Quarto de casal",
                "A Defesa Civil orienta guardar objetos de valor em local alto e protegido. Os documentos vão com você, dentro do kit.");

            data.stepPower = Step("Step_02_DesligarEnergia", "step_02_energia", 1,
                "Desligue o quadro geral de energia",
                "Abaixe a alavanca do quadro de energia na parede do corredor.",
                "Corredor",
                "Água e eletricidade juntas causam choque e curto-circuito. Desligar o quadro geral evita acidentes.");

            data.stepWater = Step("Step_03_FecharRegistro", "step_03_registro", 2,
                "Feche o registro de entrada de água",
                "Saia pela porta da frente, vá até a garagem e gire o registro geral, na parede da casa ao lado do carro, até fechar.",
                "Garagem (parede da casa, ao lado do carro)",
                "Fechar o registro impede que a água contaminada do alagamento entre na rede interna da casa.");

            data.stepKit = Step("Step_04_KitEmergencia", "step_04_kit", 3,
                "Monte o kit de emergência",
                "Coloque na mochila da sala: a pasta de documentos (sofá), água potável (bancada da cozinha), remédios (bancada do banheiro) e lanterna (cama do quarto de casal).",
                "Sala, cozinha, banheiro e quarto → mochila na sala",
                "O kit reúne o essencial para sair rápido: documentos, água, remédios e lanterna. Não use velas: risco de incêndio.");

            data.stepEvacuate = Step("Step_05_Evacuar", "step_05_evacuar", 4,
                "Vá para o ponto de encontro seguro",
                "Saia pela porta da frente, siga as setas verdes pelo portão e suba no ponto de encontro na calçada. Nunca use o carro.",
                "Calçada, em frente à casa",
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
            data.scenario.EditorSetTimeLimit(420f);
            EditorUtility.SetDirty(data.scenario);

            foreach (var old in new[] { "Scenario_Deslizamento", "Scenario_Enxurrada", "Scenario_Vendaval" })
            {
                var p = $"{k_DataFolder}/{old}.asset";
                if (AssetDatabase.LoadAssetAtPath<ScenarioSO>(p) != null)
                    AssetDatabase.DeleteAsset(p);
            }

            var incendio = BuildIncendio().scenario;
            var granizo = BuildGranizo().scenario;

            data.catalog = CreateOrLoad<ScenarioCatalogSO>($"{k_ResourcesFolder}/ScenarioCatalog.asset");
            data.catalog.EditorInitialize(k_MenuSceneName, new List<ScenarioSO> { data.scenario, incendio, granizo });
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

        static MissionStepSO StepAt(string folder, string file, string id, int order, string title, string instruction, string location, string why)
        {
            var step = CreateOrLoad<MissionStepSO>($"{folder}/{file}.asset");
            step.EditorInitialize(id, order, title, instruction, location, why);
            EditorUtility.SetDirty(step);
            return step;
        }

        static EducationCardSO CardAt(string folder, string file, string source, string title, string tip)
        {
            var card = CreateOrLoad<EducationCardSO>($"{folder}/Cards/{file}.asset");
            card.EditorInitialize(title, tip, source);
            EditorUtility.SetDirty(card);
            return card;
        }

        public const string k_IncendioFolder = "Assets/SafeZoneVR/Data/Incendio";
        public const string k_IncendioSceneName = "Incendio_EmCasa";
        const string k_FonteCBMSC = "Corpo de Bombeiros Militar de Santa Catarina — Incêndio em Edificação";
        const string k_FonteCBMCE = "Corpo de Bombeiros Militar do Ceará — Fogo em panela com óleo";
        const string k_FonteCBMAL = "Corpo de Bombeiros Militar de Alagoas — Incêndio";
        const string k_FonteCBMVix = "Corpo de Bombeiros — Instruções em caso de incêndio (Prefeitura de Vitória)";

        public class IncendioData
        {
            public ScenarioSO scenario;
            public MissionStepSO gas, tampa, ventilar, energia, sair, porta, ligar, vizinho, encontro;
        }

        public static IncendioData BuildIncendio()
        {
            UIBuilderUtil.EnsureFolder($"{k_IncendioFolder}/Cards");
            var f = k_IncendioFolder;
            var d = new IncendioData();

            d.gas = StepAt(f, "Step_01_Gas", "inc_01_gas", 0, "Feche o gás",
                "Gire o botão do fogão para desligar e feche o registro do botijão, ao lado do fogão.",
                "Cozinha",
                "Sem gás, o fogo da panela deixa de ser alimentado. É o primeiro passo indicado pelos Bombeiros.");
            d.tampa = StepAt(f, "Step_02_Tampa", "inc_02_tampa", 1, "Abafe a panela com a tampa",
                "Pegue a tampa no balcão e coloque sobre a panela. Nunca jogue água no óleo.",
                "Cozinha",
                "Sem ar, o fogo apaga. Água em óleo quente vira vapor na hora e espalha o óleo em chamas.");
            d.ventilar = StepAt(f, "Step_03_Ventilar", "inc_03_ventilar", 2, "Ventile e deixe esfriar",
                "Abra a janela da cozinha e não tire a tampa: deixe a panela esfriar.",
                "Cozinha",
                "A fumaça é tóxica: ventilar ajuda a tirá-la de casa. Se a tampa sair cedo, o fogo pode voltar.");
            d.energia = StepAt(f, "Step_04_Energia", "inc_04_energia", 3, "Desligue a energia",
                "Faíscas atrás da TV! Não jogue água. Vá ao corredor e abaixe a alavanca do quadro de energia.",
                "Corredor",
                "Fogo em aparelho elétrico nunca se apaga com água. Desligar a energia corta a fonte das faíscas.");
            d.sair = StepAt(f, "Step_05_Sair", "inc_05_sair", 4, "Saia abaixado",
                "O fogo cresceu: não tente apagar sozinho. Abaixe-se por baixo da fumaça e saia pela porta da frente.",
                "Corredor → porta da frente",
                "A fumaça tóxica sobe e fica no alto; perto do chão o ar é mais limpo. Se não conseguir apagar, saia rapidamente.");
            d.porta = StepAt(f, "Step_06_Porta", "inc_06_porta", 5, "Feche a porta sem trancar",
                "Já do lado de fora, feche a porta da frente. Não tranque: os Bombeiros podem precisar passar.",
                "Porta da frente",
                "A porta fechada segura o fogo e a fumaça lá dentro; destrancada, não atrapalha quem precisa entrar ou sair.");
            d.ligar = StepAt(f, "Step_07_Ligar", "inc_07_193", 6, "Ligue para os Bombeiros",
                "Na calçada, em segurança, ligue 193 pelo relógio.",
                "Ponto de encontro (calçada)",
                "Ligar 193 depois de sair garante o socorro sem que você fique perto do fogo.");
            d.vizinho = StepAt(f, "Step_08_Vizinho", "inc_08_vizinho", 7, "Não deixe ninguém entrar",
                "O vizinho quer entrar para salvar a TV. Aponte para ele e aperte o gatilho para impedi-lo.",
                "Portão da frente",
                "Nenhum objeto vale o risco. Não permita que ninguém entre em um local em chamas: espere os Bombeiros.");

            d.encontro = StepAt(f, "Step_09_Encontro", "inc_09_encontro", 8, "Vá para o ponto de encontro",
                "Afaste-se da casa e espere os Bombeiros na placa de ponto de encontro, na calçada.",
                "Ponto de encontro (calçada)",
                "O ponto de encontro combinado reúne todo mundo em local seguro e longe do fogo, e deixa a frente livre para os Bombeiros.");

            d.gas.EditorSetOrderGroup(1);
            d.tampa.EditorSetOrderGroup(1);

            var cards = new List<EducationCardSO>
            {
                CardAt(f, "Card_01_Panela", k_FonteCBMCE, "Fogo na panela", "Feche o registro do fogão e o do botijão. Abafe a panela com a tampa ou com um pano úmido e bem torcido e espere esfriar antes de destampar. Nunca jogue água."),
                CardAt(f, "Card_02_Eletrico", k_FonteCBMSC, "Fogo em aparelho elétrico", "Nunca jogue água em aparelho ligado na energia. Desligue a rede elétrica. O extintor indicado é o de CO2 ou pó químico. Se não conseguir apagar, saia e ligue 193."),
                CardAt(f, "Card_03_Principio", k_FonteCBMSC, "Só o que é pequeno", "Tente apagar apenas princípios de incêndio. Se o fogo crescer ou você não conseguir controlar, não combata sozinho: saia rapidamente."),
                CardAt(f, "Card_04_Fumaca", k_FonteCBMSC, "Fumaça: abaixe-se", "A fumaça é o maior perigo em um incêndio. Ande agachado, porque a fumaça fica no alto, e use um pano úmido sobre o nariz."),
                CardAt(f, "Card_05_Sair", k_FonteCBMAL, "Ao sair", "Feche as portas e janelas atrás de você, sem trancar. Não perca tempo salvando objetos. Depois de sair, ligue 193 e não deixe ninguém entrar."),
                CardAt(f, "Card_06_Preso", k_FonteCBMAL, "Se não conseguir sair", "Feche, sem trancar, a porta do cômodo; vede as frestas com cobertor ou tapete; fique perto de uma janela e sinalize; mantenha-se junto ao chão com um pano molhado no nariz e na boca."),
                CardAt(f, "Card_07_Extintor", k_FonteCBMSC, "Extintor", "Use o extintor da classe certa, levando-o na vertical. Tire o pino, aproxime-se com cuidado e ataque a base do fogo em leque. Não use extintor de água em eletrônicos ou líquidos inflamáveis."),
                CardAt(f, "Card_08_PrevencaoEletrica", k_FonteCBMSC, "Prevenção: eletricidade", "Evite benjamins e \"T\", não ligue vários aparelhos na mesma tomada e não faça gambiarras. Desligue os aparelhos ao sair de casa e tire o ferro de passar da tomada. Não use notebook sobre cama ou sofá."),
                CardAt(f, "Card_09_PrevencaoCozinha", k_FonteCBMSC, "Prevenção: cozinha e gás", "Não deixe o fogão ligado sem supervisão e mantenha as crianças longe dele. Faça a manutenção do gás e não guarde álcool, gasolina, panos ou óleos perto de fontes de calor. Prefira lanternas a velas."),
                CardAt(f, "Card_10_Telefones", k_FonteCBMVix, "Telefones úteis", "Bombeiros: 193\nDefesa Civil: 199\nSAMU: 192\nPolícia Militar: 190"),
            };

            d.scenario = CreateOrLoad<ScenarioSO>($"{k_DataFolder}/Scenario_Incendio.asset");
            d.scenario.EditorInitialize("incendio", "Incêndio em Casa",
                "A panela pegou fogo e, depois, um benjamim sobrecarregado atrás da TV. Aprenda a apagar só o que é pequeno, sem água no óleo ou na eletricidade, e a sair abaixado, fechar a porta sem trancar e ligar 193.",
                k_IncendioSceneName, true, 300f, 480f,
                new List<MissionStepSO> { d.gas, d.tampa, d.ventilar, d.energia, d.sair, d.porta, d.ligar, d.vizinho, d.encontro }, cards);
            d.scenario.EditorSetTimeLimit(480f);
            EditorUtility.SetDirty(d.scenario);
            return d;
        }

        public const string k_GranizoFolder = "Assets/SafeZoneVR/Data/Granizo";
        public const string k_GranizoSceneName = "Granizo_Praca";
        const string k_FonteGranizo = "Defesa Civil / MDR — Como agir? Granizo (MDR-08/2021)";

        public class GranizoData
        {
            public ScenarioSO scenario;
            public MissionStepSO vaga, joaquim, abrigo, aguardar, voltar, forro, sair, comunicar;
        }

        public static GranizoData BuildGranizo()
        {
            UIBuilderUtil.EnsureFolder($"{k_GranizoFolder}/Cards");
            var f = k_GranizoFolder;
            var d = new GranizoData();

            d.vaga = StepAt(f, "Step_01_Vaga", "gran_01_vaga", 0, "Oriente o motorista",
                "O Carlos quer deixar o carro antes do granizo. Aponte para a vaga mais segura e aperte o gatilho.",
                "Rua em frente à praça",
                "Torres de transmissão e placas de propaganda podem cair com o vento e o granizo. Estacione longe delas.");
            d.joaquim = StepAt(f, "Step_02_Joaquim", "gran_02_joaquim", 1, "Ajude o Seu Joaquim",
                "O Seu Joaquim usa andador e está no banco da praça. Aponte para ele e aperte o gatilho para acompanhá-lo.",
                "Banco da praça",
                "Idosos e pessoas com dificuldade de locomoção precisam de ajuda para chegar a um abrigo a tempo.");
            d.abrigo = StepAt(f, "Step_03_Abrigo", "gran_03_abrigo", 2, "Leve-o a um abrigo seguro",
                "Leve o Seu Joaquim até a UBS de alvenaria, na esquina à direita da praça. Evite as árvores, o pergolado, o ponto de ônibus metálico e a barraca.",
                "UBS, na esquina da praça",
                "Construções resistentes, sem risco de destelhamento, protegem do granizo e do vento. Árvores, estruturas metálicas e construções precárias podem cair.");
            d.aguardar = StepAt(f, "Step_04_Aguardar", "gran_04_aguardar", 3, "Espere o granizo passar",
                "Fique dentro da UBS até o granizo parar.",
                "UBS",
                "Em local seguro, espere a chuva de granizo terminar antes de sair.");
            d.voltar = StepAt(f, "Step_05_Voltar", "gran_05_voltar", 4, "Volte para casa com cuidado",
                "O chão está coberto de gelo. Volte pela calçada da praça e atravesse a rua na faixa de pedestres até o portão de casa.",
                "Calçada → faixa de pedestres → portão de casa",
                "O granizo deixa o piso escorregadio. Prefira o caminho mais seguro, mesmo que seja mais longo.");
            d.forro = StepAt(f, "Step_06_Forro", "gran_06_forro", 5, "Confira a casa por dentro",
                "Entre em casa e olhe o teto do quarto de casal. Aponte para o forro e aperte o gatilho.",
                "Quarto de casal",
                "Telhas quebradas pelo granizo deixam a água entrar e podem enfraquecer o telhado e o forro.");
            d.sair = StepAt(f, "Step_07_Sair", "gran_07_sair", 6, "Saia do quarto imediatamente",
                "O forro está cedendo. Saia do quarto e vá para a sala.",
                "Quarto → sala",
                "Se o telhado ou o forro puder desabar, saia do local imediatamente.");
            d.comunicar = StepAt(f, "Step_08_Comunicar", "gran_08_comunicar", 7, "Comunique às autoridades",
                "Ligue para a Defesa Civil (199) ou para os Bombeiros (193) pelo relógio e conte o que viu.",
                "Relógio de pulso",
                "Avisar as autoridades permite vistoriar o telhado e evitar acidentes.");

            d.vaga.EditorSetOrderGroup(1);
            d.joaquim.EditorSetOrderGroup(1);

            var src = k_FonteGranizo;
            var cards = new List<EducationCardSO>
            {
                CardAt(f, "Card_01_Abrigo", src, "Onde se abrigar", "Longe de casa e em local aberto, proteja-se em locais seguros e resistentes a ventos fortes, onde não há risco de destelhamento. Evite construções precárias."),
                CardAt(f, "Card_02_ArvoresMetal", src, "Longe de árvores e estruturas metálicas", "Não fique debaixo de árvores nem de estruturas metálicas: há risco de quedas."),
                CardAt(f, "Card_03_Veiculos", src, "Onde estacionar", "Não estacione veículos perto de torres de transmissão e placas de propaganda."),
                CardAt(f, "Card_04_Cuidar", src, "Cuide de quem precisa", "Auxilie crianças, idosos e pessoas com dificuldade de locomoção próximas a você."),
                CardAt(f, "Card_05_Depois", src, "Depois do granizo", "Cuidado ao se deslocar: o granizo deixa o piso escorregadio. Não suba em telhados molhados. Se notar que o telhado pode desabar, saia do local imediatamente e comunique às autoridades."),
                CardAt(f, "Card_06_Prevencao", src, "Prevenção", "Mantenha em boas condições a estrutura da casa, principalmente o madeiramento do telhado. Mantenha as árvores do terreno sadias, podadas e longe da rede elétrica, e informe a Prefeitura sobre árvores doentes na calçada."),
                CardAt(f, "Card_07_OQueE", src, "O que é granizo", "Pedras de gelo com 5 mm ou mais, formadas em nuvens do tipo cumulonimbus. Destroem telhados, derrubam árvores, danificam a rede elétrica, amassam latarias e quebram vidros de veículos."),
                CardAt(f, "Card_08_Oficial", src, "Informação oficial", "Mantenha a calma e siga a Defesa Civil. Receba alertas pelo SMS 40199. Desconfie de mensagens não oficiais e não as repasse."),
                CardAt(f, "Card_09_Telefones", src, "Telefones úteis", "Defesa Civil: 199\nBombeiros: 193\nSAMU: 192\nPolícia Militar: 190"),
            };

            d.scenario = CreateOrLoad<ScenarioSO>($"{k_DataFolder}/Scenario_Granizo.asset");
            d.scenario.EditorInitialize("granizo", "Granizo",
                "Tempestade com granizo chegando e você na praça em frente de casa. Oriente onde estacionar, ajude o Seu Joaquim, escolha um abrigo resistente e, depois, volte com cuidado e aja se o telhado ameaçar ceder.",
                k_GranizoSceneName, true, 360f, 600f,
                new List<MissionStepSO> { d.vaga, d.joaquim, d.abrigo, d.aguardar, d.voltar, d.forro, d.sair, d.comunicar }, cards);
            d.scenario.EditorSetTimeLimit(600f);
            EditorUtility.SetDirty(d.scenario);
            return d;
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
