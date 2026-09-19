# Levantamento de requisitos faltantes — ERS SafeZone-VR v1.0

| Campo | Valor |
|---|---|
| Documento de referência | `ERS-SafeZone-VR.md` **v1.1 (19/09/2026)**. As decisões desta análise **já foram aplicadas** à ERS: 30 RF, RN09, CSU03 e 7 RNF removidos, 9 requisitos reescritos conforme o implementado, e a nova seção 10 lista o que foi entregue além do previsto |
| Primeira análise | 16/09/2026, commit `a64c1fb` |
| Última atualização | 19/09/2026: ERS revisada para v1.1 e APK 1.0 gerado. Antes disso, 17/09/2026 (3ª rodada): RF026 e RNF13 implementados; D2 e D6 decididas; D9 esclarecida; RF005, RF011, RF027, RF029, RF048, RF071, RN09 e CSU03 saíram do escopo; RF070 fica "só a melhor" |
| Branch | `feat/fases-incendio-granizo` (alterações ainda não commitadas) |
| Substitui | `ANALISE_REQUISITOS_NAO_ATENDIDOS.md` (feito sobre o documento de requisitos antigo, RF01–RF05) |

## Como a análise foi feita

- **16/09:** análise estática do código em `UnityProject/Assets/SafeZoneVR`, do `manifest.json`, do `EditorBuildSettings` e dos documentos do repositório.
- **17/09:** três rodadas de implementação, cada uma testada no editor (Play Mode, via MCP do Unity, **sem headset**). Ver seção 3.
- As cenas são geradas pelos builders em `Editor/`: o que eles montam é o que existe na cena.

**Legenda de status**

| Símbolo | Significado |
|---|---|
| ✅ | Atendido |
| 🟡 | Parcial |
| ❌ | Não atendido |
| ➖ | **Não se aplica**: decisão registrada na seção 2. Deve ser ajustado na próxima versão da ERS |
| 🔍 | Só dá para verificar com teste no headset ou com usuários |

---

## 1. Resumo executivo

| Grupo | Total | ✅ | 🟡 | ❌ | ➖ | 🔍 |
|---|---|---|---|---|---|---|
| Requisitos funcionais (RF001–RF071) | 71 | 36 | 5 | 1 | 29 | — |
| Regras de negócio (RN01–RN09) | 9 | 8 | — | — | 1 | — |
| Requisitos não funcionais (RNF01–RNF27) | 27 | 11 | 5 | 2 | 2 | 7 |
| Casos de uso (CSU01–CSU03) | 3 | 2 | — | — | 1 | — |

Evolução dos RF atendidos: 11 (16/09) → 19 → 34 → **36**. Não há mais divergência em aberto (🔀 = 0).

**Único requisito Essencial ainda em aberto:** **RF036** — perder integridade ao ficar desabrigado durante o granizo. Como D2 manteve o roteiro da praça, o jogador ainda pode ficar na área aberta durante a precipitação; hoje isso só registra erro, sem consequência física. A base (`PlayerIntegrity`) já existe.

**O que resta, por tamanho**

1. **RF036** (acima).
2. **Conteúdo do Granizo:** vento/temperatura/alerta (RF032), impacto em vidros e veículos (RF034), abrigos errados que faltam (RF037), danos pós-tempestade (RF047), exigir "sem erro grave" para concluir (RF050).
3. **Acessibilidade e robustez:** reduzir flashes (RNF14), tratar falha de carregamento (RNF21), legendas de sons (RNF11), revisão do que depende só de cor (RNF12), saída por comando único (RNF16).
4. **Manutenção:** parâmetros das fases em ScriptableObject (RNF19) e textos fora do código (RNF27).
5. **Validação no headset:** RNF01, RNF02, RNF04, RNF07–RNF10 e a medição do tempo de carga (RNF03).

---

## 2. Decisões (ERS × jogo)

### 2.1 Decididas

| # | Tema | Decisão | Efeito |
|---|---|---|---|
| D1 | Casa térrea × dois pavimentos | **Manter o cenário do jogo** | RF030 ✅; SUP03 a corrigir na ERS |
| D2 | Roteiro do Granizo | **Manter o roteiro do jogo** (praça → UBS → volta para casa), 17/09 | RF033, RF035, RF038–RF044 e RF046 ➖. Continuam valendo RF032, RF034, RF036, RF037, RF045, RF047, RF050, RF051 |
| D3 | Roteiro do Incêndio | **Manter o roteiro do jogo** | RF054, RF055, RF057, RF060–RF063 ➖; RF053 ✅ |
| D4 | Pontuação | **Implementar o modelo da ERS** — feito | RN01–RN05 ✅, RF066 ✅, RF069 ✅ |
| D5 | Falha e integridade | **Implementar** — feito | RN07 ✅, RF025 ✅, RF058 ✅ |
| D6 | Telhado depois do granizo | **Não terá** a cobertura provisória do telhado (17/09) | RF049 ➖. O jogo mantém o erro grave ao subir no telhado (RF045) e a ligação para 199/193 |
| D8 | Registro de gás no Alagamento | **Recusada**: a fase continua com o registro de água | RF028 ➖ |
| D9 | Premissas marcadas **[CONFIRMAR]** na ERS | **Esclarecida (17/09): não há nada a fazer no jogo.** São lacunas do texto da ERS, que foram confirmadas pela implementação. Na revisão da ERS, trocar por: motor **Unity 6000.5.10f1 (URP)**; **XR Interaction Toolkit 3.5.1 sobre OpenXR 1.17.1 + Meta OpenXR**; alvo **Meta Quest 3/3S standalone** (PC VR não está configurado); persistência em **PlayerPrefs**; distribuição por **APK de sideload**; meta de **72 fps** | RNF01, RNF24, §1.5 e §2.1 da ERS |
| RF001 | Menu em lista | **Não faz sentido**: o menu é o hub 3D `DC_Hub` | ➖ |
| RF005, RF071, RN09, CSU03 | Histórico de tentativas, exportação e consulta | **Não terão** (17/09) | ➖ |
| RF006, RF007 | Tutorial obrigatório e pular/repetir | **Não fazem sentido** | ➖ (briefing + dicas por inatividade + aviso inicial cobrem a entrada) |
| RF011 | Ajuste de altura do ponto de vista | **Não terá** (17/09) | ➖. Ficam o modo Sentado/Em pé e o Recentrar |
| RF027 | Elevar eletroeletrônicos com pontuação parcial | **Não terá** | ➖ |
| RF029 | Trecho com correnteza a pé | **Não terá** | ➖. Continua o erro grave de sair de carro |
| RF031 | Riscos secundários da enchente | **Não faz sentido** | ➖ |
| RF048 | Fiação caída/poça energizada no Granizo | **Não terá** | ➖ |
| RF070 | Persistência de resultados | **Somente a melhor tentativa** (17/09) | ✅ como está: melhor pontuação (`safezone.best.score.<id>`) e melhor tempo |
| RNF17, RNF18 | Sugerir pausa; pausar ao tirar o HMD | **Não fazem sentido** | ➖ (o sistema do Quest já suspende o app) |

### 2.2 Aguardando decisão

Nenhuma. Todas as divergências levantadas em 16/09 foram decididas.

---

## 3. Registro de implementações

### 3.1 17/09 — 1ª rodada
RF012 (mão dominante), RF013 (volumes por categoria), RF015 (destaque de interagíveis), RF016 (portas pela maçaneta), RF018/RN06 (dicas por inatividade), RF023 (lentidão na água), RNF15 (aviso de segurança) e a troca dos placeholders pelos modelos de `Assets/Itens` e `Assets/NPC`. Detalhes e limitações: ver o histórico do commit desta rodada.

### 3.2 17/09 — 2ª rodada
D4 (pontuação RN01–RN05, RF066, RF069), D5 (integridade e falha: RN07, RF025, RF058), RF065/RF067/RF068 (registro e relatório novos), RF019 (vibração), RF020 (não atravessar paredes), RF024 (prazo do quadro), RF056, RF059, RF064 (fim no ponto de encontro) e RNF03 (carregamento com progresso).

### 3.3 17/09 — 3ª rodada

| ID | O que foi feito | Onde | Observações |
|---|---|---|---|
| **RF026** Documentos no kit | A pasta de documentos saiu do passo "objetos de valor" e entrou no **kit de emergência**: a mochila agora tem **4 espaços** (documentos, água, remédios, lanterna). O passo 1 ficou só com o porta-retrato, em um único socket no alto do guarda-roupa. Textos dos dois passos reescritos | `AlagamentoGameplayBuilder`, `SafeZoneDataBuilder` | A ordem continua a mesma: proteger valores → energia → registro → kit → evacuar |
| **RNF13** Agachar pelo controle | Segurar **B ou Y** (Ctrl esquerdo no simulador) abaixa o ponto de vista 55 cm, suave, e solta de volta ao liberar. Vale em todas as fases; a dica aparece no painel de conforto | `CrouchController`, `ScenarioSceneKit` | Resolve a fumaça do Incêndio sem agachar de verdade: a cabeça desce abaixo da camada e o dano para. O modo Sentado continua isento como antes |

**Testado no editor nesta rodada:** passo dos valores concluindo só com o porta-retrato; kit concluindo com os 4 itens (documentos incluídos); agachar descendo e voltando ao soltar; cenas regeradas sem erro no Console. Recordes do jogador preservados e o aviso de segurança reposto para aparecer na próxima abertura.

**Ainda não testado:** tudo no **headset**.

---

## 4. Requisitos funcionais — situação atual

### 4.1 Atendidos (36)
RF002, RF003, RF004, RF008, RF009, RF010, RF012, RF013, RF014, RF015, RF016, RF017, RF018, RF019, RF020, RF021, RF022, RF023, RF024, RF025, RF026, RF030, RF045, RF051, RF052, RF053, RF056, RF058, RF059, RF064, RF065, RF066, RF067, RF068, RF069, RF070.

### 4.2 Parciais (5)

| ID | Prior. | O que falta |
|---|---|---|
| RF032 | Essencial | Sinais de aproximação: rajadas de vento, queda de temperatura e alerta sonoro (hoje há céu de tempestade e mensagem da Defesa Civil) |
| RF034 | Essencial | Impacto do granizo em **vidros e veículos** (hoje há intensidade crescente, som no telhado e gelo acumulando) |
| RF037 | Essencial | Abrigos errados que faltam: **poste, marquise e telha translúcida**. Já são erro grave: árvore, pergolado, ponto de ônibus metálico e barraca |
| RF047 | Essencial | Danos depois da tempestade: **vidros estilhaçados e galhos** (hoje há telhas quebradas e gelo acumulado) |
| RF050 | Essencial | Exigir **nenhum erro grave** para concluir a fase com sucesso |

### 4.3 Não atendido (1)

| ID | Prior. | O que falta |
|---|---|---|
| RF036 | Essencial | Perder integridade enquanto estiver desabrigado durante o granizo, proporcional à intensidade. Hoje só registra o erro grave "sair durante o granizo". `PlayerIntegrity` e `HailController.intensity` já existem |

### 4.4 Fora do escopo (29)
RF001, RF005, RF006, RF007, RF011, RF027, RF028, RF029, RF031, RF033, RF035, RF038, RF039, RF040, RF041, RF042, RF043, RF044, RF046, RF048, RF049, RF054, RF055, RF057, RF060, RF061, RF062, RF063, RF071 — todos com a decisão correspondente na seção 2.1.

---

## 5. Regras de negócio

| ID | Status | Situação |
|---|---|---|
| RN01–RN05 | ✅ | 100 pontos, −5 leve, −25 grave, −10 fora do tempo, quatro faixas |
| RN06 | ✅ | Dicas em 30 s e 60 s |
| RN07 | ✅ | Falha por integridade zero ou erro grave fatal |
| RN08 | ✅ | Erros permanecem no relatório |
| RN09 | ➖ | Sem histórico: o jogo guarda só a melhor tentativa (decisão) |

---

## 6. Requisitos não funcionais

| ID | Status | Situação / o que falta |
|---|---|---|
| RNF01, RNF02, RNF04 | 🔍 | Medir fps, latência e custo gráfico no Quest 3 |
| RNF03 | ✅ | Carga assíncrona com progresso; falta **medir** se fica ≤ 15 s |
| RNF05 | ✅ | Todas as opções de conforto que o jogo terá estão no menu e nas fases |
| RNF06 | ✅ | Sem movimento de câmera sem comando do jogador |
| RNF07 | 🔍 | Medir a duração real das fases com usuários |
| RNF08, RNF09 | 🔍 | Legibilidade e profundidade dos painéis |
| RNF10 | 🔍 | Sem tutorial, por decisão: validar com usuário novato |
| RNF11 | 🟡 | Legenda/ícone para alarme de fumaça, fogo, granizo e alerta da Defesa Civil |
| RNF12 | 🟡 | Revisar o que depende só de cor (zonas de risco, estrelas) |
| RNF13 | ✅ | Agachar pelo controle |
| RNF14 | ❌ | Opção "reduzir flashes" (fogo pisca a luz; há *flare*) |
| RNF15 | ✅ | Aviso de segurança na primeira sessão |
| RNF16 | 🟡 | Sair da simulação ainda leva dois toques |
| RNF17, RNF18 | ➖ | Decisão |
| RNF19 | 🟡 | Água, fogo, granizo, dicas e integridade ainda vêm dos builders |
| RNF20, RNF22–RNF26 | ✅ | Offline, arquitetura orientada a dados, Git, OpenXR, entrada abstraída, pt-BR |
| RNF21 | ❌ | Tratar falha de carregamento de recurso |
| RNF27 | 🟡 | Textos ainda nascem no código dos builders |

---

## 7. Casos de uso

| ID | Status | Situação |
|---|---|---|
| CSU01 | ✅ | Fluxos principal, 4a (dica), 4b (falha) e 4c (interromper) funcionam; a melhor tentativa fica gravada |
| CSU02 | ✅ | Locomoção, giro, vinheta, posição, mão dominante e volumes aplicados na hora e persistidos |
| CSU03 | ➖ | Sem consulta de histórico (decisão) |

---

## 8. Pendências fora da ERS

| # | Pendência | Situação |
|---|---|---|
| P1 | Teste completo no headset | Pendente — é a maior lacuna agora |
| P2 | Testes automatizados e Assembly Definitions | Pendente |
| P3 | Placeholders de primitiva | Parcial: itens, NPCs, botijão, registro, caminhão, plataforma, ponto de ônibus, outdoor, vagas, forro e UBS feitos; faltam benjamim, disjuntor, escada, telhas, barraca e torre |
| P4 | Totem do hub gravado como "TERREMOTOS" | Pendente |
| P5 | PDFs dos Bombeiros e revisão dos textos do Incêndio | Pendente |
| P6 | README e `CONFIGURATION_GUIDE.md` desatualizados | Pendente (`ITENS_A_MODELAR.md` e `REFERENCIA_TECNICA.md` atualizados) |
| P7 | Névoa *Exponential Squared* no *shader stripping* | Conferir no APK |
| P8 | Keystore próprio | Pendente |
| P9 | Matriz RF × teste de aceitação | Pendente |
| P10 | `IMPORTAR_NPC.md` diz frente +Z; o FBX olha para −Z | Contornado no builder |
| P11 | Alterações de 17/09 não commitadas | Resolvido: tudo commitado e enviado |
| P12 | Editar script com o Play Mode ligado quebra o XRI | Sair do Play antes de editar |

---

## 9. Backlog sugerido

### Onda 1 — Fechar o Granizo
- **RF036** integridade sob granizo (único Essencial em aberto).
- RF032 (vento, temperatura, alerta), RF034 (vidros e veículos), RF037 (poste, marquise, telha translúcida), RF047 (vidros e galhos), RF050 (sucesso sem erro grave).

### Onda 2 — Acessibilidade e robustez
- RNF14 (reduzir flashes), RNF21 (falha de carregamento), RNF11 (legendas de sons), RNF12 (não depender de cor), RNF16 (saída por comando único).

### Onda 3 — Manutenção e arte
- RNF19 (parâmetros em ScriptableObject), RNF27 (textos fora do código), modelos restantes (P3), `.asmdef` e testes (P2).

### Onda 4 — Validação
- APK de desenvolvimento, profiling e teste com usuários no Quest 3: RNF01, RNF02, RNF04, RNF07–RNF10, medição do RNF03 e o roteiro completo das três fases (P1).

### Onda 5 — Documentação
- Atualizar a ERS para v1.1 com todas as decisões da seção 2.1 (inclusive os [CONFIRMAR] do D9) e atualizar README/CONFIGURATION_GUIDE (P6).

---

## 10. Como manter este documento

1. Ao implementar: mudar o status, registrar na seção 3 (data, arquivos, limitações) e recalcular o resumo da seção 1.
2. Ao decidir: registrar na seção 2.1 e marcar os requisitos como ➖ ou ❌.
3. As cenas são geradas: o que vale é `Editor/*GameplayBuilder.cs`, `ScenarioSceneKit.cs` e `SafeZoneDataBuilder.cs`.
4. Os itens 🔍 só mudam com medição no headset (APK dev pelo menu **SafeZone VR > Build > 5b**).

---

## 11. Fechamento (19/09/2026)

- **APK de release gerado**: `UnityProject/Builds/SafeZoneVR-1.0-release.apk`, 64,8 MB, GameActivity + IL2CPP + ARM64 + Linear.
- **ERS revisada para v1.1**, alinhada ao que existe. Os requisitos que estavam abertos foram tratados assim:
  - **RF036** (integridade sob granizo) e os itens não entregues dos parciais foram **removidos** da ERS, por decisão registrada nesta data. O jogo continua registrando erro grave ao ficar na praça aberta durante a precipitação, sem consequência física.
  - **RF032, RF034, RF037, RF047 e RF050** foram **reescritos** para descrever o que o jogo faz.
- **Continua valendo o que esta análise sempre disse:** nada foi validado no headset. Os critérios de desempenho e conforto (RNF01, RNF02, RNF04, RNF07–RNF09) seguem sem medição no dispositivo, e é a maior lacuna da entrega.
