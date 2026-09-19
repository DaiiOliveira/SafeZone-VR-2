# Importar os NPCs no Unity

Gerado por `blender/scripts/npc/build_npc.py` (corpo e roupa em
`personagens.py`, esqueleto e animações em `npc_rig.py`, cores em
`npc_paleta.py`). **Um FBX por personagem.**

> Os scripts C# desta pasta foram escritos, mas **não foram executados no
> Unity**. A conferência foi feita reimportando os FBX no Blender (medidas,
> ossos e clipes, ver `DC_NPC_manifesto.json`).

## O que tem nesta pasta

| Arquivo | O que é |
|---|---|
| `Carlos.fbx`, `Vizinho.fbx`, `SeuJoaquim.fbx` | malha + esqueleto Humanoid + clipes `Idle`, `Apontar`, `Andar` |
| `Andador.fbx` | andador do Seu Joaquim (prop, sem pele) |
| `Texturas/DC_T_NPC_Atlas.png` | paleta 256² de cores chapadas — **copie junto** |
| `Editor/DCNpcImportador.cs` | Humanoid, nomes e loop dos clipes, textura e material |
| `Editor/DCNpcAnimator.cs` | menu **SafeZone VR > NPC > Criar Animator Controller** |
| `DC_NPC_manifesto.json` | medidas, triângulos, ossos e clipes lidos do FBX exportado |

## Passo a passo

1. Copie a pasta **`NPC` inteira** para `Assets/`.
2. Se os FBX entraram antes do script compilar: selecione os FBX → **Reimport**.
3. Confira em cada FBX, aba **Rig**: `Humanoid`, avatar válido (✓ verde).
4. Rode **SafeZone VR > NPC > Criar Animator Controller** → `NPC/DC_NPC.controller`.
5. No `CreateNpc`, troque cápsula + esfera pelo FBX e adicione um `Animator`
   com esse controller.

## Personagens

| Objeto | Altura | Tris | Visual |
|---|---:|---:|---|
| `Carlos` | 1,77 m | 2.132 | polo verde (cor do placeholder), jeans, tênis, cabelo curto e barba |
| `Vizinho` | 1,71 m | 2.020 | camiseta cor de telha (cor do placeholder), bermuda, chinelo, calvo, bigode |
| `SeuJoaquim` | 1,72 m (com boina) | 2.472 | cardigã (cor do placeholder), camisa clara, boina, óculos, bigode branco, postura curvada |
| `Andador` | 0,88 m | 448 | alumínio, pegas de borracha, travessa na frente |

Todos bem abaixo dos ~6 mil tris pedidos. Um material (`DC_M_NPC_Atlas`) por personagem.

- **Pivô nos pés**, igual ao `feetPos` do `CreateNpc`. Frente para **+Z**.
- **Boina** faz parte da malha do Joaquim: a primitiva `Boina` do builder sai.
- **Andador**: filho do Seu Joaquim em **posição local (0, 0, 0,25)** (o
  placeholder usava 0,35). Os clipes do Joaquim põem as mãos nas pegas a
  essa distância. Origem do andador no chão, entre os pés de trás.
- Colisor e balão de fala continuam como estão (o builder já cria).

## Esqueleto

27 ossos com nomes do mapeamento automático do Humanoid: `Hips`, `Spine`,
`Chest`, `Neck`, `Head`, `Left/RightShoulder`, `UpperArm`, `LowerArm`, `Hand`,
`UpperLeg`, `LowerLeg`, `Foot`, `Toes` e três dedos por mão
(`IndexProximal`, `MiddleProximal` = os outros três dedos juntos,
`ThumbProximal`). Pose de repouso em T.

## Clipes (30 fps)

| Clipe | Frames | Loop | O que faz |
|---|---:|---|---|
| `Idle` | 0–90 | sim | respiração, cabeça olhando em volta; Joaquim apoiado no andador |
| `Apontar` | 0–66 | não | levanta o braço direito e aponta com o indicador (pico no frame ~20, segura até 50, volta) |
| `Andar` | 0–30 (Joaquim 0–40) | sim | caminhada **no lugar**; quem desloca é o `CompanionFollower`. Joaquim anda em passos curtos com as mãos no andador |

`Andar` não estava na lista do `.md`, mas o Vizinho e o Joaquim seguem rota no
jogo; sem ele, deslizariam parados.

Velocidade que casa com o passo: ~0,9 m/s (Vizinho/Carlos) e ~0,5 m/s
(Joaquim; o builder usa 0,8 — se o pé parecer escorregar, baixe a `speed` ou
aumente `Animator.speed`).

## Placeholders que estes modelos substituem

`CreateNpc` → cápsula `Corpo` + esfera `Cabeca` (os três NPCs), `Boina`
(Joaquim) e o `Andador` de cilindros (`Perna0..3` + `Barra`).
