# SafeZone VR — Guia de configuração (fases Alagamento, Incêndio e Granizo)

Toda a camada de gameplay é **gerada por scripts de Editor** sobre a casa modelada. Você não precisa
montar validadores à mão: abra o menu **SafeZone VR > Build** no Unity.

| Menu | O que faz |
|------|-----------|
| `0. Montar tudo` | Executa todos os passos abaixo em sequência. |
| `1. Dados` | Cria/atualiza os ScriptableObjects das três fases (passos, cards, cenários) e o catálogo em `Resources/ScenarioCatalog` (Alagamento, Incêndio em Casa, Granizo). |
| `1b. Prefab da casa` | Converte a geometria solta da cena Alagamento no prefab `Prefabs/Casa_SafeZone` (reaproveitado pelas fases Incêndio e Granizo) e abre o vão da janela da cozinha. |
| `2. Fase Alagamento` | Abre `Scenes/Fases/Alagamento_EmCasa`, remove o gameplay antigo e recria tudo sob `SafeZone_Gameplay` (idempotente: pode rodar quantas vezes quiser). |
| `2b. Fase Incêndio` | Cria/abre `Scenes/Fases/Incendio_EmCasa` (casa via prefab) e monta panela em chamas, gás, tampa, janela, benjamim/rack, fumaça, porta operável, vizinho e Bombeiros. |
| `2c. Fase Granizo` | Cria/abre `Scenes/Fases/Granizo_Praca`: praça, UBS, abrigos errados, vagas, Seu Joaquim, granizo por partículas (layer `HailBlockers`), gelo e a fase "Depois" na casa. |
| `3. Menu principal` | Gera a cena `Scenes/MainMenu` (sobrescreve). |
| `4. Configurações do projeto` | Nome do produto, Android/Quest (IL2CPP, ARM64, Vulkan, min SDK 32), URP e cenas no Build Settings. |

Os helpers comuns de cena vivem em `Editor/ScenarioSceneKit.cs` (XR Origin, materiais, teleporte, colisores, itens,
sockets, relógio de pulso com botão *Ligar*, painéis, painel de ligação 199/193, NPCs, legenda de fase).

## Fases Incêndio em Casa e Granizo

Especificações completas em `Documentos_Cenarios/Incendio.md` e `Documentos_Cenarios/Granizo.md` (missões,
decisões inseguras, cards, componentes e roteiro de teste). Componentes compartilhados novos em `Runtime/`:
`ScenarioPhaseSwitcher` (estados do ambiente), `ActivateOnStepCompleted`, `CompanionFollower` (NPC por waypoints),
`EmergencyCallPanel`, `StepCompleteOnInteractCount`, `StepCompleteOnStayInZone`, `StepCompleteOnCompanionInZone`,
`StepCompleteOnChoice`/`DecisionOption`, `StepCompleteOnOpenings`/`ToggleOpening`, `StepCompleteOnAllConditions`,
`FireController`, `SmokeLayer`, `LidCooldown`, `HailController`, `InspectableSign`, `SimpleLoopSound`.
Passos com o mesmo `orderGroup` (≠ 0) podem ser feitos em qualquer ordem sem contar "fora de ordem".

**Névoa no build (Incêndio):** em *Project Settings → Graphics → Shader Stripping* mantenha a névoa
**Exponential Squared** incluída, senão a fumaça funciona no Editor e some no Quest.

## O que a fase Alagamento contém

| Passo | Ação do jogador | Validador | Onde |
|-------|-----------------|-----------|------|
| 1. Proteger documentos e objetos de valor | Levar `Item_Documentos` e `Item_PortaRetrato` (sofá) até os sockets no alto do armário | `StepCompleteOnSocket` | Sala → Quarto |
| 2. Desligar o quadro de energia | Tocar/selecionar a alavanca (`Disjuntor_PowerOff/Alavanca_Pivot`) | `StepCompleteOnBreaker` | Quarto |
| 3. Fechar o registro de água | Girar o `Registro_Knob` (XRKnob) 90° | `StepCompleteOnKnob` | Garagem |
| 4. Montar o kit | Colocar água (cozinha), remédios (banheiro) e lanterna (quarto) na `Mochila_Kit` | `StepCompleteOnSocket` | Casa toda → entrada |
| 5. Evacuar | Sair pela porta, seguir as setas verdes até a plataforma na rua | `StepCompleteOnTriggerZone` | Rua |

Decisão insegura registrada (não encerra a fase): selecionar a porta da Kombi (`WrongActionInteractable`).

## Sistemas de apoio (todos sob `SafeZone_Gameplay`)

- `ScenarioManager` — passo atual, tempo, erros, eventos.
- `ObjectiveBeacons` — marcadores flutuantes sobre os alvos do passo atual (`ObjectiveTarget`) e ativação da rota de fuga.
- `Systems` — `FeedbackPlayer` (sons procedurais + vibração), `ComfortSettingsApplier` (teleporte/contínuo, giro, vinheta, sentado).
- `Environment/Flood` — água que sobe devagar (máximo 32 cm) enquanto a fase corre.
- `UI` — briefing inicial, painel de opções e relatório final (LazyFollow em frente ao jogador). O relógio de pulso fica em `XR Origin/.../Left Controller/WristUI`.

## Trocar os placeholders por modelos reais

Os itens são primitivos (cubos/cilindros). Para usar um modelo:

1. Arraste o modelo como filho do item (ex.: `Item_Agua`), zere posição/rotação e desative/remova o `MeshRenderer` do cubo.
2. Ajuste o `BoxCollider` ao novo tamanho.
3. Mantenha `ObjectiveItemId`, `XRGrabInteractable`, `ObjectiveTarget` e `RespawnIfFallen` no objeto raiz.

Se rodar o builder de novo, os placeholders voltam — nesse caso mova a substituição para o próprio
`AlagamentoGameplayBuilder.CreateItem` (troque o `PrimitiveType` pelo prefab).

## Testar sem headset

O **XR Interaction Simulator** é instanciado automaticamente no Editor (Project Settings > XR Interaction Toolkit).
Dê Play na cena `MainMenu` ou direto em `Alagamento_EmCasa`. Teclas padrão do simulador: WASD move, botão direito gira,
`Tab` alterna dispositivo, `G` pega, `T`/`Shift+T` teleporte.
