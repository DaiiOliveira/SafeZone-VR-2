# Adequação do SafeZone VR aos requisitos essenciais do Trabalho 2

**Data do levantamento:** 29/09/2026 · **Base:** commit `3ff00c8` · **Enunciado:** T2, 5OPREV
(Realidade Virtual), Prof. Adilson Vahldick — DESO/UDESC Ibirama

Este documento é um plano de execução: o diagnóstico de cada requisito do enunciado contra o
estado real do repositório, e o trabalho necessário para fechar as lacunas. Escrito para ser
executado depois, inclusive por um agente na nuvem — todo caminho de arquivo e número de linha
está explícito.

---

## Execução (30/09/2026)

Feito no mínimo necessário, sem complexidade extra:

- **A1** `CreatePokeButton` + componente `PokeButton` (afunda a tampa e toca o clique; substitui o
  `XRPokeFollowAffordance`, que é do sistema de affordance depreciado).
- **A2** celular (modelo `Itens/Celular.fbx`) com teclado numérico de poke no Incêndio e no
  Granizo — o jogador digita o número de verdade (refeito em 02/10, no lugar do painel com 4 teclas).
- **Grupo 2 de poke:** em vez dos três botões avulsos da casa (A3), um botão **JOGAR** físico em
  cada totem do hub — três botões iguais, usados em toda partida, sem mexer nos validadores.
- **B1/B2** som de fundo no hub e no Incêndio. **B4** o clique sai do próprio `PokeButton`.
- **D2** registrado em `REFERENCIA_TECNICA.md`.
- Não feitos por decisão de simplicidade: A3, A4, B3 (`AmbientZone`), C (conferência automática),
  D1, D3, D4. Pendente: testar o poke no headset.

---

## Contexto

O enunciado do T2 lista seis requisitos essenciais para o sistema de RV e os transforma em
pontos na avaliação do T4 (APK final):

| Requisito do enunciado | Peso no T4 |
|---|---|
| Mínimo 3 portas, abertas por *grab* ou por botão que dispara a animação | 3,0 |
| Mínimo 2 grupos de interações com **grab** (desconsiderando portas) | 3,0 |
| Mínimo 2 grupos de interações com **poke** (desconsiderando portas) | 3,0 |
| Um som de fundo para cada ambiente distinto | 1,0 |
| Movimentação baseada em teletransporte | — |
| Uso da aplicação deve durar no mínimo 3 minutos | −3,0 se falhar |
| **Não carregar no Meta Quest** | **nota ZERO** |

O SafeZone VR atende cinco dos seis. O furo é **poke: o projeto tem zero interações por
poke** — `grep -i poke` no código e nas quatro cenas não retorna nada. São 3,0 pontos
perdidos de graça, porque os *Poke Interactors* já estão nas duas mãos do rig; falta apenas
existir algo no mundo configurado para ser tocado. O som de fundo está parcialmente atendido
e vale 1,0.

**Escopo deste plano:** só a adequação técnica do jogo. Os artefatos de entrega (apresentação
de 16/10, PDF no template Word de 18/10, vídeo de gameplay do T3) ficam fora daqui.

**Restrição de teste:** não há headset próprio — o teste no Meta Quest depende de agendamento
no Bloco Ipê com o professor.

---

## Diagnóstico: requisito × situação atual

### ✅ 1. Três portas (3,0) — ATENDIDO com folga

`ToggleOpening` (`UnityProject/Assets/SafeZoneVR/Runtime/Interactables/ToggleOpening.cs`)
é o mecanismo único: `[RequireComponent(typeof(XRSimpleInteractable))]`, e no
`OnSelectEntered` (linha 71) decide entre **arrastar a folha pela maçaneta** (mão perto,
`m_NearDistance = 0.4f`) e **alternar aberta/fechada** (apontar e apertar). A rotação é por
código (`ApplyAngle`, linha 128), não por clipe de Animator — atende o efeito que o
enunciado pede.

Montagem em `Assets/SafeZoneVR/Editor/ScenarioSceneKit.cs`: `MakeDoorOperable` (linha 500),
`CreateDoorGrip` (linha 524, o collider `Macaneta_Pega` em volta da maçaneta) e
`OpenHouseDoors` (linha 572).

Contagem por cena: **7 portas operáveis** em Alagamento, Incêndio e Granizo (6 internas + a
da frente). O Incêndio ainda tem a **janela da cozinha** operável pelo mesmo componente
(`IncendioGameplayBuilder.cs:330`). O `DC_CASA_PortaoPedestre` tem dobradiça mas não é
interagível de propósito. **Nada a fazer.**

### ✅ 2. Dois grupos de grab (3,0) — ATENDIDO

`XRGrabInteractable` é adicionado só por `ScenarioSceneKit.CreateModelItem` (linha 658):
`useDynamicAttach`, `throwOnDetach`, `VelocityTracking`, Rigidbody, `ObjectiveItemId` e
`RespawnIfFallen`.

| Grupo | Itens | Onde |
|---|---|---|
| Kit de emergência → sockets da mochila | `Item_Agua`, `Item_Remedios`, `Item_Lanterna`, `Item_Documentos` | `AlagamentoGameplayBuilder.cs:80-89`, sockets em `:117-123` |
| Valores → socket do alto do armário | `Item_PortaRetrato`, `Item_Documentos` | `AlagamentoGameplayBuilder.cs:82-83`, socket em `:101` |
| Abafar o fogo | `Item_Tampa` → `Socket_Tampa` na panela | `IncendioGameplayBuilder.cs:84-91` |

Dois grupos claros já no Alagamento, mais um no Incêndio. **Ponto de atenção:** o Granizo
tem **zero** grabbables (tudo lá é `XRSimpleInteractable`). Não fere o requisito, mas deixa
uma das três fases sem grab — ver item opcional D1.

### ❌ 3. Dois grupos de poke (3,0) — NÃO ATENDIDO, zero implementado

- `grep -rn -i poke` em `Assets`, excluindo `Assets/Samples`: **nenhum resultado** em `.cs`,
  `.unity`, `.prefab` ou `.asset`.
- As quatro cenas instanciam
  `Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`
  (via `ScenarioSceneKit.SetupXROrigin`, linha 171), que **já traz dois `Poke Interactor`**
  (esquerda e direita) e as `Poke Point Affordances`. Nenhuma cena os desativa.
- O interator tem **`m_RequirePokeFilter: 1`**
  (`Assets/Samples/.../Prefabs/Interactors/Poke Interactor.prefab:116`): sem um
  `XRPokeFilter` no interagível, o poke **não seleciona nada**. É exatamente isso que falta.
- Parâmetros a respeitar ao dimensionar colliders: `m_InteractionLayers: 1` (Default),
  `m_PokeDepth: 0.1`, `m_PokeWidth: 0.0075`, `m_PokeSelectWidth: 0.015`,
  `m_PokeInteractionOffset: 0.005`.
- `m_EnableUIInteraction: 1` e `m_ClickUIOnDown: 1`: os canvases com
  `TrackedDeviceGraphicRaycaster` (`UIBuilderUtil.CreateWorldCanvas`, linha 140) tendem a
  responder ao dedo **por acidente**, sem affordance visual e sem ter sido testado.
- Referência de configuração pronta e não usada:
  `Assets/Samples/.../DemoAssets/Prefabs/Interactables/Push Button.prefab` (linhas 348-375,
  com `m_PokeCollider`, `m_PokeConfiguration`, `m_PokeFilter`) e o script
  `Assets/Samples/.../Starter Assets/Scripts/XRPokeFollowAffordance.cs`.

### ⚠️ 4. Som de fundo por ambiente (1,0) — PARCIAL

Todo o áudio é gerado em runtime por `Runtime/Core/ProceduralAudio.cs` — **não existe um único
arquivo de áudio autoral no repositório** (o único `.wav` em `Assets` é o `Button Pop.wav` dos
Samples do XRI, não usado). Volume por barramento em `AudioVolumes` (Effects/Narration/Ambient).

| Ambiente | Som de fundo hoje | AudioSources na cena |
|---|---|---|
| Hub / `MainMenu` | **nenhum** — `MainMenuBuilder.cs` só mexe em `RenderSettings.ambient*` | **0** |
| Alagamento (casa) | chuva em loop (`FeedbackPlayer.cs:46-53`, ligada em `AlagamentoGameplayBuilder.cs:65`) | 1 |
| Incêndio (casa) | **nenhum ambiente** — `playRainAmbience = false` (`IncendioGameplayBuilder.cs:65`); só crepitação, alarme, sirene e caminhão, todos posicionais e categoria *Effects* | 6 |
| Granizo — praça (externo) | chuva + granizo no telhado (`GranizoGameplayBuilder.cs:662`) | 3 |
| Granizo — interior da UBS | mesmo som da praça, sem distinção | — |
| Granizo — interior da casa | mesmo som da praça, sem distinção | — |

Cinco ambientes visualmente distintos compartilham dois loops, e dois (hub e Incêndio) não têm
loop de fundo nenhum. Três detalhes que aparecem na inspeção e valem corrigir de passagem:

- **Todo o áudio é 2D.** `Spatialize: 0` em todos os `AudioSource` e `spatialBlend = 0f`
  explícito em `FeedbackPlayer.cs:37,51`. Não fere o requisito, mas é o tipo de coisa que pesa
  na nota de "qualidade gráfica e som" do T3.
- `AudioCategorySource` existe e **nunca é colocado em cena** (0 instâncias nas quatro cenas),
  então o barramento *Ambient* só chega ao som por meio do `SimpleLoopSound`.
- `FeedbackPlayer.PlayUiClick()` (linha 110) é **código morto** — nenhum chamador. Serve
  perfeitamente como som dos botões de poke (item B4).

### ✅ 5. Teletransporte — ATENDIDO

Padrão é `LocomotionMode.Teleport` (`Runtime/Core/ComfortSettings.cs:32`), giro em `Snap`, e a
pilha de locomoção inteira (`LocomotionMediator`, `TeleportationProvider`, `XRBodyTransformer`)
vem dentro do prefab do rig. `TeleportationArea` por cena
(`ScenarioSceneKit.AddTeleportArea`, linha 356; camada de interação 31, isolada): **43**
Alagamento, **43** Incêndio, **71** Granizo, **1** hub. O painel de conforto permite trocar
para contínuo — é acessibilidade, e o padrão continua teleporte. **Nada a fazer**, com uma
ressalva anotada nos riscos: todas as áreas têm `m_TeleportationProvider` nulo e dependem da
busca em runtime do XRI 3.x.

### ✅ 6. Mínimo 3 minutos — ATENDIDO com folga

Dos assets gerados (`Data/Alagamento/Scenario_Alagamento.asset`, `Data/Scenario_Incendio.asset`,
`Data/Scenario_Granizo.asset`, montados por `Editor/SafeZoneDataBuilder.cs`):

| Fase | Passos | Cards | alvo / bom / limite (s) |
|---|---|---|---|
| Alagamento em Casa | 5 | 6 | 240 / 420 / 420 |
| Incêndio em Casa | 9 | 10 | 300 / 480 / 480 |
| Granizo na Praça | 8 | 9 | 360 / 600 / 600 |

**22 passos, ~15 min de tempo-alvo somado** (limite somado ~25 min). Uma fase sozinha já passa
dos 3 minutos. **Nada a fazer.**

### 🔴 7. Carregar no Meta Quest — RISCO ABERTO (nota ZERO se falhar)

`QuestProjectConfigurator.cs` aplica GameActivity, IL2CPP + ARM64, Vulkan, Single Pass
Instanced, ASTC, Blit Never, Forward+ e espaço linear; as 4 cenas estão no
`EditorBuildSettings.asset`; o `OpenXRPackageSettings.asset` tem o `MetaQuestFeature` ligado
com Quest 3 e 3S habilitados. Mas **nunca houve build instalado num headset**. Este é o único
item capaz de zerar a nota.

---

## Trabalho a fazer

### A. Poke — dois grupos diegéticos + reforço na UI (prioridade máxima)

**A1. Helper central `MakePokeButton`** em `Assets/SafeZoneVR/Editor/ScenarioSceneKit.cs`,
espelhando a forma de `MakeDoorOperable` (linha 500) para manter o estilo do arquivo:
recebe o transform do botão e o eixo de pressão; garante `XRSimpleInteractable`,
`BoxCollider` dimensionado a partir da malha (reusar `FitBoxCollider`, linha 557),
`XRPokeFilter` (com `pokeCollider` e direção local) e o `XRPokeFollowAffordance` dos
Samples para o botão afundar ao toque. Copiar os valores do `Push Button.prefab` como base.
Chamada única em todos os grupos abaixo.

**A2. Grupo 1 — teclado de emergência (diegético).**
`ScenarioSceneKit.CreateCallPanel` (linha 1456) monta o telefone como `Button`s de UGUI num
canvas. Acrescentar ao lado um teclado físico `Teclado_Emergencia` com três teclas de ~4 cm
(`Tecla_199`, `Tecla_193`, `Tecla_192`) criadas por `MakePokeButton`, cada uma ligada a
`EmergencyCallPanel.Call(numero)` — o método já é público
(`Runtime/UI/EmergencyCallPanel.cs:47`) e já dispara `onAcceptedCall`, que os validadores
consomem. Aparece nas fases **Incêndio** (passo "Ligar 193") e **Granizo** (passo "Ligar 199
ou 193"). Manter os botões de UI como caminho alternativo.

**A3. Grupo 2 — botões fixos da casa (diegético).** Mesmo helper, três botões:
- `Botao_Teste_Detector` no `DC_CASA_DetectorFumaca` (a peça existe no FBX) — aperta e o
  alarme dispara; liga em `SimpleLoopSound.Play()` do detector
  (`IncendioGameplayBuilder.cs:417`). Fase Incêndio.
- `Botao_Campainha` no portão de pedestres — Granizo (chamar o Seu Joaquim antes de ajudá-lo)
  e Incêndio (o vizinho toca antes de tentar entrar).
- `Botao_Desliga_Fogao` ao lado do `Botao_Fogao`, que hoje é `XRKnob`
  (`IncendioGameplayBuilder.cs:93`) — poke para cortar o gás, alternativa ao giro.

**A4. Reforço — UI pokeável explícita.** Em `UIBuilderUtil.Button` (linha 217) acrescentar o
afundamento visual e o collider, para o poke nos painéis deixar de ser acidental: painel de
opções (`OptionsPanelController`, ~15 botões), relatório final, briefing e os três totens do
hub (`MainMenuBuilder.CreateKiosks`, linha 198). Vale como terceiro grupo e como garantia se
um dos diegéticos não convencer o avaliador.

**A5. Validadores — nada novo.** `StepCompleteOnButtonPress.OnButtonPressed()`
(`Runtime/Validators/StepCompleteOnButtonPress.cs:16`) já serve de alvo para o
`selectEntered` do botão pokeável.

### B. Som de fundo para cada ambiente distinto

**B1. Ambiente do hub.** Novo `ProceduralAudio.CreateHubLoop` (drone suave, no molde de
`CreateRainLoop`, linha 47) + novo `ClipKind.Hub` em
`Runtime/Core/SimpleLoopSound.cs:7`; `MainMenuBuilder.Build` passa a criar o `SimpleLoopSound`
correspondente, categoria `Ambient`.

**B2. Ambiente do Incêndio.** Trocar o `playRainAmbience = false` de
`IncendioGameplayBuilder.cs:65` por um loop de fundo próprio: novo
`ProceduralAudio.CreateWindLoop` (ou crepitação em 2D com volume baixo), para a fase deixar
de rodar sem som de ambiente.

**B3. Ambientes internos distintos.** Novo componente
`Runtime/Core/AmbientZone.cs`: um trigger que faz *crossfade* entre o loop externo e um
loop interno ao entrar e sair. Aplicar no interior da **UBS** (granizo abafado) e no interior
da **casa** do Granizo. É o que demonstra sem ambiguidade "um som para cada ambiente
distinto" na avaliação.

**B4. Som do poke.** Ligar `FeedbackPlayer.PlayUiClick()` (`Runtime/Core/FeedbackPlayer.cs:110`,
hoje sem nenhum chamador) ao `selectEntered` dos botões criados por `MakePokeButton` — o toque
passa a ter retorno sonoro além do háptico, e o método deixa de ser código morto.

### C. Rede de segurança — conferência automática dos requisitos

Novo `Assets/SafeZoneVR/Editor/T2RequirementsCheck.cs`, no padrão do
`QuestProjectConfigurator.Validate()` (menu `4b`), exposto como
`SafeZone VR > Build > 6. Conferir requisitos do T2`. Abre cada cena do Build Settings e
conta `ToggleOpening`, `XRGrabInteractable` (agrupados por `ObjectiveItemId`), `XRPokeFilter`,
`AudioSource` com `loop = true` e `TeleportationArea`; imprime a tabela no Console e falha se
algum mínimo do enunciado não bater. É o que evita perder ponto por descuido depois de uma
regeneração de cena.

### D. Opcionais (não valem ponto, mas fecham lacunas)

- **D1.** Um grupo de grab no Granizo — recolher as telhas quebradas (`Telha` ×n,
  `GranizoGameplayBuilder.cs:821`) com `CreateModelItem` e um socket de descarte, para
  nenhuma das três fases ficar sem grab.
- **D2.** Registrar a tabela "ambiente → som" e a lista de grupos de poke em
  `Documentos_Cenarios/REFERENCIA_TECNICA.md`, que é onde o projeto guarda o que não se
  deduz lendo o código.
- **D3.** Espacializar os loops de ambiente e de efeito (`spatialBlend` > 0 nos
  posicionais, mantendo 2D só no leito de ambiente) e colocar `AudioCategorySource` nas
  fontes que hoje não passam pelo barramento. Pesa na nota de som do T3, não no T4.
- **D4.** Adicionar um *loader* OpenXR em **Standalone Providers**
  (`Assets/XR/XRGeneralSettingsPerBuildTarget.asset`, hoje `m_Loaders: []`). Não muda o APK,
  mas abre o caminho de testar por Quest Link caso apareça um headset antes do Bloco Ipê —
  hoje o único teste possível é o XR Interaction Simulator.

---

## Arquivos que mudam

| Arquivo | Mudança |
|---|---|
| `UnityProject/Assets/SafeZoneVR/Editor/ScenarioSceneKit.cs` | `MakePokeButton` (A1); teclado físico no `CreateCallPanel` (A2) |
| `UnityProject/Assets/SafeZoneVR/Editor/IncendioGameplayBuilder.cs` | botões do detector, campainha e fogão (A3); ambiente de fundo (B2) |
| `UnityProject/Assets/SafeZoneVR/Editor/GranizoGameplayBuilder.cs` | campainha do portão (A3); `AmbientZone` na UBS e na casa (B3); grab das telhas (D1) |
| `UnityProject/Assets/SafeZoneVR/Editor/MainMenuBuilder.cs` | som de ambiente do hub (B1) |
| `UnityProject/Assets/SafeZoneVR/Editor/UIBuilderUtil.cs` | affordance e collider de poke nos botões (A4) |
| `UnityProject/Assets/SafeZoneVR/Runtime/Core/ProceduralAudio.cs` | `CreateHubLoop`, `CreateWindLoop` (B1, B2) |
| `UnityProject/Assets/SafeZoneVR/Runtime/Core/SimpleLoopSound.cs` | novos `ClipKind` (B1, B2) |
| `UnityProject/Assets/SafeZoneVR/Runtime/Core/AmbientZone.cs` | **novo** — crossfade de ambiente por trigger (B3) |
| `UnityProject/Assets/SafeZoneVR/Editor/T2RequirementsCheck.cs` | **novo** — conferência dos mínimos (C) |
| `Documentos_Cenarios/REFERENCIA_TECNICA.md` | tabelas de som e de poke (D2) |

Convenções a seguir (já valendo no projeto): sem comentários no código, campos `m_`/`k_`/`s_`,
`.editorconfig` na raiz com as regras `UNT`, o que não se deduz do código vai para
`REFERENCIA_TECNICA.md`. As cenas são **geradas**: toda mudança vai no builder, nunca no
`.unity`.

---

## Verificação

1. **Regenerar tudo:** menu `SafeZone VR > Build > 0. Montar tudo` — reconstrói dados, as
   três fases e o menu. Console sem erro nem *warning* novo.
2. **Conferência automática:** `SafeZone VR > Build > 6. Conferir requisitos do T2` (item C)
   deve imprimir, por cena: portas ≥ 3, grupos de grab ≥ 2, grupos de poke ≥ 2,
   `AudioSource` em loop ≥ 1, `TeleportationArea` > 0.
3. **Play no editor,** cena `MainMenu`: o XR Interaction Simulator é criado automaticamente.
   Percorrer Alagamento → Incêndio → Granizo do começo ao fim; cronometrar (cada fase tem de
   passar de 3 minutos) e conferir a mão a mão: abrir e fechar três portas, pegar e encaixar
   itens dos dois grupos de grab, **tocar com o dedo** as teclas do telefone e os botões da
   casa (o afundamento tem de acontecer), e ouvir a troca de som ao entrar na UBS.
4. **Configuração do APK:** `4b. Conferir configurações do APK` — relatório sem pendência.
5. **APK:** `5. Gerar APK (release)` → `adb install -r Builds/SafeZoneVR-1.0-release.apk`.
6. **Headset (bloqueante para a nota):** agendar com o professor no Bloco Ipê. Conferir que
   carrega, que o poke funciona com o controle e com *hand tracking*, e o FPS com o OVR
   Metrics Tool (alvo 72).

---

## Riscos

- **Não carregar no Quest zera o T4.** É o único risco de nota total e o único que não se
  resolve no editor. Gerar o APK e agendar o teste **antes** de refinar qualquer outra coisa;
  se o dia no Bloco Ipê atrasar, é o item que trava a entrega.
- **Poke exige precisão física.** `m_PokeSelectWidth: 0.015` e `m_PokeDepth: 0.1` significam
  que um botão pequeno ou embutido demais não registra o toque. Testar cada botão no
  simulador e, no headset, com a mão — não só com o controle.
- **`m_RequirePokeFilter: 1`.** Esquecer o `XRPokeFilter` faz o botão parecer montado e
  simplesmente não responder ao dedo. É por isso que o helper único (A1) e a conferência
  automática (C) existem.
- **Regeneração de cena apaga ajuste manual.** Qualquer coisa feita direto no `.unity` se
  perde no próximo `Montar tudo`. Tudo vai no builder.
- **Sem caminho de teste em VR real antes do Bloco Ipê.** `Standalone Providers` está vazio
  (`Assets/XR/XRGeneralSettingsPerBuildTarget.asset`), então não há Quest Link; o
  simulador é a única validação possível no editor, e ele não reproduz bem a precisão de dedo
  que o poke exige. D4 mitiga em parte.
- **`TeleportationArea` com `m_TeleportationProvider` nulo** nas 158 áreas. Funciona pela
  busca em runtime do XRI 3.x, mas é o tipo de dependência implícita que quebra numa
  atualização de pacote. Não mexer agora; só não confundir com defeito se aparecer aviso.
