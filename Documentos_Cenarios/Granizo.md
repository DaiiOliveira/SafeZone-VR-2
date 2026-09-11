# Cenário: Granizo

> Documento de especificação para implementar a fase **Granizo** do SafeZone VR.
> Segue a arquitetura já usada em *Alagamento em Casa*: conteúdo em ScriptableObjects, validadores
> sobre eventos do XR Interaction Toolkit e cena gerada por um builder de Editor.
> O documento é **autossuficiente**: a seção 9 traz todo o código novo necessário, inclusive a
> infraestrutura compartilhada com a fase *Incêndio*. Se ela já tiver sido criada, apenas reutilize.

| Item | Valor |
|------|-------|
| `scenarioId` | `granizo` |
| Nome no menu | Granizo |
| Asset do cenário | `Assets/SafeZoneVR/Data/Scenario_Granizo.asset` (novo) |
| Cena | `Assets/Scenes/Fases/Granizo_Praca.unity` (`sceneName = "Granizo_Praca"`) |
| Fonte oficial | *Como agir? Granizo*, MDR/CENAD, MDR-08/2021 (`Documentos_Orientações_Defesa_Civil/granizo.pdf`) |
| Duração alvo | 3 estrelas ≤ **360 s** sem erros · 2 estrelas ≤ **600 s** com até 2 penalidades |
| Passos | 8 (durante na praça → depois em casa) |
| Classificação | Livre: o granizo nunca atinge o jogador nem as pessoas; só telhados, chão e carros |

---

## 1. Objetivo pedagógico

A maior parte das orientações "durante" da cartilha vale para quem está **longe de casa, em local
aberto**. Por isso a fase começa na **praça do bairro**, quando uma tempestade com granizo se aproxima.
O jogador aprende a:
1. **escolher o abrigo certo**, que é resistente e sem risco de destelhamento. Árvores, estruturas
   metálicas e construções precárias estão fora;
2. **ajudar quem precisa** (um idoso na praça);
3. **orientar sobre veículos**: nada de estacionar perto de torres de transmissão e placas de propaganda;
4. **agir depois**: andar com cuidado no piso escorregadio, não subir em telhado molhado, sair do cômodo
   se o telhado ameaçar ceder e avisar as autoridades.

## 2. Base oficial → o que vira jogo

| Orientação da cartilha | Momento | Como aparece no jogo |
|------------------------|---------|----------------------|
| Longe de casa e em local aberto, evite abrigar-se em edificações precárias | Durante | Decisão insegura: barraca de madeira com telhado de zinco solto |
| Proteja-se em locais seguros e resistentes a ventos fortes, sem risco de destelhamento | Durante | **Missão 3**: entrar na UBS de alvenaria com laje |
| Não fique debaixo de árvores e estruturas metálicas | Durante | Decisões inseguras: árvore grande e ponto de ônibus metálico |
| Auxilie crianças, idosos e pessoas com dificuldade de locomoção | Durante | **Missão 2**: o Seu Joaquim, que usa andador, acompanha o jogador até o abrigo |
| Não estacione veículos perto de torres de transmissão e placas de propaganda | Durante | **Missão 1**: indicar ao motorista uma vaga segura, entre 3 opções |
| Mantenha a calma; permaneça em local seguro (recomendações gerais) | Durante | **Missão 4**: aguardar no abrigo até o granizo passar |
| Após a chuva, cuidado ao se deslocar: o granizo deixa o piso escorregadio | Depois | **Missão 5**: voltar para casa pela calçada com corrimão, sem o atalho da rampa gelada |
| Não suba em telhados molhados | Depois | Decisão insegura: escada encostada no telhado |
| Se notar que o telhado pode desabar, saia imediatamente e comunique às autoridades | Depois | **Missões 6, 7 e 8**: perceber o forro cedendo, sair do quarto e ligar 199 ou 193 |
| Mantenha a estrutura da casa e o madeiramento do telhado em bom estado; árvores sadias, podadas e longe da rede elétrica; avise a Prefeitura de árvores doentes na calçada | Preventiva | Cards + detalhes visuais: telhas quebradas na casa e árvore com galho seco na calçada, com etiqueta |
| Acompanhe informações oficiais e desconfie de mensagens não oficiais | Geral | Alerta por SMS 40199 no briefing + boato opcional no relógio + cards |

Telefones exibidos: Defesa Civil 199 · Bombeiros 193 · Polícia Militar 190 · SAMU 192.
Para receber alertas: SMS **40199**, Telegram *Defesa Civil Alertas*, WhatsApp (61) 2034-4611.

## 3. Premissa e tom

**Briefing (texto do `IntroBriefingPanel`):**

> **Alerta da Defesa Civil (SMS 40199):** tempestade com possibilidade de **granizo** no seu bairro nos
> próximos minutos.
> Você está na praça, a duas quadras de casa. Procure um local seguro e ajude quem estiver por perto.
> O relógio no pulso esquerdo mostra cada missão e permite ligar para a Defesa Civil e os Bombeiros.

**Tom (RNF03):** céu escurecendo, vento e o barulho das pedras de gelo em telhados e carros. É urgente,
mas sem medo.
- O granizo cai **ao redor**: nas lajes, no chão, no capô dos carros.
- **Nenhuma pedra atinge pessoas** e não aparecem ferimentos.
- O vidro de um carro estacionado debaixo da placa racha. O barulho é baixo e o carro está vazio,
  mostrando a orientação sem susto.
- Em casa, depois da chuva: telhas quebradas, uma goteira e o forro do quarto cedendo devagar.
  **Nada desaba** enquanto o jogador está no cômodo.

Proibido:
- pedras voando na direção da câmera;
- pessoas atingidas;
- estrondos altos e tremor de câmera;
- forro caindo sobre o jogador.

## 4. Ambiente

### 4.1 Áreas da cena (metros, espaço de mundo)

A casa do Alagamento é reaproveitada na mesma posição (prefab `Casa_SafeZone`, seção 9.1). A rua é
estendida para oeste até a praça.

| Área | Posição / tamanho | Detalhes |
|------|-------------------|----------|
| Casa do jogador | igual ao Alagamento (piso x −35.85..−29.05, z 5.94..14.94) | Destino da missão 5; portão de pedestres em x −33.66..−32.31, z 17.95 |
| Rua | z 18..27, estendida de x −20 até x −80 | Mesmo material `Mat_Rua`; calçada norte com **corrimão** entre a praça e a casa (rota segura) |
| **Praça** | x −72..−44, z 28..46 | Piso de pedra, bancos, canteiros. **Início do jogador: `(-52, 0, 30)`**, olhando para +Z |
| Árvore grande | x −56, z 36 (copa de 8 m) | Decisão insegura: `WrongActionZone` sob a copa (raio 3,5 m) |
| Ponto de ônibus metálico | x −47, z 29 (cobertura de chapa) | Decisão insegura: `WrongActionZone` sob a cobertura |
| Barraca precária | x −66, z 33 (madeira + telhado de zinco solto) | Decisão insegura: `WrongActionZone` no interior |
| **UBS de alvenaria** (abrigo) | x −60..−50, z 40..46, laje de concreto, porta larga em x −55, z 40 | Missões 3 e 4. Placa "UBS — ABRIGO"; interior simples com bancos |
| Seu Joaquim | banco em x −49, z 33 | Idoso com andador (NPC de missão) |
| Motorista e carro | vaga provisória em x −62, z 25 | Motorista Carlos ao lado do carro (reaproveitar o modelo da Kombi) |
| Vagas da missão 1 | A: sob a placa de propaganda (x −70, z 22) · B: junto à torre de transmissão (x −76, z 30) · C: estacionamento aberto do mercado, longe de placas e torres (x −40, z 24) | Cada vaga tem uma placa/marcador selecionável |
| Rampa do atalho | escadaria/rampa lateral da praça até a rua (x −44, z 28..24) | Na fase Depois, coberta de gelo: decisão insegura (atalho escorregadio) |
| Escada no quintal | encostada no telhado da casa (x −34.8, z 15.2) | Decisão insegura na fase Depois |
| Forro do quarto | teto do quarto (x −32.8..−29.0, z 6.2..9.4, y 2.04) | Na fase Depois: goteira + barriga no forro + trinca (sinal da missão 6) |

**Navegação:** teleporte em toda a praça, na calçada, na rua e dentro da casa. A escada e o telhado
**não** são áreas de teleporte, então o jogador não consegue "subir".

### 4.2 Estados do ambiente (`ScenarioPhaseSwitcher` + `HailController`)

| Fase | Quando | Céu e luz | Granizo | Outros |
|------|--------|-----------|---------|--------|
| **Antes do granizo** | início até concluir as missões 1 e 2 | nuvens escuras chegando; vento leve | nenhum; trovões distantes suaves | carro na vaga provisória; Seu Joaquim no banco |
| **Granizo** | ao concluir 1 e 2 (sem fade) | escurece; chuva forte | `HailController`: intensidade 0,3 → 1,0 em 15 s; cai para 0 nos últimos 8 s da missão 4 | pedras quicando no chão e nos telhados; camada branca de gelo acumulando; o vidro do carro sob a placa (vaga A) racha em silêncio |
| **Depois** | ao concluir a missão 4 (fade 1 s, "Alguns minutos depois…") | céu abrindo, luz de fim de tarde | parado; o gelo continua no chão | rampa do atalho gelada; em casa: telhas quebradas, goteira, forro cedendo e escada encostada |

### 4.3 Orçamento de desempenho (Quest 3, 72 FPS)

- **Granizo:**
  - 1 sistema de partículas preso à posição do jogador, num volume de 20×20 m, com até 400 partículas.
  - Colisão em modo **World**, com qualidade **Low**, `collidesWith` restrito à layer `HailBlockers`, *bounce* 0,3 e `maxCollisionShapes` 64.
  - A layer `HailBlockers` só tem colliders simples: planos do chão, a laje da UBS, a cobertura do ponto de ônibus, o telhado da barraca e os telhados da casa.
  - Não use o modo Planes: os planos são infinitos, e o plano da laje barraria o granizo no mundo todo.
  - As partículas somem 1,5 s depois do primeiro contato.
- **Acúmulo de gelo:** 3 planos (praça, rua, calçada) com um material branco transparente cujo alpha sobe de 0 a 0,6. Sem neve simulada.
- **Batidas no telhado e no capô:** 2 fontes de áudio 3D com o loop `CreateHailLoop` (seção 9.12), sem uma fonte por pedra.
- **Árvores:** até 10, com balanço por shader de vértice e sombra só na árvore grande.
- **Meta:** até 150 draw calls e 250k triângulos na visão.

---

## 5. Missões

Os textos abaixo vão direto para os `MissionStepSO` (título, instrução do relógio, `locationHint` e `whyItMatters`).

| # | `stepId` | Título | Instrução (relógio) | Onde | Por que importa (aparece ao concluir) | Validação |
|---|----------|--------|---------------------|------|----------------------------------------|-----------|
| 1 | `gran_01_vaga` | Oriente o motorista | O Carlos quer deixar o carro antes do granizo. Aponte para a vaga mais segura e aperte o gatilho. | Rua em frente à praça | Torres de transmissão e placas de propaganda podem cair com o vento e o granizo. Estacione longe delas. | `StepCompleteOnChoice` com 3 `DecisionOption`; só a vaga C (mercado) conclui |
| 2 | `gran_02_joaquim` | Ajude o Seu Joaquim | O Seu Joaquim usa andador e está no banco da praça. Aponte para ele e aperte o gatilho para acompanhá-lo. | Banco da praça | Idosos e pessoas com dificuldade de locomoção precisam de ajuda para chegar a um abrigo a tempo. | `StepCompleteOnInteractCount` (1 interagível: o Seu Joaquim); o mesmo evento liga o `CompanionFollower` |
| 3 | `gran_03_abrigo` | Leve-o a um abrigo seguro | Entre com o Seu Joaquim na UBS de alvenaria. Evite árvores, o ponto de ônibus metálico e a barraca. | UBS, ao norte da praça | Construções resistentes, sem risco de destelhamento, protegem do granizo e do vento. Árvores, estruturas metálicas e construções precárias podem cair. | `StepCompleteOnCompanionInZone` (jogador **e** Seu Joaquim dentro da UBS) |
| 4 | `gran_04_aguardar` | Espere o granizo passar | Fique dentro da UBS até o granizo parar. | UBS | Em local seguro, espere a chuva de granizo terminar antes de sair. | `StepCompleteOnStayInZone` (30 s na UBS); o granizo para nos últimos 8 s |
| 5 | `gran_05_voltar` | Volte para casa com cuidado | O chão está coberto de gelo. Volte pela calçada com corrimão, não pela rampa do atalho. | Calçada norte → portão de casa | O granizo deixa o piso escorregadio. Prefira o caminho mais seguro, mesmo que seja mais longo. | `StepCompleteOnTriggerZone` no portão de pedestres da casa |
| 6 | `gran_06_forro` | Confira a casa por dentro | Entre em casa e olhe o teto do quarto. Aponte para o forro e aperte o gatilho. | Quarto | Telhas quebradas pelo granizo deixam a água entrar e podem enfraquecer o telhado e o forro. | `StepCompleteOnInteractCount` (1: `InspectableSign` do forro) |
| 7 | `gran_07_sair` | Saia do quarto imediatamente | O forro está cedendo. Saia do quarto e vá para a sala. | Quarto → sala | Se o telhado ou o forro puder desabar, saia do local imediatamente. | `StepCompleteOnTriggerZone` na sala. A zona **só é ligada** ao concluir a missão 6 (`ActivateOnStepCompleted`), para não concluir quando o jogador passa pela sala a caminho do quarto |
| 8 | `gran_08_comunicar` | Comunique às autoridades | Ligue para a Defesa Civil (199) ou para os Bombeiros (193) pelo relógio e conte o que viu. | Relógio de pulso | Avisar as autoridades permite vistoriar o telhado e evitar acidentes. | `EmergencyCallPanel` aceita `199` e `193` → `StepCompleteOnButtonPress.OnButtonPressed()` |

**Ordem e penalidade:** o `ScenarioManager` conta "fora de ordem" como penalidade. As missões **1 e 2**
não têm ordem entre si na cartilha e formam o **grupo de ordem 1**: podem ser feitas em qualquer
sequência sem penalidade (seção 9.2). As demais seguem a ordem da tabela.

**Transições de fase** (`ScenarioPhaseSwitcher`):
- **Antes → Granizo:** dispara quando as missões 1 e 2 estiverem concluídas, sem fade.
  - O `HailController` começa com intensidade 0,3 e sobe para 1,0 em 15 s.
  - O relógio avisa: "Defesa Civil: granizo caindo no bairro. Procure um local seguro e resistente."
- **Granizo → Depois:** dispara ao concluir a missão 4.
  - Fade de 1 s com o texto "Alguns minutos depois…".
  - O gelo continua no chão e a rampa do atalho fica gelada.
  - Em casa aparecem as telhas quebradas, a goteira, o forro cedendo e a escada no quintal.
  - O Seu Joaquim fica na UBS, onde a família dele chega. O relógio mostra: "Seu Joaquim: obrigado! Minha filha já está vindo me buscar."

**Resultado da missão 1 (vaga C):** o carro vai sozinho até o estacionamento do mercado em 6 s,
num movimento cinemático em linha reta. O motorista agradece pelo relógio: "Carlos: boa! Longe da placa e da torre."

**Falas da central (missão 8):**
- 199: "Defesa Civil, obrigado pelo aviso. Não entre no quarto e não suba no telhado. Uma equipe vai vistoriar a casa."
- 193: "Corpo de Bombeiros, ocorrência registrada. Mantenha todos fora do quarto e longe do telhado."
- 190 ou 192: "Vamos repassar, mas para risco de desabamento de telhado ligue para a Defesa Civil (199) ou para os Bombeiros (193)." Não conclui a missão e **não** é erro.

## 6. Decisões inseguras (não encerram a fase, geram aviso e contam no relatório)

| `mistakeId` | Gatilho | Ativo quando | Mensagem |
|-------------|---------|--------------|----------|
| `vaga_placa` | `DecisionOption` da vaga A (sob a placa de propaganda) | missão 1 | Não estacione perto de placas de propaganda: elas podem cair com o vento e o granizo. |
| `vaga_torre` | `DecisionOption` da vaga B (junto à torre de transmissão) | missão 1 | Não estacione perto de torres de transmissão: há risco de queda e de danos à rede elétrica. |
| `abrigo_arvore` | `WrongActionZone` sob a copa da árvore grande | fase Granizo | Não fique debaixo de árvores: galhos podem quebrar com o granizo e o vento. |
| `abrigo_metalico` | `WrongActionZone` sob o ponto de ônibus metálico | fase Granizo | Evite estruturas metálicas: elas podem cair ou ser danificadas pelo granizo. |
| `abrigo_precario` | `WrongActionZone` dentro da barraca de madeira | fase Granizo | Não se abrigue em construções precárias: o telhado pode ser arrancado. |
| `sair_durante_granizo` | `WrongActionZone` cobrindo a praça aberta (fora da UBS) | depois da missão 3 e até o fim da missão 4 | Espere o granizo passar dentro do abrigo. |
| `atalho_escorregadio` | `WrongActionZone` na rampa gelada do atalho | fase Depois | Cuidado: o gelo deixa o piso escorregadio. Use o caminho com corrimão. |
| `subir_telhado` | `WrongActionInteractable` na escada encostada no telhado | fase Depois | Não suba em telhados molhados: há risco de queda. Peça a vistoria às autoridades. |
| `voltar_ao_quarto` | `WrongActionZone` no quarto | depois da missão 7 | Não volte ao cômodo com risco de desabamento até a vistoria das autoridades. |
| `repassar_boato` *(extra recomendado)* | notificação no relógio no início do Granizo: "Grupo do bairro: granizo GIGANTE vai quebrar tudo, fujam de carro!!!", com os botões **Repassar** e **Ignorar e seguir a Defesa Civil** | uma vez | Desconfie de mensagens não oficiais: elas podem ser falsas e causar pânico. Não repasse. |

## 7. Cards do relatório

Fonte de todos: `Defesa Civil / MDR — Como agir? Granizo (MDR-08/2021)`.

| Arquivo | Título | Texto |
|---------|--------|-------|
| `Card_01_Abrigo` | Onde se abrigar | Longe de casa e em local aberto, proteja-se em locais seguros e resistentes a ventos fortes, onde não há risco de destelhamento. Evite construções precárias. |
| `Card_02_ArvoresMetal` | Longe de árvores e estruturas metálicas | Não fique debaixo de árvores nem de estruturas metálicas: há risco de quedas. |
| `Card_03_Veiculos` | Onde estacionar | Não estacione veículos perto de torres de transmissão e placas de propaganda. |
| `Card_04_Cuidar` | Cuide de quem precisa | Auxilie crianças, idosos e pessoas com dificuldade de locomoção próximas a você. |
| `Card_05_Depois` | Depois do granizo | Cuidado ao se deslocar: o granizo deixa o piso escorregadio. Não suba em telhados molhados. Se notar que o telhado pode desabar, saia do local imediatamente e comunique às autoridades. |
| `Card_06_Prevencao` | Prevenção | Mantenha em boas condições a estrutura da casa, principalmente o madeiramento do telhado. Mantenha as árvores do terreno sadias, podadas e longe da rede elétrica, e informe a Prefeitura sobre árvores doentes na calçada. |
| `Card_07_OQueE` | O que é granizo | Pedras de gelo com 5 mm ou mais, formadas em nuvens do tipo cumulonimbus. Destroem telhados, derrubam árvores, danificam a rede elétrica, amassam latarias e quebram vidros de veículos. |
| `Card_08_Oficial` | Informação oficial | Mantenha a calma e siga a Defesa Civil. Receba alertas pelo SMS 40199. Desconfie de mensagens não oficiais e não as repasse. |
| `Card_09_Telefones` | Telefones úteis | Defesa Civil: 199 · Bombeiros: 193 · SAMU: 192 · Polícia Militar: 190 |

## 8. Dados (ScriptableObjects)

Pastas: `Assets/SafeZoneVR/Data/Granizo/` (passos) e `.../Granizo/Cards/`. O cenário é um asset novo:
`Assets/SafeZoneVR/Data/Scenario_Granizo.asset`.

### 8.1 Catálogo (mudança de plano)

Hoje o `SafeZoneDataBuilder.Build()` cria três cenários "Em breve": Deslizamento, Enxurrada e Vendaval.
Eles saem do plano, e o catálogo passa a ser **Alagamento, Incêndio em Casa e Granizo**.

```csharp
// SafeZoneDataBuilder.Build() — no lugar das três chamadas LockedScenario(...) e da lista antiga:
foreach (var old in new[] { "Scenario_Deslizamento", "Scenario_Enxurrada", "Scenario_Vendaval" })
{
    var p = $"{k_DataFolder}/{old}.asset";
    if (AssetDatabase.LoadAssetAtPath<ScenarioSO>(p) != null)
        AssetDatabase.DeleteAsset(p);
}

var incendio = BuildIncendio().scenario;   // enquanto não existir: LockedScenario("Scenario_Incendio", "incendio", "Incêndio em Casa", "Princípio de incêndio na cozinha e fumaça em casa. Em breve.")
var granizo = BuildGranizo().scenario;

data.catalog = CreateOrLoad<ScenarioCatalogSO>($"{k_ResourcesFolder}/ScenarioCatalog.asset");
data.catalog.EditorInitialize(k_MenuSceneName, new List<ScenarioSO> { data.scenario, incendio, granizo });
```
Os PlayerPrefs de recorde dos cenários removidos nunca foram gravados, porque eles não eram jogáveis. Não há nada a limpar.

### 8.2 Helpers e `BuildGranizo()`

Os helpers `Step`/`Card` atuais gravam na pasta do Alagamento. Crie versões que recebem a pasta e a
fonte. Se a fase Incêndio já os criou, reutilize.

```csharp
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
```

```csharp
const string k_GranizoFolder = "Assets/SafeZoneVR/Data/Granizo";
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
        "Entre com o Seu Joaquim na UBS de alvenaria. Evite árvores, o ponto de ônibus metálico e a barraca.",
        "UBS, ao norte da praça",
        "Construções resistentes, sem risco de destelhamento, protegem do granizo e do vento. Árvores, estruturas metálicas e construções precárias podem cair.");
    d.aguardar = StepAt(f, "Step_04_Aguardar", "gran_04_aguardar", 3, "Espere o granizo passar",
        "Fique dentro da UBS até o granizo parar.",
        "UBS",
        "Em local seguro, espere a chuva de granizo terminar antes de sair.");
    d.voltar = StepAt(f, "Step_05_Voltar", "gran_05_voltar", 4, "Volte para casa com cuidado",
        "O chão está coberto de gelo. Volte pela calçada com corrimão, não pela rampa do atalho.",
        "Calçada norte → portão de casa",
        "O granizo deixa o piso escorregadio. Prefira o caminho mais seguro, mesmo que seja mais longo.");
    d.forro = StepAt(f, "Step_06_Forro", "gran_06_forro", 5, "Confira a casa por dentro",
        "Entre em casa e olhe o teto do quarto. Aponte para o forro e aperte o gatilho.",
        "Quarto",
        "Telhas quebradas pelo granizo deixam a água entrar e podem enfraquecer o telhado e o forro.");
    d.sair = StepAt(f, "Step_07_Sair", "gran_07_sair", 6, "Saia do quarto imediatamente",
        "O forro está cedendo. Saia do quarto e vá para a sala.",
        "Quarto → sala",
        "Se o telhado ou o forro puder desabar, saia do local imediatamente.");
    d.comunicar = StepAt(f, "Step_08_Comunicar", "gran_08_comunicar", 7, "Comunique às autoridades",
        "Ligue para a Defesa Civil (199) ou para os Bombeiros (193) pelo relógio e conte o que viu.",
        "Relógio de pulso",
        "Avisar as autoridades permite vistoriar o telhado e evitar acidentes.");

    d.vaga.EditorSetOrderGroup(1);       // missões 1 e 2 em qualquer ordem (seção 9.2)
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
        "Tempestade com granizo chegando e você na praça do bairro. Oriente onde estacionar, ajude o Seu Joaquim, escolha um abrigo resistente e, depois, volte com cuidado e aja se o telhado ameaçar ceder.",
        "Granizo_Praca", true, 360f, 600f,
        new List<MissionStepSO> { d.vaga, d.joaquim, d.abrigo, d.aguardar, d.voltar, d.forro, d.sair, d.comunicar }, cards);
    EditorUtility.SetDirty(d.scenario);
    return d;
}
```

---

## 9. Código novo

Os itens marcados **(compartilhado)** também são usados pela fase *Incêndio em Casa*, e o documento
dela traz o mesmo código. Crie cada um **uma única vez**, em `Assets/SafeZoneVR/Runtime/...`. Todos
seguem as regras do projeto:
- nada de `Find`/`GetComponent` em `Update`;
- verificações por intervalo;
- eventos do XRI em vez de polling;
- `using` do XRI 3 (`UnityEngine.XR.Interaction.Toolkit`, `.Interactables`, `.Interactors`).

### 9.1 Pré-requisitos de cena (compartilhado)

**Prefab da casa.** A casa do Alagamento é geometria solta na cena: paredes ProBuilder `ParedeGaragem` e
móveis como raízes. Para reutilizá-la, crie o prefab `Assets/SafeZoneVR/Prefabs/Casa_SafeZone.prefab`:
1. Abra `Alagamento_EmCasa.unity`.
2. Crie um objeto vazio `Casa_SafeZone` na origem.
3. Torne filhas dele todas as raízes da casa: pisos, paredes, teto, telhados, muros, pilares, gramados, portas, móveis e a Kombi. **Não** inclua `SafeZone_Gameplay`, XR Origin, EventSystem, luz, quadro de energia nem registro.
4. Arraste o objeto para a pasta de prefabs.
5. Na cena do Alagamento, mantenha a instância do prefab; o builder dela continua funcionando, porque procura os objetos pelo nome.

Os colisores dos móveis (MeshCollider/BoxCollider) já estão nas peças e vão junto.

**`ScenarioSceneKit` (Editor).** O `AlagamentoGameplayBuilder` concentra helpers que toda fase nova precisa.
Mova-os, sem mudar o comportamento, para uma classe estática `Editor/ScenarioSceneKit.cs`:

| Helper | Ajuste ao mover |
|--------|-----------------|
| `SetupXROrigin` | recebe a posição inicial |
| `CreateMaterials` | materiais públicos no kit |
| `CreateMarkerTemplate` | nenhum |
| `CreateWristUI` | parâmetro `water` opcional (pode ser `null`); com `withCallButton = true`, divide a faixa inferior em *Opções* (0–0,5) e *Ligar* (0,5–1) e retorna o `Button` "Ligar" |
| `CreateOptionsPanel` | já é público |
| `CreateIntroPanel` | recebe título e texto do briefing |
| `CreateEndReportPanel` | nenhum |
| `CreateItem`, `CreateSocket`, `SetupSocket`, `CreateBackpackSlot` | nenhum |
| `AddTeleportArea` | nenhum |
| `AddFurnitureColliders` | nenhum |
| `CreatePlayerSpawn` | novo: cria o `PlayerSpawnPoint` |

O Alagamento passa a chamar o kit. Depois de mover, **regenere o Alagamento e repita os testes dele**
para garantir que nada mudou.

### 9.2 Núcleo: grupos de ordem e mensagens informativas (compartilhado)

**Grupos de ordem.** Passos com o mesmo grupo (≠ 0) podem ser feitos em qualquer ordem entre si sem contar "fora de ordem".

```csharp
// MissionStepSO.cs
[SerializeField]
[Tooltip("0 = ordem fixa. Passos com o mesmo número podem ser feitos em qualquer ordem entre si.")]
int m_OrderGroup = 0;
public int orderGroup => m_OrderGroup;
#if UNITY_EDITOR
public void EditorSetOrderGroup(int group) => m_OrderGroup = group;
#endif
```

```csharp
// ScenarioManager.CompleteStep — trocar "if (actualIndex != expectedIndex) outOfOrderCount++;" por:
if (!IsInOrder(steps, actualIndex))
    outOfOrderCount++;

// Em ordem = todos os passos anteriores já feitos, exceto os do mesmo grupo.
bool IsInOrder(IReadOnlyList<MissionStepSO> steps, int index)
{
    var group = steps[index].orderGroup;
    for (var i = 0; i < index; i++)
    {
        var s = steps[i];
        if (m_CompletedStepIds.Contains(s.stepId)) continue;
        if (group != 0 && s.orderGroup == group) continue;
        return false;
    }
    return true;
}
```
A variável `expectedIndex` deixa de ser usada. O Alagamento não muda de comportamento, porque todos os passos dele ficam com grupo 0.

**Mensagens informativas no relógio.** Falas oficiais e de NPCs precisam aparecer no relógio **sem** contar como erro:

```csharp
// ScenarioManager.cs
[SerializeField] StringEvent m_OnInfo = new StringEvent();
public StringEvent onInfo => m_OnInfo;

/// <summary>Mensagem informativa (alerta oficial, fala de NPC). Aparece no relógio e não conta como erro.</summary>
public void ShowInfo(string message)
{
    if (!string.IsNullOrEmpty(message))
        m_OnInfo.Invoke(message);
}
```

```csharp
// WristUIController.cs — em Start(), junto dos outros AddListener (e RemoveListener em OnDestroy):
m_ScenarioManager.onInfo.AddListener(OnInfo);

void OnInfo(string message) => ShowWarning(message, new Color(0.6f, 0.85f, 1f));   // azul-claro = informação
```

### 9.3 `StepCompleteOnInteractCount` (compartilhado) — `Runtime/Validators/`

Conclui quando N interagíveis **diferentes** da lista forem selecionados (gatilho/grip), sem contar sockets.

```csharp
public class StepCompleteOnInteractCount : StepValidatorBase
{
    [SerializeField] List<XRBaseInteractable> m_Interactables = new List<XRBaseInteractable>();
    [SerializeField, Min(1)] int m_RequiredCount = 1;
    readonly List<XRBaseInteractable> m_Done = new List<XRBaseInteractable>();

    public List<XRBaseInteractable> interactables => m_Interactables;
    public int requiredCount { get => m_RequiredCount; set => m_RequiredCount = value; }

    void OnEnable()  { foreach (var i in m_Interactables) if (i != null) i.selectEntered.AddListener(OnSelect); }
    void OnDisable() { foreach (var i in m_Interactables) if (i != null) i.selectEntered.RemoveListener(OnSelect); }

    void OnSelect(SelectEnterEventArgs args)
    {
        if (m_Completed || args.interactorObject is XRSocketInteractor) return;
        var it = args.interactableObject as XRBaseInteractable;
        if (it == null || m_Done.Contains(it)) return;
        m_Done.Add(it);
        if (it.TryGetComponent<ObjectiveTarget>(out var target)) target.SetSatisfied(true);   // some o marcador desse alvo
        ReportProgress(m_Done.Count, m_RequiredCount);
        if (m_Done.Count >= m_RequiredCount) Complete();
    }
}
```

### 9.4 `InspectableSign` (compartilhado) — `Runtime/Interactables/`

Objeto que o jogador "observa" apontando e apertando o gatilho. Aqui é o forro do quarto. Fica junto de
`XRSimpleInteractable`, `ObjectiveTarget` e um collider. Ao ser selecionado:
- mostra por 6 s uma etiqueta explicativa (`TextMeshPro` filho com `FacePlayer`);
- destaca o objeto trocando a cor por `MaterialPropertyBlock`.

```csharp
[RequireComponent(typeof(XRSimpleInteractable))]
public class InspectableSign : MonoBehaviour
{
    [SerializeField] TextMeshPro m_Label;               // começa desativado
    [SerializeField, TextArea] string m_Explanation;    // ex.: "Forro cedendo e goteira: o telhado pode estar comprometido."
    [SerializeField] Renderer m_Highlight;              // opcional
    [SerializeField] float m_LabelSeconds = 6f;
    public bool inspected { get; private set; }

    void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => Inspect());

    public void Inspect()
    {
        inspected = true;
        if (m_Label != null) { m_Label.text = m_Explanation; m_Label.gameObject.SetActive(true); CancelInvoke(); Invoke(nameof(HideLabel), m_LabelSeconds); }
        if (m_Highlight != null) { var b = new MaterialPropertyBlock(); b.SetColor("_BaseColor", new Color(1f, 0.75f, 0.3f)); m_Highlight.SetPropertyBlock(b); }
    }

    void HideLabel() { if (m_Label != null) m_Label.gameObject.SetActive(false); }
}
```

### 9.5 `StepCompleteOnStayInZone` — `Runtime/Validators/`

Conclui depois de o jogador **acumular** X segundos com a cabeça dentro da zona. Sair da zona pausa o
contador, sem zerar. A verificação é feita a cada 0,2 s, e o progresso vai para o relógio a cada 5 s.

```csharp
[RequireComponent(typeof(Collider))]
public class StepCompleteOnStayInZone : StepValidatorBase
{
    [SerializeField] float m_RequiredSeconds = 30f;
    [SerializeField] float m_CheckInterval = 0.2f;
    [Tooltip("Só conta quando este passo é o atual (evita adiantar a espera).")]
    [SerializeField] bool m_OnlyWhenCurrent = true;
    Collider m_Zone; float m_Accumulated, m_NextCheck; int m_LastReported = -1;

    public float requiredSeconds { get => m_RequiredSeconds; set => m_RequiredSeconds = value; }

    protected override void Awake() { base.Awake(); m_Zone = GetComponent<Collider>(); m_Zone.isTrigger = true; }

    void Update()
    {
        if (m_Completed || Time.time < m_NextCheck) return;
        m_NextCheck = Time.time + m_CheckInterval;
        var m = manager;
        if (m == null || !m.hasStarted || (m_OnlyWhenCurrent && !m.IsCurrentStep(m_Step))) return;
        if (!PlayerLocator.TryGetHeadPosition(out var head) || !m_Zone.bounds.Contains(head)) return;

        m_Accumulated += m_CheckInterval;
        var shown = Mathf.FloorToInt(m_Accumulated / 5f) * 5;
        if (shown != m_LastReported) { m_LastReported = shown; ReportProgress(shown, Mathf.RoundToInt(m_RequiredSeconds)); }
        if (m_Accumulated >= m_RequiredSeconds) Complete();
    }
}
```

### 9.6 `CompanionFollower` (compartilhado) — `Runtime/Core/`

NPC que acompanha o jogador por uma rota fixa de waypoints. Aqui é o Seu Joaquim, com andador,
modelado em low-poly: corpo em cápsula, cabeça, boina e andador.
- Ao ser selecionado, começa a andar devagar (0,8 m/s) e **espera** quando o jogador fica para trás.
- Não usa NavMesh: a rota é fixa e sem obstáculos, então o custo é mínimo.
- O collider dele fica na layer `Ignore Raycast`, para o raio de chão não acertar o próprio corpo.

```csharp
public class CompanionFollower : MonoBehaviour
{
    [SerializeField] XRBaseInteractable m_TalkInteractable;          // o próprio corpo do NPC
    [SerializeField] List<Transform> m_Waypoints = new List<Transform>();
    [SerializeField] float m_Speed = 0.8f;
    [SerializeField] float m_MaxLeadDistance = 3f;                   // distância máxima à frente do jogador
    [SerializeField] TextMeshPro m_SpeechBubble;                     // "Obrigado, meu filho! Vamos."
    [SerializeField] UnityEvent m_OnStartedFollowing = new UnityEvent();
    int m_Index; bool m_Following;

    public bool isFollowing => m_Following;
    public bool arrived => m_Following && m_Index >= m_Waypoints.Count;
    public List<Transform> waypoints => m_Waypoints;
    public UnityEvent onStartedFollowing => m_OnStartedFollowing;

    void OnEnable()  { if (m_TalkInteractable != null) m_TalkInteractable.selectEntered.AddListener(OnTalk); }
    void OnDisable() { if (m_TalkInteractable != null) m_TalkInteractable.selectEntered.RemoveListener(OnTalk); }
    void OnTalk(SelectEnterEventArgs _) => StartFollowing();

    public void StartFollowing()
    {
        if (m_Following) return;
        m_Following = true;
        if (m_SpeechBubble != null) m_SpeechBubble.gameObject.SetActive(true);
        m_OnStartedFollowing.Invoke();
    }

    void Update()
    {
        if (!m_Following || m_Index >= m_Waypoints.Count) return;
        var pos = transform.position;
        var target = m_Waypoints[m_Index].position;

        // Jogador ficou para trás (longe e do lado oposto ao destino): espera por ele.
        if (PlayerLocator.TryGetFeetPosition(out var player))
        {
            var toPlayer = player - pos;
            if (toPlayer.sqrMagnitude > m_MaxLeadDistance * m_MaxLeadDistance && Vector3.Dot(toPlayer, target - pos) < 0f) return;
        }

        var next = Vector3.MoveTowards(pos, new Vector3(target.x, pos.y, target.z), m_Speed * Time.deltaTime);
        if (Physics.Raycast(next + Vector3.up, Vector3.down, out var hit, 2f, ~0, QueryTriggerInteraction.Ignore))
            next.y = hit.point.y;                                        // acompanha degraus e desníveis
        var dir = target - pos; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        transform.position = next;

        if (new Vector2(next.x - target.x, next.z - target.z).sqrMagnitude < 0.04f) m_Index++;
    }
}
```
Rota do Seu Joaquim: banco `(-49, 0, 33)` → `(-52, 0, 37)` → porta da UBS `(-55, 0, 39.5)` → interior `(-55, 0, 42)`.

### 9.7 `StepCompleteOnCompanionInZone` — `Runtime/Validators/`

```csharp
[RequireComponent(typeof(Collider))]
public class StepCompleteOnCompanionInZone : StepValidatorBase
{
    [SerializeField] CompanionFollower m_Companion;
    [SerializeField] float m_CheckInterval = 0.25f;
    Collider m_Zone; float m_NextCheck;

    public CompanionFollower companion { get => m_Companion; set => m_Companion = value; }

    protected override void Awake() { base.Awake(); m_Zone = GetComponent<Collider>(); m_Zone.isTrigger = true; }

    void Update()
    {
        if (m_Completed || Time.time < m_NextCheck || m_Companion == null) return;
        m_NextCheck = Time.time + m_CheckInterval;
        var m = manager;
        if (m == null || !m.hasStarted) return;
        var playerIn = PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head);
        var companionIn = m_Zone.bounds.Contains(m_Companion.transform.position + Vector3.up * 0.5f);
        if (playerIn && companionIn) Complete();
    }
}
```

### 9.8 `EmergencyCallPanel` (compartilhado) — `Runtime/UI/`

Painel world-space no mesmo estilo do `OptionsPanelController`: `LazyFollow` a 1,1 m e botões grandes.
Ele abre pelo botão **"Ligar"** do relógio (seção 9.1).
- **Números:** quatro botões — 199 Defesa Civil, 193 Bombeiros, 190 Polícia Militar e 192 SAMU.
- **Ao tocar um número:** mostra "Chamando…" por 1,2 s e depois a resposta da central (seção 5).
- **Números aceitos:** disparam `onAcceptedCall`. No builder, esse evento é ligado a
  `StepCompleteOnButtonPress.OnButtonPressed` com `UnityEventTools.AddPersistentListener`.

```csharp
public class EmergencyCallPanel : MonoBehaviour
{
    [Serializable] public class Line { public string number; public string label; [TextArea] public string reply; public Button button; }

    [SerializeField] GameObject m_Root;
    [SerializeField] Button m_OpenButton;      // botão "Ligar" do relógio
    [SerializeField] Button m_CloseButton;
    [SerializeField] TextMeshProUGUI m_ReplyText;
    [SerializeField] List<Line> m_Lines = new List<Line>();
    [SerializeField] List<string> m_AcceptedNumbers = new List<string> { "199", "193" };
    [SerializeField] UnityEvent m_OnAcceptedCall = new UnityEvent();

    public List<Line> lines => m_Lines;
    public List<string> acceptedNumbers => m_AcceptedNumbers;
    public UnityEvent onAcceptedCall => m_OnAcceptedCall;
    public GameObject root { get => m_Root; set => m_Root = value; }
    public Button openButton { get => m_OpenButton; set => m_OpenButton = value; }
    public Button closeButton { get => m_CloseButton; set => m_CloseButton = value; }
    public TextMeshProUGUI replyText { get => m_ReplyText; set => m_ReplyText = value; }

    void Awake()
    {
        if (m_OpenButton != null) m_OpenButton.onClick.AddListener(Toggle);
        if (m_CloseButton != null) m_CloseButton.onClick.AddListener(() => m_Root.SetActive(false));
        foreach (var line in m_Lines)
        {
            var captured = line;
            if (line.button != null) line.button.onClick.AddListener(() => StartCoroutine(Call(captured)));
        }
    }

    void Start() { if (m_Root != null) m_Root.SetActive(false); }

    public void Toggle() { if (m_Root != null) m_Root.SetActive(!m_Root.activeSelf); }

    IEnumerator Call(Line line)
    {
        if (m_ReplyText != null) m_ReplyText.text = $"Chamando {line.label} ({line.number})…";
        yield return new WaitForSeconds(1.2f);
        if (m_ReplyText != null) m_ReplyText.text = line.reply;
        if (m_AcceptedNumbers.Contains(line.number)) m_OnAcceptedCall.Invoke();
    }
}
```
A corrotina roda no painel, que precisa estar **ativo** durante a ligação. Por isso o botão "Fechar"
só desativa o `m_Root`, que é filho do objeto do painel, e nunca o próprio objeto com o componente.

### 9.9 `ActivateOnStepCompleted` (compartilhado) — `Runtime/Core/`

Liga ou desliga objetos quando um passo é concluído. Exemplos aqui: a zona da sala (missão 7), que só vale
depois da missão 6, e a zona `voltar_ao_quarto`.

```csharp
public class ActivateOnStepCompleted : MonoBehaviour
{
    [SerializeField] MissionStepSO m_Step;
    [SerializeField] List<GameObject> m_Activate = new List<GameObject>();
    [SerializeField] List<GameObject> m_Deactivate = new List<GameObject>();
    [SerializeField] bool m_ApplyInitialState = true;   // começa com "Activate" desligado e "Deactivate" ligado

    public MissionStepSO step { get => m_Step; set => m_Step = value; }
    public List<GameObject> activate => m_Activate;
    public List<GameObject> deactivate => m_Deactivate;

    void Start()
    {
        if (m_ApplyInitialState) Apply(false);
        var m = ScenarioManager.instance;
        if (m != null) m.onStepCompleted.AddListener(OnStepCompleted);
    }

    void OnDestroy() { var m = ScenarioManager.instance; if (m != null) m.onStepCompleted.RemoveListener(OnStepCompleted); }

    void OnStepCompleted(MissionStepSO s) { if (s == m_Step) Apply(true); }

    void Apply(bool completed)
    {
        foreach (var go in m_Activate) if (go != null) go.SetActive(completed);
        foreach (var go in m_Deactivate) if (go != null) go.SetActive(!completed);
    }
}
```
Um `ActivateOnStepCompleted` **não pode** estar dentro de um objeto que ele mesmo desliga. Coloque todos
sob `SafeZone_Gameplay/Phases`.

### 9.10 `ScenarioPhaseSwitcher` (compartilhado) — `Runtime/Core/`

Troca o estado do ambiente quando **todos** os passos de gatilho de uma fase estiverem concluídos.
- Com `fadeSeconds > 0`, a troca usa o `ScreenFader`: escurece, mostra uma legenda, troca e clareia.
- **Nunca move o jogador** (conforto em VR).
- O `info` aparece no relógio via `ScenarioManager.ShowInfo`.
- O `onEnter` liga controladores específicos, como o `HailController`.

```csharp
public class ScenarioPhaseSwitcher : MonoBehaviour
{
    [Serializable]
    public class Phase
    {
        public string name;
        public List<MissionStepSO> triggerSteps = new List<MissionStepSO>();
        public float fadeSeconds;
        [TextArea] public string caption;    // texto no escuro do fade
        [TextArea] public string info;       // mensagem no relógio ao entrar
        public List<GameObject> activate = new List<GameObject>();
        public List<GameObject> deactivate = new List<GameObject>();
        public UnityEvent onEnter = new UnityEvent();
        [NonSerialized] public bool entered;
    }

    [SerializeField] List<Phase> m_Phases = new List<Phase>();
    [SerializeField] TextMeshProUGUI m_Caption;            // canvas preso à câmera, desligado por padrão
    ScenarioManager m_Manager;

    public List<Phase> phases => m_Phases;

    void Start()
    {
        m_Manager = ScenarioManager.instance;
        if (m_Manager != null) m_Manager.onStepCompleted.AddListener(OnStepCompleted);
        if (m_Caption != null) m_Caption.gameObject.SetActive(false);
    }

    void OnDestroy() { if (m_Manager != null) m_Manager.onStepCompleted.RemoveListener(OnStepCompleted); }

    void OnStepCompleted(MissionStepSO _)
    {
        foreach (var p in m_Phases)
        {
            if (p.entered || p.triggerSteps.Count == 0) continue;
            var all = true;
            foreach (var s in p.triggerSteps) all &= m_Manager.IsStepCompleted(s);
            if (all) StartCoroutine(Enter(p));
        }
    }

    IEnumerator Enter(Phase p)
    {
        p.entered = true;
        var fader = ScreenFader.instance;
        var useFade = p.fadeSeconds > 0f && fader != null;
        if (useFade)
        {
            fader.FadeTo(1f);
            yield return new WaitForSeconds(0.55f);                      // duração padrão do ScreenFader
            if (m_Caption != null && !string.IsNullOrEmpty(p.caption)) { m_Caption.text = p.caption; m_Caption.gameObject.SetActive(true); }
        }
        foreach (var go in p.deactivate) if (go != null) go.SetActive(false);
        foreach (var go in p.activate) if (go != null) go.SetActive(true);
        p.onEnter.Invoke();
        if (!string.IsNullOrEmpty(p.info) && m_Manager != null) m_Manager.ShowInfo(p.info);
        if (useFade)
        {
            yield return new WaitForSeconds(p.fadeSeconds);
            if (m_Caption != null) m_Caption.gameObject.SetActive(false);
            fader.FadeTo(0f);
        }
    }
}
```
A legenda fica num canvas world-space filho da câmera, a 0,6 m. Ela renderiza **na frente** do
`FadeQuad` do `ScreenFader`, com renderQueue 4001 no material do texto.

### 9.11 `DecisionOption`, `StepCompleteOnChoice` e `SimpleMover` — escolha da vaga (missão 1)

Mecânica de decisão diegética: cada vaga tem uma placa selecionável ("Aqui?"). Escolher uma opção
errada registra o erro com a explicação e deixa o jogador tentar de novo. A opção certa conclui o passo
e dispara `onCorrect`, que leva o carro até a vaga.

```csharp
[RequireComponent(typeof(XRSimpleInteractable))]
public class DecisionOption : MonoBehaviour
{
    [SerializeField] bool m_IsCorrect;
    [SerializeField] string m_MistakeId;                 // só para opções erradas (ex.: "vaga_placa")
    [SerializeField, TextArea] string m_Feedback;        // erro: mensagem do erro · certa: fala no relógio
    public bool isCorrect { get => m_IsCorrect; set => m_IsCorrect = value; }
    public string mistakeId { get => m_MistakeId; set => m_MistakeId = value; }
    public string feedback { get => m_Feedback; set => m_Feedback = value; }
    public event Action<DecisionOption> chosen;

    void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => chosen?.Invoke(this));
}

public class StepCompleteOnChoice : StepValidatorBase
{
    [SerializeField] List<DecisionOption> m_Options = new List<DecisionOption>();
    [SerializeField] UnityEvent m_OnCorrect = new UnityEvent();

    public List<DecisionOption> options => m_Options;
    public UnityEvent onCorrect => m_OnCorrect;

    void OnEnable()  { foreach (var o in m_Options) if (o != null) o.chosen += OnChosen; }
    void OnDisable() { foreach (var o in m_Options) if (o != null) o.chosen -= OnChosen; }

    void OnChosen(DecisionOption option)
    {
        if (m_Completed) return;
        var m = manager;
        if (m == null) return;
        if (!option.isCorrect)
        {
            m.RegisterMistake(option.mistakeId, option.feedback, 10f);
            return;
        }
        if (!string.IsNullOrEmpty(option.feedback)) m.ShowInfo(option.feedback);
        m_OnCorrect.Invoke();
        Complete();
    }
}

/// <summary>Move um objeto em linha reta até um alvo, sem física (o carro indo para a vaga C).</summary>
public class SimpleMover : MonoBehaviour
{
    [SerializeField] Transform m_Target;
    [SerializeField] float m_Seconds = 6f;
    public Transform target { get => m_Target; set => m_Target = value; }

    public void MoveToTarget() => StartCoroutine(Move());

    IEnumerator Move()
    {
        Vector3 fromPos = transform.position; Quaternion fromRot = transform.rotation;
        for (var t = 0f; t < 1f; t += Time.deltaTime / m_Seconds)
        {
            var k = Mathf.SmoothStep(0f, 1f, t);
            transform.SetPositionAndRotation(Vector3.Lerp(fromPos, m_Target.position, k), Quaternion.Slerp(fromRot, m_Target.rotation, k));
            yield return null;
        }
        transform.SetPositionAndRotation(m_Target.position, m_Target.rotation);
    }
}
```
Configuração das opções da missão 1:

| Vaga | `isCorrect` | `mistakeId` | `feedback` |
|------|-------------|-------------|------------|
| A (placa) | falso | `vaga_placa` | texto da seção 6 |
| B (torre) | falso | `vaga_torre` | texto da seção 6 |
| C (mercado) | verdadeiro | — | "Carlos: boa! Longe da placa e da torre." |

### 9.12 `HailController` e som do granizo — `Runtime/Core/`

Uma única fonte de verdade para a intensidade do granizo (0..1). O controlador cuida de:
- **Emissão:** controla a emissão das partículas.
- **Emissor:** mantém o emissor sobre a cabeça do jogador. A colisão com a laje (layer `HailBlockers`) impede que caia granizo dentro da UBS.
- **Gelo no chão:** acumula a camada de gelo nos planos externos.
- **Som:** ajusta o volume do som nos telhados.
- **Fim:** para sozinho quando faltam 8 s para concluir a missão 4, lendo o `onStepProgress` do `StepCompleteOnStayInZone`.

```csharp
public class HailController : MonoBehaviour
{
    static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

    [SerializeField] ParticleSystem m_Hail;                      // Simulation Space = World; Collision World/Low/HailBlockers
    [SerializeField] float m_MaxEmission = 260f;
    [SerializeField] float m_EmitterHeight = 6f;
    [SerializeField] List<Renderer> m_IceLayers = new List<Renderer>();   // planos brancos transparentes
    [SerializeField] float m_AccumulateSeconds = 40f;
    [SerializeField] float m_MaxIceAlpha = 0.6f;
    [SerializeField] List<AudioSource> m_RoofSounds = new List<AudioSource>();   // loop CreateHailLoop, 3D
    [SerializeField] MissionStepSO m_StopStep;                   // gran_04_aguardar
    [SerializeField] float m_StopWhenSecondsLeft = 8f;
    [SerializeField] float m_UpdateInterval = 0.1f;
    float m_Intensity, m_Target, m_Rate, m_Ice, m_NextUpdate;
    MaterialPropertyBlock m_Block; Transform m_Head;

    public float intensity => m_Intensity;

    void Awake() { m_Block = new MaterialPropertyBlock(); Apply(); }

    void Start()
    {
        var m = ScenarioManager.instance;
        if (m != null) m.onStepProgress.AddListener(OnProgress);
        var cam = PlayerLocator.playerCamera;
        if (cam != null) m_Head = cam.transform;
    }

    /// <summary>Leva a intensidade até <paramref name="target"/> em <paramref name="seconds"/> segundos.</summary>
    public void RampTo(float target, float seconds)
    {
        m_Target = Mathf.Clamp01(target);
        m_Rate = Mathf.Abs(m_Target - m_Intensity) / Mathf.Max(0.1f, seconds);
        if (m_Target > 0f && m_Hail != null && !m_Hail.isPlaying) m_Hail.Play();
        foreach (var s in m_RoofSounds) if (s != null && m_Target > 0f && !s.isPlaying) s.Play();
    }

    void OnProgress(MissionStepSO step, int done, int total)
    {
        if (step == m_StopStep && total - done <= m_StopWhenSecondsLeft) RampTo(0f, m_StopWhenSecondsLeft);
    }

    void Update()
    {
        if (m_Head != null && m_Hail != null)                    // segue a cabeça sem girar junto
        {
            var p = m_Head.position;
            m_Hail.transform.position = new Vector3(p.x, p.y + m_EmitterHeight, p.z);
        }
        if (Time.time < m_NextUpdate) return;
        m_NextUpdate = Time.time + m_UpdateInterval;
        m_Intensity = Mathf.MoveTowards(m_Intensity, m_Target, m_Rate * m_UpdateInterval);
        if (m_Intensity > 0.2f) m_Ice = Mathf.Min(1f, m_Ice + m_UpdateInterval / m_AccumulateSeconds * m_Intensity);
        Apply();
    }

    void Apply()
    {
        if (m_Hail != null)
        {
            var e = m_Hail.emission;
            e.rateOverTime = m_MaxEmission * m_Intensity;
            if (m_Intensity <= 0.01f && m_Target <= 0f && m_Hail.isPlaying) m_Hail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        foreach (var r in m_IceLayers)
        {
            if (r == null) continue;
            m_Block.SetColor(k_BaseColor, new Color(0.92f, 0.95f, 1f, m_Ice * m_MaxIceAlpha));
            r.SetPropertyBlock(m_Block);
        }
        foreach (var s in m_RoofSounds)
        {
            if (s == null) continue;
            s.volume = 0.5f * m_Intensity;
            if (m_Intensity <= 0.01f && m_Target <= 0f && s.isPlaying) s.Stop();
        }
    }
}
```
O gelo **permanece** na fase Depois, porque `m_Ice` não diminui. É ele que torna visível o "piso
escorregadio" da missão 5.

**Som das pedras.** Acrescente em `ProceduralAudio`, no mesmo padrão de `CreateRainLoop`. É um loop de
"tics" curtos e agudos, com volume moderado e sem estalos altos:

```csharp
public static AudioClip CreateHailLoop(string name, float seconds = 3f, float hitsPerSecond = 90f, float volume = 0.35f)
{
    const int rate = 22050;
    var n = Mathf.CeilToInt(seconds * rate);
    var data = new float[n];
    var rng = new System.Random(5);
    var hits = Mathf.RoundToInt(seconds * hitsPerSecond);
    for (var h = 0; h < hits; h++)
    {
        var start = rng.Next(n);
        var len = 220 + rng.Next(440);                                   // 10–30 ms
        var amp = (float)(0.3 + rng.NextDouble() * 0.7) * volume;
        var freq = 1800f + (float)rng.NextDouble() * 2500f;              // "tic" de gelo
        for (var i = 0; i < len; i++)
        {
            var idx = (start + i) % n;                                   // dá a volta: loop sem emenda
            data[idx] += Mathf.Sin(2f * Mathf.PI * freq * i / rate) * Mathf.Exp(-i / (len * 0.25f)) * amp;
        }
    }
    for (var i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
    var clip = AudioClip.Create(name, n, 1, rate, false);
    clip.SetData(data, 0);
    return clip;
}
```

---

## 10. Builder da cena — `Editor/GranizoGameplayBuilder.cs`

### 10.1 Roteiro do `Build()`

Siga o padrão idempotente do Alagamento: tudo o que é gerado fica sob `SafeZone_Gameplay` e é recriado
a cada execução.

```text
1. Abrir/criar Assets/Scenes/Fases/Granizo_Praca.unity
   (primeira vez: cena vazia + luz direcional nublada + Casa_SafeZone.prefab na origem).
2. SafeZoneDataBuilder.BuildGranizo() → passos e cenário.
3. ScenarioSceneKit.CreateMaterials(); CleanupLegacy (remove SafeZone_Gameplay antigo).
4. XR Origin em (-52, 0, 30), olhando para +Z; PlayerSpawnPoint no mesmo ponto.
5. Núcleo: ScenarioManager (autoStart = false), FeedbackPlayer (chuva fraca), ComfortSettingsApplier,
   ObjectiveBeaconSystem.
6. Ambiente (sob SafeZone_Gameplay/Environment):
   a. Rua estendida até x -80 + calçada norte com corrimão (cilindros finos) + TeleportationArea.
   b. Praça: piso (TeleportationArea), bancos, canteiros, árvore grande (shader de balanço),
      ponto de ônibus metálico, barraca de madeira com telhado de zinco, UBS de alvenaria
      (paredes em caixa + laje + vão da porta + piso interno com TeleportationArea + placa "UBS — ABRIGO").
   c. Placa de propaganda (x -70, z 22) e torre de transmissão (x -76, z 30, só visual).
   d. Rampa do atalho (x -44, z 28..24), com TeleportationArea (o erro vem da zona, não do bloqueio).
   e. Layer HailBlockers: colliders simples do chão, da laje, da cobertura do ponto, da barraca e dos telhados.
   f. Planos de gelo (praça, rua, calçada, rampa) com material branco transparente, alpha 0.
   g. HailController + ParticleSystem de granizo (seção 4.3) + 2 AudioSources com CreateHailLoop
      (telhado da UBS e capô do carro).
7. NPCs:
   - Seu Joaquim: corpo + andador, XRSimpleInteractable, ObjectiveTarget (passo 2), CompanionFollower
     (waypoints da seção 9.6), balão de fala.
   - Carlos: parado ao lado do carro (modelo da Kombi), balão "Onde eu deixo o carro?".
8. Vagas: 3 placas "Aqui?" com XRSimpleInteractable + DecisionOption (tabela da seção 9.11) + ObjectiveTarget (passo 1).
   Carro com SimpleMover, cujo target é a vaga C.
9. Estado "Depois" da casa (sob Phases/Depois, desligado): decals de telhas quebradas no telhado,
   goteira (partículas) + poça no quarto, forro cedendo (quad com "barriga" + trinca) com
   XRSimpleInteractable + InspectableSign + ObjectiveTarget (passo 6), escada encostada no telhado
   com WrongActionInteractable.
10. Validadores (seção 5), cada um com scenarioManager e step:
   - V1 StepCompleteOnChoice (3 opções; onCorrect → SimpleMover.MoveToTarget)
   - V2 StepCompleteOnInteractCount (Seu Joaquim)
   - V3 StepCompleteOnCompanionInZone (interior da UBS; companion = Seu Joaquim)
   - V4 StepCompleteOnStayInZone (interior da UBS, 30 s)
   - V5 StepCompleteOnTriggerZone (portão de pedestres da casa)
   - V6 StepCompleteOnInteractCount (forro)
   - V7 StepCompleteOnTriggerZone (sala), com o objeto DESLIGADO + ActivateOnStepCompleted(step = passo 6)
   - V8 StepCompleteOnButtonPress ← EmergencyCallPanel.onAcceptedCall (UnityEventTools.AddPersistentListener)
11. Decisões inseguras (seção 6):
   - zonas árvore / ponto metálico / barraca → objeto "ZonasAbrigoErrado", ligado na fase Granizo
     e desligado na fase Depois;
   - zona praça aberta → ActivateOnStepCompleted(step = passo 3) liga; a fase Depois desliga;
   - zona da rampa gelada e escada → dentro de Phases/Depois;
   - zona voltar_ao_quarto → ActivateOnStepCompleted(step = passo 7).
12. Rota segura: setas verdes da porta da UBS pela calçada com corrimão até o portão de casa,
    dentro de ObjectiveTarget(passo 5).showWhileActive.
13. ScenarioPhaseSwitcher (sob SafeZone_Gameplay/Phases):
    - "Granizo": triggerSteps = [passo 1, passo 2], sem fade; activate = [ZonasAbrigoErrado];
      onEnter → HailController.RampTo(1, 15); info = "Defesa Civil: granizo caindo no bairro. Procure um local seguro e resistente."
    - "Depois": triggerSteps = [passo 4], fadeSeconds = 1, caption = "Alguns minutos depois…";
      activate = [Depois]; deactivate = [ZonasAbrigoErrado, ZonaPracaAberta];
      info = "Seu Joaquim: obrigado! Minha filha já está vindo me buscar."
14. UI: OptionsPanel, WristUI (com botão Ligar), EmergencyCallPanel (linhas e falas da seção 5; aceitos 199 e 193),
    IntroBriefing (título + texto da seção 3), EndReport, legenda de fase presa à câmera.
15. MarkSceneDirty + SaveScene + QuestProjectConfigurator.RegisterScenes().
```

### 10.2 Trechos que exigem atenção

**Criar a layer `HailBlockers` pelo builder**, se ainda não existir:

```csharp
static int EnsureLayer(string name)
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
```
Os colliders da `HailBlockers` são **extras**, só para as partículas. Eles não entram na layer do
teleporte nem na do jogador. Os pisos continuam tendo os próprios colliders normais.

**Ligar o painel de ligação ao validador** (listener persistente, salvo na cena):

```csharp
var v8 = UIBuilderUtil.Child(validators, "Validator_08_Comunicar").AddComponent<StepCompleteOnButtonPress>();
v8.scenarioManager = manager; v8.step = data.comunicar;
UnityEditor.Events.UnityEventTools.AddPersistentListener(callPanel.onAcceptedCall, v8.OnButtonPressed);
```

**Carro que vai para a vaga C:** `UnityEventTools.AddPersistentListener(v1.onCorrect, mover.MoveToTarget)`.

**FeedbackPlayer:** o volume da chuva é privado (`m_RainVolume`). Exponha `public void SetRainVolume(float v)`
(ajusta `m_Ambience.volume`) para o `onEnter` do Granizo aumentar a chuva e o do Depois reduzi-la. É uma
mudança pequena e não afeta o Alagamento.

## 11. Integração no projeto

| Arquivo | Mudança |
|---------|---------|
| `Editor/SafeZoneDataBuilder.cs` | `StepAt`/`CardAt`, `BuildGranizo()` e o novo catálogo (seção 8.1) |
| `Data/MissionStepSO.cs`, `Runtime/Core/ScenarioManager.cs`, `Runtime/UI/WristUIController.cs` | grupos de ordem + `ShowInfo`/`onInfo` (9.2) |
| `Editor/ScenarioSceneKit.cs` e `Prefabs/Casa_SafeZone.prefab` | novos (9.1); `AlagamentoGameplayBuilder` passa a usar o kit |
| `Runtime/...` | componentes da seção 9 (os compartilhados só uma vez) |
| `Runtime/Core/ProceduralAudio.cs` | `CreateHailLoop` (9.12) |
| `Runtime/Core/FeedbackPlayer.cs` | `SetRainVolume` (10.2) |
| `Editor/GranizoGameplayBuilder.cs` | novo; `[MenuItem("SafeZone VR/Build/2c. Fase Granizo")]` chamando `Build()` + diálogo |
| `Editor/QuestProjectConfigurator.cs` | `k_GranizoScenePath = "Assets/Scenes/Fases/Granizo_Praca.unity"` em `RegisterScenes()` |
| `Editor/SafeZoneBuildAll.cs` | chamar `GranizoGameplayBuilder.Build()` antes de `MainMenuBuilder.Build()` |
| `ProjectSettings/TagManager.asset` | layer `HailBlockers` (criada pelo builder) |
| `CONFIGURATION_GUIDE.md` e `README.md` | tabela de missões da nova fase; o README deixa de citar deslizamento, enxurrada e vendaval como próximos cenários |

O menu principal não precisa mudar. O botão "Granizo" aparece jogável sozinho, porque o `ScenarioSO` tem
`isAvailable = true` e `sceneName` preenchido.

## 12. Checklist de implementação e aceite

**Ordem sugerida**
1. [ ] Pré-requisitos comuns (9.1 e 9.2), se a fase Incêndio ainda não os criou. Regenere o Alagamento e repita os testes dele.
2. [ ] Componentes da seção 9 + `CreateHailLoop` + `SetRainVolume`.
3. [ ] `BuildGranizo()` e novo catálogo (seção 8).
4. [ ] `GranizoGameplayBuilder` (seção 10) e integração (seção 11).
5. [ ] Modelos definitivos: Seu Joaquim, Carlos, UBS, barraca, ponto de ônibus, placa, torre e escada. Os primitivos servem até lá.
6. [ ] Teste no Editor (roteiro abaixo) e no Quest 3 (72 FPS, Build and Run).

**Critérios de aceite**
- [ ] O menu mostra Alagamento, Incêndio em Casa e Granizo; Granizo carrega com fade.
- [ ] As 8 missões concluem pelos gatilhos da seção 5; as missões 1 e 2 em qualquer ordem **sem** penalidade.
- [ ] Vagas A e B registram o erro e deixam tentar de novo; a vaga C conclui e o carro vai até ela.
- [ ] O Seu Joaquim espera o jogador quando ele fica para trás e entra na UBS junto.
- [ ] Não cai granizo dentro da UBS; o granizo para sozinho no fim da missão 4 e o gelo permanece no chão.
- [ ] Passar pela sala antes de ver o forro **não** conclui a missão 7.
- [ ] Ligar 190 ou 192 orienta sem concluir nem gerar erro; 199 e 193 concluem.
- [ ] Cada decisão insegura da seção 6 aparece no relógio e no relatório, sem encerrar a fase.
- [ ] O relatório mostra estrelas, os 9 cards e o recorde (`safezone.best.*.granizo`).
- [ ] Console sem erros ao entrar e sair do Play Mode e ao voltar ao menu.
- [ ] Quest 3: ≥ 72 FPS estáveis no pico do granizo, ≤ 150 draw calls.
- [ ] Revisão de tom (RNF03): nenhuma pedra atinge pessoas ou a câmera; nada desaba sobre o jogador.

**Roteiro de teste no Editor (via MCP, como no Alagamento)**
1. **Salvar o recorde:** ler e guardar `safezone.best.time.granizo` e `safezone.best.stars.granizo`, e restaurar no fim. Não edite scripts com o Play Mode ativo.
2. **Na praça (missões 1–3):**
   - Entrar no Play Mode e chamar `IntroBriefingPanel.Begin()`.
   - Missão 1: escolher a vaga A (confere o erro) e depois a C. Use `XRInteractionManager.SelectEnter` com o Near-Far Interactor direito e a `DecisionOption`.
   - Missão 2: selecionar o Seu Joaquim.
   - Missão 3: levar o XR Origin até a UBS, usando o `PlayerSpawnPoint` como referência de altura, e esperar ele entrar.
3. **Durante o granizo (missão 4):**
   - Ficar 30 s na UBS.
   - Conferir que o `HailController.intensity` sobe, depois cai para 0.
   - Conferir que nenhuma partícula fica dentro da UBS (contar partículas com `ParticleSystem.GetParticles` dentro dos bounds).
4. **Depois (missões 5–8):**
   - Missão 5: ir até o portão.
   - Missão 6: selecionar o forro.
   - Missão 7: ir para a sala.
   - Missão 8: invocar o botão 199 do `EmergencyCallPanel`.
5. **Resultado:**
   - `ScenarioManager.isComplete` e `outOfOrderCount == 0`;
   - relatório visível e captura de câmera do relatório;
   - console sem erros.
6. **Erros:** repetir abrigando-se sob a árvore, saindo da UBS durante o granizo, usando a rampa, selecionando a escada e voltando ao quarto. Cada erro deve aparecer uma vez no relatório.
