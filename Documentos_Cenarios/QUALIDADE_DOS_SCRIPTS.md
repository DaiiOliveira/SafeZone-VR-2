# Como medir a qualidade dos scripts

Resumo: **três camadas** — analisador no editor (já ligado), auditoria do projeto (um comando),
e medição em runtime no headset (a única que diz se o jogo roda bem). Ferramenta de análise
genérica de C# (SonarQube e afins) é a de menor retorno aqui e fica por último.

---

## Linha de base de hoje (16/09/2026)

Medido no repositório, só o código do jogo (`Assets/SafeZoneVR`), sem pacotes nem samples:

| Métrica | Valor |
|---|---|
| Arquivos `.cs` | 74 (60 runtime + 10 editor + 4 outros) |
| Linhas totais | 9.033 |
| Maior arquivo | `Editor/ScenarioSceneKit.cs`, 1.138 linhas |
| Maior arquivo de runtime | `Core/ScenarioManager.cs`, 327 linhas |
| `GetComponent` dentro de `Update`/`FixedUpdate` | **0** |
| `Camera.main` por frame | **0** (só em `PlayerLocator`, com cache) |
| `Invoke`/`SendMessage`/`StartCoroutine` por string | **0** |
| `Find*ObjectByType` | 5, todas em `Awake` ou cache de singleton |
| `Debug.Log` em runtime | 5, todas `LogWarning`/`LogError` de configuração faltando |
| Assembly Definitions no código do jogo | **0** ← maior lacuna estrutural |
| Testes automatizados | **0** |

Leitura: os anti-padrões clássicos de Unity que custam desempenho **não estão presentes**.
Os dois buracos reais são de estrutura (sem `.asmdef`, sem teste), não de escrita do código.
`ScenarioSceneKit.cs` com 1.138 linhas é o único arquivo que pede divisão, e é código de editor
— não vai para o APK.

---

## Camada 1 — Analisador no editor (já configurado neste commit)

**Microsoft.Unity.Analyzers** é o padrão da indústria para boas práticas específicas de Unity
(as regras `UNT****`). Ele **já vem** no projeto através de `com.unity.feature.development`
(pacotes `ide.visualstudio` e `ide.rider`) — só faltava dizer o que é erro e o que é aviso.

Foi criado um `.editorconfig` na raiz do repositório com as severidades. Ele não tem
comentários, como o resto do código; o critério está aqui:

- **Erro** para regras que quebram comportamento: assinatura errada de mensagem da Unity
  (`UNT0006`), `??`/`?.`/`??=` em `UnityEngine.Object` (`UNT0007/8/23` — o operador ignora a
  destruição do objeto e você trabalha com uma referência "viva que não existe"), `new` em
  MonoBehaviour/ScriptableObject, `Destroy(transform)`.
- **Aviso** para custo de desempenho em Quest: comparação de tag por string, reflexão em
  `Update`, `position`/`rotation` atribuídos separado, `GetComponent` onde cabe
  `TryGetComponent`, APIs de física que alocam.
- Desligadas as duas queixas que são falso positivo em Unity: `CS0649` e `IDE0051` (campo
  `[SerializeField]` privado "nunca atribuído" — quem atribui é o inspetor).

Também ficaram registradas as convenções de nome já usadas no projeto (`m_`, `k_`, `s_`),
como *suggestion*, para quem abrir o projeto no Rider/VS seguir o mesmo padrão.

**Como ver as métricas:** abrir a solução no Rider ou Visual Studio — os avisos aparecem na
lista de erros. Sem IDE, pela linha de comando:

```bash
dotnet build UnityProject/SafeZoneVR.sln -warnaserror:none
```

(o `.sln` é gerado pela Unity; se não existir, `Edit > Preferences > External Tools > Regenerate project files`.)

## Camada 2 — Unity Project Auditor (recomendado, ~10 min para instalar)

É a ferramenta da própria Unity para tirar relatório do projeto inteiro: problemas de código,
de configurações do Player/Quality, assets pesados, shaders, chamadas caras em builds. Dá
exatamente o tipo de "métrica" pedida, e entende do alvo Android/XR.

O projeto **já tem** `ProjectSettings/ProjectAuditorSettings.asset` (sobra de uma instalação
anterior), mas o pacote não está no `manifest.json`. Para ligar:

`Window > Package Manager > + > Add package by name` → `com.unity.project-auditor`

Depois `Window > Analysis > Project Auditor > Analyze`. Vale filtrar por *Code* e por
*Settings*; a aba de *Settings* costuma repetir o que o nosso `QuestProjectConfigurator.Validate()`
(menu `SafeZone VR > Build > 4b`) já checa, então dá para cruzar os dois.

Não instalei por conta própria porque mexer no `manifest.json` força uma resolução de pacotes
e um domain reload — melhor fazer com o editor fechado ou em momento combinado.

## Camada 3 — Desempenho real (a métrica que decide o jogo)

Nenhuma análise estática diz se o jogo mantém 72 Hz no Quest 3. Para isso:

1. **APK de desenvolvimento**: menu `SafeZone VR > Build > 5b` (já configura profiler e debug).
2. **Unity Profiler** conectado por USB, ou o **OVR Metrics Tool** no próprio headset para ver
   FPS, *stale frames* e temperatura sem PC.
3. **RenderDoc / Meta Quest Developer Hub** se aparecer gargalo de GPU.

Números-alvo do Quest 3: ≤ 13,8 ms por frame (72 Hz), ≤ 200 draw calls, ≤ 500k triângulos
visíveis, zero alocação de GC por frame em cena de gameplay.

## Camada 4 — Testes (o que falta de verdade)

Unity Test Framework já está disponível via `com.unity.feature.development`. O que falta são
os dois pré-requisitos:

1. **Criar Assembly Definitions** (`SafeZoneVR.Runtime.asmdef` e `SafeZoneVR.Editor.asmdef`).
   Hoje todo o código está na assembly padrão `Assembly-CSharp`, o que (a) recompila tudo a
   cada alteração e (b) **impede** que uma assembly de teste referencie o código.
2. Com isso feito, uma pasta `Assets/Tests` com EditMode tests cobre bem a parte determinística:
   `ScenarioSO`/`MissionStepSO` bem formados, catálogo apontando para cenas que existem no Build
   Settings, validadores concluindo o passo certo, `SceneFlow` não carregando duas cenas.
   Depois disso, `com.unity.testtools.codecoverage` dá o percentual de cobertura.

Isso é um trabalho de umas 2–3 horas e é o maior salto de qualidade disponível — posso fazer
quando você quiser.

## Camada 5 — SonarQube / SonarCloud (baixa prioridade aqui)

Funciona com C#, mas: não conhece nada de Unity (não tem as regras `UNT`), exige um servidor ou
conta na nuvem, e precisa de build via `dotnet` com cobertura para render o que promete. Num
projeto de 9k linhas com o código já limpo nos pontos que importam, entrega pouco além do que a
camada 1 entrega de graça. Só vale se o projeto virar requisito acadêmico/institucional de
métricas formais (aí sim ele gera os gráficos de dívida técnica que bancas gostam).

---

## Ordem sugerida

| # | Ação | Custo | Retorno |
|---|---|---|---|
| 1 | `.editorconfig` com os UNT | **feito** | alto |
| 2 | Instalar Project Auditor e rodar uma vez | 10 min | alto |
| 3 | Criar os dois `.asmdef` | 30 min | alto (compilação + destrava testes) |
| 4 | APK dev + OVR Metrics Tool no headset | 20 min | alto (é o que o jogador sente) |
| 5 | Suíte de EditMode tests + Code Coverage | 2–3 h | médio-alto |
| 6 | SonarCloud | 1–2 h + conta | baixo |
