# Importar os itens P1 no Unity

Gerado por `blender/scripts/itens/build_itens.py` (formas em `modelos.py`,
cores e decalques em `itens_atlas.py`). **Um FBX por item**, cada um na origem.

## O que tem nesta pasta

| Arquivo | O que é |
|---|---|
| `<Objeto>.fbx` | um item por arquivo; objeto raiz com o nome do placeholder |
| `Texturas/DC_T_Itens_Atlas.png` | atlas único do conjunto (2048²) — **copie junto** |
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

### Granizo — abrigos e cenário

| Objeto | Medida | Tris | Notas |
|---|---|---:|---|
| `PontoOnibus` | 3,44 × 2,65 × 1,73 | 920 | abrigo **errado**: aberto dos lados, telha de zinco com caimento para a frente (−Z). Raiz = calçada + banco |
| ├ `PosteA` / `PosteB` | filhos | — | tubos azuis com sapata |
| ├ `Fundo` | filho | — | painel de fundo (lado +Z) |
| └ `Cobertura` | filho | — | telha ondulada; **pivô no apoio**, já inclinada 7° |
| `UBS` | 18,60 × 4,00 × 13,20 | 23372 | refeita a partir de `referencia_ubs.png`. Prédio de 14 × 10 m, porta na face **oeste (−X)**; a medida inclui calçada, rampa e marquise. O jogador entra durante o granizo |
| ├ `PisoUBS` / `Teto` / `Laje` | filhos | — | porcelanato com juntas, forro com 9 luminárias, laje com platibanda e 2 condensadoras |
| ├ `ParedeNorte/Sul/Leste/Oeste` | filhos | — | janelas **abertas** (caixilho, montante, vidro e peitoril — dá para ver a rua); faixa azul até 1,05 m por fora e por dentro, interrompida nos vãos de porta |
| ├ `Porta` / `PortaB` | filhos | — | duas folhas de vidro da entrada, **pivô na dobradiça**, abertas para dentro |
| ├ `Marquise` | filho | — | cobertura azul sobre a entrada, em duas colunas |
| ├ `Placa` | filho | — | letreiro "UBS · UNIDADE BÁSICA DE SAÚDE" na fachada |
| ├ `ParedeInterna` | filho | — | separa espera/recepção do corredor, com passagem de 1,30 |
| ├ `ParedeCorredorNorte/Sul` | filhos | — | corredor de 2,2 m com as portas dos consultórios (folha de madeira e placa) |
| ├ `Balcao` / `PainelRecepcao` | filhos | — | balcão com frente azul, monitor, painel "UBS" e placa "RECEPÇÃO" |
| ├ `BancoInterno` / `SalaEspera` | filhos | — | 3 filas de cadeiras (levemente desalinhadas) mais uma cadeira solta, painel de chamada com senha atual e próxima, totem "retire sua senha", lixeira hospitalar de pedal, álcool em gel, cartazes e vasos |
| ├ `Corredor` | filho | — | 3 cadeiras, extintor, planta e o cartaz da rota de saída |
| ├ `Consultorio1` / `Consultorio2` | filhos | — | mesa com monitor e receituário, cadeira giratória e cadeira do paciente **viradas uma para a outra**, maca com lençol e travesseiro verde-água (contraste com o colchão azul) e rolo de papel, bancada com cuba e o aparelho de pressão (mostrador virado para a porta, é o equipamento que se lê de longe), armário de 2 portas com vitrine, quadro branco, cartazes, lixeira de pedal e dispenser de álcool |
| └ `DC_LUZ_UBS_*` | 11 Empties | — | marcadores de luz: viram Point Lights *Baked* no import (forro 1900 lm / 4000 K, marquise 1500 lm / 3000 K) |

### Padrão de portas, placas e cartazes (etapa de polimento)

| | Medida |
|---|---|
| vão de passagem | 1,00 × 2,20 m (todos: consultórios, serviço e saída) |
| folha | 0,98 × 2,12 m, maçaneta a 1,05 m |
| placa de porta/parede | 0,62 × 0,17 m, base a 2,32 m do piso |
| cartaz funcional | 0,72 × 0,92 m, centro a 1,72 m |

Os três cartazes funcionais são conteúdo de jogo, não enfeite:

| Cartaz | Onde | O que diz |
|---|---|---|
| `cartaz_granizo` | sala de espera | os 5 passos do que fazer no granizo |
| `cartaz_apoio` | sala de espera | ponto de apoio da Defesa Civil, com o **logo oficial** e o 199 |
| `cartaz_rota` | corredor | planta com a rota até a saída |
| `Placa` (outdoor) | 6,20 × 4,90 × 0,84 | 568 | painel de 6,0 × 2,0 m a 4,8 m do chão, face para **−Z**, com o aviso de granizo e dois projetores |
| └ `PostePlaca` | filho | — | poste central com sapata |
| `Vaga` | 2,70 × 2,02 × 5,66 | 512 | **não é vaga pintada de rua**: é o local sugerido (2,6 × 5,2 m) onde o jogador induz o NPC a parar o carro. Cantoneiras amarelas de alvo, "P" no chão e piso de realce raso (10 mm) — dá para acender/apagar trocando o material do realce |
| ├ `LinhaEsq` / `LinhaDir` | filhos | — | tracejado lateral (sugestão, não demarcação); se virarem decal, basta apagar os dois |
| └ `Poste` / `Placa` | filhos | — | placa "P" virada para −Z |
| `Forro` | 1,88 × 0,33 × 1,38 | 560 | forro de PVC, réguas com mancha de umidade; lê-se **por baixo** |
| ├ `Barriga` | filho | — | calota da infiltração; **pivô no centro** — dá para crescer por escala conforme a água acumula |
| └ `Trinca` | filho | — | risco logo abaixo da superfície |

Todos os itens de mão estão abaixo do orçamento (1,5k tris); o caminhão (3,3k) está dentro do orçamento de construção (8k).
A **UBS (23,4k)** passa dele de propósito: é um prédio com interior completo,
não uma peça de fundo — na cena ela aparece uma vez só.

### Nomes repetidos

`Placa` e `Poste` aparecem em mais de um conjunto (`Plataforma`, `Vaga`, `UBS`
e o outdoor). Dentro de cada FBX o nome é o do placeholder; o build resolve a
colisão renomeando só na hora de exportar cada conjunto. Nenhum FBX tem collider.

## Fora da caixa de propósito

- **Panela:** o cabo passa do Ø 0,28 (o placeholder também tem o `Cabo` fora do cilindro).
- **Chave:** os 0,04 da tabela são a parte visível; a lâmina fica dentro da fechadura.

## Placeholders que estes modelos substituem

`Panela` (+ o filho `Cabo`, que some), `Item_Tampa`, `Jarra_Agua`,
`Item_Documentos`, `Item_PortaRetrato`, `Item_Remedios`, `Item_Lanterna`,
`Item_Agua`, `Mochila_Kit` + `Abertura`, `Chave`, `Notebook`, `Cobertor`,
`Botijao` + `Registro_Botijao`, `Registro_Knob` (`Volante` + `Cano`),
`Caminhao_Bombeiros` (`Carroceria`, `Cabine`, `Vidro`, `Giroflex`, `Roda0..3`)
`Plataforma` + `Poste` + `Placa`, `PontoOnibus` (`PosteA/B`, `Cobertura`,
`Fundo`), `UBS` (`PisoUBS`, `Paredes*`, `Laje`, `BancoInterno`, `Placa`),
`Placa`/`PostePlaca` (outdoor), `Vaga` (`LinhaEsq/Dir`, `Placa`, `Poste`) e
`Forro` (`Barriga` + `Trinca`).

O nome do caminhão segue o placeholder do builder: **`Caminhao_Bombeiros`**, no
plural.

Com a `Panela` nova, `DC_CASA_PanelaChaleira` pode continuar escondida
(`IncendioGameplayBuilder.cs:61`): as duas ocupariam o fogão.
