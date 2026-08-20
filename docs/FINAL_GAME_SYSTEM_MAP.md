# FINAL GAME SYSTEM MAP — C0 technical baseline

Evidence baseline: 2026-08-19, commit `25a34749bcc1ec45dfe7c0b23b25a9824d4f0f2b`, post-GitHub publication. C3 code changes are working-tree changes after that baseline. `VERIFIED_READY` is automated evidence only; it does not approve visual quality or human gameplay.

| Check | Result | Evidence |
|---|---|---|
| Compile | PASS | published reference baseline |
| Missing References | 0 | published reference baseline |
| EditMode | 12/12 PASS | `TestResults/editmode-manual-assets-20260819.xml` |
| PlayMode | 3/3 PASS | `TestResults/playmode-manual-assets-20260819.xml` |
| Smoke | 11/11 PASS | `TestResults/ct-evidence-current.json` (`CT-001`–`CT-011`) |

No C0 runtime code, scenes, Tilemaps, props, or visual assets were changed.

| System | Scope / implementation | Status | Evidence / review gap |
|---|---|---|---|
| Build flow | `MainMenu → Quarto → Labirinto → DominioLua → FinalBeta`; five enabled scenes | VERIFIED_READY | EditMode validates order; smoke completes chain. |
| Bootstrap/global state | Bootstrap, state, input gate, loader, manager, accessibility, progression | VERIFIED_READY | Five scenes have bootstrap/configuration; state ownership overlap remains. |
| Player/camera | controller, Rigidbody2D/colliders, camera bounds | NEEDS_REVIEW | Scene bindings exist; movement/collision/bounds have no focused test. |
| Interaction + prompt | detector, common interface/context, prompt UI | NEEDS_REVIEW | Bound in scenes; proximity, priority, and input need human pass. |
| Dialogue | `DialogueData`, trigger, manager; 42 assets | VERIFIED_READY | Required/literal asset contracts pass; C1 audits all content and bindings. |
| Audio | dynamic `RuntimeAudio`, mixer, 20 WAV clips | PARTIAL | Files route at runtime; mixer/volume/loop/caption paths not verified. |
| Menu/pause/reset | menu, pause, manager | VERIFIED_READY | PlayMode covers Play→Quarto; CT-011 pause/menu round trip. Credits/quit untested. |
| Room progression | opening, seven interactions, bed, ritual/dream | VERIFIED_READY | CT-002–004. |
| Labirinto progression | Poe, false door, echoes, checkpoint/portals | VERIFIED_READY | CT-005/006/008; human navigation pass remains. |
| Moon garden | petal puzzle, Poe constraint | VERIFIED_READY | CT-007, including route trace. |
| Mirrors | mirror puzzle/symbols/hints/door | VERIFIED_READY | EditMode, PlayMode and CT-007/008. |
| Sigil rings | ring puzzle/interactable/visual state | VERIFIED_READY | CT-009; lacks direct PlayMode test. |
| Sigil puzzle coverage | `SigilRingPuzzle` plus migrated tests; legacy sequence removed | VERIFIED_READY | Tests now exercise the published ring implementation; Unity rerun is pending license availability. |
| Fragment/final | collectible, final portal, FinalBeta | VERIFIED_READY | CT-010/011. |
| Prefabs | 10 character/interactable prefabs | NEEDS_REVIEW | No prefab override/contract audit. |
| Editor/dev tools | 5 retained Editor utilities, CT runner, tests | DEV_ONLY | Excluded from shipped runtime; obsolete one-shot setup/migration tools removed in C3. |

Post-C3 code inventory: `FIRST_PARTY_SCRIPTS=58`; six obsolete files/tools removed. Runtime system statuses remain unchanged until Unity validation completes.

| Asset group | Inventory | Status |
|---|---:|---|
| Official scenes | 5 | VERIFIED_READY |
| Prefabs | 10 | NEEDS_REVIEW |
| Dialogue ScriptableObjects | 42 | VERIFIED_READY for required contracts; C1 content audit pending |
| Audio | 20 WAV + `MementoMoriMixer.mixer` | PARTIAL |
| Tests | 12 EditMode + 3 PlayMode | VERIFIED_READY; ring coverage gap recorded |

## C1 handoff

Audit dialogue content, conditions, repeat behavior, accessibility text settings and scene bindings. Do not change maps, Tilemaps, or props.
