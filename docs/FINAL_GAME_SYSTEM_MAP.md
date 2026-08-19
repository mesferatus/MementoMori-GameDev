# Final Game System Map

Evidence date: 2026-08-18. `VERIFIED_READY` means a current automated contract passed; it is not visual approval.

| System | Implementation | Scenes | Dependencies | Status | Missing / test method |
|---|---|---|---|---|---|
| MainMenu | `MainMenuController`, `GameManager`, `SceneLoader` | MainMenu | Button_Jogar, build settings | VERIFIED_READY | PlayMode loads Quarto. |
| PlayerMovement | `PlayerController`, `InputGate` | Quarto, Labirinto, DominioLua | Rigidbody2D, input actions | EXISTS_NEEDS_TEST | Human movement/collision pass. |
| Camera | `CameraFollow2D` | gameplay scenes | Player transform | EXISTS_NEEDS_TEST | Human camera bounds pass. |
| Collision | colliders plus `DoorController` | gameplay scenes | Physics2D | EXISTS_NEEDS_TEST | Door collider EditMode contract passes; scene-wide scan pending. |
| Interaction | `IInteractable`, `InteractionDetector`, contexts | gameplay scenes | Player, prompts | EXISTS_NEEDS_TEST | Controlled scene interactions pending. |
| Dialogue | `DialogueManager`, `DialogueTrigger`, `DialogueData` | Quarto, Labirinto, DominioLua | Resources/Dialogue | EXISTS_NEEDS_TEST | 8 required assets and narrative contracts pass in EditMode. |
| GameState | `GameState`, `StoryProgression`, checkpoints | all flow scenes | GameManager | EXISTS_NEEDS_TEST | Flag contract passes; complete progression smoke pending. |
| SceneTransitions | `SceneLoader`, `DreamTransitionController`, `Portal` | all flow scenes | build settings | EXISTS_NEEDS_TEST | MainMenu to Quarto passes; full chain pending. |
| Ritual | `RitualController` | Quarto | GameState, dialogue | EXISTS_NEEDS_TEST | Controlled trigger test pending. |
| Poe | follower, event/reveal/reaction scripts | Labirinto, DominioLua | walls, triggers, GameState | EXISTS_NEEDS_TEST | State/no-speech unit test passes; pathing needs human test. |
| Andrealphus | dialogue assets/triggers | Labirinto, DominioLua | DialogueManager | EXISTS_NEEDS_TEST | Dialogue line contracts pass; scene trigger pending. |
| LabirintoProgression | doors, echoes, checkpoints, transitions | Labirinto | GameState, Poe | EXISTS_NEEDS_TEST | Controlled flow pending. |
| MirrorPuzzle | `PuzzleMirror`, `MirrorSymbol` | DominioLua | GameState, hint/UI | VERIFIED_READY | EditMode and PlayMode contracts pass. |
| IllusoryCorridor | `EchoCorridorPuzzle`, `FalseDoorController` | Labirinto | passages, Poe, GameState | EXISTS_NEEDS_TEST | Controlled progression pending. |
| SigilPuzzle | sequence/ring/part scripts | DominioLua | GameState, portal | VERIFIED_READY | EditMode and PlayMode contracts pass. |
| FragmentCollection | `FragmentCollectible` | DominioLua | final portal, GameState | EXISTS_NEEDS_TEST | Controlled collection/transition pending. |
| InteractionPrompt | `InteractionPromptUI` | gameplay scenes | detector/UI canvas | EXISTS_NEEDS_TEST | Scene binding scan pending. |
| ObjectiveUI | `HintController`, toasts in runtime UI | gameplay scenes | GameState, dialogue | PARTIAL | No dedicated objective-toast script/test found. |
| Audio | `RuntimeAudio`, 20 WAV files | all flow scenes | mixer/resources | EXISTS_NEEDS_TEST | Files present; serialized usage audit pending. |
| FinalBeta/NewGameReset | `GameManager`, menu, `Portal`, fragment | FinalBeta, MainMenu | GameState/SceneLoader | EXISTS_NEEDS_TEST | Reset method exists; full round trip pending. |

Current totals: `VERIFIED_READY=3`, `EXISTS_NEEDS_TEST=16`, `PARTIAL=1`, `MISSING=0`.
