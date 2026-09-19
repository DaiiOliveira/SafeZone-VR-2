# Especificação de Requisitos de Software (ERS)
## SafeZone-VR — Simulador de Treinamento em Emergências em Realidade Virtual

| Campo | Valor |
|---|---|
| Projeto | SafeZone-VR |
| Documento | Especificação de Requisitos de Software |
| Versão | 1.1 |
| Data | 19/09/2026 |
| Autora | Dai Oliveira |
| Revisão 1.1 | Ajuste ao produto entregue |
| Repositório | https://github.com/DaiiOliveira/SafeZone-VR |
| Norma de referência | ISO/IEC/IEEE 29148:2018 (estrutura adaptada da IEEE 830) |

> **Nota sobre esta revisão:** a versão 1.0 foi escrita antes da implementação, a partir do escopo informado.
> Esta versão 1.1 foi ajustada ao **produto efetivamente entregue**: requisitos não implementados foram removidos,
> requisitos parcialmente atendidos foram reescritos para descrever o que existe, e as premissas marcadas
> **[CONFIRMAR]** foram substituídas pela stack real. O que foi construído além do previsto está na seção 10.
>
> As decisões que levaram a cada remoção estão registradas em `LEVANTAMENTO_REQUISITOS_ERS.md`, seção 2.1.

---

## 1. Introdução

### 1.1 Propósito

Este documento especifica os requisitos funcionais e não funcionais do **SafeZone-VR**, um simulador em realidade virtual voltado ao treinamento de procedimentos de segurança em três cenários de emergência: alagamento, tempestade de granizo e incêndio.

O público-alvo do documento são: a autora do trabalho, o orientador, avaliadores da banca e eventuais desenvolvedores que venham a manter ou estender o sistema.

### 1.2 Escopo do produto

O SafeZone-VR é uma aplicação de realidade virtual imersiva na qual o usuário é colocado em ambientes tridimensionais sob situação de emergência e deve executar as ações corretas de autoproteção, abrigo e evacuação. O sistema avalia as decisões tomadas e apresenta um relatório de desempenho ao final de cada fase.

**Está no escopo:**

- Três fases jogáveis independentes: alagamento, granizo e incêndio;
- Interação em VR com objetos do cenário (portas, janelas, disjuntores, extintores, objetos de proteção);
- Sistema de avaliação de desempenho por fase, com pontuação e feedback educativo;
- Hub tridimensional de seleção de fase e configurações de conforto;
- Registro local da melhor pontuação e do melhor tempo por fase.

**Não está no escopo (nesta versão):**

- Modo multiusuário ou cooperativo;
- Backend remoto, contas de usuário em nuvem ou ranking online;
- Versão para desktop sem VR (modo *flat screen*);
- Fases adicionais (deslizamento de terra, descargas atmosféricas, acidente químico), previstas apenas como evolução futura;
- Integração com sistemas institucionais de treinamento (LMS/SCORM);
- Tutorial guiado de familiarização (substituído por briefing por fase, dicas por inatividade e aviso inicial);
- Histórico de tentativas e exportação de resultados (guarda-se apenas a melhor tentativa).

### 1.3 Objetivos do sistema

| ID | Objetivo |
|---|---|
| OBJ01 | Ensinar, por meio de vivência prática simulada, os procedimentos corretos diante de alagamento, tempestade de granizo e incêndio. |
| OBJ02 | Permitir a prática de situações de risco sem exposição física real do treinando. |
| OBJ03 | Fornecer retorno objetivo sobre os acertos e erros cometidos durante a simulação. |
| OBJ04 | Corrigir concepções equivocadas frequentes (por exemplo, retorno ao interior de imóvel em chamas, travessia de área alagada ou permanência sob árvores durante tempestade de granizo). |

### 1.4 Definições, acrônimos e abreviações

| Termo | Definição |
|---|---|
| **VR / RV** | *Virtual Reality* — Realidade Virtual. |
| **HMD** | *Head-Mounted Display* — visor de realidade virtual acoplado à cabeça. |
| **6DoF** | Seis graus de liberdade: rastreamento de posição e rotação da cabeça e das mãos. |
| **Fase** | Cenário jogável completo, correspondente a um tipo de emergência. |
| **Evento crítico** | Ação obrigatória cuja execução (ou omissão) determina o resultado da fase. |
| **Ponto de encontro** | Local seguro predefinido onde o usuário deve chegar para concluir a evacuação. |
| **Granizo** | Precipitação sólida em forma de pedras de gelo, associada a tempestades severas, frequentemente acompanhada de rajadas de vento e descargas atmosféricas. |
| **Abrigo seguro** | Ambiente interno de alvenaria, afastado de aberturas envidraçadas e de coberturas frágeis, onde o usuário deve permanecer durante a tempestade. |
| ***Cybersickness*** | Mal-estar (náusea, tontura) induzido pelo uso de RV. |
| **Teleporte** | Modo de locomoção por saltos instantâneos, com menor indução de *cybersickness*. |
| **Locomoção contínua** | Deslocamento suave por analógico, mais imersivo e mais propenso a *cybersickness*. |
| **PASS** | Mnemônico para uso de extintor: Puxar o pino, Apontar para a base, Apertar o gatilho, Sacudir/varrer a base do fogo. |
| **RF / RNF / RN** | Requisito Funcional / Requisito Não Funcional / Regra de Negócio. |

### 1.5 Referências

- ISO/IEC/IEEE 29148:2018 — *Systems and software engineering — Life cycle processes — Requirements engineering*.
- ABNT NBR 9077 — Saídas de emergência em edifícios (referência conceitual para rotas de fuga, adaptada ao contexto residencial).
- ABNT NBR 12693 — Sistemas de proteção por extintores de incêndio.
- Manuais de orientação da Defesa Civil e do Corpo de Bombeiros Militar quanto a enchentes, tempestades severas e incêndios estruturais.
- Sistema de alertas meteorológicos do INMET e da Defesa Civil de Santa Catarina, quanto a avisos de tempestade com granizo.
- Documentação do Unity XR Interaction Toolkit e do padrão OpenXR.

---

## 2. Descrição geral

### 2.1 Perspectiva do produto

O SafeZone-VR é um produto autônomo (*standalone*), sem dependência de serviços externos em tempo de execução.
A arquitetura efetivamente empregada é:

- **Motor:** Unity 6000.5.10f1 com Universal Render Pipeline;
- **XR:** XR Interaction Toolkit 3.5.1 sobre OpenXR 1.17.1 com o conjunto de recursos Meta OpenXR;
- **Plataforma-alvo:** Meta Quest 3 e 3S em execução autônoma. PC VR **não** está configurado;
- **Compilação:** IL2CPP, ARM64, Vulkan, espaço de cor linear, meta de 72 fps;
- **Persistência:** `PlayerPrefs` no dispositivo;
- **Distribuição:** APK instalado por *sideload* (`adb install`).

### 2.2 Funções principais do produto

1. Hub tridimensional de seleção de fase;
2. Execução das três fases de emergência;
3. Detecção e avaliação das ações do usuário;
4. Geração de relatório de desempenho ao final de cada fase, com cards educativos da Defesa Civil;
5. Configuração de parâmetros de conforto e acessibilidade;
6. Persistência da melhor pontuação e do melhor tempo por fase.

### 2.3 Características dos usuários

| Perfil | Descrição | Implicação para os requisitos |
|---|---|---|
| **Treinando iniciante em VR** | Usuário sem experiência prévia com HMD; pode sentir desconforto e ter dificuldade com os controles. | Exige briefing no início da fase, dicas por inatividade, teleporte como padrão e vinheta de conforto ativa. |
| **Treinando experiente** | Usuário familiarizado com VR, busca repetir fases para melhorar a pontuação. | Exige locomoção contínua opcional e registro da melhor pontuação para comparação entre tentativas. |
| **Instrutor / avaliador** | Docente ou responsável por segurança que acompanha a sessão e consulta os resultados. | Exige relatório de desempenho legível ao final da fase. |

### 2.4 Restrições

| ID | Restrição |
|---|---|
| RES01 | O sistema deve executar em hardware autônomo de VR, cujo orçamento gráfico é limitado — o que restringe densidade de polígonos, resolução de texturas e uso de iluminação dinâmica. |
| RES02 | A taxa de quadros não pode cair abaixo do mínimo especificado em RNF01, sob risco de indução de *cybersickness*. |
| RES03 | Toda a interação deve ser possível apenas com dois controles 6DoF, sem teclado ou mouse. |
| RES04 | O conteúdo procedimental das fases deve estar aderente às orientações oficiais brasileiras de Defesa Civil e Corpo de Bombeiros. |
| RES05 | A sessão de uso contínuo não deve exceder a duração recomendada por fase (RNF07), por conforto e higiene do equipamento. |
| RES06 | O sistema deve funcionar integralmente offline. |
| RES07 | O cenário-base é uma residência unifamiliar; requisitos que pressupõem edificação coletiva (elevador, escadas de emergência, brigada, alarme predial) devem ser adaptados a equivalentes domésticos. |

### 2.5 Suposições e dependências

| ID | Suposição |
|---|---|
| SUP01 | O usuário dispõe de área física livre mínima compatível com o *room-scale* configurado no dispositivo, ou joga sentado/em pé sem deslocamento real. |
| SUP02 | O dispositivo já possui limites de segurança (*guardian/boundary*) configurados pelo sistema operacional do HMD. |
| SUP03 | As três fases se passam em uma residência unifamiliar **térrea**, com quintal, garagem e telhado em telhas cerâmicas, e na praça do outro lado da rua. O mesmo cenário-base é reaproveitado pelas três fases, variando o evento simulado, os danos e os objetos interativos relevantes. |
| SUP04 | Há sempre um acompanhante presente durante a sessão para auxiliar em caso de desconforto do usuário. |

---

## 3. Requisitos funcionais

Prioridade: **Essencial** (indispensável à entrega), **Importante** (agrega valor significativo), **Desejável** (pode ser postergado).

### 3.1 Módulo — Menu e navegação

| ID | Requisito | Prioridade |
|---|---|---|
| RF002 | O sistema deve permitir a seleção individual de uma das três fases (alagamento, granizo, incêndio), exibindo para cada uma o título, uma breve descrição e a melhor pontuação já obtida. | Essencial |
| RF003 | O sistema deve permitir que o usuário interrompa a fase em andamento a qualquer momento e retorne ao menu principal, descartando a tentativa. | Essencial |
| RF004 | O sistema deve permitir reiniciar a fase corrente sem retornar ao menu principal. | Importante |

### 3.2 Módulo — Tutorial e conforto

| ID | Requisito | Prioridade |
|---|---|---|
| RF008 | O sistema deve permitir a escolha entre locomoção por teleporte e locomoção contínua, adotando o teleporte como padrão. | Essencial |
| RF009 | O sistema deve permitir a escolha entre rotação por incrementos (*snap turn*) e rotação suave, adotando a rotação por incrementos como padrão. | Essencial |
| RF010 | O sistema deve permitir ativar ou desativar a vinheta de conforto (escurecimento periférico durante o deslocamento), mantendo-a ativa por padrão. | Essencial |
| RF012 | O sistema deve permitir a definição da mão dominante, espelhando as funções dos controles. | Desejável |
| RF013 | O sistema deve permitir o ajuste independente do volume dos efeitos sonoros, da narração e da trilha ambiente. | Importante |

### 3.3 Módulo — Interação comum às fases

| ID | Requisito | Prioridade |
|---|---|---|
| RF014 | O sistema deve permitir ao usuário agarrar, soltar e arremessar objetos interativos do cenário com qualquer uma das mãos. | Essencial |
| RF015 | O sistema deve destacar visualmente os objetos interativos quando a mão do usuário estiver a distância de alcance. | Essencial |
| RF016 | O sistema deve permitir abrir e fechar portas por interação física com a maçaneta. | Essencial |
| RF017 | O sistema deve permitir acionar interruptores, disjuntores e alarmes por toque ou pressão do gatilho. | Essencial |
| RF018 | O sistema deve exibir orientações contextuais discretas (texto flutuante ou narração) quando o usuário permanecer inativo por período superior ao limite definido em RN06. | Importante |
| RF019 | O sistema deve emitir realimentação tátil (vibração do controle) ao agarrar objetos, ao colidir com obstáculos e ao executar ações críticas. | Importante |
| RF020 | O sistema deve impedir que o usuário atravesse paredes e objetos sólidos, aplicando bloqueio de deslocamento ou escurecimento da visão. | Essencial |
| RF021 | O sistema deve exibir, durante toda a fase, um indicador de tempo decorrido e o objetivo corrente. | Importante |

### 3.4 Fase 1 — Alagamento

**Contexto:** residência térrea atingida por enchente, com nível de água em elevação progressiva. O ponto de encontro elevado, na calçada em frente à casa, é o refúgio seguro do cenário.

| ID | Requisito | Prioridade |
|---|---|---|
| RF022 | O sistema deve simular a elevação progressiva do nível da água ao longo da fase, com taxa de subida configurável por parâmetro. | Essencial |
| RF023 | O sistema deve reduzir a velocidade de deslocamento do usuário proporcionalmente à profundidade da água em que ele se encontra. | Essencial |
| RF024 | O sistema deve exigir, como evento crítico, o desligamento do quadro de disjuntores antes que a água atinja tomadas e equipamentos elétricos. | Essencial |
| RF025 | O sistema deve registrar erro grave e encerrar a fase em condição de falha caso o usuário toque em equipamento elétrico energizado estando em contato com a água. | Essencial |
| RF026 | O sistema deve exigir, como evento crítico, a coleta de itens essenciais do kit de emergência (documentos, água potável, lanterna, medicamentos) antes da evacuação. | Essencial |
| RF030 | O sistema deve concluir a fase com sucesso quando o usuário alcançar o pavimento superior ou o ponto elevado seguro definido no cenário. | Essencial |

### 3.5 Fase 2 — Granizo

**Contexto:** o usuário inicia a fase na praça, do outro lado da rua de casa, sob alerta meteorológico iminente. Deve orientar o vizinho sobre onde estacionar, conduzir um idoso com andador até a UBS da esquina, permanecer abrigado durante a precipitação e, ao final, voltar para casa e vistoriar os danos.

| ID | Requisito | Prioridade |
|---|---|---|
| RF032 | O sistema deve apresentar, no início da fase, sinais de aproximação da tempestade: escurecimento do céu, névoa e alerta textual da Defesa Civil exibido ao usuário. | Essencial |
| RF034 | O sistema deve simular a precipitação de granizo com intensidade crescente, com impacto visual e sonoro sobre telhados e solo, e com acúmulo progressivo de pedras de gelo no terreno. | Essencial |
| RF037 | O sistema deve registrar erro grave caso o usuário se abrigue sob árvore, pergolado de ripas, ponto de ônibus metálico ou barraca de cobertura frágil. | Essencial |
| RF045 | O sistema deve registrar erro grave caso o usuário suba ao telhado ou tente reparar a cobertura enquanto a tempestade estiver em curso. | Essencial |
| RF047 | O sistema deve encerrar a precipitação após o intervalo definido para o cenário e simular os danos resultantes: telhas quebradas no quintal, forro cedendo com goteira no quarto e pedras de gelo acumuladas. | Essencial |
| RF050 | O sistema deve concluir a fase com sucesso quando o usuário tiver permanecido abrigado durante a precipitação, vistoriado os danos e comunicado a ocorrência à Defesa Civil. | Essencial |
| RF051 | O sistema deve apresentar, no relatório final, esclarecimento quanto ao risco de abrigar-se sob árvores durante tempestades, caso o usuário adote esse comportamento. | Desejável |

### 3.6 Fase 3 — Incêndio

**Contexto:** princípio de incêndio iniciado na cozinha da residência, com propagação de fogo e fumaça ao longo do tempo. A saída pela porta principal é a rota de fuga primária e o portão da frente é o ponto de encontro.

| ID | Requisito | Prioridade |
|---|---|---|
| RF052 | O sistema deve simular a propagação progressiva do fogo e o acúmulo de fumaça, reduzindo a visibilidade de cima para baixo ao longo do tempo. | Essencial |
| RF053 | O sistema deve exigir, como evento crítico, o alerta aos demais moradores da residência e o acionamento do serviço de emergência (193) a partir do telefone do cenário, após a saída do imóvel. | Essencial |
| RF056 | O sistema deve permitir o combate ao foco apenas enquanto este for classificado como princípio de incêndio, e registrar erro grave caso o usuário insista em combater foco já propagado em vez de evacuar. | Essencial |
| RF058 | O sistema deve exigir o deslocamento agachado nas áreas com fumaça, reduzindo a integridade do usuário caso ele permaneça em pé na camada de fumaça. | Essencial |
| RF059 | O sistema deve registrar erro grave caso o usuário retorne ao interior da residência após ter saído, seja para buscar pertences, documentos ou animais. | Essencial |
| RF064 | O sistema deve concluir a fase com sucesso quando o usuário alcançar o ponto de encontro externo. | Essencial |

### 3.7 Módulo — Avaliação e relatório

| ID | Requisito | Prioridade |
|---|---|---|
| RF065 | O sistema deve registrar, durante a execução da fase, todas as ações relevantes do usuário, com identificação da ação, instante de ocorrência e classificação (acerto, erro leve ou erro grave). | Essencial |
| RF066 | O sistema deve calcular a pontuação final da fase conforme as regras definidas em RN01 a RN05. | Essencial |
| RF067 | O sistema deve exibir, ao término da fase, um relatório contendo: resultado (sucesso ou falha), pontuação, tempo de conclusão, lista de acertos e lista de erros. | Essencial |
| RF068 | O sistema deve apresentar, para cada erro cometido, explicação textual do procedimento correto e da razão pela qual a ação adotada é inadequada. | Essencial |
| RF069 | O sistema deve exibir a classificação de desempenho obtida, conforme a faixa de pontuação definida em RN05. | Importante |
| RF070 | O sistema deve armazenar localmente o resultado de cada tentativa, preservando a melhor pontuação por fase. | Essencial |

---

## 4. Regras de negócio

| ID | Regra |
|---|---|
| RN01 | Cada fase inicia com 100 pontos. |
| RN02 | Cada erro leve (procedimento subótimo, omissão de item não essencial) subtrai 5 pontos. |
| RN03 | Cada erro grave (ação que, na realidade, implicaria risco de morte) subtrai 25 pontos. |
| RN04 | A conclusão da fase acima do tempo-limite definido para o cenário subtrai 10 pontos; o tempo-limite não encerra a fase por si só. |
| RN05 | A classificação de desempenho segue as faixas: 90–100 pontos, Excelente; 70–89, Adequado; 50–69, Requer revisão; abaixo de 50, Insuficiente — com recomendação de repetição da fase. |
| RN06 | Após 30 segundos de inatividade do usuário, o sistema exibe orientação contextual; após 60 segundos, exibe orientação explícita quanto à próxima ação esperada. |
| RN07 | A fase é encerrada em condição de falha quando a integridade do usuário chega a zero ou quando é cometido erro grave classificado como fatal no cenário. |
| RN08 | Erros graves permanecem registrados no relatório ainda que o usuário execute posteriormente o procedimento correto. |

---

## 5. Requisitos não funcionais

### 5.1 Desempenho

| ID | Requisito |
|---|---|
| RNF01 | A aplicação deve manter no mínimo 72 quadros por segundo em execução autônoma no HMD-alvo, sem quedas sustentadas abaixo desse valor. O alvo é o Quest 3/3S autônomo; PC VR não faz parte da entrega. |
| RNF02 | A latência entre o movimento da cabeça do usuário e a atualização da imagem exibida deve permanecer abaixo de 20 ms. |
| RNF03 | O carregamento de uma fase não deve exceder 15 segundos, exibindo-se indicação de progresso em ambiente estático durante a espera. |
| RNF04 | Os efeitos de água, fogo e fumaça devem ser implementados com técnicas compatíveis com o orçamento gráfico do dispositivo autônomo, sem comprometer o requisito RNF01. |

> **Não verificados no dispositivo:** RNF01, RNF02, RNF04, RNF07, RNF08 e RNF09 são critérios de aceitação que
> dependem de medição no Quest 3 com APK de desenvolvimento e usuários reais. Até esta revisão a validação foi
> feita apenas no editor. Permanecem na especificação por serem critérios de aceitação, não funcionalidades.

### 5.2 Usabilidade e conforto

| ID | Requisito |
|---|---|
| RNF05 | Todas as configurações de conforto (RF008 a RF010, RF012 e RF013) devem ser acessíveis tanto no hub quanto durante a fase, sem perda do progresso. |
| RNF06 | O sistema não deve, em nenhuma circunstância, mover a câmera do usuário sem comando dele, nem aplicar aceleração ou rotação forçada do ponto de vista. |
| RNF07 | Cada fase deve ser concluível em até 10 minutos por usuário de desempenho mediano, limitando a exposição contínua ao HMD. |
| RNF08 | Textos exibidos em ambiente virtual devem ser legíveis a partir da distância típica de interação, com corpo e contraste adequados à resolução do HMD-alvo. |
| RNF09 | Elementos de interface fixados ao campo de visão devem ser posicionados em profundidade confortável, evitando conflito de convergência e acomodação. |

### 5.3 Acessibilidade

| ID | Requisito |
|---|---|
| RNF13 | Todas as ações do jogo devem ser executáveis sem exigir que o usuário se agache fisicamente, disponibilizando-se alternativa por comando do controle. |

### 5.4 Segurança do usuário

| ID | Requisito |
|---|---|
| RNF15 | O sistema deve exibir, antes da primeira sessão, aviso quanto a riscos de desconforto, à necessidade de área física livre e à orientação de interromper o uso em caso de mal-estar. |
| RNF16 | O sistema deve disponibilizar interrupção da simulação a qualquer momento, pelo painel de opções acessível no relógio de pulso, com retorno ao hub. |

### 5.5 Confiabilidade e manutenibilidade

| ID | Requisito |
|---|---|
| RNF19 | O conteúdo de cada fase (cenário, passos de missão, textos, cards educativos e metas de tempo) deve ser externalizado em objetos parametrizáveis (`ScriptableObject`), sem necessidade de alteração de código. |
| RNF20 | O sistema não deve depender de conexão de rede para nenhuma de suas funcionalidades. |
| RNF22 | A arquitetura deve permitir a inclusão de novas fases mediante reaproveitamento dos módulos de interação, avaliação e relatório. |
| RNF23 | O código-fonte deve ser versionado em repositório Git, com histórico de commits descritivo. |

### 5.6 Portabilidade

| ID | Requisito |
|---|---|
| RNF24 | A aplicação deve ser construída sobre o padrão OpenXR, de modo a permitir a execução em dispositivos de fabricantes distintos com alteração mínima de código. |
| RNF25 | O mapeamento de controles deve ser abstraído por camada de entrada, sem dependência direta de um modelo específico de controle. |

### 5.7 Localização

| ID | Requisito |
|---|---|
| RNF26 | Todo o conteúdo textual e sonoro deve ser apresentado em português brasileiro. |
| RNF27 | Os textos de missão, de erro e dos cards educativos devem ser mantidos em objetos de dados externos ao código, viabilizando revisão e tradução futura. |

---

## 6. Casos de uso principais

### CSU01 — Executar fase de emergência

- **Ator:** Treinando
- **Pré-condições:** HMD posicionado, limites de segurança configurados e aviso inicial de segurança aceito.
- **Fluxo principal:**
  1. O treinando seleciona uma fase no menu principal.
  2. O sistema carrega o cenário e apresenta a situação inicial.
  3. O sistema inicia a simulação do evento de emergência e a contagem de tempo.
  4. O treinando executa ações de proteção e evacuação.
  5. O sistema registra e classifica cada ação relevante.
  6. O treinando alcança o ponto seguro do cenário.
  7. O sistema encerra a fase, calcula a pontuação e exibe o relatório de desempenho.
- **Fluxos alternativos:**
  - 4a. O treinando permanece inativo além do limite de RN06 → o sistema exibe orientação contextual.
  - 4b. O treinando comete erro grave fatal → o sistema encerra a fase em condição de falha e exibe o relatório (RN07).
  - 4c. O treinando interrompe a sessão → o sistema descarta a tentativa e retorna ao menu (RF003).
- **Pós-condições:** Melhor pontuação e melhor tempo da fase atualizados no dispositivo, se superados.

### CSU02 — Configurar conforto

- **Ator:** Treinando
- **Fluxo principal:** o treinando acessa as configurações, ajusta modo de locomoção, tipo de rotação, vinheta, modo sentado/em pé, mão dominante e volumes por categoria; o sistema aplica as alterações imediatamente e as persiste para sessões futuras.

---

## 7. Matriz de rastreabilidade (objetivos × requisitos)

| Objetivo | Requisitos relacionados |
|---|---|
| OBJ01 | RF022–RF026, RF030, RF032, RF034, RF037, RF045, RF047, RF050–RF053, RF056, RF058, RF059, RF064, RF018 |
| OBJ02 | RF025, RF056, RF058, RNF15, RNF16 |
| OBJ03 | RF065–RF070, RN01–RN08 |
| OBJ04 | RF037, RF045, RF051, RF059, RF068 |

---

## 8. Evolução futura

| ID | Item |
|---|---|
| EV01 | Novas fases: deslizamento de terra, descargas atmosféricas, acidente com produtos perigosos. |
| EV02 | Modo cooperativo com múltiplos usuários no mesmo cenário. |
| EV03 | Painel web para instrutores, com consolidação de resultados de turmas. |
| EV04 | Registro e reprodução da trajetória do usuário para análise posterior. |
| EV05 | Variação procedural do cenário a cada tentativa, evitando memorização do percurso. |
| EV06 | Suporte a rastreamento de mãos sem controles. |

---

## 9. Histórico de revisões

| Versão | Data | Autor | Descrição |
|---|---|---|---|
| 1.0 | 10/09/2026 | Dai Oliveira | Versão inicial, cobrindo as fases de alagamento, granizo e incêndio. |
| 1.1 | 19/09/2026 | Revisão de escopo | Ajuste ao produto entregue: 30 RF, RN09, CSU03 e 7 RNF removidos; RF032, RF034, RF037, RF047, RF050, RNF05, RNF16, RNF19 e RNF27 reescritos conforme o implementado; premissas **[CONFIRMAR]** substituídas pela stack real; incluída a seção 10 com o que foi entregue além do previsto. |

---

## 10. Entregue além da especificação

Itens construídos que não constavam da versão 1.0 e que passam a fazer parte do produto.

### 10.1 Navegação e orientação

| ID | Item |
|---|---|
| EX01 | **Hub tridimensional da Defesa Civil**: a seleção de fase acontece em três totens de um terraço navegável, cada um com nome, resumo e melhor pontuação da fase, no lugar de um menu em lista. |
| EX02 | **Relógio de pulso** com a missão corrente, instrução, progresso, tempo decorrido e avisos, preso à mão não dominante e acompanhando a troca de mão dominante. |
| EX03 | **Bússola diegética** no relógio, apontando para o objetivo pendente mais próximo. |
| EX04 | **Marcadores flutuantes** sobre os alvos da missão corrente, que desaparecem conforme cada alvo é resolvido. |
| EX05 | **Setas verdes no piso** marcando a rota segura de evacuação, acesas apenas na missão de evacuar. |
| EX06 | **Briefing inicial** por fase, com o contexto da ocorrência e a lista de missões; o cronômetro só começa quando o usuário confirma. |

### 10.2 Conteúdo e simulação

| ID | Item |
|---|---|
| EX07 | **Painel de ligação de emergência** com 199, 193, 190 e 192, cada número com resposta própria; apenas o número correto conclui a missão. |
| EX08 | **Cards educativos da Defesa Civil** no relatório final, com a orientação oficial correspondente aos erros cometidos. |
| EX09 | **NPCs animados** com rig humanoide e estados Parado, Andando e Apontar: o vizinho que tenta entrar na casa em chamas, o Seu Joaquim com andador e o Carlos com o carro. |
| EX10 | **Caminhão dos Bombeiros** que chega dirigindo pela rua com sirene espacializada, substituindo a sirene distante quando estaciona. |
| EX11 | **Iluminação da casa ligada ao quadro de energia**: 22 luminárias que se apagam quando o disjuntor geral é desligado. |
| EX12 | **Áudio gerado em tempo de execução**, sem arquivos de som: sinos de acerto, aviso, chuva, granizo, crepitar do fogo, alarme de fumaça e sirene. |
| EX13 | **Agachar por botão** (RNF13) com a câmera descendo 55 cm, para atravessar a camada de fumaça sem agachar fisicamente. |
| EX14 | **Tela de carregamento** com barra de progresso em ambiente estático entre as cenas. |

### 10.3 Produção e manutenção

| ID | Item |
|---|---|
| EX15 | **Cenas geradas por código**: cada fase é montada por um builder no editor, o que torna o cenário reproduzível e versionável em texto, em vez de editado à mão. |
| EX16 | **Importadores automáticos de FBX** para casa, praça, itens e NPCs: escala, eixos, colisores, materiais, rig e conversão dos marcadores de luz em Point Lights são configurados no import. |
| EX17 | **Modelagem própria** de casa, praça, UBS, NPCs e 21 itens, com atlas de textura compartilhado. |
| EX18 | **Configurador e script de build do APK** para Quest, com validação das regras do OpenXR e relatório das configurações aplicadas. |
| EX19 | **Documentação de apoio** no repositório: referência técnica do cenário, levantamento de requisitos, itens pendentes de modelagem e guia de qualidade de código. |
