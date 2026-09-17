# Itens que ainda faltam modelar

Levantamento de tudo que hoje é **placeholder de primitiva** (cubo, cilindro, cápsula, quad)
criado por código nos builders de cena. Nada aqui é modelo importado: são formas do Unity com
material chapado, feitas para o jogo funcionar antes da arte existir.

Fonte: `Assets/SafeZoneVR/Editor/{Alagamento,Incendio,Granizo}GameplayBuilder.cs` e `ScenarioSceneKit.cs`.

## Convenções para quem for modelar

- **Escala real, em metros.** As medidas da tabela são as do placeholder atual e já estão
  calibradas com a casa (`DC_Casa.fbx`) e a praça (`DC_Praca.fbx`) — o modelo novo deve caber
  na mesma caixa, com tolerância de ~10%.
- **Pivô na base** (no ponto que encosta na superfície), exceto onde a tabela disser outra coisa.
  Itens que o jogador pega podem ter o pivô no centro de massa, mas nunca deslocado do objeto.
- **Eixo:** exportar do Blender como FBX com Y-up, ou aceitar a rotação `(-90, 0, 0)` que o
  `ScenarioSceneKit` aplica na raiz (é o que já acontece com a casa e a praça).
- **Sem collider no FBX.** Quem monta o collider é o builder.
- **Orçamento de polígonos (Quest 3):** item de mão até ~1,5k tri, mobiliário até ~3k tri,
  construção até ~8k tri. Um atlas por conjunto, sem material por peça.
- **Nome do objeto raiz** igual ao nome atual do placeholder (coluna "Objeto"), para o builder
  trocar a primitiva pelo modelo sem mexer na lógica.

Legenda de prioridade:
**P1** = o jogador segura na mão e olha de perto · **P2** = interage mas não segura ·
**P3** = cenário, visto de longe.

---

## P1 — Itens que o jogador pega (maior impacto visual)

> **Substituídos em 17/09/2026.** Todos os itens desta tabela vêm de `Assets/Itens/` (um FBX por item,
> atlas `DC_T_Itens_Atlas`). O builder monta cada um com `ScenarioSceneKit.CreateModelItem` (item que o
> jogador pega), `CreateWrongInteractableModel` (`Notebook`, `Cobertor`) ou `CreateModelVisual`
> (`Panela`, `Jarra_Agua`, `Mochila_Kit`, `Chave`). O `BoxCollider` é calculado a partir da malha.
> A tabela fica como referência de medidas.

| Objeto | Fase | Hoje | Tamanho alvo (m) | Observações |
|---|---|---|---|---|
| `Panela` | Incêndio | cilindro achatado + cubo de cabo | Ø 0,28 × 0,12 | **Prioridade máxima.** Precisa de corpo com parede e fundo, cabo de baquelite e borda onde a tampa encaixa. Fica em cima do fogão e o fogo sai de dentro dela — deixar o interior vazado. |
| `Item_Tampa` | Incêndio | disco de Ø 0,30 | Ø 0,30 × 0,04 | Tampa da panela, com puxador central. É o item correto para abafar o fogo, então o jogador olha para ela de perto. Modelar junto com a panela, mesmo atlas. |
| `Jarra_Agua` | Incêndio | cilindro Ø 0,12 | Ø 0,14 × 0,22 | Jarra de água (ação **errada** de propósito: espalha o fogo). Vidro/plástico translúcido, com alça e água dentro. |
| `Item_Documentos` | Alagamento | cubo | 0,22 × 0,03 × 0,30 | Pasta/envelope de documentos, de preferência com RG e certidão visíveis na capa. |
| `Item_PortaRetrato` | Alagamento | cubo | 0,18 × 0,02 × 0,22 | Porta-retrato com moldura e vidro. É o item afetivo da fase. |
| `Item_Remedios` | Alagamento | cubo | 0,14 × 0,08 × 0,10 | Caixinha/nécessaire de remédios; caixa de papelão com cartela aparecendo já resolve. |
| `Item_Lanterna` | Alagamento | cilindro deitado | Ø 0,06 × 0,18 | Lanterna de mão com lente e botão. O feixe é luz do Unity, o modelo só precisa da lente. |
| `Item_Agua` | Alagamento | cilindro | Ø 0,10 × 0,26 | Garrafa de água de 2 L com rótulo. |
| `Mochila_Kit` + `Abertura` | Alagamento | cubo + cilindro | 0,34 × 0,44 × 0,20 | Mochila do kit de emergência. A `Abertura` é a boca onde os itens são depositados — modelar como bolso aberto, para o jogador entender onde soltar. |
| `Chave` | Incêndio | cubinho na porta | 0,02 × 0,05 × 0,04 | Chave na fechadura da porta de saída. |
| `Notebook`, `Cobertor` | Incêndio | cubos | 0,34 × 0,03 × 0,24 / 0,40 × 0,18 × 0,30 | Itens-isca (levar coisa material em vez de sair). Baixa prioridade dentro do P1, mas são segurados. |

## P2 — Objetos com que o jogador interage sem segurar

| Objeto | Fase | Hoje | Tamanho alvo (m) | Observações |
|---|---|---|---|---|
| `Botijao` + `Registro_Botijao` | Incêndio | cilindro + manopla | Ø 0,30 × 0,54 | Botijão P13 com regulador e mangueira. O registro precisa ser uma manopla distinta e girável. |
| `Botao_Fogao` | Incêndio | cilindro + marca | Ø 0,05 | Manípulo do fogão. Hoje é um disco com um risco; um manípulo real ajuda a ler o estado ligado/desligado. |
| `Benjamim` + `Plugue0..2` | Incêndio | cubo + cubinhos | 0,22 × 0,05 × 0,08 | Benjamim sobrecarregado (risco elétrico). Deve parecer visivelmente lotado. |
| `Disjuntor_PowerOff` (`Caixa`/`Alavanca`/`Indicador`) | todas | cubos | 0,26 × 0,34 × 0,08 | Disjuntor sobre o quadro de luz da casa. O modelo da casa tem `DC_CASA_QDL_DisjuntorGeral`, mas ele é escondido porque não tem alavanca articulada — **um disjuntor com alavanca separada resolveria os dois** (ver seção de reaproveitamento). |
| `Registro_Knob` (`Volante` + `Cano`) | Alagamento | cilindros | Ø 0,16 | Registro de água geral. Volante tipo roda de válvula. |
| `Escada` (`LateralA/B` + `Degrau0..n`) | Granizo | cubos | 0,5 × 3,4 | Escada de alumínio encostada na casa. |
| `Forro` (`Barriga` + `Trinca`) | Granizo | cubo inclinado + risco | 1,8 × 1,3 | Forro de PVC cedendo com infiltração. Uma malha levemente abaulada com trinca no UV fica muito melhor que um cubo rotacionado. |
| `Telha` (×n) | Granizo | cubos finos | 0,4 × 0,5 | Telhas quebradas no chão. Bastam 2–3 variações de caco. |

Dois placeholders desta faixa **não precisam de modelo**:
`Detector_Fumaca` só aparece se `DC_CASA_DetectorFumaca` sumir do FBX, e `Carro_PortaMotorista`
é apenas a zona de clique invisível da porta do carro (o carro é `DC_CASA_Carro`, já modelado).

## P3 — Cenário

| Objeto | Fase | Hoje | Observações |
|---|---|---|---|
| **UBS** (`PisoUBS`, `Paredes*`, `Laje`, `BancoInterno`, `Placa`) | Granizo | 8 cubos | É o abrigo seguro da fase — o jogador entra nela. Merece virar um modelo de verdade: posto de saúde simples, ~8 × 9 m, porta na face oeste. Prioridade mais alta dentro do P3. |
| `Caminhao_Bombeiros` (`Carroceria`, `Cabine`, `Vidro`, `Giroflex`, `Roda0..3`) | Incêndio | 8 primitivas | Aparece no desfecho da fase, em plano aberto. Só a silhueta de caminhão já ajuda muito. |
| `PontoOnibus` (`PosteA/B`, `Cobertura`, `Fundo`) | Granizo | 4 primitivas | Abrigo **errado** (o jogador não deve ficar ali). |
| `Barraca` (`Poste0..3`, `Balcao`, `Fundo`, `TelhadoZinco`) | Granizo | 7 primitivas | Barraca de feira com telhado de zinco — o telhado é o que faz barulho no granizo. |
| `Placa`/`PostePlaca` (outdoor) e `Torre` (`Perna*`, `Travessa`, `Braco`) | Granizo | cubos/cilindros | Outdoor e torre de transmissão vistos de longe; são a referência de "não fique embaixo". |
| `Plataforma` + `Poste` + `Placa` do ponto de encontro | Alagamento / Incêndio | cubo + cilindro + cubo | Placa de ponto de encontro. O texto é canvas, só a placa física precisa de modelo. |
| `Vaga` (`LinhaEsq/Dir`, `Placa`, `Poste`) | Granizo | cubos finos | Vaga de estacionamento. As linhas podem virar decal/textura no chão da praça. |
| `Poca`, `Gelo` (quads), fumaça (quads) | Granizo / Incêndio | quads com material | São efeitos, não modelos — resolver com shader/VFX, não com malha. |

## NPCs

> **Substituídos em 17/09/2026** pelos modelos de `Assets/NPC/` (`Carlos`, `Vizinho`, `SeuJoaquim`,
> `Andador`), com Animator `DC_NPC.controller` (Idle, Andar, Apontar) dirigido por `NpcAnimatorDriver`.
> O `CreateNpc` usa o FBX quando existe `Assets/NPC/<nome>.fbx` e cai nas primitivas só se não existir.
> Atenção: o FBX importado olha para **−Z** (o `IMPORTAR_NPC.md` diz +Z); o builder gira o modelo 180°.

Hoje todo NPC é **cápsula + esfera** (`ScenarioSceneKit.CreateNpc`): `Vizinho` (Incêndio),
`Joaquim` e `Carlos` (Granizo). O Joaquim ainda ganha uma `Boina` (cilindro) e um andador de
cilindros.

Um personagem humanoide simples, mesmo estilizado e sem rig facial, muda a percepção da cena
inteira — os NPCs falam com o jogador e são o principal ponto de contato humano do jogo.
Se houver orçamento para **um** modelo novo além da panela, é aqui.
Requisito mínimo: humanoide rigado (Mecanim), ~6k tri, com idle e gesto de apontar.

## Já modelado — não refazer

Estas peças vêm de `DC_Casa.fbx` / `DC_Praca.fbx` e já estão em uso: casa inteira (cômodos,
portas, janelas, móveis), carro, portão, quadro de luz, detector de fumaça, fogão, guarda-roupa,
TV, praça (piso, árvores, pergolado, casas vizinhas).

Duas peças do modelo estão **escondidas por código** e voltariam a ser úteis se fossem ajustadas:

- `DC_CASA_PanelaChaleira` — escondida em `IncendioGameplayBuilder.cs:61` porque a panela da
  gameplay ocupa o fogão. Se a panela nova sair boa, ela substitui esta peça e o `HidePart` some.
- `DC_CASA_QDL_DisjuntorGeral` — escondida em `ScenarioSceneKit.cs:685` porque a alavanca é
  parte da malha e não dá para animar. Separar a alavanca em um objeto filho com pivô no eixo
  resolve, e o placeholder `Disjuntor_PowerOff` some.

## Como entregar

1. FBX em `Assets/<NomeDoConjunto>/`, com as texturas ao lado.
2. Objeto raiz nomeado como na coluna "Objeto".
3. Avisar quais placeholders foram substituídos — a troca é feita no builder da fase e as
   cenas são regeneradas pelo menu `SafeZone VR > Build`.
