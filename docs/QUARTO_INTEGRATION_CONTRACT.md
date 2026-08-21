# C5.2 — Contrato de integração visual: Quarto

## Escopo

Este contrato liga a montagem visual da Luiza aos sistemas já existentes. Não autoriza mudança de mapa, composição, posição de props ou conteúdo de gameplay.

## Checklist técnico

| ID | OBJECT_NAME_EXPECTED | VISUAL_OWNER | SCRIPT_OR_SYSTEM | TAG/LAYER | COLLIDER | TRIGGER | INTERAÇÃO | ESTADO / MUDANÇA | ÁUDIO | DIÁLOGO | POSIÇÃO | STATUS |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Q01 | `Melantha_TEMP` / Player prefab | Luiza | `PlayerController`, spawn | Player | sim | não | movimento | spawn/retorno | passos | abertura | sim | CONNECT |
| Q02 | `SPAWN_Quarto_Entrada` | Luiza | `SceneLoader`/`SceneConfiguration` | Spawn | não | não | não | entrada | transição | `DLG_ROOM_OPENING` | sim | CONNECT |
| Q03 | `Floor_Tilemap`, `Ground`, `Tilemap_Ground` | Luiza | `PlayerController` | Ground | sim | não | não | navegável | passos | não | sim | PROVIDE+CONNECT |
| Q04 | `Walls_Tilemap`, `WallNorth/South/East/West`, `Collision` | Luiza | física 2D | Walls/Obstacle | sim | não | não | bloqueio | impacto opcional | não | sim | PROVIDE |
| Q05 | `Main Camera` | Luiza | `CameraFollow2D` | Camera | não | não | não | limites | não | não | sim | PROVIDE+CONNECT |
| Q06 | `Bed` | Luiza | `BedController` | Interactable | sim | não | sim | `RoomSleepConfirmed` | interação | `DLG_ROOM_BED_LOCKED_*` | sim | CONNECT |
| Q07 | `Window_Q06` / `Window` | Luiza | `DialogueTrigger` | Interactable | sim | não | sim | observação | blip | `DLG_ROOM_WINDOW_*` | sim | CONNECT |
| Q08 | `Desk`, `Grimoire` | Luiza | `DialogueTrigger` | Interactable | sim | não | sim | `RoomGrimoireRead` | blip | `DLG_ROOM_GRIMOIRE_*` | sim | CONNECT |
| Q09 | `Photo` / `Portrait` | Luiza | `DialogueTrigger` | Interactable | sim | não | sim | leitura da fotografia | blip | `DLG_ROOM_PHOTO_*` | sim | CONNECT |
| Q10 | `EmptyBowl` / `PoeBowlPoeToy` | Luiza | `DialogueTrigger` | Interactable | sim | não | sim | pistas de Poe | blip | `DLG_ROOM_BOWL_*` | sim | CONNECT |
| Q11 | `PoeToy` / `ToyEcho` | Luiza | `ToyEchoController` | Interactable | sim | sim quando evento | sim | `PoeToyExamined` | reação | `DLG_ROOM_TOY` | sim | CONNECT |
| Q12 | `RitualCircle`, `RitualItem`, `RitualPaper` | Luiza | `RitualController`, `BedController` | Interactable | sim | sim | sim | `RoomRitualItemStored`, `RitualCompleted` | `04_ritual_circle_loop` | `DLG_ROOM_RITUAL_ITEM`, `DLG_SIGIL_HINT_*` | sim | CONNECT |
| Q13 | `CandleNE/NW/SE/SW` | Luiza | `DialogueTrigger` | Interactable | sim | não | sim | feedback das velas | ritual/feedback | `DLG_ROOM_CANDLES` | sim | CONNECT |
| Q14 | `Door_Q01`, `PortalToLabirinto` | Luiza | `DoorController`, `Portal` | Portal | sim | sim | sim | ritual gate | `12_portal_open` | `DLG_DREAM_TRANSITION` | sim | CONNECT |
| Q15 | `OpeningSequence`, `DreamTransitionController` | Callisto | sequência/loader | System | não | sim | não | bloqueio temporário | `05_transition_sleep` | opening/transition | não | CONNECT |
| Q16 | `DialogueManager`, `DialoguePanel`, `InteractionPrompt` | Luiza (skin) | `DialogueManager`, `InteractionPromptUI` | UI | não | não | sim | aberto/fechado | blip | apresentação | não | CONNECT |
| Q17 | `PauseMenu`, `EventSystem`, `GameplayCanvas` | Luiza (skin) | `PauseMenuController`, `InputGate` | UI | não | não | sim | pause/input lock | menu | não | não | CONNECT |

## LUIZA_MUST_PROVIDE

- Cena abre sem Missing References causadas pela montagem.
- Visual principal, chão, paredes, câmera, objetos e sorting básico montados.
- Os nomes acima preservados ou entregues com uma tabela de alias explícita.
- Colliders de chão/parede e colliders de interação básicos nos objetos marcados.
- Pontos `SPAWN_Quarto_Entrada` e `SPAWN_Quarto_PortalLabirinto` preservados.
- Elementos visuais de ritual, porta, janela, cama, grimório, fotografia e objetos de Poe presentes.

## CALLISTO_MUST_CONNECT

`PlayerController`, `CameraFollow2D`, `BedController`, `RitualController`, `ToyEchoController`, `DialogueTrigger`, `Portal`, `SceneLoader`, `RuntimeAudio`, `DialogueManager`, `InteractionPromptUI`, `PauseMenuController` e `EventSystem`; depois validar flags, transição, áudio, diálogo e regressão.

## Gate

`QUARTO_READY_CONTRACT = YES`. A cena só é `READY_FOR_INTEGRATION` quando todos os itens `LUIZA_MUST_PROVIDE` estiverem presentes e o scan de referências estiver limpo. A Luiza não implementa gameplay.

## AUDIO_VISUAL_PENDING

| ID | SCENE | VISUAL_DEPENDENCY | AUDIO_EVENT_READY | WHAT_LUIZA_PROVIDES | WHAT_CALLISTO_ADJUSTS_AFTER |
|---|---|---|---|---|---|
| AV01 | Quarto | feedback visual do círculo/velas | `04_ritual_circle_loop` | ativação visual e ponto de leitura do ritual | confirmar timing, volume e bloqueio do ritual |
