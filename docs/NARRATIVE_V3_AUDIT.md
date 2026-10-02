# Auditoria narrativa V3

Data: 2026-09-13

Escopo desta passada: alinhar os contratos tecnicos mais criticos da V3 sem refatorar sistemas funcionais e sem alterar composicao visual/manual das cenas alem da configuracao serializada necessaria dos Espelhos.

| Arquivo / sistema | Estado | Acao | Motivo |
|---|---|---|---|
| `Assets/Scripts/Core/GameState.cs` | KEEP | Sem mudanca | Ja preserva flags, counters, checkpoints e estados de ritual/fragmento. |
| `Assets/Scripts/World/BedController.cs` | MODIFY | Cama agora depende de `RitualCompleted` | V3 transforma tigela, brinquedo e outros objetos em conteudo opcional. |
| `Assets/Scripts/World/RitualController.cs` | MODIFY | Ritual conclui estado e libera cama, sem carregar Labirinto | V3 exige dormir/confirmar na cama para transicao onirica. |
| `Assets/Scripts/Puzzles/PuzzleMirror.cs` | RECONFIGURE | Sem mudanca de codigo | O sistema ja aceita solucao configuravel por IDs. |
| `Assets/Scenes/DominioLua.unity` | RECONFIGURE | Solucao serializada dos Espelhos mudou para `Delayed + Ahead + Room` | V3 remove `Absent` da solucao final e torna `Room` essencial. |
| `Assets/Scripts/Narrative/MoonDomainNarrativeController.cs` | MODIFY | Progresso narrativo dos Espelhos conta `Room`, nao `Absent` | Evita feedback de progresso alinhado a regra antiga. |
| `Assets/Editor/SceneConfigurationBaker.cs` | MODIFY | Baker grava `Delayed + Ahead + Room` | Evita regressao quando a configuracao de cenas for rebakeada. |
| `Assets/Editor/DialogueAssetsBaker.cs` | MODIFY | Removida fala explicita "Poe esta morto" | A V3 proibe revelar morte de Poe no beta. |
| `Assets/Resources/Dialogue/DLG_ANDREALPHUS_02.asset` | MODIFY | Asset runtime atualizado com fala ambigua sobre Poe | O jogo usa o asset existente, nao apenas o baker. |
| `Assets/Scripts/UI/ObjectiveToastController.cs` | MODIFY | Objetivo do Quarto passa a orientar investigacao/ritual sem listar solucao | V3 pede objetivos sem entregar respostas de puzzle. |
| `Assets/Scripts/Verification/CtEvidenceRunner.cs` | MODIFY | CT atualizado para ritual liberar cama e Espelhos usarem `Room` | Evidencia automatizada deve refletir a V3. |
| `Assets/Tests/EditMode/*` e `Assets/Tests/PlayMode/*` | MODIFY | Testes atualizados para cama por ritual e Espelhos V3 | Testes antigos codificavam narrativa anterior. |
| Sprites V3 em `Assets/Art/Sprites` | KEEP | Pacote validado como presente no workspace | Os PNGs Unity-ready ja existem; nao houve regeneracao nem upscale. |

Pendencias honestas:

- Nao foi feito playthrough humano; isso continua obrigatorio antes de afirmar fechamento total da V3.
- Nao foi feita rebake geral de dialogos/cenas no Editor nesta etapa.
- Hooks de animacao finais de Melantha, Poe e Andrealphus continuam dependentes dos sprites/personagens finais.
- Integracao visual fina de estados, colliders, sorting e revisao da Luiza continua pendente onde a cena exige autoria visual.

## Segunda etapa - matriz de implementacao

| V3_ID / momento | Implementacao atual | Status | Acao |
|---|---|---|---|
| Q: investigacao ate ritual/cama | BedController + RitualController | PARTIAL | Hooks e flags mantidos; bindings de dialogo V3 dependem de assets rebaked. |
| L: Poço de Vozes | VoiceWellController | MATCH | Quatro interacoes progressivas, flag final e estagios visuais expostos. |
| L: Corredor de Ecos | EchoCorridorPuzzle | MATCH | Tres rodadas, erro por rodada, retorno seguro e dica de Poe. |
| L: Andrealphus 01/02 | assets antigos com 02 revisado | PARTIAL | 02 sem revelacao proibida; migracao integral para IDs V3 pendente. |
| D: Jardim Lunar | GardenPetalPuzzle | PARTIAL | Regras Crescente/Cheia/Minguante preservadas; hooks visuais finais pendentes. |
| D: Espelhos | PuzzleMirror + DominioLua | MATCH | Solucao Delayed + Ahead + Room; estados narrativos restantes ainda dependem de leitura em cena. |
| D: Sigilo/Fragmento | SigilRingPuzzle + FragmentCollectible | PARTIAL | Fluxo e nome neutro presentes; memoria/vanish/portal ainda nao estao encadeados integralmente. |
| F: FinalBeta | FinalBetaController | PARTIAL | Gate e tela final presentes; evidencia de despertar exposta como hook, texto/ordem V3 dependem rebake e cena. |

## Etapa 3 - migracao de dialogos

Contagem extraida da Biblia: 82 IDs unicos. Contagem runtime gerada: 82 assets, todos com `sequenceId` correspondente e ao menos uma linha. O catalogo unico esta em `NarrativeV3DialogueCatalog` e o gerador reproduz a extracao somente quando a Biblia continua fornecendo exatamente 82 blocos.

| OLD_DIALOGUE_ID | V3_DIALOGUE_ID | STATUS |
|---|---|---|
| `DLG_ROOM_*`, `DLG_LABYRINTH_*`, `DLG_C4B_*`, `DLG_C4C_*` | Familias `DLG_Q_*`, `DLG_L_*`, `DLG_D_*`, `DLG_F_*` | ALIAS_TEMPORARY / STILL_VALID |
| `DLG_ANDREALPHUS_02` | `DLG_D_ANDREALPHUS_02` | ALIAS_TEMPORARY; asset antigo preservado por bindings existentes |

O `SceneConfigurationBaker` foi atualizado para exigir os IDs V3 por cena. A troca efetiva de referências serializadas no Inspector continua `VISUAL_BINDING_PENDING` enquanto não houver Editor conectado.

## Validacao da Etapa 3

- Static: 82 IDs no catalogo e 82 assets runtime encontrados.
- Static: `git diff --check` sem erro de whitespace.
- Secret scan: nenhuma fala runtime encontrada com revelacao proibida sobre Poe ou identidade do Fragmento.
- EditMode: `BLOCKED_BY_UNITY_LICENSE`; nenhum resultado foi inventado. O Editor retornou `No valid Unity Editor license found` e nao produziu XML.
- Fragmento: fluxo runtime agora possui abordagem, toque, memoria, coleta, bloqueio de movimento e hook de desaparecimento/reaparicao de Poe; `poeReappearPoint` permanece `VISUAL_BINDING_PENDING` quando nao serializado.
- Extracao V3: parser corrigido para respeitar falantes `Tela` do end card e nao incorporar a secao de personagens como dialogo.
- Editor: Unity foi confirmado aberto pelo usuario, mas `unity status` nao detecta instancia porque `com.unity.pipeline` nao esta instalado. `unity pipeline install` falhou por SSL antes de alterar o projeto; instalacao manual pelo Package Manager continua pendente.
