# Importar os itens P1 no Unity

Gerado por `blender/scripts/itens/build_itens.py` (formas em `modelos.py`,
cores e decalques em `itens_atlas.py`). **Um FBX por item**, cada um na origem.

## O que tem nesta pasta

| Arquivo | O que é |
|---|---|
| `<Objeto>.fbx` | um item por arquivo; objeto raiz com o nome do placeholder |
| `Texturas/DC_T_Itens_Atlas.png` | atlas único do conjunto (1024²) — **copie junto** |
| `Editor/DCItensImportador.cs` | configura import, textura e materiais sozinho |
| `DC_Itens_manifesto.json` | medidas, triângulos, filhos e materiais de cada item |

## Passo a passo

1. Copie a pasta **`Itens` inteira** para `Assets/` (ex.: `Assets/Itens/`). O
   importador só atua em caminhos que contêm `/Itens/`.
2. Se os FBX entraram antes do script compilar: selecione os FBX → **Reimport**.
3. Arraste o FBX para a cena **ou** deixe o builder da fase trocar a primitiva
   pelo modelo (o nome do objeto raiz é igual ao do placeholder).

## Materiais

Dois materiais para o conjunto inteiro, os dois sobre o mesmo atlas:

- `DC_M_Itens_Atlas` — opaco, usado por quase tudo;
- `DC_M_Itens_Translucido` — URP Transparent: corpo e água da `Jarra_Agua` e
  vidro do `Item_PortaRetrato`. É a única transparência do conjunto. Se o
  orçamento de overdraw apertar, dá para trocar esses dois por opaco sem mexer
  na malha (a água continua lendo bem).

## Itens, pivô e diferenças do placeholder

Medidas no eixo do Unity (X × Y × Z, metros). Pivô no centro da base, salvo nota.
**Os placeholders atuais têm pivô no centro** (primitivas do Unity): ao trocar,
o builder deve pôr o modelo na altura da superfície em vez de `+ meia altura`.

| Objeto | Medida | Tris | Notas |
|---|---|---:|---|
| `Panela` | 0,47 × 0,12 × 0,28 | 1012 | corpo Ø 0,28, cabo de baquelite para **+X** (mesmo lado do `Cabo` atual). Interior vazado, ressalto na borda para a tampa. Fundo interno em y = 0,007 |
| `Item_Tampa` | 0,30 × 0,044 × 0,30 | 768 | anel de encaixe por baixo (entra na boca da panela), puxador central |
| `Jarra_Agua` | 0,15 × 0,23 × 0,11 | 564 | alça em +X, bico em −X, água até 0,14 m |
| `Item_Documentos` | 0,23 × 0,02 × 0,30 | 188 | pasta com elásticos; capa com etiqueta, certidão e RG |
| `Item_PortaRetrato` | 0,18 × 0,02 × 0,22 | 316 | deitado, foto para +Y (igual ao placeholder), cavalete dobrado por baixo |
| `Item_Remedios` | 0,15 × 0,08 × 0,10 | 736 | caixa aberta em +X com duas cartelas, uma saindo |
| `Item_Lanterna` | 0,19 × 0,06 × 0,06 | 844 | **deitada ao longo de X, lente em +X** (o placeholder é cilindro girado 90° em Z: usar rotação 0) |
| `Item_Agua` | 0,10 × 0,26 × 0,10 | 960 | garrafa 2 L com rótulo nos dois lados |
| `Mochila_Kit` | 0,33 × 0,45 × 0,20 | 648 | frente (bolso) para **+Z**, alças atrás; boca aberta no topo |
| └ `Abertura` | filho | — | moldura amarela do zíper; **pivô no centro da boca** (y ≈ 0,41 do pivô da mochila) — é onde o builder põe a zona de soltar |
| `Chave` | 0,004 × 0,026 × 0,065 | 212 | **pivô na entrada da fechadura**; a cabeça sai para **−Z** (lado em que o builder põe o placeholder), a lâmina (0,025) fica para +Z, dentro da porta |
| `Notebook` | 0,34 × 0,02 × 0,24 | 384 | fechado, dobradiça em −Z, portas no lado −X |
| `Cobertor` | 0,42 × 0,18 × 0,31 | 872 | quatro dobras em xadrez |

### P2/P3 — cenário e objetos que o jogador aciona

| Objeto | Medida | Tris | Notas |
|---|---|---:|---|
| `Botijao` | 0,30 × 0,59 × 0,32 | 1552 | P13 com alça, válvula, regulador e mangueira saindo para **−Z** (frente) |
| └ `Registro_Botijao` | filho | — | manopla vermelha; **pivô no eixo do regulador** (y ≈ 0,56 do pivô do botijão), gira em torno do eixo vertical |
| `Registro_Knob` | 0,16 × 0,43 × 0,18 | 932 | registro geral de água |
| ├ `Cano` | filho | — | prumada até a válvula e o trecho que entra na parede (+Z) |
| └ `Volante` | filho | — | roda Ø 0,14; **pivô no eixo**, gira em torno do eixo vertical |
| `Caminhao_Bombeiros` | 2,76 × 2,73 × 7,64 | 3276 | frente para **−Z**; raiz = chassi e para-choques |
| ├ `Cabine` / `Vidro` / `Giroflex` | filhos | — | vidro no material translúcido; giroflex com 6 lentes |
| ├ `Carroceria` | filho | — | armários, corrimão, carretel e a faixa "CORPO DE BOMBEIROS 193" |
| └ `Roda0..3` | filhos | — | pivô no centro de cada roda (dianteiras 0/1, traseiras 2/3) — dá para girar |
| `Plataforma` | 2,64 × 2,47 × 2,64 | 496 | ponto de encontro: laje com faixa amarela |
| ├ `Poste` | filho | — | tubo Ø 0,10, base na laje |
| └ `Placa` | filho | — | 1,00 × 0,75, face verde para **−Z**; o canvas do jogo entra por cima dela |

Todos os itens de mão estão abaixo do orçamento (1,5k tris); o caminhão (3,3k)
está dentro do orçamento de construção (8k). Nenhum FBX tem collider.

## Fora da caixa de propósito

- **Panela:** o cabo passa do Ø 0,28 (o placeholder também tem o `Cabo` fora do cilindro).
- **Chave:** os 0,04 da tabela são a parte visível; a lâmina fica dentro da fechadura.

## Placeholders que estes modelos substituem

`Panela` (+ o filho `Cabo`, que some), `Item_Tampa`, `Jarra_Agua`,
`Item_Documentos`, `Item_PortaRetrato`, `Item_Remedios`, `Item_Lanterna`,
`Item_Agua`, `Mochila_Kit` + `Abertura`, `Chave`, `Notebook`, `Cobertor`,
`Botijao` + `Registro_Botijao`, `Registro_Knob` (`Volante` + `Cano`),
`Caminhao_Bombeiros` (`Carroceria`, `Cabine`, `Vidro`, `Giroflex`, `Roda0..3`)
e `Plataforma` + `Poste` + `Placa`.

O nome do caminhão segue o placeholder do builder: **`Caminhao_Bombeiros`**, no
plural.

Com a `Panela` nova, `DC_CASA_PanelaChaleira` pode continuar escondida
(`IncendioGameplayBuilder.cs:61`): as duas ocupariam o fogão.
