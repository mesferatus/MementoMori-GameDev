# C5.2 — Contrato de integração visual: Domínio da Lua

## Checklist técnico

| ID | EXPECTED_OBJECT / AREA | VISUAL_DEPENDENCY | EXPECTED_EFFECT | EXPECTED_POSITION | SCRIPT_TO_CONNECT | TRIGGER_OR_EVENT | TEST_TO_RUN |
|---|---|---|---|---|---|---|---|
| D01 | `SPAWN_DominioLua_Entrada`, `Entrada` | entrada montada | chegada/ambientação | spawn preservado | `SceneConfiguration`, player | load de Labirinto | entrada e retorno |
| D02 | área de leitura do espaço | visual da área final | ambientação estável | área publicada pela Luiza | `MoonDomainNarrativeController` | entrada na área | trigger + diálogo |
| D03 | jardim/canteiro/prop inicial | prop específico | pista visual inicial | `JardimLunar` | `DialogueTrigger` / `WaningFlowerNode` | inspeção | interação do prop |
| D04 | `FlorMinguante_1..3`, `Petala_*` | flores/pétalas | estados crescente/cheia/minguante | jardim existente | `GardenPetalPuzzle`, `WaningFlowerNode` | colocação de pétala | ordem 1→2→0 |
| D05 | `SalaDosEspelhos`, `Mirror_01..05` | espelhos publicados | estados de reflexo | sala existente | `MirrorSymbol`, `PuzzleMirror` | interação/símbolo | erro/progresso/sucesso |
| D06 | `D14` hook de conclusão | animação visual final | reação pós-animação | espelhos | `MoonDomainNarrativeController` | completion event | animação antes da reação |
| D07 | `CorredorIlusorio`, `EcoPassagem_*` | corredor e caminhos | feedback de eco | corredor existente | `EchoCorridorPuzzle` | escolhas | rota recuperável |
| D08 | `Galeria dos Ciclos`, `GalleryDoor_*` | portas/fases | leitura dos ciclos | galeria existente | `DialogueTrigger` | observação | diálogos de fases |
| D09 | `D19` `SigilCentralVisual`, `Sigil_*` | sigilo e anéis finais | feedback de configuração | `CamaraDoSigilo` | `SigilRingPuzzle`, `SigilVisualState` | input Lua→Olho→Espiral | três anéis |
| D10 | `Fragment`, `SalaDoFragmento` | fragmento/efeito | coleta e brilho | sala existente | `FragmentCollectible` | `SigilPuzzleComplete` | coleta única |
| D11 | `MoonPortalArt`, `Checkpoint_FinalPortal` | portal final | abertura/transição | portal existente | `Portal`, `SceneLoader` | `FragmentCollected` | FinalBeta |
| D12 | colliders/triggers de `Walls`, `Obstacles`, áreas | navegação visual | passagem sem softlock | áreas publicadas | Player/physics | checkpoints | scan + percurso |
| D13 | `DialogueManager`, `InteractionPrompt`, `PauseMenu` | skin visual | prompt/diálogo/pause | canvas existente | UI controllers | eventos existentes | abrir/fechar |
| AV03 | áudio sob `Audio` | posições/efeitos finais | ambientação e feedback | por área | `RuntimeAudio` | puzzle/portal | volume/eventos |

## Pendências narrativas visuais

| ID | VISUAL_DEPENDENCY | EXPECTED_OBJECT | EXPECTED_ANIMATION_OR_EFFECT | EXPECTED_POSITION | SCRIPT_TO_CONNECT | TRIGGER_OR_EVENT | TEST_TO_RUN |
|---|---|---|---|---|---|---|---|
| D02 | leitura da chegada/área final | marco ambiental da área | efeito ambiental, se houver | entrada/área definida | `MoonDomainNarrativeController` | entrada na área | carregar e atravessar |
| D03 | pista inicial do jardim | canteiro/prop final | destaque discreto | `JardimLunar` | `DialogueTrigger` | inspeção | prompt + diálogo |
| D14 | conclusão dos espelhos | hook da animação final | reação visual completa | `SalaDosEspelhos` | controller narrativo | completion event | reação depois do efeito |
| D19 | leitura do sigilo | sigilo/anel final | feedback de leitura | `CamaraDoSigilo` | `SigilRingPuzzle` / `SigilVisualState` | configuração final | sequência e feedback |

## LUIZA_MUST_PROVIDE

Visual de Entrada, Jardim, flores/pétalas, Espelhos, Corredor Ilusório, Galeria, Sigilo Fragmentado, fragmento, portal final, colliders, triggers espaciais, sorting e pontos de spawn/checkpoint preservados. Nomes existentes devem ser mantidos ou mapeados em aliases.

## CALLISTO_MUST_CONNECT

Conectar `GardenPetalPuzzle`, `MirrorSymbol`, `PuzzleMirror`, `EchoCorridorPuzzle`, `SigilRingPuzzle`, `SigilVisualState`, `FragmentCollectible`, `Portal`, narrativa, UI, áudio, estados e regressão completa até FinalBeta.

`DOMINIO_LUA_READY_CONTRACT = YES`.

## AUDIO_VISUAL_PENDING

| ID | SCENE | VISUAL_DEPENDENCY | AUDIO_EVENT_READY | WHAT_LUIZA_PROVIDES | WHAT_CALLISTO_ADJUSTS_AFTER |
|---|---|---|---|---|---|
| AV03 | Domínio da Lua | brilho/reação visual de espelhos, sigilo e fragmento | `11_mirror_shimmer`, `13_fragment_collect` | animação/efeito final e posição dos hooks | disparar áudio após o evento visual, ajustar volume e regressão |
