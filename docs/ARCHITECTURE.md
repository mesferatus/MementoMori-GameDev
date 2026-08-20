# Arquitetura — C0 technical baseline

```text
MainMenu → Quarto → Labirinto → DominioLua → FinalBeta
             room             Poe/echoes       garden/mirrors/rings → fragment
```

Each official scene owns `GameSystemsBootstrap` and `SceneConfiguration`. Bootstrap creates the persistent `MementoMoriSystems` host: `GameState`, `StoryProgression`, `AccessibilitySettings`, `InputGate`, `SceneLoader`, and `GameManager`.

`GameState` owns flags, counters, puzzle progress, checkpoints, reset and `PlayerPrefs` serialization. `StoryProgression` remains only as a compatibility facade so existing scene callers do not change ownership or behavior.

| Layer | Responsibility | Main types |
|---|---|---|
| Core | session, input, fade/load, accessibility | `GameState`, bootstrap, loader, manager |
| Player | movement and camera | `PlayerController`, `CameraFollow2D` |
| Interaction | candidate selection/common contract | detector, context, interface |
| Dialogue/UI | data sequences and presentation | dialogue types, menu/pause/prompt |
| World/Puzzles/Poe | scene gameplay and progress flags | controllers under respective folders |
| Audio | resource-loaded clips and loops | `RuntimeAudio` |
| Verification | tests and CT smoke evidence | `Assets/Tests`, `CtEvidenceRunner` |

Complexity: `S` simple adapter; `M` bounded state/dependency; `L` multi-step flow, persistence, or orchestration. “Referenced by” means serialized game binding or direct runtime/test ownership.

| Script | Path | Purpose | System | Referenced by | Status | Complexity | Action |
|---|---|---|---|---|---|---|---|
| AccessibilitySettings | `Assets/Scripts/Core/AccessibilitySettings.cs` | session access settings | Core | bootstrap; dialogue/audio | NEEDS_REVIEW | M | KEEP |
| GameManager | `Assets/Scripts/Core/GameManager.cs` | new game/menu reset | Core | bootstrap; menu/pause | VERIFIED_READY | S | KEEP |
| GameState | `Assets/Scripts/Core/GameState.cs` | flags/counters/checkpoints | Core | bootstrap; gameplay | VERIFIED_READY | L | KEEP |
| GameSystemsBootstrap | `Assets/Scripts/Core/GameSystemsBootstrap.cs` | persistent services/loops | Core | 5 scenes | VERIFIED_READY | M | KEEP |
| InputGate | `Assets/Scripts/Core/InputGate.cs` | reason-based input lock | Core | bootstrap; UI/dialogue | VERIFIED_READY | M | KEEP |
| SceneConfiguration | `Assets/Scripts/Core/SceneConfiguration.cs` | inspector scene contract | Core | 5 scenes; tests | VERIFIED_READY | S | KEEP |
| SceneLoader | `Assets/Scripts/Core/SceneLoader.cs` | fade + guarded load | Core | bootstrap; flow | VERIFIED_READY | L | KEEP |
| StoryProgression | `Assets/Scripts/Core/StoryProgression.cs` | compatibility facade over GameState | Core | bootstrap; puzzles | VERIFIED_READY | S | KEEP |
| RuntimeAudio | `Assets/Scripts/Audio/RuntimeAudio.cs` | clip/loop playback | Audio | core/dialogue/world | PARTIAL | L | KEEP |
| DialogueData | `Assets/Scripts/Dialogue/DialogueData.cs` | dialogue SO schema | Dialogue | 42 assets/triggers | VERIFIED_READY | M | KEEP |
| DialogueManager | `Assets/Scripts/Dialogue/DialogueManager.cs` | dialogue UI/history | Dialogue | 3 scenes; world | NEEDS_REVIEW | L | KEEP |
| DialogueTrigger | `Assets/Scripts/Dialogue/DialogueTrigger.cs` | dialogue/flags/event | Dialogue | 9 bindings | VERIFIED_READY | L | KEEP |
| GardenPetalInteractable | `Assets/Scripts/Interaction/GardenPetalInteractable.cs` | place petal | Interaction | garden scene | VERIFIED_READY | S | KEEP |
| IInteractable | `Assets/Scripts/Interaction/IInteractable.cs` | common interface | Interaction | detector/interactables | VERIFIED_READY | S | KEEP |
| InteractionContext | `Assets/Scripts/Interaction/InteractionContext.cs` | caller context | Interaction | detector/interactables | VERIFIED_READY | S | KEEP |
| InteractionDetector | `Assets/Scripts/Interaction/InteractionDetector.cs` | proximity/input selection | Interaction | player binding | NEEDS_REVIEW | M | KEEP |
| SigilRingInteractable | `Assets/Scripts/Interaction/SigilRingInteractable.cs` | changes ring | Interaction | moon scene | VERIFIED_READY | S | KEEP |
| WaningFlowerNode | `Assets/Scripts/Interaction/WaningFlowerNode.cs` | waning garden input | Interaction | moon scene | VERIFIED_READY | S | KEEP |
| CameraFollow2D | `Assets/Scripts/Player/CameraFollow2D.cs` | follow/bounds | Player | 3 bindings | NEEDS_REVIEW | S | KEEP |
| PlayerController | `Assets/Scripts/Player/PlayerController.cs` | movement/footsteps/test hook | Player | player prefab | NEEDS_REVIEW | M | CLEANUP |
| PoeEnvironmentalReaction | `Assets/Scripts/Poe/PoeEnvironmentalReaction.cs` | state reaction | Poe | labyrinth | VERIFIED_READY | S | KEEP |
| PoeEventPoint | `Assets/Scripts/Poe/PoeEventPoint.cs` | route marker | Poe | labyrinth | VERIFIED_READY | S | KEEP |
| PoeEventTrigger | `Assets/Scripts/Poe/PoeEventTrigger.cs` | activates event | Poe | labyrinth | VERIFIED_READY | S | KEEP |
| PoeFollower | `Assets/Scripts/Poe/PoeFollower.cs` | follow/state | Poe | prefab + scene | NEEDS_REVIEW | L | KEEP |
| PoeRevealTrigger | `Assets/Scripts/Poe/PoeRevealTrigger.cs` | reveal/dialogue | Poe | labyrinth | VERIFIED_READY | M | KEEP |
| GardenPetalPuzzle | `Assets/Scripts/Puzzles/GardenPetalPuzzle.cs` | three petal rules | Puzzles | moon scene | VERIFIED_READY | L | KEEP |
| HintController | `Assets/Scripts/Puzzles/HintController.cs` | staged hint events | Puzzles | puzzle bindings | NEEDS_REVIEW | M | KEEP |
| MirrorSymbol | `Assets/Scripts/Puzzles/MirrorSymbol.cs` | mirror interaction/view | Puzzles | moon scene | VERIFIED_READY | M | KEEP |
| PuzzleMirror | `Assets/Scripts/Puzzles/PuzzleMirror.cs` | mirror solve state | Puzzles | scene/tests/CT | VERIFIED_READY | L | KEEP |
| SigilRingPuzzle | `Assets/Scripts/Puzzles/SigilRingPuzzle.cs` | active three-ring solve | Puzzles | moon scene; CT | VERIFIED_READY | M | KEEP |
| SigilVisualState | `Assets/Scripts/Puzzles/SigilVisualState.cs` | ring visual feedback | Puzzles | moon scene | NEEDS_REVIEW | M | KEEP |
| InteractionPromptUI | `Assets/Scripts/UI/InteractionPromptUI.cs` | interaction label | UI | 3 bindings | NEEDS_REVIEW | S | KEEP |
| MainMenuController | `Assets/Scripts/UI/MainMenuController.cs` | menu/credits/quit | UI | MainMenu | PARTIAL | S | KEEP |
| PauseMenuController | `Assets/Scripts/UI/PauseMenuController.cs` | pause/restart/menu | UI | 3 bindings; CT | VERIFIED_READY | S | KEEP |
| BedController | `Assets/Scripts/World/BedController.cs` | room gate/sleep | World | Quarto | VERIFIED_READY | L | KEEP |
| CheckpointTrigger | `Assets/Scripts/World/CheckpointTrigger.cs` | safe checkpoint | World | 2 bindings | VERIFIED_READY | S | KEEP |
| DoorController | `Assets/Scripts/World/DoorController.cs` | unlock collider/animation | World | puzzle/world; test | VERIFIED_READY | S | KEEP |
| DreamTransitionController | `Assets/Scripts/World/DreamTransitionController.cs` | sleep transition | World | Quarto | VERIFIED_READY | L | KEEP |
| EchoCorridorPuzzle | `Assets/Scripts/World/EchoCorridorPuzzle.cs` | recoverable echoes | World | Labirinto | VERIFIED_READY | M | KEEP |
| EchoPassageChoice | `Assets/Scripts/World/EchoPassageChoice.cs` | echo choice adapter | World | 2 bindings | VERIFIED_READY | S | KEEP |
| FalseDoorController | `Assets/Scripts/World/FalseDoorController.cs` | false-door loop | World | 2 bindings | VERIFIED_READY | S | KEEP |
| FragmentCollectible | `Assets/Scripts/World/FragmentCollectible.cs` | final unlock | World | DominioLua | VERIFIED_READY | M | KEEP |
| OpeningSequenceController | `Assets/Scripts/World/OpeningSequenceController.cs` | opening sequence | World | Quarto | VERIFIED_READY | M | KEEP |
| Portal | `Assets/Scripts/World/Portal.cs` | flag-gated transition | World | 3 bindings | VERIFIED_READY | L | KEEP |
| RitualController | `Assets/Scripts/World/RitualController.cs` | ritual transition | World | Quarto | VERIFIED_READY | S | KEEP |
| ToyEchoController | `Assets/Scripts/World/ToyEchoController.cs` | toy event | World | Quarto | VERIFIED_READY | S | KEEP |
| VoiceWellController | `Assets/Scripts/World/VoiceWellController.cs` | sequential well dialogue | World | 2 bindings | VERIFIED_READY | S | KEEP |
| CtEvidenceRunner | `Assets/Scripts/Verification/CtEvidenceRunner.cs` | CT-001…011 harness | Verification | batch/editor | DEV_ONLY | L | DEV_ONLY |
| AcceptanceContractsTests | `Assets/Tests/EditMode/AcceptanceContractsTests.cs` | build/dialogue/door tests | Tests | EditMode assembly | DEV_ONLY | L | DEV_ONLY |
| PoeAccessibilityEditModeTests | `Assets/Tests/EditMode/PoeAccessibilityEditModeTests.cs` | Poe/settings units | Tests | EditMode assembly | DEV_ONLY | M | DEV_ONLY |
| PuzzleEditModeTests | `Assets/Tests/EditMode/PuzzleEditModeTests.cs` | mirror + legacy sigil | Tests | EditMode assembly | DEV_ONLY | M | REFACTOR |
| MainMenuPlayModeTests | `Assets/Tests/PlayMode/MainMenuPlayModeTests.cs` | Play→Quarto | Tests | PlayMode assembly | DEV_ONLY | S | DEV_ONLY |
| PuzzlePlayModeTests | `Assets/Tests/PlayMode/PuzzlePlayModeTests.cs` | mirror + legacy sigil | Tests | PlayMode assembly | DEV_ONLY | M | REFACTOR |
| BuildWindows | `Assets/Editor/BuildWindows.cs` | development Windows build | Editor | menu/batch | DEV_ONLY | M | DEV_ONLY |
| DialogueAssetsBaker | `Assets/Editor/DialogueAssetsBaker.cs` | dialogue generator | Editor | manual authoring | DEV_ONLY | L | DEV_ONLY |
| SceneConfigurationBaker | `Assets/Editor/SceneConfigurationBaker.cs` | serializes scene contract | Editor | manual authoring | DEV_ONLY | M | DEV_ONLY |
| SceneContentBootstrap | `Assets/Editor/SceneContentBootstrap.cs` | initial content baker | Editor | manual authoring | DEV_ONLY | M | DEV_ONLY |
| UnityBatchLaunchDiagnostic | `Assets/Editor/UnityBatchLaunchDiagnostic.cs` | startup sentinel | Editor | batch diagnostic | DEV_ONLY | S | DEV_ONLY |

## Deferred decisions

- Before removing legacy sigil code, replace both generic puzzle test suites with direct `SigilRingPuzzle` coverage; the shipped scene/CT use rings while those tests use the legacy sequence.
- C3 merged progression storage into `GameState`, kept `StoryProgression` as a compatibility facade, migrated puzzle tests to `SigilRingPuzzle`, and removed six obsolete files/tools. No narrative content was added.
- Retain dialogue/scene bakers until C1 confirms no regeneration is needed; reassess one-shot bootstrap/migration tools after that.
