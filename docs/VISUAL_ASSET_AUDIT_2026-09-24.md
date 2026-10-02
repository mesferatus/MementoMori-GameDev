# Memento Mori — auditoria dos assets visuais (24/09/2026)

## 1. RESUMO

**Resultado:** inventário e triagem concluídos; **nenhum asset novo confirmado para geração**. O projeto tem 626 PNG em `Assets/Art` (60 Melantha, 50 Poe, 30 Andrealphus, 177 FinalProduction, 38 Quarto, 89 Labirinto, 106 Domínio da Lua, 46 VFX e 30 UI) e 19 PNG fonte em `ArtStaging`. Foram inspecionados importadores no Editor Unity 6000.4.12f1, GUIDs em cenas/prefabs/clips/assets, hashes SHA-256 dos PNG em `Assets/Art` e capturas do ponto de entrada de Quarto, Labirinto e Domínio da Lua em Play Mode. Não houve geração, alteração de importador, movimentação, exclusão, commit ou push.

**Critério de uso:** “referenciado” significa GUID encontrado em cena, prefab, animação, controller ou `.asset` sob `Assets`. Ausência desse vínculo **não prova** ausência de carregamento por caminho/código, Addressables ou uso editorial. `ArtStaging` está fora de `Assets`, portanto não é importado pela Unity. As capturas cobrem a entrada das cenas, não todos os estados de puzzle ou uma partida completa. O Game View do MainMenu apareceu uniforme na captura por câmera; Canvas Overlay não foi validado visualmente por esse método. Nenhum item foi marcado `LEGACY_SAFE_TO_ARCHIVE` só por não ter referência direta.

**Histórico versus estado atual:** `docs/FINAL_ART_PRODUCTION_MANIFEST.md` planejava gerar vários conjuntos e retratos antes das rodadas R2/R3. Hoje as sheets, os retratos e boa parte dos substitutos já existem. `docs/VISUAL_R2_PROGRESS_2026-09-22.md` relata integração parcial e testes daquela rodada; eles não são revalidação desta auditoria.

**Padrão dos importadores:** os 77 PNG R2 e 72 PNG R3 usam Sprite Single, PPU 32, Point, Uncompressed, mipmaps desligados e `alphaIsTransparency=true`. As sheets runtime de personagens/tiles usam Multiple e grades uniformes. Os 30 PNG de UI usam Single, PPU 100, Point, Uncompressed, mipmaps desligados e alpha ativo; PPU de UI não equivale a escala de mundo. Os 140 PNG de personagens antigos usam Multiple com um único recorte por arquivo, Point, Uncompressed, sem mipmaps e alpha ativo. Esta configuração recorta margens transparentes e mantém seus pivôs; não foi alterada em massa.

## 2. PERSONAGENS

| Grupo/caminho | Resolução; importação; slices e pivô | Vínculo encontrado | Classificação |
|---|---|---|---|
| `Assets/Art/Characters/Melantha` (60 PNG) | Predominam canvas 64×64; alguns 96×96. Multiple, PPU 24, Point, Uncompressed, alpha, mip off; **um recorte ajustado por PNG**, tamanho variável (ex.: idle frontal 32×60, pivô 16×2 px). | 40/60 GUID com referência, sobretudo ações/cutscenes em 18 clips; idle e walk antigos praticamente sem uso nos clips atuais. | `FIX_SCALE` para transições entre ações antigas e locomoção R3; preservar arte. |
| `Assets/Art/FinalProduction/Sheets/M1_MelanthaLocomotion_Runtime.png` | 256×384; Multiple, PPU 32, Point, Uncompressed, alpha, mip off; **16 quadros 64×96**, pivô perto dos pés, X=32, Y variável ~1–8 px. | Referência direta nas quatro cenas jogáveis/final. | `KEEP` como recurso runtime existente; não substituir sem mapear referências. |
| `Assets/Art/FinalProduction/R3/Melantha` (24 PNG) | Idle e walk 64×64; Single, PPU 32, Point, Uncompressed, alpha, mip off; pivô nos pés (ex. 32×2 px). | 3 idle e 18 walk em `Assets/Art/Animations/Melantha`; o idle R3 foi visto no Player em Play Mode no Quarto. | `KEEP` para idle/walk; `FIX_SCALE` na integração com as ações antigas. |
| `Assets/Art/FinalProduction/R2/M1_Walk_*` (12 PNG) | Quadros 64×96/variações; Single, PPU 32, Point, Uncompressed; pivôs de pé. | Clips de walk foram religados ao R3; quadros R2 sem referência direta atual. | `UNKNOWN`/OLDER, manter para revisão; não arquivar. |
| `Assets/Art/Characters/Poe` (50 PNG) + `Assets/Art/FinalProduction/Sheets/P1_PoeLocomotion_Runtime.png` | Antigos: canvas 48×48 ou 64×48, recortes variáveis, Multiple com um sprite/PNG, PPU 32. Runtime: 192×192, 16 quadros 48×48, pivôs de pé. Todos Point, Uncompressed, alpha, mip off. | 30/50 PNG antigos em clips de ação; a sheet runtime nos clips idle/walk e nas cenas Labirinto/Domínio. | `KEEP`; nenhuma regeneração. |
| `Assets/Art/Characters/Andrealphus` (30 PNG) | Canvas 96×128, recortes de silhueta variáveis (ex. 80×109), Multiple com um sprite/PNG, PPU 32, Point, Uncompressed, alpha, mip off; pivô ~2 px dos pés. | 30/30 PNG nos 7 clips; idle direto em Labirinto/Domínio. | `KEEP`; nenhuma regeneração. |
| `Assets/Art/FinalProduction/Sheets/C1_Portraits_Pixel.png` | 1254×1254; Multiple, PPU 100, Point, Uncompressed, alpha, mip off; **9 recortes 418×418**, pivôs centrais. | Quatro cenas; três expressões para cada um dos três personagens estão presentes. | `KEEP`; retratos não faltam. |

**Melantha em Edit/Play:** no Quarto em Edit Mode, `Melantha_TEMP` tem renderer desativado e sprite nulo; o `Player.prefab` também guarda `RuntimeVisual` sem sprite, com escala local 0,75. Em Play Mode, o Player instanciado apresentou `RuntimeVisual` ativo, escala global 1 e `Assets/Art/FinalProduction/R3/Melantha/idle_front.png`, ligado a `Melantha.controller`. Portanto a imagem efetiva vem da inicialização/animação runtime, não do marcador temporário. Os clips atuais de walk usam 18 quadros R3 (6 por direção), os idle usam 3 PNG R3, e as 12 demais ações/cutscenes ainda usam recortes antigos PPU 24. O salto potencial de tamanho entre estados é o problema comprovável por configuração; a aparência em movimento e cada transição ainda exigem revisão visual. **Conclusão Melantha: `FIX_SCALE`**, não `REPLACE` nem `FIX_SLICE` da sheet runtime.

As sheets grandes `M1_MelanthaLocomotion.png`/`_Source.png` têm recortes automáticos irregulares, mas não alimentam os clips atuais. São fontes; `M1_MelanthaLocomotion_Runtime.png` está em grade regular. A R3 oferece versão atual conectada; a prioridade é alinhar as ações antigas antes de considerar arte nova.

## 3. QUARTO

| Grupo/caminho | Resolução/importação/slices/pivô | Uso e avaliação | Classificação |
|---|---|---|---|
| `Assets/Art/FinalProduction/Tiles/Q1_RoomTiles32.png` | 128×128, Multiple, PPU 32, Point, Uncompressed, alpha, mip off; 16 tiles 32×32, pivô 16×16. | Referenciado em Quarto e FinalBeta. Piso e paredes estão visíveis na captura de entrada; não há ausência física de parede. | `KEEP` |
| `Assets/Art/FinalProduction/R2/Q2_Window_States/window_open.png` e `window_secured.png` | 96×64, Single, PPU 32, Point, Uncompressed, alpha, mip off; um sprite, pivô de base (48×2). | Ambos referenciados no Quarto; a janela aparece na parede norte. | `KEEP` |
| `Assets/Art/FinalProduction/R2/Q2_Bed`, `Q2_Furniture`, `Q3_Props_States`, `Q4_Ritual_States` | Sprites individuais, Single, PPU 32, Point, Uncompressed, alpha, mip off; dimensões variáveis, um recorte/PNG. | Cama, mesa, objetos e tapete/velas conectados ao Quarto e FinalBeta. Captura de entrada confirma composição básica. | `KEEP` |
| `Assets/Art/Sprites/Quarto` (38 PNG) | Mistura de PPU 32 (14), 100 (22) e Multiple PPU 32 (2), Point/Uncompressed/alpha/mip off; 1 sprite por PNG ou por recorte. | 22/38 GUID referenciados; inclui arquitetura, móveis, props e estados anteriores. O recorte de parede antigo de 15×6 px é inadequado para alongar uma parede, mas o tile runtime Q1 já existe. | `UNKNOWN` para legado não conectado; `FIX_SCALE` somente se reutilizado no mundo. |

**Posição/sorting:** a captura do ponto de entrada mostrou cama, janela, mesa, Melantha e tapete sem sobreposição flagrante. Não prova ordenação em toda a sala nem em movimento. **Não gerar parede ou janela agora:** ambas já existem e estão conectadas; qualquer ajuste posterior deve partir de comparação visual com a referência aprovada e da composição real.

## 4. LABIRINTO

`Assets/Art/Sprites/Labirinto` contém 89 PNG: floors, walls, corners, pilares, arcos, correntes, banners, braseiros, poço, selos, Arcana/ícones de domínio, porta falsa e estados do portal. `Assets/Art/FinalProduction/R2/L3_Details_States` e `L3_FalseDoor_States` trazem estados novos; `Assets/Art/FinalProduction/R3/LabyrinthTiles` traz 16 tiles 32×32, seis diretamente referenciados pela cena de entrada, além de decoração R3. O ponto de entrada em Play Mode mostra piso, paredes, portal/porta e sigilo; não equivale a inspeção de cada corredor/puzzle.

| Grupo | Configuração e uso | Classificação |
|---|---|---|
| `FinalProduction/Sheets/L1_LabyrinthTiles_Runtime.png` | 128×128; Multiple, PPU 32, Point, Uncompressed, alpha, mip off; 16 slices 32×32, pivôs 16×16; referenciado por tiles `.asset`. | `KEEP` |
| `FinalProduction/R3/LabyrinthTiles` + `R3/Decoration` | 16 tiles e 16 decorações, Single/PPU 32/Point/Uncompressed; 6 tiles e 9 decorações referenciados diretamente por `Labirinto.unity`. | `KEEP` conectados; `UNKNOWN` para variações sem uso direto. |
| `FinalProduction/R2/L3_*` | 10 sprites, Single/PPU 32/Point/Uncompressed; porta falsa, poço e braseiro conectados à cena. | `KEEP` |
| `Sprites/Labirinto` | 89 PNG, 48 PPU 32 e 41 PPU 100; 21 referências GUID. Variantes antigas ainda podem ser invocadas por código/fase. | `UNKNOWN` para não conectados; sem arquivo confirmado para descarte. |

**Echo Corridor, Voice Well e Arcana:** os marcos físicos/selos e o poço existem; sua identidade de interação depende de cena, componentes e estado narrativo, não de um PNG novo comprovadamente ausente. Portais e VFX também têm fontes existentes. Não há justificativa atual para nova geração do Labirinto.

## 5. DOMÍNIO DA LUA

`Assets/Art/Sprites/DominioDaLua` contém 106 PNG de floors/walls/corners, arquitetura, cristais, flores/pétalas, espelhos e reflexos, Espelho Negro, sigilo, fragmento, portal, pedestal e FX. `FinalProduction/R2/D2` a `D5_D6` acrescenta estados do jardim, molduras/espelhos, rachaduras e props; `FinalProduction/R3/MoonTiles` e `R3/Decoration` dão tiles e decoração novos. O ponto de entrada em Play Mode mostra piso, parede, portal/arquitetura e Melantha; jardim e Espelho Negro exigem inspeção visual no estado interativo.

| Grupo | Configuração e uso | Classificação |
|---|---|---|
| `FinalProduction/Sheets/D1_MoonTiles_Runtime.png` | 128×128; Multiple, PPU 32, Point, Uncompressed, alpha, mip off; 16 slices 32×32, pivôs 16×16; referenciado por tiles `.asset`. | `KEEP` |
| `FinalProduction/R3/MoonTiles` + `R3/Decoration` | Single/PPU 32/Point/Uncompressed; 6 tiles e 11 decorações aparecem em `DominioLua.unity`. | `KEEP` conectados; `UNKNOWN` para variações sem uso direto. |
| `FinalProduction/R2/D2_Garden_States`, `D3_Mirror_Modules`, `D4_BlackMirror_States`, `D5_D6_MoonProps` | Single/PPU 32/Point/Uncompressed; flores, moldura, espelho negro, ciclos, fragmento e pedestal referenciados na cena. | `KEEP` |
| `Sprites/DominioDaLua` | 106 PNG, 62 PPU 32 e 44 PPU 100, Point/Uncompressed; 20 GUID com referência. Inclui 8 `03_Puzzle_Espelhos`, reflexos/rachaduras em `08_Puzzle_Espelhos`, 14 flores/estados, sigilo e portal. | `UNKNOWN` para os antigos sem referência direta; não arquivar. |

O fato de `D2_LunarGarden_Runtime.png` e `D3_Mirrors_Runtime.png` não terem referência GUID direta não transforma jardim/espelhos em faltantes: os módulos R2 e sprites antigos já estão ligados. Priorizar revisão de composição e estado, não geração.

## 6. UI

`Assets/Art/UI` contém 30 PNG (10 MainMenu, 10 Gameplay, 10 Screens), todos Single/PPU 100/Point/Uncompressed/alpha/mip off, um sprite por PNG, pivôs centrais. 15/30 têm referência GUID direta. `U1_Logo.png` (1774×887, Single), `U2_DialogueUI.png` (1536×1024, Multiple, 4 recortes de tamanhos variáveis), `U3_PauseEndUI.png` (1254×1254, Multiple, 4 recortes) e `C1_Portraits_Pixel.png` estão conectados às cenas/prefabs.

| Área | Referência comprovada | Classificação |
|---|---|---|
| Main Menu | `U1_Logo`, botões e ornamentos `Assets/Art/UI/MainMenu` em `MainMenu.unity`. | `KEEP` dos PNG; composição/Canvas `UNKNOWN`. |
| Dialogue / Objective / Interaction Prompt | `U2_DialogueUI` nos prefabs `DialogueUI`, `ObjectiveUI`, `InteractionPromptUI` e `GameplayUI`; cursor/keycap/ícone existentes. | `KEEP` dos PNG; posição/anchors/escala dependem de revisão de Canvas. |
| Pause / FinalBeta | `U3_PauseEndUI` em PauseUI, MainMenu, FinalBeta; painéis de fim/créditos e fragmento presentes. | `KEEP` dos PNG; apresentação final `UNKNOWN`. |
| Portraits | Nove retratos dedicados em `C1_Portraits_Pixel`, referenciados nas cenas. | `KEEP`. |

O manifesto UI Beta informa que **fundo separado do menu não foi incluído por decisão de produção**. Não tratá-lo como asset faltante. A captura por câmera do menu ficou uniforme e não verificou o Canvas Overlay; inspecionar Game View/Canvas de fato antes de atribuir defeito ao PNG. A auditoria não encontrou justificativa para nova UI.

## 7. VFX

`Assets/Art/Sprites/VFX` tem 46 PNG (flamas, partículas lunares, Poe vanish, aparição de Andrealphus, portal, sigilo, névoa e brilho de interação), todos Single, PPU 32, Point, Uncompressed, alpha ativo e mip off. Nenhum GUID direto apareceu em cena/prefab/clip/asset no scan; podem ser reservados ou carregados por código. Labirinto tem seis flamas em `05_FX_Flames` e Domínio oito FX em `06_FX`. **Classificação:** `UNKNOWN` para o conjunto VFX genérico, `KEEP` para efeitos específicos comprovadamente presentes/ligados. Antes de criar VFX, tentar conectar ou avaliar os 46 existentes.

## 8. DUPLICATAS

SHA-256 encontrou **um par de PNG idênticos** em `Assets/Art`: `Assets/Art/UI/MainMenu/06_menu_moon_emblem.png` e `Assets/Art/UI/Screens/26_final_center_emblem.png`. O primeiro tem referência no MainMenu, o segundo não tem referência GUID direta; papéis de menu e encerramento são distintos. Marcação: primeiro **CANONICAL para menu**, segundo **DUPLICATE de conteúdo / UNKNOWN quanto ao uso futuro**. Não excluir ou mover.

As sheets grandes `*_Source.png`/sem `_Runtime` e seus equivalentes runtime são **versões funcionais diferentes**, não duplicatas de hash: as primeiras têm dimensões grandes e recortes automáticos, as segundas são grades reduzidas importadas para o jogo. `ArtStaging/VisualRebuildR2` guarda 15 fontes e `ArtStaging/VisualRefinementR3` guarda 4; os 77 PNG R2 e 72 R3 em `Assets/Art/FinalProduction` são os recortes promovidos. O critério CANONICAL é a referência runtime vigente, não o arquivo mais recente por nome.

## 9. LEGACY

**`LEGACY_SAFE_TO_ARCHIVE`: zero confirmado.** Há candidatos OLDER/UNUSED por GUID direto — walks/idle antigos de Melantha, parte de `Sprites/Quarto`, Labirinto, Domínio, UI e muitas sheets grandes — mas alguns podem ser fonte de produção, reserva de estados ou carregados por código. `Assets/_Legacy_Review` não foi criado. Nenhum `.meta` foi movido. Reavaliar somente após levantamento de carregamento por caminho e aprovação visual das cenas/estados.

## 10. PROBLEMAS DE IMPORT

As **sheets fonte não referenciadas** de `FinalProduction/Sheets` ainda usam PPU 100, Bilinear e Compressed, embora muitas sejam pixel art: `A1_AndrealphusLocomotion.png`, `A2_AndrealphusActions.png`, `C1_Portraits_Source.png`, `D1_MoonTiles.png`, `D2_LunarGarden.png`, `D3_Mirrors.png`, `L1_LabyrinthTiles.png`, `M1_MelanthaLocomotion.png`, `M1_MelanthaLocomotion_Source.png`, `M2_MelanthaActions.png`, `P1_PoeLocomotion.png`, `P2_PoeActions.png`, `Q1_RoomArchitecture.png` (13 arquivos). Isso seria `FIX_IMPORT` **se usados diretamente como sprite no jogo**; atualmente são fontes não referenciadas, e suas contrapartes runtime estão Point/Uncompressed/PPU 32. Alterá-las agora não melhora a apresentação e confundiria fonte com runtime. `C1_Portraits_Pixel`, `U1/U2/U3` já usam Point/Uncompressed e são UI em PPU 100 por escolha de Canvas.

Os PNG antigos ambientais em PPU 100 misturados a PPU 32 são `FIX_SCALE` **condicional** à reutilização no mundo. A alteração global de PPU afetaria dimensões, pivôs e cenas; nenhuma mudança em massa foi aplicada. Para Melantha, o descompasso PPU 24 versus R3 PPU 32 e quadros de altura distinta requer revisão de transições no jogo antes de corrigir import ou Transform.

## 11. PROBLEMAS DE SLICING

**Runtime verificado por grade:** M1 16×64×96; P1 16×48×48; A1/A2 16×96×128; D1/L1/Q1 16×32×32; D2 16×64×64; D3 12×64×128; C1 9×418×418. Não há evidência de fatias vazias nas sheets runtime examinadas visualmente. Os pivôs de personagem ficam perto dos pés; os dos tiles no centro. Os nomes das fatias runtime foram conferidos para os grupos principais por importador; não houve renomeação de subassets e, portanto, nenhum fileID/GUID foi perturbado.

**Fontes com recorte automático inadequado para uso direto:** `A1_AndrealphusLocomotion.png` (16 slices, um 7×12), `D2_LunarGarden.png` (44, vários 6–18 px), `D3_Mirrors.png` (14, um 7×8), `M1_MelanthaLocomotion_Source.png` (17, um 11×9), `M2_MelanthaActions.png` (6, um 11×13), `P1_PoeLocomotion.png` (50, vários 6–20 px), `P2_PoeActions.png` (42, vários 6–20 px). Classificação `FIX_SLICE` **apenas se promovidas para uso direto**; as versões runtime estão disponíveis e a mudança agora não é segura nem necessária. Outras sheets fonte com um recorte do canvas inteiro (`A2`, `D1`, `L1`, `Q1`) são arquivos de origem, não tilesets runtime. Não modificar o slicing delas sem objetivo de integração.

## 12. ASSETS REALMENTE FALTANTES

**Nenhum asset novo confirmado.** Portanto não há item com ASSET / MOTIVO / REFERÊNCIA / TIPO / RESOLUÇÃO-ESCALA / PRIORIDADE a enviar para geração. Em particular: parede e janela do Quarto, tiles do Labirinto/Lua, espelhos, jardim, fragmento, retratos e UI já existem. Melantha é caso de **escala/consistência entre clips**; as sheets runtime estão recortadas. Poe e Andrealphus permanecem preservados.

**Condição para eventual entrada futura nesta lista:** demonstrar em cena/estado específico que o asset existente não atende à referência visual aprovada, registrar caminho/GUID do substituído e excluir primeiro problemas de import, slicing, escala, sorting e ligação. Até essa revisão humana, a fila de geração é vazia. **Pronto para enviar pedido de geração ao Astra: NÃO** — não há lacuna confirmada para solicitar.
