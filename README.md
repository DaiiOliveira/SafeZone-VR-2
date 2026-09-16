# SafeZone VR

**Jogo educativo em Realidade Virtual que ensina como agir antes, durante e depois de desastres naturais — com base nas orientações oficiais da Defesa Civil.**

---

## A ideia do projeto

Todo ano o Brasil registra enchentes, deslizamentos, vendavais e outros desastres naturais que
poderiam ter consequências muito menores se as pessoas soubessem exatamente o que fazer nos
primeiros minutos. As orientações da **Defesa Civil** existem, são públicas e são boas — o
problema é que quase ninguém lê uma cartilha em PDF antes da emergência acontecer, e menos gente
ainda lembra dela na hora do susto.

O **SafeZone VR** nasce dessa lacuna: em vez de *ler* sobre o que fazer, a pessoa **vive a
situação** em realidade virtual e **pratica** as decisões corretas em um ambiente seguro. A
memória de ter feito vale muito mais do que a memória de ter lido.

Cada cenário do jogo é montado a partir das cartilhas oficiais da Defesa Civil (reunidas em
[`Documentos_Orientações_Defesa_Civil/`](Documentos_Orientações_Defesa_Civil)): alagamentos,
enxurradas, deslizamentos, chuvas intensas, granizo, vendaval, tornado, baixa umidade do ar,
onda de calor, rompimento de barragens e doenças infecciosas virais. As missões dentro do jogo
não são inventadas — elas são a tradução dessas recomendações em ações que o jogador executa
com as próprias mãos.

### Princípios de conscientização

- **Ensinar pela ação, não pelo susto.** O objetivo é empoderar quem joga, não traumatizar.
  Nada de sangue, ferimentos graves ou pânico: classificação livre, foco na atitude correta.
- **Fidelidade às fontes oficiais.** Toda orientação apresentada vem de material da Defesa Civil.
- **Aprendizado que fica.** Ao final de cada fase o jogador recebe um relatório com o tempo de
  reação, os acertos, os erros e *cards* educativos que reforçam o conteúdo.
- **Conforto em primeiro lugar.** Locomoção por teleporte, interface diegética e 72+ FPS para
  evitar enjoo — quem passa mal não aprende.

---

## Como o jogo funciona

1. O jogador escolhe um **cenário de desastre** (o primeiro implementado é *Alagamento em casa*).
2. A cena o coloca no ambiente já em situação de risco, com **missões sequenciais** exibidas em
   uma interface no pulso, como um relógio inteligente.
3. Cada missão é validada por uma **ação real dentro do mundo virtual** — pegar o kit de
   emergência, desligar a chave geral de energia, fechar o registro de água, proteger objetos de
   valor, evacuar pela rota segura.
4. Ao concluir, um **relatório final** mostra o desempenho e apresenta os cards educativos com as
   orientações da Defesa Civil por trás de cada passo.

### Cenário implementado: Alagamento em casa

| # | Missão | Orientação da Defesa Civil |
|---|--------|-----------------------------|
| 1 | Proteger documentos e objetos de valor (sofá → alto do armário) | Guardar documentos e valores em local alto e protegido |
| 2 | Desligar o quadro geral de energia (quarto) | Evitar choque elétrico e curto-circuito |
| 3 | Fechar o registro de entrada de água (garagem) | Impedir contaminação da rede interna |
| 4 | Montar o kit de emergência (água, remédios, lanterna → mochila na entrada) | Kit pronto para sair rápido; nunca usar velas |
| 5 | Evacuar pela rota sinalizada até o ponto de encontro | Procurar local alto; nunca atravessar ruas alagadas |

Decisão insegura registrada (sem punição, só aprendizado): tentar sair de carro.
Enquanto a fase corre, a água sobe lentamente (no máximo até a canela) para dar urgência sem pavor.

Cards educativos da fase: *nunca atravesse ruas alagadas*, *mantenha a calma*,
*desconfie de mensagens não oficiais*, *telefones úteis*, *depois que a água baixar*, *cuide de quem precisa*.

Ao final: relatório com estrelas (tempo + erros), pontuação, melhor resultado salvo e os cards.
Interface diegética: relógio no pulso esquerdo (missão, tempo, nível da água, bússola para o objetivo)
e painel de conforto (teleporte/contínuo, giro, vinheta, modo sentado, recentrar).

### Cenário implementado: Incêndio em casa

Fonte: orientações dos Corpos de Bombeiros Militares (SC, AL, CE e Vitória/ES). A casa é a mesma do
Alagamento (prefab `Casa_SafeZone`). Dois momentos: um princípio de incêndio controlável (panela) e um
incêndio que sai do controle (benjamim atrás da TV).

| # | Missão | Orientação oficial |
|---|--------|--------------------|
| 1 | Fechar o botão do fogão e o registro do botijão | Cortar o gás que alimenta o fogo |
| 2 | Abafar a panela com a tampa (nunca água) | Sem ar o fogo apaga; água em óleo espalha as chamas |
| 3 | Abrir a janela da cozinha e deixar esfriar | Ventilar a fumaça tóxica; tampa cedo reacende |
| 4 | Desligar o quadro de energia (faíscas na TV) | Nunca água em aparelho energizado |
| 5 | Sair abaixado pela porta da frente | Fumaça fica no alto; não combater sozinho |
| 6 | Fechar a porta sem trancar | Segura fogo e fumaça sem atrapalhar o socorro |
| 7 | Ligar 193 do lado de fora | Socorro sem ficar perto do fogo |
| 8 | Impedir o vizinho de entrar | Nenhum objeto vale o risco |

Decisões inseguras: jogar água no óleo/na TV, tirar a tampa cedo, salvar notebook/TV, abafar o fogo grande,
ficar em pé na fumaça, trancar a porta, voltar para dentro. Modo sentado: fumaça mais alta, sem penalidade.

### Cenário implementado: Granizo

Fonte: *Como agir? Granizo* (MDR/CENAD). Começa na praça do bairro e termina na casa do jogador.

| # | Missão | Orientação da Defesa Civil |
|---|--------|-----------------------------|
| 1 | Indicar ao motorista a vaga segura (longe de placa e torre) | Não estacionar perto de torres e placas |
| 2 | Ajudar o Seu Joaquim (andador) | Auxiliar idosos e pessoas com dificuldade |
| 3 | Levá-lo à UBS de alvenaria | Abrigo resistente, sem risco de destelhamento |
| 4 | Esperar o granizo passar dentro da UBS | Permanecer em local seguro |
| 5 | Voltar pela calçada com corrimão | Piso escorregadio depois do granizo |
| 6 | Conferir o forro do quarto | Telhas quebradas enfraquecem o telhado |
| 7 | Sair do quarto imediatamente | Risco de desabamento: sair na hora |
| 8 | Ligar 199 ou 193 | Comunicar às autoridades |

Decisões inseguras: abrigar-se sob a árvore, no ponto de ônibus metálico ou na barraca; sair durante o
granizo; usar a rampa gelada; subir na escada do telhado; voltar ao quarto.

---

## Tecnologia

| Item | Escolha |
|------|---------|
| Engine | Unity **6000.5.10f1** (URP) |
| Linguagem | C# |
| Plataforma-alvo | **Meta Quest 3 / Quest 3S** (Android standalone) |
| XR | XR Interaction Toolkit 3.5.1, OpenXR, Meta OpenXR, XR Hands |
| Meta de performance | 72+ FPS estáveis |

---

## Estrutura do repositório

```
SafeZone-VR/
├── Documentos_Orientações_Defesa_Civil/   # Cartilhas oficiais que embasam os cenários
├── SafeZone_VR_Requisitos_e_Skills.md     # Requisitos funcionais e não funcionais
└── UnityProject/
    ├── Assets/
    │   ├── Casa/                          # Modelo DC_Casa.fbx (casa das três fases) + importador
    │   ├── Praca/                         # Modelo DC_Praca.fbx (praça do Granizo) + importador
    │   ├── Scenes/Fases/                  # Alagamento_EmCasa, Incendio_EmCasa, Granizo_Praca
    │   └── SafeZoneVR/
    │       ├── Data/                      # ScriptableObjects: cenários, missões, cards
    │       ├── Editor/                    # Geradores de cena, configuração do Quest e build do APK
    │       ├── Materials/
    │       ├── Models/                    # DC_Hub.fbx (hub do menu principal)
    │       └── Runtime/
    │           ├── Core/                  # ScenarioManager, identificação de objetos
    │           ├── UI/                    # Interface de pulso e relatório final
    │           └── Validators/            # Conclusão de missão por grab, socket, zona, válvula
    ├── Packages/
    └── ProjectSettings/
```

O conteúdo é orientado a dados: cenários, passos e cards são **ScriptableObjects**
(`ScenarioSO`, `MissionStepSO`, `EducationCardSO`), então criar uma nova fase é escrever
conteúdo — não reescrever sistemas.

---

## Como abrir o projeto

1. Instale o **Unity 6000.5.10f1** pelo Unity Hub, com o módulo **Android Build Support**
   (incluindo SDK, NDK e OpenJDK).
2. Clone este repositório e abra a pasta `UnityProject/` pelo Unity Hub.
3. Aguarde a importação dos pacotes (a primeira abertura demora — a pasta `Library/` é gerada
   localmente e não faz parte do repositório).
4. Abra a cena `Assets/Scenes/MainMenu.unity` (menu) ou uma fase em `Assets/Scenes/Fases/`
   (`Alagamento_EmCasa`, `Incendio_EmCasa`, `Granizo_Praca`). Sem headset, o **XR Interaction Simulator**
   é criado automaticamente no Play.
5. Para regenerar dados, fase, menu e configurações: menu **SafeZone VR > Build > 0. Montar tudo**
   (detalhes em `Assets/SafeZoneVR/CONFIGURATION_GUIDE.md`).
6. Para rodar no headset, veja **Como gerar o APK** abaixo.

---

## Como gerar o APK (Meta Quest)

1. Menu **SafeZone VR > Build > 4. Configurações do projeto (Quest 3)** — aplica tudo que o APK
   precisa (é chamado automaticamente antes de cada build, então serve só para conferir).
2. Menu **SafeZone VR > Build > 5. Gerar APK (release)**. O arquivo sai em
   `UnityProject/Builds/SafeZoneVR-<versão>-release.apk` (a pasta `Builds/` não vai para o Git).
   Para investigar performance, use o **5b** e conecte o Profiler.
3. Instale no headset com o Quest em modo desenvolvedor:

```bash
adb install -r Builds/SafeZoneVR-1.0-release.apk
```

Build automatizado (CI ou terminal, com o Unity fechado):

```bash
Unity.exe -quit -batchmode -projectPath UnityProject -executeMethod SafeZoneVR.Editor.SafeZoneApkBuilder.BuildFromCommandLine
```

**Conferir o que está valendo:** menu **SafeZone VR > Build > 4b. Conferir configurações do APK**
imprime o relatório no Console.

O que a configuração aplica, e por quê:

| Ajuste | Motivo |
|---|---|
| Entrada do Android = **Activity** | O runtime da Meta não sobe com `GameActivity` (padrão do Unity 6): o app abre e fecha |
| IL2CPP + **ARM64** | Única arquitetura aceita pelo Quest |
| **Vulkan** (sem OpenGLES) | API recomendada pela Meta no Quest 3 |
| **Single Pass Instanced** + projeção simétrica + *optimize buffer discards* | Uma passada de render para os dois olhos e menos banda de memória |
| **Foveated rendering** (SRP Foveation) | Menos GPU nas bordas da imagem, onde o olho não repara |
| Texturas **ASTC** | Formato nativo do Quest; ETC2 gasta mais memória |
| *Blit type* = **Never** | Quem apresenta a imagem é o compositor do headset; o blit extra é desperdício |
| URP: MSAA 4x, sem HDR, sem depth/opaque texture, 1 cascata de sombra | Antialiasing barato e menos banda, que é o gargalo do Quest |
| Renderizador **Forward+**, luzes adicionais *per pixel* | A casa tem mais de 20 luzes em tempo real; no Forward comum elas não acenderiam |
| Espaço de cor **Linear**, sem splash da Unity | Exigência do URP em VR e entrada direta no menu |
| *Bake collision meshes*, *incremental GC*, *strip engine code* | Carregamento mais rápido e menos picos de quadro |

**Assinatura:** o build usa o keystore de debug, que serve para sideload (`adb`, SideQuest).
Para publicar na Meta Store é preciso criar um keystore próprio em
`Edit > Project Settings > Player > Publishing Settings` e guardar a senha fora do repositório.

---

## Próximos passos

- Arte definitiva para as fases Incêndio e Granizo (NPCs, chamas, UBS, praça); os primitivos são placeholders.
- Revisão dos textos do Incêndio por um oficial do Corpo de Bombeiros e PDFs das fontes em `Documentos_Orientações_Defesa_Civil/`.
- Sistema de pontuação comparativa entre tentativas.
- Narração em áudio das orientações, para acessibilidade.
- Modo de exibição para escolas e ações da Defesa Civil.

---

## Créditos e fontes

As orientações reproduzidas no jogo são de material público da **Defesa Civil**, do
**Ministério do Desenvolvimento Regional** e dos **Corpos de Bombeiros Militares** (incêndio residencial). Este é um projeto educativo, sem fins lucrativos, e
não substitui as instruções das autoridades locais durante uma emergência real.

**Em caso de emergência: Defesa Civil 199 · Bombeiros 193 · SAMU 192**
