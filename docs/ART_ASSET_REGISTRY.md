# Memento Mori — Art Asset Registry

Auditoria física em `Assets/Art` realizada em 13/09/2026.

| PATH | FILE/COUNT | CATEGORY | DIMENSIONS/IMPORT | SCENE USAGE | VISUAL ROLE | NOTES |
|---|---:|---|---|---|---|---|
| `Assets/Art/Characters/Melantha` | 60 PNG | Characters | Import settings a validar no Editor | Quarto, Labirinto, DomínioLua, FinalBeta | Player, cutscenes e narrativa | sprites oficiais; sem placeholder |
| `Assets/Art/Characters/Poe` | 50 PNG | Characters | Import settings a validar no Editor | Quarto, Labirinto, DomínioLua | follower, sinais e aparições | sprites oficiais |
| `Assets/Art/Characters/Andrealphus` | 30 PNG | Characters | Import settings a validar no Editor | Labirinto, DomínioLua | aparição e diálogo | sprites oficiais |
| `Assets/Art/Characters/Portraits` | 0 PNG neste diretório | Characters | localização adicional a auditar | UI de diálogo | retratos | não afirmar existência sem correspondência física |
| `Assets/Art/Sprites/Quarto` | assets existentes | Quarto | usar padrão ambiental comprovado | Quarto | ambiente, props e estados | mapear no Editor antes de pintar |
| `Assets/Art/Sprites/Labirinto` | assets existentes | Labirinto | usar padrão ambiental comprovado | Labirinto | áreas, selos e VFX | não importar referências como fundo |
| `Assets/Art/Sprites/DominioDaLua` | assets existentes | DomínioLua | usar padrão ambiental comprovado | DomínioLua | jardim, espelhos, sigilo e portal | não duplicar puzzles do Labirinto |
| `Assets/Art/Sprites/VFX` | assets existentes | VFX | usar padrão ambiental comprovado | todas | chamas, névoa, portais e partículas | composição independente |
| `Assets/Art/UI` | 30 imagens | UI | validar por uso | todas | HUD, diálogo e telas finais | preservar editabilidade |

Dimensões, PPU, filtro, compressão e mipmaps permanecem `A VALIDAR NO EDITOR` até execução do comando operacional, para não registrar valores por estimativa.

## Associações canônicas verificadas por nome físico

| OBJECT / AREA | ASSET PATH | SCENE | VISUAL ROLE | STATUS |
|---|---|---|---|---|
| Floor / parede / porta | `Assets/Art/Sprites/Quarto/01_Tiles`, `02_Arquitetura/02_entrada_porta_inferior.png` | Quarto | base arquitetural | EXACT_ASSET |
| Cama / janela / escrivaninha | `Assets/Art/Sprites/Quarto/02_Arquitetura/01_janela_com_cortinas.png`, `03_Moveis/01_cama.png`, `03_Moveis/03_escrivaninha.png` | Quarto | composição principal | EXACT_ASSET |
| Grimoire / bowl / toy / photo | `Assets/Art/Sprites/Quarto/06_Gameplay_States/01_grimoire_page_sigil.png`, `03_poe_bowl_altered.png`, `04_poe_toy_mouse.png`, `02_photo_altered.png` | Quarto / FinalBeta | objetos narrativos | EXACT_ASSET |
| Ritual / candles | `Assets/Art/Sprites/Quarto/05_Ritual/01_tapete_ritual_grande.png`, `04_vela_individual.png`, `06_Gameplay_States/07_ritual_circle_off.png`, `06_candle_unlit.png` | Quarto / FinalBeta | ritual e estados | EXACT_ASSET |
| Labyrinth tiles / architecture | `Assets/Art/Sprites/Labirinto/01_Tiles_Base`, `02_Arquitetura` | Labirinto | chão, paredes, arcos e portões | EXACT_ASSET |
| Well / core / domains | `Assets/Art/Sprites/Labirinto/03_Props/48_well_stone.png`, `04_Decals_Sigils/`, `06_Gameplay_States/` | Labirinto | poço, núcleo, selos e Lua | EXACT_ASSET |
| Moon ground / walls / portal | `Assets/Art/Sprites/DominioDaLua/01_Tiles_Base`, `02_Arquitetura/18_portal_frame_final.png` | DomínioLua | arquitetura lunar | EXACT_ASSET |
| Mirrors / black mirror | `Assets/Art/Sprites/DominioDaLua/03_Puzzle_Espelhos`, `08_Puzzle_Espelhos` | DomínioLua | sala dos espelhos | EXACT_ASSET |
| Garden / sigil / fragment | `Assets/Art/Sprites/DominioDaLua/07_Puzzle_Flores`, `04_Puzzle_Sigilo`, `05_Props_Interativos/39_fragment_collectible.png` | DomínioLua | jardim, sigilo e fragmento | EXACT_ASSET |
| Melantha / Poe / Andrealphus | `Assets/Art/Characters/Melantha`, `Poe`, `Andrealphus` | todas | personagens oficiais | EXACT_ASSET |
| Portraits | `Assets/Art/Characters/Portraits` | UI | retratos | NO_EXACT_ASSET (0 PNG verificado) |
