# Referência técnica

Os scripts não têm comentários. O que não dá para deduzir lendo o código está aqui.

## Coordenadas

Mundo em metros. A casa (`DC_Casa.fbx`) e a praça (`DC_Praca.fbx`) ficam as duas na origem e
já se encaixam: casa ao sul da rua, praça ao norte. Frente da casa para **+Z**, cozinha a
**−X** da sala, corredor ao fundo. Rua principal em z 7,2..12,1; praça em x −16,5..5,5,
z 12,1..27,8.

### Alagamento (`AlagamentoGameplayBuilder`)

| Constante | Valor | Lugar |
|---|---|---|
| `k_PlayerStart` | (−2,6; 0; −2,4) | sala |
| `k_PlayerRot` | Y 90° | de frente para o sofá |
| `k_SofaDocs` | (−0,80; −1,60) | assento do sofá (x, z) |
| `k_KitchenWater` | (−10,40; −3,05) | bancada da cozinha |
| `k_BathroomMeds` | (−10,55; −6,95) | bancada do banheiro, ao lado da cuba |
| `k_BedFlashlight` | (−1,50; −8,40) | cama de casal |
| `k_CarDoor` | (0,42; 1,0; −2,3) | porta do motorista, carro da garagem |

Registro de água: parede externa, lado da garagem, entre a parede e o carro. O eixo Y local
do knob é o eixo X do mundo, porque o volante fica encostado numa parede perpendicular a X.
Mochila sobre um banco na sala, perto da porta do corredor: na altura da mão e acima da água.
Ponto de encontro elevado na calçada, à esquerda do portão de pedestres; o jogador chega do
portão (z menor) olhando para +Z, então o texto com rotação zero fica legível.
Plano de água cobre 40 × 30 m (casa, lote, calçada e rua).

### Incêndio (`IncendioGameplayBuilder`)

| Constante | Valor | Lugar |
|---|---|---|
| `k_PlayerStart` | (−2,8; 0; −2,6) | sala |
| `k_PlayerRot` | Y −90° | olhando para a cozinha (−X) |
| `k_PanXZ` | (−9,36; −0,62) | boca da frente do fogão |
| `k_GasCylinder` | (−8,45; 0,28; −0,45) | no chão, ao lado do fogão |
| `k_LidXZ` | (−10,36; −3,05) | bancada |
| `k_JugXZ` | (−8,35; −2,75) | mesa da cozinha |
| `k_PowerStrip` | (−5,05; 0,03; −1,05) | chão, ponta do rack da TV |
| `k_NotebookXZ` | (−0,85; −1,70) | sofá |
| `k_FrontYardCenter` | (−4,5; 1,2; 1,3) | varanda e caminho de pedras |
| `k_MeetingSign` | (−9,0; 0; 6,3) | calçada |
| `k_Truck` | (−13,0; −0,06; 9,7) | rua |

A janela da cozinha abre para fora (−X), 80°, senão bate na bancada. A chave fica na
fechadura pela face interna (−Z), do lado da maçaneta — é a decisão insegura "trancar a porta";
o collider da folha entra explicitamente na lista do interagível da porta porque a chave é
filha e tem interagível próprio. A etiqueta da chave não herda a escala dela (ajuste 50/20/25).
`DC_CASA_PanelaChaleira` é escondida: a panela da gameplay ocupa o fogão.

### Granizo (`GranizoGameplayBuilder`)

| Constante | Valor | Lugar |
|---|---|---|
| `k_PlayerStart` | (−4,9; 0; 13,4) | praça, junto à faixa de pedestres |
| `k_JoaquimBench` | (−9,2; 0; 16,7) | banco perto do canteiro da esquerda |
| `k_BusStop` | (−12; 0; 6,3) | calçada da casa, aberto para a rua |
| `k_Shack` | (−22,4; 0,12; 20) | calçada do outro lado da rua lateral |
| `k_StreetLaneZ` | 10,9 | faixa da rua do lado da praça |
| `k_SpotA` | (−30,5; −0,06; 10,9) | vaga embaixo da placa de propaganda |
| `k_SpotB` | (24,5; −0,06; 10,9) | vaga em frente à torre de transmissão |
| `k_SpotC` | (−12,5; −0,06; 10,9) | vaga longe das duas (a correta) |
| `k_Ladder` | (−10; 0; 1,25) | encostada na fachada |
| `k_Ceiling` | (−2,9; 2,68; −8,9) | forro do quarto de casal |
| `k_GateZone` | (−4,9; 1,2; 4,4) | logo depois do portão de pedestres |

UBS modelada (`UBS.fbx`, 23 mil triângulos) no lote à direita da praça: prédio em x 8,7..22,9,
z 14,0..24,1, mais calçada e marquise até x 5,6..24,2. Fica em `k_Ubs` (15,8; 0; 19) com
**yaw 180**, porque a entrada do modelo nasce em +X e precisa olhar para a praça. O piso interno
fica em y 0,154 (`k_UbsFloorY`). A entrada **não** é a peça `Porta`: essas são folhas fechadas da
fachada, e o vão que dá passagem tem 0,9 m em **z 19,45** (`k_UbsDoorZ`). O modelo traz 11
marcadores `DC_LUZ_*` que o importador converte em Point Lights; o builder as força para Realtime
sem sombra, porque a cena não tem lightmap assado. A torre de transmissão foi para x 27,5 para
abrir espaço: o prédio não cabia entre a praça (x 5,5) e a torre onde ela estava. Montanhas da praça a ~190 m, nuvens a
~320 m. Céu de tempestade: `RenderSettings.skybox = null` e a câmera passa a pintar o céu de
cinza (ver `SetupStormCamera`). O ponto de ônibus é montado de costas para a rua e recebe meia
volta em torno do próprio centro. Telhado da casa: duas águas com cumeeira ao longo de Z em
x −5,5 (y 5,17), beirais em y 2,8, inclinação ≈ 20,8°. Faixa de pedestres em x −6,9..−2,9; o
atalho escorregadio é atravessar a rua gelada fora dela.

**Colliders do granizo são grossos**, com o topo na superfície visível: partículas rápidas
atravessam colliders finos quando a qualidade de colisão é "Low".

## Modelos importados

Os FBX saem do Blender deitados (altura no eixo Z). O `ScenarioSceneKit` gira a raiz em
(−90, 0, 0); as duas malhas continuam no mesmo referencial. O X fica espelhado em relação às
coordenadas do Blender.

Os modelos são **desempacotados** na cena: as portas ganham pivôs de dobradiça e algumas peças
são escondidas. A cena inteira é regerada a cada build, então editar à mão não adianta.

O importador acende qualquer material com "Luz" no nome — inclusive `DC_M_CasaQuadroLuz`, que
não é luminária. A emissão indevida é apagada por `MaterialPropertyBlock`, por renderer, sem
tocar nos materiais do FBX.

Física: nada vem do importador. Pisos, tapetes, calçadas, rua e gramas viram `MeshCollider` +
área de teleporte; a regra é superfície baixa e fina (altura ≤ 0,35 m, topo ≤ 0,25 m, área
≥ 0,2 m²). Paredes, móveis e estruturas ganham collider estático — `MeshCollider` para peças
leves ou grandes (parede com vão de porta), caixa para peças pequenas e pesadas como o carro.
Árvores só no tronco. Ficam **sem** collider: céu, entorno distante, fiação, plantas miúdas e
o que fica acima da cabeça.

Portas internas abertas: `FrenteCorredor0` → sala, `FrenteCorredor1` → cozinha,
`CorredorFundo0` → quarto de casal, `CorredorFundo1` → quarto 2, `CorredorFundo2` → banheiro,
`Direita1` → quintal.

Luzes da casa: os marcadores `DC_LUZ_*` viram luzes Realtime **sem sombra** — são mais de 20 e
sombra em tempo real é cara demais no Quest. Todas ficam num `HouseLights` que a alavanca do
quadro de energia apaga. Na fase Granizo não há quadro de energia e elas ficam acesas.

## Itens e NPCs modelados (17/09/2026)

- Itens em `Assets/Itens/` e NPCs em `Assets/NPC/`, cada pasta com o próprio `AssetPostprocessor`
  (atlas, material translúcido, rig Humanoid). O builder procura o FBX pelo **nome do objeto**.
- Itens têm **pivô na base**: a posição passada ao builder é o ponto da superfície (`OnSurface` com
  folga de 2–4 mm), não o centro. Sockets da mochila ficam 30 cm acima do banco, para o item entrar
  pela boca; a tampa encaixa 10,5 cm acima da base da panela; a panela é girada 180° para o cabo
  ficar do mesmo lado do placeholder antigo.
- NPCs: o FBX olha para **−Z**. `CreateNpc` gira o modelo e o andador 180°; o andador fica a 25 cm à
  frente do Seu Joaquim. `NpcAnimatorDriver` liga "Andando" pela velocidade real do NPC e dispara
  "Apontar" quando o balão de fala muda (desligado para quem usa andador).

### O que os importadores fazem

`Casa/Editor/DCCasaImportador.cs` e `Praca/Editor/DCPracaImportador.cs` configuram o import de
`DC_Casa.fbx` e `DC_Praca.fbx` sozinhos — não é preciso mexer no Inspector, basta o FBX estar ao
lado da pasta `Editor`.

- Escala 1, sem conversão de eixo extra, **UV2 gerada** para lightmap, sem luzes nem câmeras do FBX.
- **Luzes**: cada Empty `DC_LUZ_*` chega do Blender com propriedades customizadas (`lumens`,
  `temperatura_k`, `alcance_m`, `unity_modo`) e vira uma Point Light. A intensidade é **declarada no
  Blender e convertida aqui** porque não atravessa bem entre os dois. A aproximação usada para Point
  Light em espaço linear é 1800 lm (plafon da sala) ≈ **2,6**; o ajuste fino depois do primeiro bake
  é esse único número.
- **Static**: tudo que tem malha entra como estático para o bake, menos o que precisa se mexer —
  folhas de porta, portinhola do quadro e disjuntor.
- **Maçanetas viram filhas da folha da porta**, para acompanharem a abertura.
- Materiais de lâmpada, cúpula e tela recebem emissão no import, porque o FBX não carrega emissão
  de forma confiável.

**Propriedade `dc_interacao`**: gravada no Blender, o importador a converte no componente
`DCInteracao`. O gameplay procura por esse componente em vez de depender do nome do objeto.
Valores em uso: `disjuntor_geral` (alavanca do quadro, no corredor), `porta_quadro_luz` (portinhola,
origem na dobradiça), `detector_fumaca` (teto do corredor), `painel_educativo` (sob o pergolado da
praça), `totem_defesa_civil` (esquina da praça) e `abrigo_inadequado` (árvores da praça, no Granizo).


`Itens/Editor/DCItensImportador.cs` e `NPC/Editor/DCNpcImportador.cs` são `AssetPostprocessor`:
rodam sozinhos no import. **Depois de alterar um deles, selecione os FBX e mande Reimport**, senão
o ajuste não vale para o que já está importado.

- **Itens**: escala 1, sem luzes, câmeras nem animação, e **sem collider** — quem monta o collider
  é o builder da fase. Não são estáticos (o jogador pega) e não geram UV2 de lightmap. O atlas é
  `Itens/Texturas/DC_T_Itens_Atlas.png`, sRGB, com alpha, máximo 1024. Dois materiais recebem o
  atlas: `DC_M_Itens_Atlas` (opaco) e `DC_M_Itens_Translucido`, que vira URP Transparent e é usado
  no vidro, no plástico da jarra e na água.
- **NPCs**: `Carlos`, `Vizinho` e `SeuJoaquim` entram como rig **Humanoid** com avatar do próprio
  modelo, eixo convertido no import, sem collider. O FBX do Blender nomeia as takes como
  `Armature|Idle`; o importador as renomeia para **Idle**, **Apontar** e **Andar**. Idle e Andar
  ficam em loop, e Andar é **no lugar**, sem root motion — quem move o NPC é o `CompanionFollower`.
  `Andador.fbx` é prop, sem animação. O material `DC_M_NPC_Atlas` usa `NPC/Texturas/DC_T_NPC_Atlas.png`
  com filtro **Point**, porque a textura é uma paleta de cores chapadas.
- **Animator** (`SafeZone VR > NPC > Criar Animator Controller`): gera `NPC/DC_NPC.controller` a
  partir dos clipes de um dos FBX — como os três são Humanoid, o mesmo controller serve para todos.
  Idle é o estado padrão; `Andando = true` leva a Andar e `false` volta para Idle; o trigger
  `Apontar` sai de qualquer estado e volta para Idle ao terminar.

## Portas pela maçaneta

- `OpenHouseDoors` monta as portas **fechadas** no editor; o `ToggleOpening` (`startsOpen`) as abre no
  `Awake`. Portanto a cena salva mostra as portas internas fechadas, e isso é esperado.
- A pega (`Macaneta_Pega`) é uma caixa filha da folha que atravessa a folha e cobre a maçaneta dos
  dois lados. A porta da frente do modelo só tem maçaneta do lado de fora; sem isso, por dentro o
  raio batia na folha e a mão não alcançava.
- Perto (≤ 40 cm da pega, medido da mão até o colisor) a porta segue a mão; de longe, apontar e
  apertar alterna. A distância **não** pode usar `GetDistanceSqrToInteractor`: o Near-Far mede pelo
  attach, que num agarre à distância pula para o ponto atingido pelo raio (`InteractionDistance`).

## Conforto, som, dicas e aviso

- Mão dominante: a mão **não dominante** anda no modo contínuo e a dominante gira/teleporta; o
  relógio muda de pulso. Se as duas mãos tiverem `smoothMotionEnabled`, o XRI desliga o giro das duas.
- Volumes por categoria (`AudioVolumes`: Efeitos, Narração, Ambiente). `SimpleLoopSound` aplica o
  volume sozinho, exceto quando outro script controla o volume em tempo real (`manageVolume = false`
  no crepitar do fogo e no granizo).
- `IdleHintSystem`: 30 s / 60 s sem avanço, erro ou interação (andar não conta).
- `SafetyNoticePanel` (hub): chave `safezone.safety.accepted`; aumente `k_NoticeVersion` para reexibir.
- Ao testar pelo MCP, apague depois as chaves `safezone.comfort.*`, `safezone.audio.*` e
  `safezone.safety.accepted` que o teste criar.

## Avaliação, integridade e falha (17/09/2026)

- Toda ação relevante vira um `ActionRecord` no `ScenarioManager` (`id`, instante, `ActionKind`
  Acerto/ErroLeve/ErroGrave, `fatal`, `advice`). O relatório monta as listas a partir disso.
- `PerformanceEvaluator` aplica RN01–RN05: 100 pontos, −5 leve, −25 grave, −10 acima do
  `ScenarioSO.timeLimitSeconds` (420/480/600 s). Faixas 90/70/50. **Passo fora de ordem conta como
  erro leve**; repetição de passo não tira mais pontos. Recorde agora é `safezone.best.score.<id>`
  (o melhor tempo continua em `safezone.best.time.<id>`).
- `PlayerIntegrity` (Systems) zera → `RegisterMistake(..., fatal: true)` → `FailScenario`: o
  relatório vira "Fase interrompida" com o motivo e a orientação. Nada de dano visível: a proposta
  é manter a classificação livre.
- Erro grave **fatal** hoje: só o choque elétrico (`EnergizedAppliance`) e a integridade zerada.
- `BreakerDeadline` avisa aos 5 cm de água e registra erro grave aos 12 cm com o quadro ligado.
- `WallClipGuard` compara a última posição válida da cabeça com a atual; se o trajeto atravessa
  geometria, escurece (até 0,92) e vibra. Saltos > 1,2 m são teleporte e resetam a referência.
  Ele usa `ScreenFader.SetOverlay`, que não age enquanto o fader está em transição.
- `LoadingScreen` carrega a cena com `allowSceneActivation = false` e só ativa no fim, então a cena
  antiga continua de pé (ambiente estático) enquanto a barra anda. O painel usa materiais com
  `renderQueue` 4010/4011 para aparecer **por cima** do quad preto do `ScreenFader`.

## Hub (MainMenu)

O jogador entra pelos fundos do terraço, pelo lado aberto do guarda-corpo, de frente para os
totens. Os painéis do modelo ficam de costas para a entrada quando criados sem giro: a UI
world-space só é legível pelo lado −Z do próprio canvas.

O modelo grava **"TERREMOTOS"** no totem verde, mas o terceiro cenário do jogo é o **Granizo**.
O texto gravado (submalha `DC_M_White`, faixa em y = 0,83 m, 10 cm à frente do centro do
pedestal) é escondido e substituído por uma placa 3D com o nome certo.

O piso de teleporte invisível fica 2 cm acima do chão do modelo, para ser o primeiro alvo do
raio, e cobre só a área dentro do guarda-corpo.

## Armadilhas de build conhecidas

- **`MainMenuBuilder` só carrega o catálogo na hora de usar.** Qualquer gravação de asset feita
  no meio do build (materiais, reimport do modelo) invalida uma referência carregada antes.
- **Casa antiga**: a raiz é comparada por prefixo, porque sem o prefab o nome vem com sufixo
  `(Missing Prefab with guid: ...)`.
- **`FlareOnSelect` precisa continuar em arquivo com o mesmo nome da classe**, senão o Unity
  não serializa o componente.
- **`SceneFlow` trava trocas simultâneas de cena**: sem isso, dois cliques seguidos encadeavam
  dois carregamentos e a fase aberta não era a escolhida.
- **`ScreenPointGraphicRaycaster`** existe porque o `TrackedDeviceGraphicRaycaster` só responde
  a raios de controle, e o "apontar e clicar" do XR Interaction Simulator usa posição de tela.
  Ele se desliga no Quest.
- **`XRUIInputModule`** nasce com mouse e toque desligados; o kit os liga para o simulador do
  editor. No headset não muda nada.
- **Editar qualquer `.cs` durante o Play Mode** provoca recarga de domínio e faz os objetos do
  XRI se comportarem como se `Awake` nunca tivesse rodado. Saia do Play antes de mexer em código.

## Configuração do APK (`QuestProjectConfigurator`)

| Ajuste | Motivo |
|---|---|
| `colorSpace = Linear` | obrigatório para VR com URP |
| `ARM64` | o Quest é só ARM64 |
| **GameActivity** | no Unity 6 a regra do `MetaQuestFeature` (pacote OpenXR) exige; a orientação antiga do plugin Oculus está errada nesta stack |
| API mínima 32 | mínimo aceito pela Meta no Quest 3/3S |
| `gcIncremental = true` | evita picos de GC no meio do quadro |
| `buildAppBundle = false` | APK para sideload por adb/SideQuest |
| blit type Never | no VR quem apresenta a imagem é o compositor do headset |
| ASTC | formato de textura do Quest; "Generic" sobe ETC2 e gasta mais memória |
| bake collision meshes | carrega mais rápido e dispensa Read/Write nas malhas |
| `m_renderMode = 1` | Single Pass Instanced: uma passada para os dois olhos |
| `m_optimizeBufferDiscards` | Vulkan: descarta buffers e economiza banda |
| `m_symmetricProjection` | Quest 3: uma projeção só para os dois olhos |
| `m_foveatedRenderingApi = 1` | SRP Foveation |
| `m_IntermediateTextureMode = 0` | Auto: evita uma cópia de tela por quadro |
| `m_SupportsHDR = false` | HDR custa banda e não muda nada aqui |
| `m_MSAA = 4` | antialiasing barato no Quest |
| Forward+ | agrupa luzes por tile; as luzes da casa acendem sem o limite por objeto do Forward |
| luzes adicionais **PerPixel**, limite 8 | o Performance URP Config vinha com `Disabled` e limite 1: as luminárias da casa não acenderiam no APK |
| splash desligada | fica de pé em licença Personal; sem ela o app entra direto no menu |

O nível de qualidade do Android é fixado no índice do **Performance URP Config** — é o perfil
que o headset usa.

## Detalhes de gameplay

- O painel do relógio só aparece quando a fase começa; antes disso cobriria o briefing.
- Relógio, bússola e opções somem enquanto o relatório final está aberto.
- Mochila: vários sockets dividem uma área de captura do tamanho da mochila e só o primeiro
  espaço vazio fica ativo, para aparecer um "fantasma" por vez.
- `StepCompleteOnTriggerZone` também verifica a posição da câmera periodicamente, porque o
  teleporte não dispara eventos de trigger de forma confiável.
- `StepCompleteOnOpenings` só avalia quando uma abertura muda de estado, senão uma porta que
  começa fechada concluiria "fechar a porta" logo de cara.
- Saída de socket cancelada (`isCanceled`) é cena descarregando ou objeto desativado, não ação
  do jogador.
- Grupos de ordem: nas fases Incêndio e Granizo as missões 1 e 2 podem ser feitas em qualquer
  ordem (`EditorSetOrderGroup(1)`).
- A água do alagamento sobe rápido no início e desacelera perto do nível máximo, que fica na
  altura da canela e não tem efeito sobre o jogador.
