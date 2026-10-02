# Memento Mori — Visual Scene Asset Map

Mapa de referência para integração, baseado nos diretórios reais de `Assets/Art`. `NO_EXACT_ASSET` deve ser registrado quando a auditoria do Editor não encontrar correspondência.

| Cena | Elemento canônico | Fonte permitida | Função |
|---|---|---|---|
| Quarto | floor, walls, bed, window, desk, grimoire, bowl, toy, photo, ritual item, candles, circle, door, props | `Assets/Art/Sprites/Quarto` | composição do quarto e estados existentes |
| Labirinto | ground, walls, false door, well, echo corridors, empty chamber, core, portal seals, moon portal, arcana/VFX | `Assets/Art/Sprites/Labirinto`, `Assets/Art/Sprites/VFX` | conectividade e selos dos sete domínios |
| DomínioLua | ground/walls, garden, flowers, petals, mirrors, black mirror, cracks, sigil, fragment, portal, lunar VFX | `Assets/Art/Sprites/DominioDaLua`, `Assets/Art/Sprites/VFX` | entrada → jardim → espelhos → sigilo → fragmento → portal |
| FinalBeta | bed, inactive circle, unlit candles, grimoire, photo, fragment evidence, final Canvas | `Assets/Art/Sprites/Quarto`, `Assets/Art/UI` | retorno ao quarto e end card |
| Personagens | Melantha, Poe, Andrealphus, portraits | `Assets/Art/Characters` | integração nos objetos existentes |

As referências visuais canônicas não são assets de cena e não devem ser importadas como background.
