# Importar a casa no Unity

Gerado por `blender/scripts/casa/90_export_casa.py` a partir de
`blender/scenes/DC_Casa.blend`.

## O que tem nesta pasta

| Arquivo | O que é |
|---|---|
| `DC_Casa.fbx` | a casa inteira, com hierarquia por grupo |
| `DC_Casa.fbm/` | texturas dos tapetes e quadros — **copie junto com o FBX** |
| `Editor/DCCasaImportador.cs` | configura o import sozinho (ver abaixo) |
| `Runtime/DCInteracao.cs` | componente dos objetos de jogo |
| `DC_Casa_manifesto.json` | objetos, triângulos, luzes, objetos de jogo, portas |

**356 objetos · 37.448 triângulos · 132 materiais · 22 luzes · 3 objetos de jogo · 7 portas**

## Passo a passo

1. Copie a pasta **`Casa` inteira** para dentro de `Assets/` do projeto
   (FBX, `.fbm`, `Editor` e `Runtime` juntos). O Unity compila os scripts e
   importa o FBX já configurado — não precisa mexer no Inspector.
   > Se o FBX entrou **antes** dos scripts compilarem, clique com o botão
   > direito em `DC_Casa.fbx` → **Reimport**.
2. Arraste `DC_Casa.fbx` para a cena.
3. **Confira a escala antes de tudo** (seção abaixo).
4. **URP:** se os materiais vierem rosa, `Edit → Rendering → Materials →
   Convert Selected Built-in Materials to URP` com os materiais selecionados.
5. **Luz:** abra `Window → Rendering → Lighting`, crie um Lighting Settings,
   e clique **Generate Lighting**. As 22 luzes já vêm como *Baked* e a malha
   já vem estática.
6. Adicione uma Directional Light para o sol (o FBX não traz o sol de
   conferência do Blender).

## Conferência de escala e eixo

| Objeto | Deve medir (largura × altura × profundidade) |
|---|---|
| `DC_CASA_Piso` | 11,10 × 0,15 × 10,70 m |
| `DC_CASA_Carro` | ~1,86 × 1,49 × 4,23 m |
| `DC_CASA_Porta_NichoFundo0` | 0,90 × 2,05 × 0,04 m |

O objeto raiz `DC_CASA` deve estar em Rotation (0, 0, 0) e Scale (1, 1, 1).
Esses números foram conferidos reimportando o próprio FBX no Blender; se no
Unity a casa vier 100× menor ou deitada, confira no Inspector do FBX
(aba Model) se *Convert Units* está ligado e *Bake Axis Conversion*
desligado — é o que o importador define.

## Hierarquia

```
DC_CASA
├── DC_GRUPO_Terreno      lote, rua, calçada, muros, portões, vizinhos
├── DC_GRUPO_Casca        piso, paredes, forro, garagem, calha e condutores
├── DC_GRUPO_Telhado      telhado, oitões, tabeira   ← desligue para ver dentro
├── DC_GRUPO_Esquadrias   portas, janelas, batentes, maçanetas
├── DC_GRUPO_Mobilia
│   ├── DC_GRUPO_Sala / Cozinha / Quartos / Banheiro / Corredor / Garagem
├── DC_GRUPO_Externos     árvores, arbustos, postes, fiação
│   └── DC_GRUPO_Quintal  jardim, deck, varal, cerca viva, casinha
└── DC_GRUPO_Luzes        marcadores DC_LUZ_* (viram Point Lights)
```

## O que o importador faz sozinho

| | |
|---|---|
| **Model** | escala 1, sem luzes/câmeras do FBX, sem compressão, **UV2 para lightmap** gerado |
| **Luzes** | cada `DC_LUZ_*` vira Point Light *Baked*, com cor pela temperatura (K) e intensidade por `lumens / 700` — ajuste a constante `LUMENS_POR_UNIDADE` depois do primeiro bake |
| **Objetos de jogo** | recebem o componente `DCInteracao` com `id` e `estado` |
| **Static** | toda malha fica estática (GI, batching, occlusion, reflection probe), **menos** folhas de porta, portinhola do quadro e disjuntor |
| **Portas** | a maçaneta vira filha da folha: abriu a porta, ela vai junto. A origem da folha já está na dobradiça |
| **Emissão** | materiais de lâmpada, cúpula, tela e LED ganham emissão *Baked* |

## Objetos de jogo

| Objeto | `id` | Para quê |
|---|---|---|
| `DC_CASA_QDL_DisjuntorGeral` | `disjuntor_geral` (estado `ligado`) | desligar a energia no alagamento |
| `DC_CASA_QDL_Porta` | `porta_quadro_luz` | portinhola do quadro, gira pela dobradiça |
| `DC_CASA_DetectorFumaca` | `detector_fumaca` | cenário de incêndio |

Folhas de porta (giram pela dobradiça): `DC_CASA_Porta_NichoFundo0` (entrada),
`FrenteCorredor0/1`, `CorredorFundo0/1/2`, `Direita1` (cozinha → quintal).

## Para o XR Interaction Toolkit

- **Chão para teletransporte:** `DC_CASA_Piso*`, `DC_CASA_PisoSala_A/B`,
  `DC_CASA_PisoCozinha`, `DC_CASA_PisoQuarto1/2`, `DC_CASA_PisoBanheiro`,
  `DC_CASA_PisoCorredor`, `DC_CASA_Grama`, `DC_CASA_Deck`. Adicione
  `MeshCollider` e `TeleportationArea`.
- **Colisão:** o importador **não** gera colliders (em Quest, MeshCollider na
  casa inteira custa caro). Use BoxCollider nas paredes e móveis que o
  jogador pode bater.
- **Portas e disjuntor:** `XRSimpleInteractable` ou `XRGrabInteractable` com
  `HingeJoint` na folha.

## Pendências conhecidas

- **132 materiais.** Meta para Quest 3 é ≤ 10 por cena visível: é a etapa de
  atlas/otimização que ficou para depois. Para testar, funciona.
- **Queda de energia no alagamento:** com luz *Baked*, apagar exige dois
  bakes (acesa/apagada) trocados por script, ou deixar essas luzes *Mixed*.
  Decidir antes do bake final.
- **Forro do beiral** aberto nas laterais longas do telhado.
