# Cenário: Incêndio em Casa

> Documento de especificação para implementar a fase **Incêndio em Casa** do SafeZone VR.
> Segue a arquitetura já usada em *Alagamento em Casa*: conteúdo em ScriptableObjects, validadores
> sobre eventos do XR Interaction Toolkit e cena gerada por um builder de Editor.
> O documento é **autossuficiente**: a seção 9 traz todo o código novo necessário, inclusive a
> infraestrutura compartilhada com a fase *Granizo*. Se ela já tiver sido criada, apenas reutilize.

| Item | Valor |
|------|-------|
| `scenarioId` | `incendio` |
| Nome no menu | Incêndio em Casa |
| Asset do cenário | `Assets/SafeZoneVR/Data/Scenario_Incendio.asset` (novo) |
| Cena | `Assets/Scenes/Fases/Incendio_EmCasa.unity` (`sceneName = "Incendio_EmCasa"`) |
| Duração alvo | 3 estrelas ≤ **300 s** sem erros · 2 estrelas ≤ **480 s** com até 2 penalidades |
| Passos | 8, em dois momentos: um princípio de incêndio controlável e um incêndio que sai do controle |
| Classificação | Livre: chamas pequenas e fumaça cinza; ninguém se machuca, sem queimaduras, sem pessoas presas |

### Fontes oficiais

O repositório ainda não tem cartilha de incêndio residencial. As cartilhas da Defesa Civil em
`Documentos_Orientações_Defesa_Civil/` tratam de desastres naturais; a de baixa umidade do ar só cita
queimadas e incêndio em mata. Para incêndio **dentro de casa**, a orientação oficial vem dos **Corpos de
Bombeiros Militares**. Esta fase usa estas páginas:

| Sigla usada neste documento | Fonte |
|------------------------------|-------|
| **CBMSC** | Corpo de Bombeiros Militar de Santa Catarina — [Incêndio em Edificação](https://www.cbm.sc.gov.br/index.php/dicas-de-prevencao/incendio-em-edificacao) |
| **CBMAL** | Corpo de Bombeiros Militar de Alagoas — [Incêndio](https://www.bombeiros.al.gov.br/paginas/view/40/inc%C3%AAndio) |
| **CBM/Vitória** | Prefeitura de Vitória — [Corpo de Bombeiros: instruções em caso de incêndio](https://m.vitoria.es.gov.br/semob/corpo-de-bombeiros-instrucoes-em-caso-de-incendio) |
| **CBMCE** | Governo do Ceará — [Corpo de Bombeiros orienta sobre o que fazer caso panela com óleo pegue fogo](https://www.ceara.gov.br/2022/04/01/corpo-de-bombeiros-orienta-sobre-o-que-fazer-caso-panela-com-oleo-pegue-fogo/) |

**Antes de publicar a fase:**
1. Salve essas páginas em PDF dentro de `Documentos_Orientações_Defesa_Civil/` (ex.: `incendio_residencial_bombeiros.pdf`).
2. Peça a revisão do texto a um oficial do Corpo de Bombeiros ou da Defesa Civil local, seguindo o princípio de **fidelidade às fontes oficiais** do projeto.

---

## 1. Objetivo pedagógico

A fase ensina a diferença entre dois momentos:
1. **Princípio de incêndio controlável:** uma panela com óleo pega fogo no fogão. A atitude certa é
   fechar o gás, **abafar com a tampa, nunca jogar água**, ventilar e esperar esfriar.
2. **Incêndio que sai do controle:** um benjamim ("T") sobrecarregado atrás da TV começa a soltar
   faíscas e fumaça. A atitude certa é **não jogar água em aparelho energizado** e desligar a energia.
   Quando o fogo cresce: **não combater sozinho**, sair **abaixado** por baixo da fumaça, **fechar a porta
   sem trancar**, **não salvar objetos**, ligar **193** depois de sair e **não deixar ninguém entrar**.

A lição que atravessa a fase é: **tente apagar só o que é pequeno e está sob controle; se não conseguir,
saia.**

## 2. Base oficial → o que vira jogo

| Orientação oficial | Fonte | Momento | Como aparece no jogo |
|--------------------|-------|---------|----------------------|
| Fogo em panela: desligue o registro do fogão e feche o registro do botijão | CBMCE, CBMSC | Durante (princípio) | **Missão 1**: fechar o botão do fogão e o registro do botijão (2 de 2) |
| Abafe a panela com a tampa ou com um pano úmido e torcido; aguarde esfriar | CBMCE, CBMSC | Durante (princípio) | **Missão 2**: encaixar a tampa na panela |
| **Nunca jogue água** em óleo quente: a água evapora e espalha o óleo em chamas | CBMCE, CBMSC | Durante (princípio) | Decisão insegura: jarra de água ao lado da pia |
| Se levantar a tampa cedo, o fogo pode voltar | CBMCE | Durante (princípio) | Decisão insegura: tirar a tampa antes de 45 s |
| Mantenha portas e janelas abertas para ventilar: a fumaça é tóxica | CBMSC (fogo em panela) | Durante (princípio) | **Missão 3**: abrir a janela da cozinha |
| Aparelho elétrico: **nunca jogue água** em aparelho energizado; desligue a rede elétrica | CBMSC | Durante (incêndio) | **Missão 4**: desligar o quadro de energia; a jarra perto da TV é erro |
| Apague só **princípios de incêndio**; se não conseguir, abandone a edificação rapidamente; não combata o incêndio sozinho | CBMSC, CBM/Vitória | Durante (incêndio) | O fogo da TV cresce e o relógio orienta a sair |
| Evite a fumaça: ande **agachado**, porque a fumaça fica no alto; use pano úmido no nariz | CBMSC, CBMAL, CBM/Vitória | Durante (incêndio) | **Missão 5**: sair da casa abaixado pelo corredor com fumaça |
| Saia fechando as portas atrás de si, **sem trancá-las** | CBMAL, CBM/Vitória | Durante (incêndio) | **Missão 6**: fechar a porta da frente por fora; a chave é decisão insegura |
| Não perca tempo tentando salvar objetos | CBMAL, CBM/Vitória | Durante (incêndio) | Decisões inseguras: "Salvar o notebook?" e "Tirar a TV?" |
| Depois de sair, ligue **193** | CBMSC, CBMAL | Depois de sair | **Missão 7**: ligar 193 pelo relógio |
| Não permita que pessoas entrem no local em chamas | CBMSC | Depois de sair | **Missão 8**: impedir o vizinho de entrar para "pegar as coisas" |
| Se ficar preso: fique perto de uma janela e sinalize; feche, sem trancar, a porta do cômodo; vede as frestas; fique junto ao chão com pano molhado no nariz e na boca | CBMAL | — | Card (a fase não coloca o jogador preso, por tom) |
| Extintor: use o da classe certa; ataque a base do fogo em leque; não use extintor de água em eletrônicos ou líquidos inflamáveis | CBMSC | — | Card (a casa não tem extintor; o foco é saber quando **não** combater) |
| Prevenção: não usar "T"/benjamim nem ligar vários aparelhos numa tomada; evitar gambiarras; desligar aparelhos ao sair; não usar notebook sobre cama ou sofá; não abandonar o fogão; manutenção do gás; velas longe de inflamáveis (prefira lanternas); tirar o ferro da tomada; inflamáveis longe do calor; apagar bem o cigarro | CBMSC | Antes | Cards + detalhes visuais etiquetados na casa (benjamim lotado, notebook no sofá, álcool perto do fogão) |

Telefones exibidos: **Bombeiros 193** · Defesa Civil 199 · SAMU 192 · Polícia Militar 190.

## 3. Premissa e tom

**Briefing (texto do `IntroBriefingPanel`):**

> Fim de tarde. Você esquentou óleo para fritar e foi até a sala por um instante. Ao voltar, a panela
> **pegou fogo**. O fogo ainda é pequeno: é um **princípio de incêndio**.
> Mantenha a calma. Faça o que os Bombeiros orientam e, se o fogo sair do controle, saia de casa.
> O relógio no pulso esquerdo mostra cada missão e permite ligar para o 193.

**Tom (RNF03):** urgência controlada, com instruções calmas.
- **Chamas:** pequenas, estilizadas e sem gritos.
- **Fumaça:** cinza e visível, criando a urgência.
- **Alarme:** um detector de fumaça apita de forma suave e intermitente, sem som estridente.
- **Na parte 2:** o fogo no rack da TV cresce, mas nunca alcança o jogador. A fumaça desce até a altura do peito, e abaixar-se deixa a visão limpa, o que ensina o gesto certo pela própria experiência.
- **No final:** os Bombeiros chegam, com sirene distante e caminhão parado na rua. O relatório afirma que todos saíram em segurança.

Proibido:
- queimaduras, pessoas feridas ou presas;
- roupas em chamas;
- explosão, a não ser um "puff" breve e controlado se o jogador jogar água no óleo (sempre longe da câmera);
- tremor de câmera e tela vermelha.

## 4. Ambiente

### 4.1 Reaproveitamento da casa

A casa do Alagamento é reaproveitada na mesma posição (prefab `Casa_SafeZone`, seção 9.1). A cozinha
fica na parede oeste (x ≈ −35.4, z 9..13), o quadro de energia no quarto `(-29.68, 1.50, 6.22)` e a
porta da frente em x −32.95..−31.81, z 14.9.

| Elemento | Posição (metros, mundo) | Detalhes |
|----------|-------------------------|----------|
| Início do jogador | `(-32.8, 0, 11.6)` (sala), olhando para a cozinha (−X) | Vê a panela em chamas a ~2,5 m |
| Fogão e panela | `Fogao` (x −35.49, z 12.49); panela no queimador da frente (y ≈ 0.93) | `FireController` "Panela" (pequeno) |
| Botão do fogão | frente do fogão (x −35.15, y 0.85, z 12.35) | `XRKnob` pequeno |
| Botijão e registro | ao lado do fogão, dentro do balcão (x −35.3, y 0.25, z 13.6) | `XRKnob` (volante amarelo) |
| Tampa da panela | sobre o balcão (x −35.45, y 0.92, z 11.7) | `XRGrabInteractable` + `ObjectiveItemId = "tampa_panela"` |
| Jarra de água | ao lado da pia (x −35.4, y 0.95, z 10.4) | `WrongActionInteractable` (`agua_no_oleo`) |
| Janela da cozinha | parede oeste (x −35.9, y 1.4, z 11) | `ToggleOpening`, **começa fechada** |
| TV, rack e benjamim | `TV` (x −29.37, z 12.18); benjamim com 5 plugues no chão atrás do rack (x −29.2, y 0.05, z 12.6) | `FireController` "Rack" (começa desligado) |
| Quadro de energia | igual ao Alagamento | `BreakerLever` |
| Corredor com fumaça | da sala à porta da frente (caixa x −34..−31, z 10..14.9) | `SmokeLayer` desce de 2,0 m para 1,15 m |
| Notebook no sofá | x −31.6, y 0.6, z 11.9 | `WrongActionInteractable` ("Salvar o notebook?") |
| Porta da frente | igual ao Alagamento | `ToggleOpening` (começa fechada) + chave na fechadura (`WrongActionInteractable`, `trancar_porta`) |
| Ponto de encontro | calçada, em frente ao portão (x −33, z 19) | Placa simples "PONTO DE ENCONTRO" |
| Vizinho | chega pela calçada (x −26, z 19) e tenta ir para o portão depois da missão 7 | NPC (`CompanionFollower` reaproveitado para andar) |

### 4.2 Estados do ambiente (`ScenarioPhaseSwitcher` + `FireController` + `SmokeLayer`)

| Fase | Quando | Fogo | Fumaça | Outros |
|------|--------|------|--------|--------|
| **Panela** | início | panela: pequeno, estável | fina, sobre o fogão | detector apitando suave |
| **Panela apagada** | ao concluir a missão 2 | panela: apaga (fumaça residual) | dissipa em 20 s se a janela estiver aberta, em 40 s se estiver fechada | detector para quando a fumaça some |
| **Rack** | ao concluir as missões 1–3 (sem fade) | benjamim solta faíscas (2 s) → fogo no rack, que cresce de pequeno para médio em 20 s e de médio para grande em mais 25 s | `SmokeLayer` desce de 2,0 m para 1,15 m em 40 s na sala e no corredor | detector volta; o relógio mostra: "Faíscas no benjamim atrás da TV! Não use água em aparelhos ligados." |
| **Fora de casa** | ao concluir a missão 5 | continua dentro de casa, visto pela janela | escapa pela janela da cozinha | na missão 7, sirene distante; na missão 8, o caminhão dos Bombeiros estaciona na rua |

### 4.3 Orçamento de desempenho (Quest 3, 72 FPS)

- **Chamas:**
  - partículas URP Unlit aditivas, com até 60 por foco;
  - uma única `Light` pontual por foco ativo, sem sombra e com cintilação por script a cada 0,05 s;
  - nunca mais que 2 luzes extras ao mesmo tempo.
- **Fumaça:**
  - `SmokeLayer` com 3 a 4 quads grandes empilhados e material transparente com ruído rolando no UV;
  - mais a **névoa do URP** (`RenderSettings.fog`), ligada **só** com a cabeça dentro da camada, na cor cinza e em modo exponencial.
  - Não use volumetria nem partículas enchendo o cômodo, porque a sobreposição de transparência pesa no Quest.
- **Áudio:** fogo crepitando (1 fonte 3D por foco), detector de fumaça (1 fonte) e sirene distante (1 fonte, na fase final).
- **Meta:** até 150 draw calls e 250k triângulos na visão. Atenção ao *overdraw*: no máximo 4 camadas transparentes na frente da câmera.

---

## 5. Missões

Os textos abaixo vão direto para os `MissionStepSO` (título, instrução do relógio, `locationHint` e `whyItMatters`).

| # | `stepId` | Título | Instrução (relógio) | Onde | Por que importa (aparece ao concluir) | Validação |
|---|----------|--------|---------------------|------|----------------------------------------|-----------|
| 1 | `inc_01_gas` | Feche o gás | Gire o botão do fogão para desligar e feche o registro do botijão, ao lado do fogão. | Cozinha | Sem gás, o fogo da panela deixa de ser alimentado. É o primeiro passo indicado pelos Bombeiros. | `StepCompleteOnAllConditions` (`knobsTurned` = botão do fogão + registro do botijão); progresso "1 de 2" |
| 2 | `inc_02_tampa` | Abafe a panela com a tampa | Pegue a tampa no balcão e coloque sobre a panela. Nunca jogue água no óleo. | Cozinha | Sem ar, o fogo apaga. Água em óleo quente vira vapor na hora e espalha o óleo em chamas. | `StepCompleteOnSocket` (socket da panela, `requiredItemIds = ["tampa_panela"]`); o encaixe chama `FireController.Extinguish()` da panela |
| 3 | `inc_03_ventilar` | Ventile e deixe esfriar | Abra a janela da cozinha e não tire a tampa: deixe a panela esfriar. | Cozinha | A fumaça é tóxica: ventilar ajuda a tirá-la de casa. Se a tampa sair cedo, o fogo pode voltar. | `StepCompleteOnOpenings` (`targetOpen = true`, janela da cozinha) |
| 4 | `inc_04_energia` | Desligue a energia | Faíscas atrás da TV! Não jogue água. Vá ao quarto e abaixe a alavanca do quadro de energia. | Quarto | Fogo em aparelho elétrico nunca se apaga com água. Desligar a energia corta a fonte das faíscas. | `StepCompleteOnBreaker` (já existe), com o quadro de energia |
| 5 | `inc_05_sair` | Saia abaixado | O fogo cresceu: não tente apagar sozinho. Abaixe-se por baixo da fumaça e saia pela porta da frente. | Corredor → porta da frente | A fumaça tóxica sobe e fica no alto; perto do chão o ar é mais limpo. Se não conseguir apagar, saia rapidamente. | `StepCompleteOnTriggerZone` no quintal da frente (x −34.5..−31, z 15.3..17.5). Ficar em pé na fumaça gera `em_pe_na_fumaca` |
| 6 | `inc_06_porta` | Feche a porta sem trancar | Já do lado de fora, feche a porta da frente. Não tranque: os Bombeiros podem precisar passar. | Porta da frente | A porta fechada segura o fogo e a fumaça lá dentro; destrancada, não atrapalha quem precisa entrar ou sair. | `StepCompleteOnOpenings` (`targetOpen = false`, porta da frente, `requirePlayerInZone` = quintal da frente) |
| 7 | `inc_07_193` | Ligue para os Bombeiros | Na calçada, em segurança, ligue 193 pelo relógio. | Ponto de encontro (calçada) | Ligar 193 depois de sair garante o socorro sem que você fique perto do fogo. | `EmergencyCallPanel` aceita **só** `193` → `StepCompleteOnButtonPress.OnButtonPressed()` |
| 8 | `inc_08_vizinho` | Não deixe ninguém entrar | O vizinho quer entrar para salvar a TV. Aponte para ele e aperte o gatilho para impedi-lo. | Portão da frente | Nenhum objeto vale o risco. Não permita que ninguém entre em um local em chamas: espere os Bombeiros. | `StepCompleteOnInteractCount` (1: o vizinho); o mesmo evento faz o `CompanionFollower` levá-lo ao ponto de encontro |

**Ordem e penalidade:** o `ScenarioManager` conta "fora de ordem" como penalidade. A cartilha do CBMCE
indica gás e depois tampa, mas as duas ações apagam o fogo e nenhuma ordem entre elas é perigosa. Por
isso as missões **1 e 2** formam o **grupo de ordem 1** (seção 9.2). As demais seguem a ordem da tabela.

**Transições de fase** (`ScenarioPhaseSwitcher`):
- **Panela → Rack:** dispara quando as missões 1–3 estiverem concluídas, sem fade.
  - O benjamim solta faíscas e o fogo do rack acende pequeno.
  - O detector volta a apitar e o `SmokeLayer` começa a descer.
  - O relógio avisa: "Faíscas no benjamim atrás da TV! Não use água em aparelhos ligados."
- **Ao concluir a missão 4:** o fogo do rack **não apaga**. Ele já pegou no móvel e cresce para "grande".
  - O relógio orienta: "O fogo cresceu. Não tente apagar sozinho: abaixe-se e saia de casa."
  - É a demonstração de "se não conseguir extinguir, abandone o local".
- **Ao concluir a missão 5:** a zona `voltar_para_dentro` é ligada (`ActivateOnStepCompleted`).
- **Ao concluir a missão 7:**
  - Toca uma sirene distante.
  - O vizinho aparece pela calçada, em direção ao portão.
  - O relógio mostra: "Vizinho: vou lá dentro pegar sua TV!"
- **Ao concluir a missão 8:** o caminhão dos Bombeiros estaciona na rua e a fase termina com o relatório.

**Falas da central (missão 7):**
- 193: "Corpo de Bombeiros. Endereço anotado, uma viatura está a caminho. Todos saíram? Não entre na casa e mantenha as pessoas afastadas."
- 199: "Defesa Civil: vamos acionar os Bombeiros, mas em caso de incêndio ligue direto para o 193." Não conclui a missão e **não** é erro.
- 190: "Polícia Militar: vamos repassar. Para incêndio, o número é 193." Não conclui a missão.
- 192: "SAMU: se alguém estiver passando mal, estamos à disposição. Para o incêndio, ligue 193." Não conclui a missão.

## 6. Decisões inseguras (não encerram a fase, geram aviso e contam no relatório)

| `mistakeId` | Gatilho | Ativo quando | Mensagem |
|-------------|---------|--------------|----------|
| `agua_no_oleo` | `WrongActionInteractable` na jarra de água ao lado da pia; o fogo da panela dá um "puff" rápido, sem atingir ninguém | fase Panela, com o fogo aceso | Nunca jogue água em óleo quente: a água vira vapor na hora e espalha o óleo em chamas. Abafe com a tampa. |
| `tampa_cedo` | `LidCooldown`: tirar a tampa antes de 45 s reacende um fogo pequeno | depois da missão 2 | Não levante a tampa logo: espere a panela esfriar, senão o fogo pode voltar. |
| `agua_no_aparelho` | a mesma jarra; o `onEnter` da fase Rack troca `mistakeId` e `message` do componente | fase Rack | Nunca jogue água em aparelho elétrico ligado: risco de choque. Desligue a energia. |
| `combater_sozinho` | `WrongActionInteractable` no cobertor do sofá ("Abafar o fogo da TV?") | depois da missão 4 (fogo grande) | Não tente combater sozinho um incêndio que já cresceu. Saia de casa e ligue 193. |
| `salvar_objetos` | `WrongActionInteractable` no notebook do sofá ("Salvar o notebook?") e na TV ("Tirar a TV?") | fase Rack | Não perca tempo tentando salvar objetos. Sua vida vale mais: saia de casa. |
| `em_pe_na_fumaca` | `SmokeLayer`: cabeça acima da camada de fumaça dentro da zona do corredor e da sala (intervalo mínimo de 10 s entre avisos) | fase Rack | Abaixe-se: a fumaça tóxica fica no alto e o ar perto do chão é mais limpo. |
| `trancar_porta` | `WrongActionInteractable` na chave da porta da frente | fase Rack em diante | Não tranque a porta: os Bombeiros e outras pessoas podem precisar passar. |
| `voltar_para_dentro` | `WrongActionZone` cobrindo o interior da casa | depois da missão 5 | Não volte para dentro de uma casa em chamas. Espere os Bombeiros. |

**Acessibilidade — modo sentado:** com `ComfortSettings.seatedMode`, o jogador não consegue se abaixar
fisicamente. Nesse modo:
- a camada de fumaça para em 1,6 m, acima da cabeça sentada;
- `em_pe_na_fumaca` fica desligado;
- o relógio mostra a dica "Em pé, você deveria se abaixar: a fumaça fica no alto".

O comportamento vive no `SmokeLayer` (seção 9.13).

## 7. Cards do relatório

Cada card mostra a própria fonte (campo `source` do `EducationCardSO`).

| Arquivo | Título | Texto | Fonte |
|---------|--------|-------|-------|
| `Card_01_Panela` | Fogo na panela | Feche o registro do fogão e o do botijão. Abafe a panela com a tampa ou com um pano úmido e bem torcido e espere esfriar antes de destampar. Nunca jogue água. | CBMCE · CBMSC |
| `Card_02_Eletrico` | Fogo em aparelho elétrico | Nunca jogue água em aparelho ligado na energia. Desligue a rede elétrica. O extintor indicado é o de CO2 ou pó químico. Se não conseguir apagar, saia e ligue 193. | CBMSC |
| `Card_03_Principio` | Só o que é pequeno | Tente apagar apenas princípios de incêndio. Se o fogo crescer ou você não conseguir controlar, não combata sozinho: saia rapidamente. | CBMSC · CBM/Vitória |
| `Card_04_Fumaca` | Fumaça: abaixe-se | A fumaça é o maior perigo em um incêndio. Ande agachado, porque a fumaça fica no alto, e use um pano úmido sobre o nariz. | CBMSC · CBM/Vitória |
| `Card_05_Sair` | Ao sair | Feche as portas e janelas atrás de você, sem trancar. Não perca tempo salvando objetos. Depois de sair, ligue 193 e não deixe ninguém entrar. | CBMAL · CBMSC |
| `Card_06_Preso` | Se não conseguir sair | Feche, sem trancar, a porta do cômodo; vede as frestas com cobertor ou tapete; fique perto de uma janela e sinalize; mantenha-se junto ao chão com um pano molhado no nariz e na boca. | CBMAL |
| `Card_07_Extintor` | Extintor | Use o extintor da classe certa, levando-o na vertical. Tire o pino, aproxime-se com cuidado e ataque a base do fogo em leque. Não use extintor de água em eletrônicos ou líquidos inflamáveis. | CBMSC |
| `Card_08_PrevencaoEletrica` | Prevenção: eletricidade | Evite benjamins e "T", não ligue vários aparelhos na mesma tomada e não faça gambiarras. Desligue os aparelhos ao sair de casa e tire o ferro de passar da tomada. Não use notebook sobre cama ou sofá. | CBMSC |
| `Card_09_PrevencaoCozinha` | Prevenção: cozinha e gás | Não deixe o fogão ligado sem supervisão e mantenha as crianças longe dele. Faça a manutenção do gás e não guarde álcool, gasolina, panos ou óleos perto de fontes de calor. Prefira lanternas a velas. | CBMSC |
| `Card_10_Telefones` | Telefones úteis | Bombeiros: 193 · Defesa Civil: 199 · SAMU: 192 · Polícia Militar: 190 | — |

## 8. Dados (ScriptableObjects)

Pastas: `Assets/SafeZoneVR/Data/Incendio/` (passos) e `.../Incendio/Cards/`. O cenário é um asset novo:
`Assets/SafeZoneVR/Data/Scenario_Incendio.asset`.

### 8.1 Catálogo (mudança de plano)

Hoje o `SafeZoneDataBuilder.Build()` cria três cenários "Em breve": Deslizamento, Enxurrada e Vendaval.
Eles saem do plano, e o catálogo passa a ser **Alagamento, Incêndio em Casa e Granizo**. O documento da
fase Granizo traz o mesmo trecho; aplique uma vez só.

```csharp
// SafeZoneDataBuilder.Build() — no lugar das três chamadas LockedScenario(...) e da lista antiga:
foreach (var old in new[] { "Scenario_Deslizamento", "Scenario_Enxurrada", "Scenario_Vendaval" })
{
    var p = $"{k_DataFolder}/{old}.asset";
    if (AssetDatabase.LoadAssetAtPath<ScenarioSO>(p) != null)
        AssetDatabase.DeleteAsset(p);
}

var incendio = BuildIncendio().scenario;
var granizo = BuildGranizo().scenario;     // enquanto não existir: LockedScenario("Scenario_Granizo", "granizo", "Granizo", "Tempestade com granizo na praça do bairro. Em breve.")

data.catalog = CreateOrLoad<ScenarioCatalogSO>($"{k_ResourcesFolder}/ScenarioCatalog.asset");
data.catalog.EditorInitialize(k_MenuSceneName, new List<ScenarioSO> { data.scenario, incendio, granizo });
```

### 8.2 Helpers e `BuildIncendio()`

Os helpers `Step`/`Card` atuais gravam na pasta do Alagamento. Crie versões que recebem a pasta e a
fonte. Se a fase Granizo já os criou, reutilize.

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
const string k_IncendioFolder = "Assets/SafeZoneVR/Data/Incendio";
const string k_FonteCBMSC = "Corpo de Bombeiros Militar de Santa Catarina — Incêndio em Edificação";
const string k_FonteCBMCE = "Corpo de Bombeiros Militar do Ceará — Fogo em panela com óleo";
const string k_FonteCBMAL = "Corpo de Bombeiros Militar de Alagoas — Incêndio";
const string k_FonteCBMVix = "Corpo de Bombeiros — Instruções em caso de incêndio (Prefeitura de Vitória)";

public class IncendioData
{
    public ScenarioSO scenario;
    public MissionStepSO gas, tampa, ventilar, energia, sair, porta, ligar, vizinho;
}

public static IncendioData BuildIncendio()
{
    UIBuilderUtil.EnsureFolder($"{k_IncendioFolder}/Cards");
    var f = k_IncendioFolder;
    var d = new IncendioData();

    d.gas = StepAt(f, "Step_01_Gas", "inc_01_gas", 0, "Feche o gás",
        "Gire o botão do fogão para desligar e feche o registro do botijão, ao lado do fogão.",
        "Cozinha",
        "Sem gás, o fogo da panela deixa de ser alimentado. É o primeiro passo indicado pelos Bombeiros.");
    d.tampa = StepAt(f, "Step_02_Tampa", "inc_02_tampa", 1, "Abafe a panela com a tampa",
        "Pegue a tampa no balcão e coloque sobre a panela. Nunca jogue água no óleo.",
        "Cozinha",
        "Sem ar, o fogo apaga. Água em óleo quente vira vapor na hora e espalha o óleo em chamas.");
    d.ventilar = StepAt(f, "Step_03_Ventilar", "inc_03_ventilar", 2, "Ventile e deixe esfriar",
        "Abra a janela da cozinha e não tire a tampa: deixe a panela esfriar.",
        "Cozinha",
        "A fumaça é tóxica: ventilar ajuda a tirá-la de casa. Se a tampa sair cedo, o fogo pode voltar.");
    d.energia = StepAt(f, "Step_04_Energia", "inc_04_energia", 3, "Desligue a energia",
        "Faíscas atrás da TV! Não jogue água. Vá ao quarto e abaixe a alavanca do quadro de energia.",
        "Quarto",
        "Fogo em aparelho elétrico nunca se apaga com água. Desligar a energia corta a fonte das faíscas.");
    d.sair = StepAt(f, "Step_05_Sair", "inc_05_sair", 4, "Saia abaixado",
        "O fogo cresceu: não tente apagar sozinho. Abaixe-se por baixo da fumaça e saia pela porta da frente.",
        "Corredor → porta da frente",
        "A fumaça tóxica sobe e fica no alto; perto do chão o ar é mais limpo. Se não conseguir apagar, saia rapidamente.");
    d.porta = StepAt(f, "Step_06_Porta", "inc_06_porta", 5, "Feche a porta sem trancar",
        "Já do lado de fora, feche a porta da frente. Não tranque: os Bombeiros podem precisar passar.",
        "Porta da frente",
        "A porta fechada segura o fogo e a fumaça lá dentro; destrancada, não atrapalha quem precisa entrar ou sair.");
    d.ligar = StepAt(f, "Step_07_Ligar", "inc_07_193", 6, "Ligue para os Bombeiros",
        "Na calçada, em segurança, ligue 193 pelo relógio.",
        "Ponto de encontro (calçada)",
        "Ligar 193 depois de sair garante o socorro sem que você fique perto do fogo.");
    d.vizinho = StepAt(f, "Step_08_Vizinho", "inc_08_vizinho", 7, "Não deixe ninguém entrar",
        "O vizinho quer entrar para salvar a TV. Aponte para ele e aperte o gatilho para impedi-lo.",
        "Portão da frente",
        "Nenhum objeto vale o risco. Não permita que ninguém entre em um local em chamas: espere os Bombeiros.");

    d.gas.EditorSetOrderGroup(1);        // missões 1 e 2 em qualquer ordem (seção 9.2)
    d.tampa.EditorSetOrderGroup(1);

    var cards = new List<EducationCardSO>
    {
        CardAt(f, "Card_01_Panela", k_FonteCBMCE, "Fogo na panela", "Feche o registro do fogão e o do botijão. Abafe a panela com a tampa ou com um pano úmido e bem torcido e espere esfriar antes de destampar. Nunca jogue água."),
        CardAt(f, "Card_02_Eletrico", k_FonteCBMSC, "Fogo em aparelho elétrico", "Nunca jogue água em aparelho ligado na energia. Desligue a rede elétrica. O extintor indicado é o de CO2 ou pó químico. Se não conseguir apagar, saia e ligue 193."),
        CardAt(f, "Card_03_Principio", k_FonteCBMSC, "Só o que é pequeno", "Tente apagar apenas princípios de incêndio. Se o fogo crescer ou você não conseguir controlar, não combata sozinho: saia rapidamente."),
        CardAt(f, "Card_04_Fumaca", k_FonteCBMSC, "Fumaça: abaixe-se", "A fumaça é o maior perigo em um incêndio. Ande agachado, porque a fumaça fica no alto, e use um pano úmido sobre o nariz."),
        CardAt(f, "Card_05_Sair", k_FonteCBMAL, "Ao sair", "Feche as portas e janelas atrás de você, sem trancar. Não perca tempo salvando objetos. Depois de sair, ligue 193 e não deixe ninguém entrar."),
        CardAt(f, "Card_06_Preso", k_FonteCBMAL, "Se não conseguir sair", "Feche, sem trancar, a porta do cômodo; vede as frestas com cobertor ou tapete; fique perto de uma janela e sinalize; mantenha-se junto ao chão com um pano molhado no nariz e na boca."),
        CardAt(f, "Card_07_Extintor", k_FonteCBMSC, "Extintor", "Use o extintor da classe certa, levando-o na vertical. Tire o pino, aproxime-se com cuidado e ataque a base do fogo em leque. Não use extintor de água em eletrônicos ou líquidos inflamáveis."),
        CardAt(f, "Card_08_PrevencaoEletrica", k_FonteCBMSC, "Prevenção: eletricidade", "Evite benjamins e \"T\", não ligue vários aparelhos na mesma tomada e não faça gambiarras. Desligue os aparelhos ao sair de casa e tire o ferro de passar da tomada. Não use notebook sobre cama ou sofá."),
        CardAt(f, "Card_09_PrevencaoCozinha", k_FonteCBMSC, "Prevenção: cozinha e gás", "Não deixe o fogão ligado sem supervisão e mantenha as crianças longe dele. Faça a manutenção do gás e não guarde álcool, gasolina, panos ou óleos perto de fontes de calor. Prefira lanternas a velas."),
        CardAt(f, "Card_10_Telefones", k_FonteCBMVix, "Telefones úteis", "Bombeiros: 193\nDefesa Civil: 199\nSAMU: 192\nPolícia Militar: 190"),
    };

    d.scenario = CreateOrLoad<ScenarioSO>($"{k_DataFolder}/Scenario_Incendio.asset");
    d.scenario.EditorInitialize("incendio", "Incêndio em Casa",
        "A panela pegou fogo e, depois, um benjamim sobrecarregado atrás da TV. Aprenda a apagar só o que é pequeno, sem água no óleo ou na eletricidade, e a sair abaixado, fechar a porta sem trancar e ligar 193.",
        "Incendio_EmCasa", true, 300f, 480f,
        new List<MissionStepSO> { d.gas, d.tampa, d.ventilar, d.energia, d.sair, d.porta, d.ligar, d.vizinho }, cards);
    EditorUtility.SetDirty(d.scenario);
    return d;
}
```

---

## 9. Código novo

Os itens marcados **(compartilhado)** também são usados pela fase *Granizo*, e o documento dela traz o
mesmo código. Crie cada um **uma única vez**, em `Assets/SafeZoneVR/Runtime/...`. Todos seguem as
regras do projeto:
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
| `SetupBreaker` | usado aqui para o quadro de energia |
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
Aqui é o vizinho da missão 8.

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

### 9.4 `EmergencyCallPanel` (compartilhado) — `Runtime/UI/`

Painel world-space no mesmo estilo do `OptionsPanelController`: `LazyFollow` a 1,1 m e botões grandes.
Ele abre pelo botão **"Ligar"** do relógio (seção 9.1).
- **Números:** quatro botões — 199 Defesa Civil, 193 Bombeiros, 190 Polícia Militar e 192 SAMU.
- **Ao tocar um número:** mostra "Chamando…" por 1,2 s e depois a resposta da central (seção 5).
- **Números aceitos:** disparam `onAcceptedCall`. Nesta fase, `acceptedNumbers = ["193"]`. No builder, o
  evento é ligado a `StepCompleteOnButtonPress.OnButtonPressed` com `UnityEventTools.AddPersistentListener`.

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

### 9.5 `ActivateOnStepCompleted` (compartilhado) — `Runtime/Core/`

Liga ou desliga objetos quando um passo é concluído. Exemplos aqui: a zona `voltar_para_dentro` depois
da missão 5, a sirene depois da missão 7 e o caminhão dos Bombeiros depois da missão 8.

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

### 9.6 `ScenarioPhaseSwitcher` (compartilhado) — `Runtime/Core/`

Troca o estado do ambiente quando **todos** os passos de gatilho de uma fase estiverem concluídos.
- Com `fadeSeconds > 0`, a troca usa o `ScreenFader`. Nesta fase todas as trocas são **sem fade**, porque o jogador acompanha o fogo em tempo real.
- **Nunca move o jogador** (conforto em VR).
- O `info` aparece no relógio via `ScenarioManager.ShowInfo`.
- O `onEnter` liga os controladores do fogo e da fumaça.

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

### 9.7 `CompanionFollower` (compartilhado) — `Runtime/Core/`

NPC que anda por uma rota fixa de waypoints quando é selecionado. Aqui é o vizinho: ao ser impedido,
ele diz "Tem razão, vamos esperar os Bombeiros" e vai para o ponto de encontro. Sem NavMesh. O
collider fica na layer `Ignore Raycast`, para o raio de chão não acertar o próprio corpo.

```csharp
public class CompanionFollower : MonoBehaviour
{
    [SerializeField] XRBaseInteractable m_TalkInteractable;          // o próprio corpo do NPC
    [SerializeField] List<Transform> m_Waypoints = new List<Transform>();
    [SerializeField] float m_Speed = 0.8f;
    [SerializeField] float m_MaxLeadDistance = 3f;                   // distância máxima à frente do jogador
    [SerializeField] TextMeshPro m_SpeechBubble;
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
            next.y = hit.point.y;
        var dir = target - pos; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        transform.position = next;

        if (new Vector2(next.x - target.x, next.z - target.z).sqrMagnitude < 0.04f) m_Index++;
    }
}
```
**Vizinho (missões 7 e 8).** O vizinho usa duas rotas: chega sozinho até o portão e, quando o jogador o
impede, vai para o ponto de encontro. Acrescente ao `CompanionFollower` um método para trocar de rota:

```csharp
public void Restart(List<Transform> newRoute, bool follow = true)
{
    m_Waypoints = newRoute;
    m_Index = 0;
    m_Following = false;
    if (follow) StartFollowing();
}
```

E um componente pequeno no vizinho, `Runtime/Core/NeighborIntruder.cs`.
- O objeto dele começa **desligado**.
- O `ActivateOnStepCompleted` da missão 7 liga o objeto.
- No `CompanionFollower` dele:
  - `m_TalkInteractable = null`, porque quem reage ao gatilho é o `NeighborIntruder`;
  - `m_MaxLeadDistance = 100`, para ele não esperar o jogador.

```csharp
public class NeighborIntruder : MonoBehaviour
{
    [SerializeField] CompanionFollower m_Follower;
    [SerializeField] XRSimpleInteractable m_Interactable;              // o mesmo usado pela StepCompleteOnInteractCount da missão 8
    [SerializeField] List<Transform> m_ToGate = new List<Transform>();     // calçada (-26,0,19) → portão (-32.5,0,18.4)
    [SerializeField] List<Transform> m_ToMeeting = new List<Transform>();  // portão → ponto de encontro (-36,0,19.5)
    [SerializeField] TextMeshPro m_Speech;
    bool m_Stopped;

    void OnEnable()
    {
        m_Interactable.selectEntered.AddListener(OnStopped);
        m_Follower.Restart(m_ToGate);
        if (m_Speech != null) { m_Speech.text = "Vou lá dentro pegar sua TV!"; m_Speech.gameObject.SetActive(true); }
    }

    void OnDisable() => m_Interactable.selectEntered.RemoveListener(OnStopped);

    void OnStopped(SelectEnterEventArgs _)
    {
        if (m_Stopped) return;
        m_Stopped = true;
        m_Follower.Restart(m_ToMeeting);
        if (m_Speech != null) m_Speech.text = "Tem razão. Vamos esperar os Bombeiros.";
    }
}
```

### 9.8 `ToggleOpening` (compartilhado) — `Runtime/Interactables/`

Janela ou porta que abre e fecha com um toque (select), com animação curta. O padrão é o mesmo do
`BreakerLever`: sem física de dobradiça, robusto no Quest. O collider do `XRSimpleInteractable` deve ficar
na **moldura**, não na folha que gira; assim o raio continua acertando a abertura aberta. O collider
**sólido** da folha fica na própria folha, para barrar a passagem com a porta fechada. `BoolEvent` já
existe em `BreakerLever.cs`.

```csharp
[RequireComponent(typeof(XRSimpleInteractable))]
public class ToggleOpening : MonoBehaviour
{
    [SerializeField] Transform m_Hinge;                 // pivô da folha
    [SerializeField] Vector3 m_Axis = Vector3.up;
    [SerializeField] float m_OpenAngle = 80f;
    [SerializeField] float m_DegreesPerSecond = 240f;
    [SerializeField] bool m_StartsOpen;
    [SerializeField] BoolEvent m_OnStateChanged = new BoolEvent();   // true = aberta
    Quaternion m_Closed; float m_Target; bool m_IsOpen;

    public bool isOpen => m_IsOpen;
    public BoolEvent onStateChanged => m_OnStateChanged;
    public Transform hinge { get => m_Hinge; set => m_Hinge = value; }
    public bool startsOpen { get => m_StartsOpen; set => m_StartsOpen = value; }
    public float openAngle { get => m_OpenAngle; set => m_OpenAngle = value; }

    void Awake()
    {
        if (m_Hinge == null) m_Hinge = transform;
        m_Closed = m_Hinge.localRotation;
        m_IsOpen = m_StartsOpen;
        m_Target = m_IsOpen ? m_OpenAngle : 0f;
        m_Hinge.localRotation = m_Closed * Quaternion.AngleAxis(m_Target, m_Axis);
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => SetOpen(!m_IsOpen));
    }

    void Update()
    {
        var target = m_Closed * Quaternion.AngleAxis(m_Target, m_Axis);
        if (m_Hinge.localRotation != target)
            m_Hinge.localRotation = Quaternion.RotateTowards(m_Hinge.localRotation, target, m_DegreesPerSecond * Time.deltaTime);
    }

    public void SetOpen(bool open)
    {
        if (m_IsOpen == open) return;
        m_IsOpen = open;
        m_Target = open ? m_OpenAngle : 0f;
        m_OnStateChanged.Invoke(open);
    }
}
```

### 9.9 `StepCompleteOnOpenings` — `Runtime/Validators/`

Conclui quando todas as aberturas da lista chegam ao estado-alvo (`targetOpen`):
- **missão 3:** a janela da cozinha **aberta**;
- **missão 6:** a porta da frente **fechada**, com o jogador **do lado de fora** (`requirePlayerInZone`).

A avaliação só acontece quando uma abertura muda de estado. Assim a porta, que começa fechada, não
conclui a missão 6 logo no início.

```csharp
public class StepCompleteOnOpenings : StepValidatorBase
{
    [SerializeField] List<ToggleOpening> m_Openings = new List<ToggleOpening>();
    [SerializeField] bool m_TargetOpen = true;
    [Tooltip("Opcional: só conclui com a cabeça do jogador dentro deste collider (ex.: quintal da frente).")]
    [SerializeField] Collider m_RequirePlayerInZone;

    public List<ToggleOpening> openings => m_Openings;
    public bool targetOpen { get => m_TargetOpen; set => m_TargetOpen = value; }
    public Collider requirePlayerInZone { get => m_RequirePlayerInZone; set => m_RequirePlayerInZone = value; }

    void OnEnable()  { foreach (var o in m_Openings) if (o != null) o.onStateChanged.AddListener(OnChanged); }
    void OnDisable() { foreach (var o in m_Openings) if (o != null) o.onStateChanged.RemoveListener(OnChanged); }

    void OnChanged(bool _)
    {
        if (m_Completed) return;
        if (m_RequirePlayerInZone != null &&
            !(PlayerLocator.TryGetHeadPosition(out var head) && m_RequirePlayerInZone.bounds.Contains(head)))
            return;
        var ok = 0;
        foreach (var o in m_Openings) if (o != null && o.isOpen == m_TargetOpen) ok++;
        if (m_Openings.Count > 1) ReportProgress(ok, m_Openings.Count);
        if (ok >= m_Openings.Count) Complete();
    }
}
```

### 9.10 `StepCompleteOnAllConditions` — `Runtime/Validators/`

Conclui quando **todas** as condições forem atendidas, com progresso "x de y". Nesta fase só são usados
os registros (`XRKnob`): o botão do fogão e o do botijão, que precisam girar pelo menos `requiredDelta`
a partir do valor inicial. As listas de alavancas e sockets ficam para reuso em outras fases.

```csharp
public class StepCompleteOnAllConditions : StepValidatorBase
{
    [SerializeField] List<BreakerLever> m_LeversOff = new List<BreakerLever>();
    [SerializeField] List<XRKnob> m_KnobsTurned = new List<XRKnob>();
    [SerializeField, Range(0.05f, 1f)] float m_RequiredDelta = 0.25f;
    [SerializeField] List<XRSocketInteractor> m_SocketsEmpty = new List<XRSocketInteractor>();
    readonly Dictionary<XRKnob, float> m_KnobStart = new Dictionary<XRKnob, float>();
    int m_LastDone = -1;

    public List<BreakerLever> leversOff => m_LeversOff;
    public List<XRKnob> knobsTurned => m_KnobsTurned;
    public List<XRSocketInteractor> socketsEmpty => m_SocketsEmpty;

    void Start()
    {
        foreach (var l in m_LeversOff) if (l != null) l.onStateChanged.AddListener(_ => Evaluate());
        foreach (var k in m_KnobsTurned) if (k != null) { m_KnobStart[k] = k.value; k.onValueChange.AddListener(_ => Evaluate()); }
        foreach (var s in m_SocketsEmpty) if (s != null) s.selectExited.AddListener(_ => Evaluate());
    }

    void Evaluate()
    {
        if (m_Completed) return;
        int done = 0, total = m_LeversOff.Count + m_KnobsTurned.Count + m_SocketsEmpty.Count;
        foreach (var l in m_LeversOff) if (l != null && !l.isOn) done++;
        foreach (var k in m_KnobsTurned) if (k != null && Mathf.Abs(k.value - m_KnobStart[k]) >= m_RequiredDelta) done++;
        foreach (var s in m_SocketsEmpty) if (s != null && !s.hasSelection) done++;
        if (done != m_LastDone) { m_LastDone = done; ReportProgress(done, total); }   // XRKnob dispara a cada grau
        if (done >= total) Complete();
    }
}
```
`XRKnob` fica no namespace `Unity.VRTemplate`, o mesmo usado por `StepCompleteOnKnob`.

### 9.11 `FireController` e `FlareOnSelect` (específicos desta fase) — `Runtime/Core/`

Um foco de fogo com quatro tamanhos: apagado, pequeno, médio e grande. O controlador cuida de:
- **Aparência:** emissão das partículas, escala e intensidade da luz em cada tamanho.
- **Cintilação:** a luz oscila a cada 0,05 s.
- **Crescimento automático:** o fogo sobe de tamanho em intervalos configuráveis.
- **Faíscas:** uma rajada de 2 s antes de acender (usada pelo benjamim).
- **"Puff":** um aumento breve e controlado quando o jogador erra (água no óleo), que nunca alcança o jogador.

Os dois focos usam o mesmo componente:

| Foco | Começa | Crescimento | Fumaça residual ao apagar |
|------|--------|-------------|----------------------------|
| **Panela** | pequeno | não cresce | 20 s |
| **Rack** | apagado | faíscas → pequeno → médio em 20 s → grande em mais 25 s | — (não apaga) |

```csharp
public class FireController : MonoBehaviour
{
    public enum Size { Out = 0, Small = 1, Medium = 2, Large = 3 }

    [SerializeField] ParticleSystem m_Flames;
    [SerializeField] ParticleSystem m_Smoke;
    [SerializeField] ParticleSystem m_Sparks;                        // opcional (benjamim)
    [SerializeField] Light m_Light;                                  // pontual, sem sombra
    [SerializeField] AudioSource m_Crackle;                          // loop CreateCrackleLoop, 3D
    [SerializeField] float[] m_FlameRate = { 0f, 12f, 30f, 60f };
    [SerializeField] float[] m_FlameScale = { 0f, 0.35f, 0.8f, 1.4f };
    [SerializeField] float[] m_LightIntensity = { 0f, 0.8f, 1.6f, 2.6f };
    [SerializeField] Size m_StartSize = Size.Small;
    [SerializeField] float[] m_GrowSteps = new float[0];             // Rack: { 20, 25 } segundos por degrau
    [SerializeField] float m_ResidualSmokeSeconds = 20f;
    Size m_Size; int m_GrowIndex; float m_NextGrow = -1f, m_NextFlicker; bool m_Flaring;

    public Size size => m_Size;
    public bool isBurning => m_Size != Size.Out;

    void Awake() => SetSize(m_StartSize);

    public void Ignite(Size s) { SetSize(s == Size.Out ? Size.Small : s); ScheduleGrowth(); }

    /// <summary>Faíscas por 2 s e depois fogo pequeno que cresce sozinho (benjamim atrás da TV).</summary>
    public void SparkThenIgnite() => StartCoroutine(SparkRoutine());

    public void Extinguish()
    {
        SetSize(Size.Out);
        m_NextGrow = -1f;
        if (m_Smoke != null) { m_Smoke.Play(); CancelInvoke(nameof(StopSmoke)); Invoke(nameof(StopSmoke), m_ResidualSmokeSeconds); }
    }

    /// <summary>"Puff" rápido e controlado (água no óleo). Volta ao tamanho anterior em 0,6 s.</summary>
    public void Flare()
    {
        if (!isBurning || m_Flaring) return;
        StartCoroutine(FlareRoutine());
    }

    IEnumerator SparkRoutine()
    {
        if (m_Sparks != null) m_Sparks.Play();
        yield return new WaitForSeconds(2f);
        if (m_Sparks != null) m_Sparks.Stop();
        Ignite(Size.Small);
    }

    IEnumerator FlareRoutine()
    {
        m_Flaring = true;
        var before = m_Size;
        SetSize((Size)Mathf.Min((int)Size.Large, (int)before + 2));
        yield return new WaitForSeconds(0.6f);
        SetSize(before);
        m_Flaring = false;
    }

    void ScheduleGrowth()
    {
        m_NextGrow = m_GrowIndex < m_GrowSteps.Length ? Time.time + m_GrowSteps[m_GrowIndex] : -1f;
    }

    void Update()
    {
        if (!isBurning) return;
        if (m_NextGrow > 0f && Time.time >= m_NextGrow && m_Size < Size.Large)
        {
            SetSize(m_Size + 1);
            m_GrowIndex++;
            ScheduleGrowth();
        }
        if (m_Light != null && Time.time >= m_NextFlicker)
        {
            m_NextFlicker = Time.time + 0.05f;
            m_Light.intensity = m_LightIntensity[(int)m_Size] * (0.85f + 0.3f * Mathf.PerlinNoise(Time.time * 6f, 0f));
        }
    }

    void SetSize(Size s)
    {
        m_Size = s;
        var i = (int)s;
        if (m_Flames != null)
        {
            var e = m_Flames.emission; e.rateOverTime = m_FlameRate[i];
            m_Flames.transform.localScale = Vector3.one * Mathf.Max(0.01f, m_FlameScale[i]);
            if (i > 0 && !m_Flames.isPlaying) m_Flames.Play();
            if (i == 0) m_Flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (m_Smoke != null && i > 0 && !m_Smoke.isPlaying) m_Smoke.Play();
        if (m_Light != null) { m_Light.enabled = i > 0; m_Light.intensity = m_LightIntensity[i]; }
        if (m_Crackle != null)
        {
            m_Crackle.volume = 0.15f * i;
            if (i > 0 && !m_Crackle.isPlaying) m_Crackle.Play();
            if (i == 0) m_Crackle.Stop();
        }
    }

    void StopSmoke() { if (m_Smoke != null) m_Smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
}

/// <summary>Na jarra de água: se o fogo da panela estiver aceso, dá o "puff" (o erro é registrado pelo WrongActionInteractable).</summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class FlareOnSelect : MonoBehaviour
{
    [SerializeField] FireController m_Fire;
    public FireController fire { get => m_Fire; set => m_Fire = value; }
    void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => { if (m_Fire != null) m_Fire.Flare(); });
}
```
O fogo do rack **não apaga** quando o quadro é desligado, porque já pegou no móvel. Nenhum
componente chama `Extinguish()` nele: é a demonstração de "se não conseguir extinguir, abandone o local".

### 9.12 `LidCooldown` — `Runtime/Interactables/`

Fica no socket da tampa, que é filho da panela. Com a tampa encaixada, o fogo apaga. Se a tampa sair
antes de 45 s, um fogo pequeno volta e o erro `tampa_cedo` é registrado. Tampar de novo apaga outra vez.

```csharp
public class LidCooldown : MonoBehaviour
{
    [SerializeField] XRSocketInteractor m_LidSocket;
    [SerializeField] FireController m_PanFire;
    [SerializeField] float m_CoolSeconds = 45f;
    [SerializeField] string m_MistakeId = "tampa_cedo";
    [SerializeField, TextArea] string m_Message = "Não levante a tampa logo: espere a panela esfriar, senão o fogo pode voltar.";
    [SerializeField] UnityEvent m_OnCovered = new UnityEvent();       // ex.: SmokeAlarm.StopAfter(20)
    float m_CoveredAt = -1f;

    public UnityEvent onCovered => m_OnCovered;

    void Awake()
    {
        m_LidSocket.selectEntered.AddListener(_ => { m_CoveredAt = Time.time; m_PanFire.Extinguish(); m_OnCovered.Invoke(); });
        m_LidSocket.selectExited.AddListener(OnLidRemoved);
    }

    void OnLidRemoved(SelectExitEventArgs args)
    {
        if (args.isCanceled || m_CoveredAt < 0f) return;              // cena descarregando: não é ação do jogador
        if (Time.time - m_CoveredAt < m_CoolSeconds)
        {
            m_PanFire.Ignite(FireController.Size.Small);
            var m = ScenarioManager.instance;
            if (m != null) m.RegisterMistake(m_MistakeId, m_Message, 10f);
        }
        m_CoveredAt = -1f;
    }
}
```
A validação da missão 2 continua sendo o `StepCompleteOnSocket` padrão sobre esse mesmo socket.
Ele conclui no primeiro encaixe e não "desconclui" se a tampa sair; o erro é o que ensina.

### 9.13 `SmokeLayer` — `Runtime/Core/`

Camada de fumaça que desce do teto, na sala e no corredor, e ensina a se abaixar pela própria experiência.
- **Visual:** 3 quads empilhados (base, base + 0,3 m, base + 0,6 m) com material cinza transparente e ruído rolando no UV.
- **Descida:** a base desce de 2,0 m para 1,15 m em 40 s.
- **Névoa:** com a cabeça **acima** da base e dentro da zona, liga a névoa do URP e a visão fica cinza. Abaixado, a névoa desliga e a visão limpa. É o "ar mais limpo perto do chão".
- **Erro `em_pe_na_fumaca`:** só é registrado depois que a camada desceu pelo menos a metade, com intervalo mínimo de 10 s entre avisos.
- **Modo sentado (acessibilidade):** a base para em 1,6 m, sem erro e com uma dica no relógio.

```csharp
public class SmokeLayer : MonoBehaviour
{
    [SerializeField] BoxCollider m_Zone;                    // sala + corredor, trigger; bounds.min.y = piso
    [SerializeField] Transform m_LayerVisual;               // raiz dos quads; começa desligada
    [SerializeField] float m_StartBottom = 2.0f, m_EndBottom = 1.15f, m_SeatedBottom = 1.6f, m_DescendSeconds = 40f;
    [SerializeField] Color m_FogColor = new Color(0.36f, 0.36f, 0.38f);
    [SerializeField] float m_FogDensity = 0.35f;
    [SerializeField] string m_MistakeId = "em_pe_na_fumaca";
    [SerializeField, TextArea] string m_Message = "Abaixe-se: a fumaça tóxica fica no alto e o ar perto do chão é mais limpo.";
    [SerializeField, TextArea] string m_SeatedTip = "Em pé, você deveria se abaixar: a fumaça fica no alto.";
    [SerializeField] float m_CheckInterval = 0.2f;
    float m_StartTime, m_NextCheck; bool m_Active, m_FogOn;
    bool m_SavedFog; Color m_SavedColor; float m_SavedDensity; FogMode m_SavedMode;

    public void Begin()                                     // ligado ao onEnter da fase Rack
    {
        m_Active = true;
        m_StartTime = Time.time;
        m_LayerVisual.gameObject.SetActive(true);
        m_SavedFog = RenderSettings.fog; m_SavedColor = RenderSettings.fogColor;
        m_SavedDensity = RenderSettings.fogDensity; m_SavedMode = RenderSettings.fogMode;
        if (ComfortSettings.seatedMode && ScenarioManager.instance != null) ScenarioManager.instance.ShowInfo(m_SeatedTip);
    }

    void Update()
    {
        if (!m_Active || Time.time < m_NextCheck) return;
        m_NextCheck = Time.time + m_CheckInterval;

        var seated = ComfortSettings.seatedMode;
        var k = Mathf.Clamp01((Time.time - m_StartTime) / m_DescendSeconds);
        var bottom = Mathf.Lerp(m_StartBottom, seated ? m_SeatedBottom : m_EndBottom, k);
        var p = m_LayerVisual.position;
        p.y = m_Zone.bounds.min.y + bottom;
        m_LayerVisual.position = p;

        var inSmoke = PlayerLocator.TryGetHeadPosition(out var head) && m_Zone.bounds.Contains(head) && head.y > p.y;
        SetFog(inSmoke);
        if (inSmoke && !seated && k >= 0.5f && ScenarioManager.instance != null)
            ScenarioManager.instance.RegisterMistake(m_MistakeId, m_Message, 10f);
    }

    void SetFog(bool on)
    {
        if (on == m_FogOn) return;
        m_FogOn = on;
        if (on) { RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogColor = m_FogColor; RenderSettings.fogDensity = m_FogDensity; }
        else    { RenderSettings.fog = m_SavedFog; RenderSettings.fogMode = m_SavedMode; RenderSettings.fogColor = m_SavedColor; RenderSettings.fogDensity = m_SavedDensity; }
    }

    void OnDisable() { if (m_FogOn) SetFog(false); }
}
```
**Névoa no URP:** os shaders URP Lit e Unlit já trazem a variante de névoa, mas o *stripping* do build pode
removê-la se nenhuma cena usar névoa. Em *Project Settings → Graphics → Shader Stripping*, deixe a névoa
**Exponential Squared** incluída. Sem isso, a névoa funciona no Editor e some no Quest.

**Teste do "abaixar" no simulador:** a tecla que abaixa o HMD simulado desce a cabeça. No MCP, baixe o
`XROrigin.CameraYOffset` ou mova o `Camera Offset` para simular o agachamento.

### 9.14 Sons procedurais — `ProceduralAudio`

Mesmo padrão de `CreateRainLoop`: volumes moderados e nada estridente (RNF03).

```csharp
/// <summary>Crepitar do fogo: ruído grave + estalinhos esparsos. Loop.</summary>
public static AudioClip CreateCrackleLoop(string name, float seconds = 3f, float volume = 0.3f)
{
    const int rate = 22050;
    var n = Mathf.CeilToInt(seconds * rate);
    var data = new float[n];
    var rng = new System.Random(3);
    var lp = 0f;
    for (var i = 0; i < n; i++)
    {
        lp += 0.05f * ((float)(rng.NextDouble() * 2.0 - 1.0) - lp);    // chiado grave do fogo
        data[i] = lp * 1.5f * volume;
    }
    var pops = Mathf.RoundToInt(seconds * 14f);
    for (var h = 0; h < pops; h++)
    {
        var start = rng.Next(n); var len = 60 + rng.Next(160);          // 3–10 ms
        var amp = (float)(0.2 + rng.NextDouble() * 0.5) * volume;
        for (var i = 0; i < len; i++) data[(start + i) % n] += (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-i / (len * 0.3f)) * amp;
    }
    for (var i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
    var clip = AudioClip.Create(name, n, 1, rate, false);
    clip.SetData(data, 0);
    return clip;
}

/// <summary>Detector de fumaça "suave": três bipes de 1,8 kHz e uma pausa. Loop de 2 s.</summary>
public static AudioClip CreateSmokeAlarmLoop(string name, float volume = 0.18f)
{
    const int rate = 22050;
    var n = rate * 2;
    var data = new float[n];
    for (var b = 0; b < 3; b++)
    {
        var start = (int)(b * 0.3f * rate); var len = (int)(0.18f * rate);
        for (var i = 0; i < len; i++)
        {
            var env = Mathf.Min(1f, i / 200f) * Mathf.Min(1f, (len - i) / 200f);   // sem clique
            data[start + i] = Mathf.Sin(2f * Mathf.PI * 1800f * i / rate) * env * volume;
        }
    }
    var clip = AudioClip.Create(name, n, 1, rate, false);
    clip.SetData(data, 0);
    return clip;
}
```

**Detector e sirene** são `AudioSource` simples, controlados por um componente mínimo com métodos
públicos, para ligar por listeners persistentes:

```csharp
public class SimpleLoopSound : MonoBehaviour
{
    [SerializeField] AudioSource m_Source;
    public void Play() { if (m_Source != null && !m_Source.isPlaying) m_Source.Play(); }
    public void Stop() { if (m_Source != null) m_Source.Stop(); }
    public void StopAfter(float seconds) { CancelInvoke(nameof(Stop)); Invoke(nameof(Stop), seconds); }
}
```
- **Detector:** toca desde o início; `LidCooldown.onCovered` chama `StopAfter(20)`; a fase Rack chama `Play()`.
- **Sirene distante:** o `ActivateOnStepCompleted` da missão 7 liga o objeto, com `playOnAwake = true`, loop, volume 0,1 e fonte 3D a 40 m.
  O clip alterna dois tons suaves e é filtrado para soar longe:

```csharp
/// <summary>Sirene ao longe: dois tons alternados (650/900 Hz) a cada 0,6 s, abafados. Loop de 2,4 s.</summary>
public static AudioClip CreateDistantSirenLoop(string name, float volume = 0.25f)
{
    const int rate = 22050;
    var n = (int)(2.4f * rate);
    var data = new float[n];
    float phase = 0f, lp = 0f;
    for (var i = 0; i < n; i++)
    {
        var t = (float)i / rate;
        var freq = Mathf.Lerp(650f, 900f, 0.5f + 0.5f * Mathf.Sign(Mathf.Sin(t * Mathf.PI / 0.6f)));
        phase += 2f * Mathf.PI * freq / rate;
        lp += 0.12f * (Mathf.Sin(phase) - lp);                          // abafa: soa distante
        data[i] = lp * volume;
    }
    var clip = AudioClip.Create(name, n, 1, rate, false);
    clip.SetData(data, 0);
    return clip;
}
```

---

## 10. Builder da cena — `Editor/IncendioGameplayBuilder.cs`

### 10.1 Pré-requisitos de modelagem (uma vez, à mão)

1. **Janela da cozinha com vão real:** no prefab `Casa_SafeZone`, abra um vão de 1,0 × 1,0 m na parede oeste da cozinha (x ≈ −35.9, centro em y 1.4, z 11), com ProBuilder (*Subdivide Edge* + *Delete Face*) ou trocando o trecho da parede por 3 peças. O vão real mostra a fumaça saindo pela janela. Guarde o prefab e **regenere o Alagamento** para conferir que nada mudou lá.
2. **Porta da frente operável:** o modelo `PortaFrente` da casa é uma porta aberta **estática** (moldura e folha numa malha só). Nesta cena o builder:
   - desliga o renderer e o collider desse modelo, como override da instância do prefab;
   - cria `PortaFrente_Operavel`: moldura (2 montantes + verga, com colliders estáticos) e folha de 0,95 × 2,0 × 0,04 m com dobradiça em x −32.93, `ToggleOpening` (`startsOpen = false`, `openAngle = 95`, abrindo para dentro) e `BoxCollider` sólido;
   - põe o `XRSimpleInteractable` nas **maçanetas** (uma de cada lado da folha), para o raio acertar com a porta aberta ou fechada.

### 10.2 Roteiro do `Build()`

Siga o padrão idempotente do Alagamento: tudo o que é gerado fica sob `SafeZone_Gameplay` e é recriado a cada execução.

```text
1. Abrir/criar Assets/Scenes/Fases/Incendio_EmCasa.unity
   (primeira vez: cena vazia + luz de fim de tarde + Casa_SafeZone.prefab na origem + rua do Alagamento).
2. SafeZoneDataBuilder.BuildIncendio() → passos e cenário.
3. ScenarioSceneKit.CreateMaterials(); CleanupLegacy.
4. XR Origin em (-32.8, 0, 11.6) com rotação Y = -90 (olhando para a cozinha); PlayerSpawnPoint igual.
5. Núcleo: ScenarioManager (autoStart = false), FeedbackPlayer (playRainAmbience = false),
   ComfortSettingsApplier, ObjectiveBeaconSystem.
6. Cozinha:
   - panela no fogão + FireController "Panela" (startSize Small, sem crescimento, fumaça residual 20 s);
   - socket da tampa no topo da panela (SocketItemFilter = tampa_panela) + LidCooldown;
   - tampa no balcão (ScenarioSceneKit.CreateItem, id tampa_panela, ObjectiveTarget passo 2);
   - botão do fogão e registro do botijão: XRKnob (helper CreateKnob no kit, no padrão do SetupValve),
     com ObjectiveTarget do passo 1;
   - jarra: XRSimpleInteractable + WrongActionInteractable (agua_no_oleo) + FlareOnSelect(fire = Panela);
   - janela: ToggleOpening (startsOpen = false) + ObjectiveTarget do passo 3.
7. Sala:
   - benjamim atrás do rack + FireController "Rack" (startSize Out, growSteps {20, 25}, faíscas);
   - notebook e TV com WrongActionInteractable (salvar_objetos, "Salvar o notebook?" / "Tirar a TV?");
   - cobertor no sofá com WrongActionInteractable (combater_sozinho), desligado até a fase Evacuar;
   - SmokeLayer: zona da sala e do corredor (caixa x -33.9..-29.1, z 9.6..14.8, y 0..2.1) + 3 quads.
   - detector de fumaça no teto da sala: SimpleLoopSound com CreateSmokeAlarmLoop, tocando desde o início.
8. Quarto: quadro de energia (ScenarioSceneKit.SetupBreaker) + ObjectiveTarget do passo 4.
9. Porta da frente operável (10.1) + chave na fechadura (WrongActionInteractable trancar_porta).
10. Exterior:
    - zona "quintal da frente" (x -34.5..-31, z 15.3..17.5, altura 2.4): missão 5 e requirePlayerInZone da missão 6;
    - placa "PONTO DE ENCONTRO" na calçada (x -33, z 19);
    - vizinho (desligado): corpo low-poly + XRSimpleInteractable + ObjectiveTarget (passo 8)
      + CompanionFollower + NeighborIntruder (rotas da seção 9.7);
    - sirene distante (desligada) e caminhão dos Bombeiros (desligado) na rua (x -30, z 22).
11. Validadores (seção 5):
    - V1 StepCompleteOnAllConditions (knobsTurned = botão do fogão + registro do botijão)
    - V2 StepCompleteOnSocket (socket da tampa; required = tampa_panela)
    - V3 StepCompleteOnOpenings (janela; targetOpen = true)
    - V4 StepCompleteOnBreaker (quadro)
    - V5 StepCompleteOnTriggerZone (quintal da frente)
    - V6 StepCompleteOnOpenings (porta; targetOpen = false; requirePlayerInZone = quintal da frente)
    - V7 StepCompleteOnButtonPress ← EmergencyCallPanel.onAcceptedCall (acceptedNumbers = ["193"])
    - V8 StepCompleteOnInteractCount (vizinho)
12. ActivateOnStepCompleted (sob SafeZone_Gameplay/Phases):
    - passo 5 → liga a zona voltar_para_dentro (interior da casa);
    - passo 7 → liga o vizinho e a sirene;
    - passo 8 → liga o caminhão dos Bombeiros.
13. ScenarioPhaseSwitcher:
    - "Rack": triggerSteps = [p1, p2, p3], sem fade;
      onEnter → Rack.SparkThenIgnite(), SmokeLayer.Begin(), Detector.Play(), troca da mensagem da jarra (10.3);
      info = "Faíscas no benjamim atrás da TV! Não use água em aparelhos ligados."
    - "Evacuar": triggerSteps = [p4], sem fade; activate = [Cobertor];
      info = "O fogo cresceu. Não tente apagar sozinho: abaixe-se e saia de casa."
14. UI: OptionsPanel, WristUI (com botão Ligar), EmergencyCallPanel (falas da seção 5), IntroBriefing
    (título + texto da seção 3), EndReport, legenda de fase presa à câmera.
15. MarkSceneDirty + SaveScene + QuestProjectConfigurator.RegisterScenes().
```

### 10.3 Trechos que exigem atenção

**Trocar a mensagem da jarra na fase Rack** (listeners persistentes de string nos setters do `WrongActionInteractable`):

```csharp
var rack = phaseSwitcher.phases.Find(p => p.name == "Rack");
var setId  = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), jarraWrong, "set_mistakeId");
var setMsg = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), jarraWrong, "set_message");
UnityEventTools.AddStringPersistentListener(rack.onEnter, setId, "agua_no_aparelho");
UnityEventTools.AddStringPersistentListener(rack.onEnter, setMsg, "Nunca jogue água em aparelho elétrico ligado: risco de choque. Desligue a energia.");
UnityEventTools.AddPersistentListener(rack.onEnter, rackFire.SparkThenIgnite);
UnityEventTools.AddPersistentListener(rack.onEnter, smokeLayer.Begin);
UnityEventTools.AddPersistentListener(rack.onEnter, detector.Play);
```
O `FlareOnSelect` da jarra não precisa mudar: ele só dá o "puff" se a panela ainda estiver acesa.

**Ligar o painel de ligação ao validador:**

```csharp
var v7 = UIBuilderUtil.Child(validators, "Validator_07_Ligar").AddComponent<StepCompleteOnButtonPress>();
v7.scenarioManager = manager; v7.step = data.ligar;
callPanel.acceptedNumbers.Clear(); callPanel.acceptedNumbers.Add("193");
UnityEventTools.AddPersistentListener(callPanel.onAcceptedCall, v7.OnButtonPressed);
```

**Detector parando após tampar:** `UnityEventTools.AddFloatPersistentListener(lidCooldown.onCovered, detector.StopAfter, 20f)`.

## 11. Integração no projeto

| Arquivo | Mudança |
|---------|---------|
| `Editor/SafeZoneDataBuilder.cs` | `StepAt`/`CardAt`, `BuildIncendio()` e o novo catálogo (seção 8.1) |
| `Data/MissionStepSO.cs`, `Runtime/Core/ScenarioManager.cs`, `Runtime/UI/WristUIController.cs` | grupos de ordem + `ShowInfo`/`onInfo` (9.2) |
| `Editor/ScenarioSceneKit.cs` e `Prefabs/Casa_SafeZone.prefab` | novos (9.1) + helper `CreateKnob`; vão da janela da cozinha no prefab (10.1) |
| `Runtime/...` | componentes da seção 9 (os compartilhados só uma vez) |
| `Runtime/Core/ProceduralAudio.cs` | `CreateCrackleLoop`, `CreateSmokeAlarmLoop`, `CreateDistantSirenLoop` (9.14) |
| `Editor/IncendioGameplayBuilder.cs` | novo; `[MenuItem("SafeZone VR/Build/2b. Fase Incêndio")]` chamando `Build()` + diálogo |
| `Editor/QuestProjectConfigurator.cs` | `k_IncendioScenePath = "Assets/Scenes/Fases/Incendio_EmCasa.unity"` em `RegisterScenes()` |
| `Editor/SafeZoneBuildAll.cs` | chamar `IncendioGameplayBuilder.Build()` antes de `MainMenuBuilder.Build()` |
| *Project Settings → Graphics → Shader Stripping* | manter a névoa **Exponential Squared** (9.13) |
| `Documentos_Orientações_Defesa_Civil/` | PDFs das páginas dos Bombeiros usadas como fonte (seção "Fontes oficiais") |
| `CONFIGURATION_GUIDE.md` e `README.md` | tabela de missões da nova fase; o README deixa de citar deslizamento, enxurrada e vendaval como próximos cenários |

O menu principal não precisa mudar. O botão "Incêndio em Casa" aparece jogável sozinho, porque o
`ScenarioSO` tem `isAvailable = true` e `sceneName` preenchido.

## 12. Checklist de implementação e aceite

**Ordem sugerida**
1. [ ] Pré-requisitos comuns (9.1 e 9.2), se a fase Granizo ainda não os criou. Regenere o Alagamento e repita os testes dele.
2. [ ] Pré-requisitos de modelagem (10.1): vão da janela e porta operável.
3. [ ] Componentes da seção 9 + sons da 9.14.
4. [ ] `BuildIncendio()` e novo catálogo (seção 8).
5. [ ] `IncendioGameplayBuilder` (seção 10) e integração (seção 11).
6. [ ] Arte definitiva: chamas e fumaça (texturas de partícula leves), panela, tampa, botijão, benjamim, vizinho e caminhão. Os primitivos servem até lá.
7. [ ] Revisão do texto com um oficial dos Bombeiros ou da Defesa Civil local.
8. [ ] Teste no Editor (roteiro abaixo) e no Quest 3 (72 FPS, Build and Run).

**Critérios de aceite**
- [ ] O menu mostra Alagamento, Incêndio em Casa e Granizo; Incêndio carrega com fade.
- [ ] As 8 missões concluem pelos gatilhos da seção 5; as missões 1 e 2 em qualquer ordem **sem** penalidade.
- [ ] A tampa apaga a panela. Tirá-la antes de 45 s reacende o fogo e registra `tampa_cedo`. A jarra dá o "puff" e registra `agua_no_oleo`.
- [ ] O fogo do rack acende com faíscas, cresce sozinho e **não apaga** ao desligar o quadro.
- [ ] A fumaça desce; em pé dentro dela a visão fica cinza e o erro aparece (no máximo 1 a cada 10 s); abaixado, a visão fica limpa.
- [ ] Modo sentado: fumaça em 1,6 m, sem o erro e com a dica no relógio.
- [ ] A porta só conclui a missão 6 quando é fechada com o jogador do lado de fora; a chave registra `trancar_porta`.
- [ ] Ligar 199, 190 ou 192 orienta a ligar 193, sem concluir e sem erro; 193 conclui.
- [ ] O vizinho chega depois da missão 7, é impedido e vai para o ponto de encontro; o caminhão chega no fim.
- [ ] O relatório mostra estrelas, os 10 cards com as fontes e o recorde (`safezone.best.*.incendio`).
- [ ] Console sem erros ao entrar e sair do Play Mode e ao voltar ao menu; a névoa volta ao normal ao sair da cena.
- [ ] Quest 3: ≥ 72 FPS estáveis com o fogo grande e a fumaça, ≤ 150 draw calls; a névoa funciona **no build**.
- [ ] Revisão de tom (RNF03): chamas contidas, nenhum ferido, sem roupas em chamas, sem tela vermelha nem tremor de câmera.

**Roteiro de teste no Editor (via MCP, como no Alagamento)**
1. **Salvar o recorde:** ler e guardar `safezone.best.time.incendio` e `safezone.best.stars.incendio`, e restaurar no fim. Não edite scripts com o Play Mode ativo.
2. **Panela (missões 1–3):**
   - Entrar no Play Mode e chamar `IntroBriefingPanel.Begin()`.
   - Selecionar a jarra: confere o `agua_no_oleo` e o "puff".
   - Girar o botão do fogão e o registro do botijão (`XRKnob.value`).
   - Encaixar a tampa com `StartManualInteraction` no socket. Confere que o fogo apaga.
   - Retirar a tampa em 5 s para conferir `tampa_cedo` e o fogo voltando; tampar de novo.
   - Abrir a janela com `ToggleOpening.SetOpen(true)`.
3. **Rack (missões 4–5):**
   - Conferir as faíscas e o rack acendendo; `SmokeLayer` ativo.
   - Desligar o quadro com `BreakerLever.SetState(false)`. Confere que o rack não apaga.
   - Levar o XR Origin ao corredor **com a câmera baixa**: reduzir `XROrigin.CameraYOffset` para 0,9 e conferir que não há erro. Repetir em pé para conferir `em_pe_na_fumaca`.
   - Levar o XR Origin ao quintal da frente.
4. **Fora de casa (missões 6–8):**
   - Missão 6: `SetOpen(false)` na porta com o jogador no quintal.
   - Missão 7: invocar primeiro o 199, que não conclui, e depois o 193.
   - Missão 8: selecionar o vizinho.
5. **Resultado:**
   - `ScenarioManager.isComplete` e `outOfOrderCount == 0`;
   - relatório visível e captura de câmera do relatório;
   - console sem erros e `RenderSettings.fog` restaurado.
