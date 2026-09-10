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
    │   ├── Scenes/Fases/                  # Cenas das fases (Alagamento_EmCasa)
    │   └── SafeZoneVR/
    │       ├── Data/                      # ScriptableObjects: cenários, missões, cards
    │       ├── Editor/                    # Geradores de cena e de dados das fases
    │       ├── Materials/
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
4. Abra a cena `Assets/Scenes/MainMenu.unity` (menu) ou `Assets/Scenes/Fases/Alagamento_EmCasa.unity`
   (fase). Sem headset, o **XR Interaction Simulator** é criado automaticamente no Play.
5. Para regenerar dados, fase, menu e configurações: menu **SafeZone VR > Build > 0. Montar tudo**
   (detalhes em `Assets/SafeZoneVR/CONFIGURATION_GUIDE.md`).
6. Para rodar no headset: `File > Build Settings`, plataforma **Android**, com o Quest conectado
   em modo desenvolvedor.

---

## Próximos passos

- Novos cenários a partir das demais cartilhas: deslizamento, enxurrada, vendaval, granizo.
- Sistema de pontuação comparativa entre tentativas.
- Narração em áudio das orientações, para acessibilidade.
- Modo de exibição para escolas e ações da Defesa Civil.

---

## Créditos e fontes

As orientações reproduzidas no jogo são de material público da **Defesa Civil** e do
**Ministério do Desenvolvimento Regional**. Este é um projeto educativo, sem fins lucrativos, e
não substitui as instruções das autoridades locais durante uma emergência real.

**Em caso de emergência: Defesa Civil 199 · Bombeiros 193 · SAMU 192**
