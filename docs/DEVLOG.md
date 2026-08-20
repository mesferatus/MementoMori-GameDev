# Devlog

## C3 — cleanup e preparação narrativa — 2026-08-20

Escopo executado sem novos diálogos, alteração de mapas ou mudança de direção visual. A validação Unity ficou bloqueada por licença headless indisponível (`return code 198`); nenhum PASS foi inferido dessa tentativa.

| SCRIPT | BEFORE | ACTION | AFTER | BEHAVIOR_CHANGED | TEST_USED |
|---|---|---|---|---|---|
| `GameState.cs` | flags/counters/checkpoints separados de progresso | MERGE | fonte única também guarda puzzle progress e checkpoint counters | NO | EditMode pendente; revisão estática |
| `StoryProgression.cs` | dicionários próprios duplicavam estado | MERGE | fachada compatível delegando para `GameState` | NO | EditMode pendente; revisão de referências |
| `RuntimeAudio.cs` | carga de recurso e `EnsureActive()` repetidos no one-shot | REFACTOR | caminho de recurso constante e instância reutilizada localmente | NO | Compile/teste Unity pendente |
| `PlayerController.cs` | retorno antecipado separava atualização do Animator | CLEANUP | bloco condicional único para Animator | NO | Compile/teste Unity pendente |
| `PuzzleMirror.cs` | `PuzzleState` em arquivo legado compartilhado | MERGE/CLEANUP | enum aninhado no dono real do estado | NO | testes EditMode/PlayMode migrados |
| `SigilRingPuzzle.cs` | progresso dependia da fachada `StoryProgression` | MERGE | lê/grava progresso diretamente na fonte única `GameState` | NO | testes EditMode/PlayMode pendentes por licença |
| `PuzzleEditModeTests.cs` | cobria sequência sigilada removida | REFACTOR | cobre `SigilRingPuzzle` publicado, erro e solução | NO | EditMode pendente por licença |
| `PuzzlePlayModeTests.cs` | cobria sequência sigilada removida | REFACTOR | cobre progresso persistente de anéis após erro | NO | PlayMode pendente por licença |
| `PuzzleSigilSequence.cs` | implementação sem binding de produção | REMOVE | removido após migração dos testes | NO | busca de referências |
| `PuzzleState.cs` | enum compartilhado pelo legado | REMOVE | removido; enum vive em `PuzzleMirror` | NO | busca de referências |
| `SigilPart.cs` | componente do legado sem binding | REMOVE | removido após migração dos testes | NO | busca de referências |
| `F6TextMeshProSetup.cs` | helper one-shot de setup | REMOVE | removido | NO | busca de referências |
| `ProjectBootstrap.cs` | scaffolder one-shot de fundação | REMOVE | removido | NO | busca de referências |
| `SharedSpawnPointsMigration.cs` | migração one-shot concluída | REMOVE | removido | NO | busca de referências |

## Resultado estrutural

- `FIRST_PARTY_SCRIPTS_BEFORE=64`
- `FIRST_PARTY_SCRIPTS_AFTER=58`
- `DEV_ONLY_BEFORE=9`
- `DEV_ONLY_AFTER=6` (action-classified DEV_ONLY; the two migrated puzzle test files remain test-only under `Assets/Tests`)
- `CLEANUP_COMPLETED=1/1`
- `REFACTOR_COMPLETED=3/3`
- `MERGE_COMPLETED=1/1` (com `PuzzleMirror` como consolidação local adicional)
- `REMOVE_COMPLETED=6/6`
- `CODE_REFACTORED_FILES=8`
- `CODE_REMOVED_FILES=6`

Narrative storage remains `DialogueData` ScriptableObjects under `Resources/Dialogue`; trigger model remains `DialogueTrigger`/`IInteractable` plus direct scene event hooks. No strings narrativas novas foram adicionadas.

`DIALOGUE_STORAGE=DialogueData ScriptableObjects (42 assets/144 entries)`. `DIALOGUE_TRIGGER_MODEL=DialogueTrigger + IInteractable + scene/puzzle events`. `DIALOGUE_SCALABILITY_FOR_34_NEW_ENTRIES=GOOD`, with state/binding review still required before content authoring.

`ENVIRONMENT_INTERACTION_SYSTEM_READY=YES`; `PUZZLE_FEEDBACK_SYSTEM_READY=YES`; `POE_NARRATIVE_EVENT_SYSTEM_READY=YES`; `FINALBETA_NARRATIVE_SYSTEM_READY=YES` for the existing serialized final UI, with dynamic fragment/epilogue content still planned.
