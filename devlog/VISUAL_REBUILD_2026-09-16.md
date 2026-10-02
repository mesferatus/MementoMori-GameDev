# Visual rebuild 2026 09 16

## Scope

Visual pass requested against the V3 functional baseline. Existing scripts, scenes, controllers, dialogue IDs, puzzle states and build pipeline remain in scope for preservation. The attached images are treated as visual references; the narrative DOCX is treated as the narrative source.

## Completed

- Audited 449 physical PNGs under `Assets/Art` and captured scene inventory for Quarto, Labirinto, DominioLua, FinalBeta and MainMenu.
- Created `docs/FINAL_ART_PRODUCTION_MANIFEST.md` and `docs/FINAL_ART_ASSET_INVENTORY.json` with per asset classification, target scale, pivot, scene and animation data.
- Added `docs/FINAL_ART_GENERATION_QUEUE.json` so blocked image batches can be resumed without re-planning.
- Generated the first architecture sheet and normalized it into a 32 pixel tile source under `Assets/Art/FinalProduction`.
- Added the presentation migration editor and depth sorting component. They use the live Unity Editor and keep gameplay references intact.
- Added Cinzel, Cormorant Garamond and Unifraktur Maguntia font files with OFL notices under `Assets/Art/Fonts`.

## Blockers

The image generation service and Unity MCP compilation query returned the account usage limit. No workaround was used. Character sheets, new logo/UI sheets, lunar replacement sheets, scene application and post migration test runs are queued for the next available window.

## Verification boundary

Static file checks passed for the generated sheet, normalized tile source, manifest, inventory and queue. Unity Editor application, compilation, visual capture, EditMode, PlayMode, CT and Windows build verification are still pending after the quota is restored.
