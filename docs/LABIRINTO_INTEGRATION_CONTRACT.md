# C5.2 — Contrato de integração visual: Labirinto

## Checklist técnico

| ID | OBJECT_NAME_EXPECTED | VISUAL_OWNER | SCRIPT_OR_SYSTEM | COLLIDER/TRIGGER | INTERAÇÃO / ESTADO | ÁUDIO / DIÁLOGO | POSIÇÃO | STATUS |
|---|---|---|---|---|---|---|---|---|
| L01 | `SPAWN_Labirinto_Inicio`, `Ground`, corredores/câmaras | Luiza | `SceneConfiguration`, `PlayerController` | chão e paredes; não trigger | spawn/navegação | wake | sim | PROVIDE+CONNECT |
| L02 | `Collision`, `Walls`, `Obstacles` | Luiza | física 2D | collider obrigatório | bloqueio | passos | sim | PROVIDE |
| L03 | `Poe`, `PoeReveal`, `PoeRevealTrigger` | Luiza | `PoeFollower`, `PoeRevealTrigger` | trigger no ponto | `PoeRevealed` | `DLG_POE_REVEAL` | sim | CONNECT |
| L04 | marca/prop ambiental final | Luiza | `DialogueTrigger` / narrativa | collider e trigger | pendência narrativa L04 | diálogo a definir no hook | sim | VISUAL_PENDING |
| L05 | parede sem símbolo | Luiza | `DialogueTrigger` | collider e ponto de interação | pendência narrativa L05 | feedback sem revelar rota | sim | VISUAL_PENDING |
| L06 | `PoePoint_01/02`, `PoeEventPoint`, `PoeRouteTrigger_*` | Luiza | `PoeEventPoint`, `PoeEventTrigger` | trigger obrigatório | eventos/rotas de Poe | reações de Poe | sim | CONNECT |
| L07 | `AndrealphusAlcove`, `Checkpoint_Andrealphus` | Luiza | `DialogueTrigger`, `CheckpointTrigger` | collider + trigger | checkpoint/encontro | `DLG_ANDREALPHUS_01/02` | sim | CONNECT |
| L08 | `EcoPassagem_1..4`, `EchoCorridorPuzzle` | Luiza | `EchoPassageChoice`, `EchoCorridorPuzzle` | collider + interação | `EchoTrial*` | `DLG_C4B_L06/07/08` | sim | CONNECT |
| L09 | `FalseDoor` | Luiza | `FalseDoorController` | collider + interação | repetição/feedback L10 | diálogo final do binding | sim | CONNECT+L10_PENDING |
| L10 | `DomainPortal_*`, `MoonPortal`, `SPAWN_Labirinto_AreaPortais` | Luiza | `Portal`, `SceneLoader` | collider + trigger/interação | acesso à Lua após requisitos | `12_portal_open`, `DLG_C4B_L15` | sim | CONNECT |
| L11 | `Checkpoint_LabyrinthStart`, `Checkpoint_Echoes`, `Checkpoint_Andrealphus` | Luiza | `CheckpointTrigger`, `GameState` | trigger obrigatório | retorno seguro | não | sim | CONNECT |
| L12 | `DialogueManager`, `InteractionPrompt`, `PauseMenu`, `EventSystem` | Luiza (skin) | UI controllers | não | diálogo/prompt/pause | blip/menu | não | CONNECT |
| L13 | áudio/ambiente sob `Audio` | Luiza fornece posição/objeto | `RuntimeAudio` | não | loops/eventos | passos, Poe, portais | sim | AUDIO_DEPENDENT |

## LUIZA_MUST_PROVIDE

Montar visual principal do labirinto, corredores, câmaras, alcova, obstáculos, portais, porta falsa, Ecos/Corredor Ilusório, poço/elementos de Poe, sorting e colliders básicos. Preservar os nomes acima ou entregar aliases; manter os pontos de spawn e checkpoints; não inserir gameplay.

## CALLISTO_MUST_CONNECT

Conectar player/câmera, `PoeFollower`, eventos e triggers de Poe, encontro de Andrealphus, `EchoCorridorPuzzle`, `FalseDoorController`, checkpoints, `Portal`, diálogos, prompts, áudio e acesso à Lua. Testar a rota completa e regressão do estado.

## Pendências narrativas visuais

| ID | WHAT_LUIZA_MUST_PROVIDE | WHAT_CALLISTO_WILL_INTEGRATE |
|---|---|---|
| L04 | Marca/prop ambiental final, posição e sorting estáveis. | Hook de inspeção/trigger e diálogo correspondente. |
| L05 | Parede final sem símbolo, com ponto de interação. | Binding que mantém a rota ambígua e apenas fornece feedback. |
| L10 | Porta falsa final, posição e leitura visual da repetição. | `FalseDoorController` e binding do feedback final. |
| L14 | Limiar visual do portal da Lua, posição e camada de passagem. | Fala imediatamente antes da travessia e teste do gate. |

`LABIRINTO_READY_CONTRACT = YES`. O contrato está pronto; a cena somente estará `READY_FOR_INTEGRATION` após o gate visual e o scan sem Missing References.

## AUDIO_VISUAL_PENDING

| ID | SCENE | VISUAL_DEPENDENCY | AUDIO_EVENT_READY | WHAT_LUIZA_PROVIDES | WHAT_CALLISTO_ADJUSTS_AFTER |
|---|---|---|---|---|---|
| AV02 | Labirinto | leitura visual do portal/limiar de passagem | `12_portal_open` | portal final, camada, posição e efeito de abertura | ligar evento ao `Portal`, ajustar timing e testar travessia |
