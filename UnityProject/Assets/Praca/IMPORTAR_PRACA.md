# Importar a praça no Unity

Gerado por `blender/scripts/praca/90_export_praca.py` a partir de
`blender/scenes/DC_Praca.blend`.

## O que tem nesta pasta

| Arquivo | O que é |
|---|---|
| `DC_Praca.fbx` | a praça inteira, com hierarquia por grupo |
| `DC_Praca.fbm/` | logo oficial da Defesa Civil (painel e totem) — **copie junto com o FBX** |
| `Editor/DCPracaImportador.cs` | configura o import sozinho |
| `DC_Praca_manifesto.json` | objetos, triângulos, materiais, objetos de jogo |

**127 objetos · 33.360 triângulos · 37 materiais · 9 objetos de jogo**

## Passo a passo

1. A pasta **`Casa`** precisa já estar em `Assets/` (a praça usa o componente
   `DCInteracao` de `Casa/Runtime`).
2. Copie a pasta **`Praca` inteira** para `Assets/`. Se o FBX entrou antes dos
   scripts compilarem: botão direito em `DC_Praca.fbx` → **Reimport**.
3. Arraste `DC_Casa.fbx` **e** `DC_Praca.fbx` para a cena, os dois em
   Position (0, 0, 0). Eles saem no mesmo referencial: a praça fica do outro
   lado da rua, com a faixa de pedestre ligando o caminho central ao portão
   da casa. **Não mova um sem o outro.**
4. **URP:** se os materiais vierem rosa, `Edit → Rendering → Materials →
   Convert Selected Built-in Materials to URP`.
5. Directional Light para o sol (dia) e **Generate Lighting**.
6. Se o céu padrão cortar as montanhas/nuvens, aumente o **Far Clip Plane**
   da câmera do XR Origin para ~600 m (montanhas a ~190 m, nuvens a ~320 m).

## Conferência de escala

| Objeto | Deve medir (largura × altura × profundidade) |
|---|---|
| `DC_PRACA_Piso` | 22,00 × 0,13 × 15,70 m |
| `DC_PRACA_TotemDefesaCivil` | 1,70 × 2,30 × 0,62 m |
| `DC_PRACA_Poste0` | 0,40 × 4,32 × 0,40 m |

Conferido reimportando o próprio FBX no Blender.

## Hierarquia

```
DC_PRACA
├── DC_GRUPO_Chao         piso, juntas, meio-fio, rua lateral, faixa, piso tátil, canteiros, muretas, muro
├── DC_GRUPO_Estruturas   pergolado, painel educativo, parquinho, totem
├── DC_GRUPO_Mobiliario   bancos, postes, lixeiras, jardineiras
├── DC_GRUPO_Vegetacao    árvores e plantas dos canteiros
└── DC_GRUPO_Entorno      casas vizinhas, chão distante, montanhas, nuvens
```

## Objetos de jogo

| Objeto | `id` | Para quê |
|---|---|---|
| `DC_PRACA_PainelEducativo` (sob o pergolado) | `painel_educativo` | conteúdo educativo |
| `DC_PRACA_TotemDefesaCivil` | `totem_defesa_civil` | início/orientação de missão |
| `DC_PRACA_Arvore*` (7) | `abrigo_inadequado` | granizo: árvore **não** é abrigo, ir para casa |

O pergolado também é abrigo inadequado (ripas abertas); marcar por nome se a
missão precisar.

## Para o XR Interaction Toolkit

- **Teletransporte:** `DC_PRACA_Piso`, `DC_PRACA_PisoEmborrachado`, gramas dos
  canteiros e a rua e a calçada (`DC_CASA_Rua` e `DC_CASA_Calcada` vêm da casa). `MeshCollider` +
  `TeleportationArea`.
- **Colisão:** não é gerada. BoxCollider em muretas, muro, bancos, postes,
  totem, pilares do pergolado e troncos.

## Pendências conhecidas

- **37 materiais** (casa 132): atlas de paleta fica para a etapa de otimização.
- Posições de spawn e marcadores de missão: ajustar depois.
