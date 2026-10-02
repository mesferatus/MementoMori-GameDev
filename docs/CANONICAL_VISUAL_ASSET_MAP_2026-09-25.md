# Memento Mori — mapa proposto de canonização visual

**Estado:** proposta para aprovação da Callisto, sem alterações em PNG, `.meta`, cena, prefab, Animator ou código. Inspeção em 25/09/2026, Unity 6000.4.12f1 conectada à instância do projeto; Editor estava fora de Play Mode, com `Quarto` aberto. Foram lidos arquivos, importadores e GUIDs, e comparadas as imagens nas oito pranchas abaixo. **Nenhum Play Mode novo foi executado nesta rodada.** A observação visual de `idle_front` em Play Mode vem da auditoria de 24/09.

## Como ler

- `CURRENTLY_USED` = referência GUID estática em cena, prefab, clip, controller ou `.asset`, salvo quando se especifica observação em Play Mode. Código pode carregar por caminho ou criar imagem em runtime; ausência de GUID não prova que um PNG nunca aparece.
- `CANONICAL` = escolha aprovada pela Callisto ou recomendação provisória identificada como tal. Um asset referenciado pode estar visualmente errado.
- `RELINK` = proposta para fase posterior; **nenhum vínculo foi alterado agora**.
- Nomes de pastas nas tabelas são relativos a `Assets/Art/`; o [índice completo](CanonicalVisualReview/asset_reference_matrix.csv) contém caminho, dimensão, PPU, pivô de importador, GUID e cada arquivo com referência direta para os **623 PNG existentes hoje**. A [matriz dos 18 clips](CanonicalVisualReview/melantha_clip_matrix.csv) contém cada frame, PPU, pivô do sprite, dimensão, versão e estado de verificação em Play Mode. O pivô da matriz é normalizado (0–1); nos antigos `Multiple`, vem do SpriteRect, e nos R3 `Single`, do `spritePivot`.

**Mudança desde a auditoria de 24/09:** aquela registrava 626 PNG e `U2_DialogueUI.png`/`U3_PauseEndUI.png`. A leitura atual encontrou 623 PNG: U2 e U3 não estão em `FinalProduction/Sheets/`; o terceiro arquivo ausente não foi inferido apenas pela diferença de contagem. O GUID `f60f66050791d9242b7e891f6d8aa05d`, usado por seis referências nos prefabs UI, **não possui `.meta` correspondente no disco**. O Editor ainda retorna `Assets/Art/FinalProduction/Sheets/U2_DialogueUI.png` para esse GUID e mantém o Texture2D carregado, mas `File.Exists` para o PNG e seu `.meta` retornou `False`. Assim, a sessão atual pode mascarar a ausência persistente; reabrir o projeto exige verificação específica. Não restaurar por suposição; verificar a intenção visual e a origem desse arquivo na etapa de integração.

## Decisões prioritárias

| CATEGORY | CANONICAL | CURRENTLY_USED | ALTERNATIVES | STATUS / ACTION |
|---|---|---|---|---|
| MainMenu Logo | `FinalProduction/Sheets/U1_Logo.png` **aprovada pela Callisto** | `MainMenu.unity` aponta para U1 | `UI/MainMenu/01_menu_logo_beta.png` aparece em `FinalBeta.unity` | KEEP U1 no menu; RELINK logo beta no FinalBeta se aquele objeto representa o título principal. Não substituir outros ornamentos. |
| Poe | Conjunto atual `Characters/Poe/` e sheet `FinalProduction/Sheets/P1_PoeLocomotion_Runtime.png` | Clips, prefab e cenas usam o conjunto atual | Nenhuma versão nova solicitada | KEEP; aprovado. |
| Andrealphus | Conjunto atual `Characters/Andrealphus/` | 30 quadros antigos em clips e cenas, conforme auditoria prévia | Sheets de produção são fonte/runtime auxiliares | KEEP; aprovado. |
| Portraits | `FinalProduction/Sheets/C1_Portraits_Pixel.png` **candidato atual**; aprovação de expressões específicas a confirmar | Sheet referenciada nas cenas; `DialogueUI.prefab/PortraitImage` é slot nulo, preenchido em runtime | `Characters/Portraits/` só tem subpastas `.meta`, sem PNG; `UI/Gameplay/12_portrait_frame.png` é moldura, não portrait | KEEP sheet; REVIEW mapeamento de personagem/expressão e frame. |

## Melantha — matriz de estados

`Assets/Art/Animations/Melantha/Melantha.controller` contém os 18 clips abaixo. A coluna de quadros é o número de amostras/PNG únicos no clip. Caminhos integrais, pivôs de cada sprite e dimensões constam da matriz CSV.

| Estado / clip `melantha_*.anim` | Quadros | Sprites utilizados | PPU e dimensão de arquivo | Versão / classificação | Play Mode |
|---|---:|---|---|---|---|
| idle_back | 1/1 | `FinalProduction/R3/Melantha/idle_back.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL para locomoção | Não verificado nesta rodada |
| idle_front | 1/1 | `FinalProduction/R3/Melantha/idle_front.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL | **Sim**, auditoria de 24/09 |
| idle_side | 1/1 | `FinalProduction/R3/Melantha/idle_side.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL | Não verificado |
| walk_back | 6/6 | `FinalProduction/R3/Melantha/walk_back_0..5.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL | Não verificado |
| walk_front | 6/6 | `FinalProduction/R3/Melantha/walk_front_0..5.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL | Não verificado |
| walk_side | 6/6 | `FinalProduction/R3/Melantha/walk_side_0..5.png` | 32; 64×64 | R3 / MELANTHA_CANONICAL | Não verificado |
| interact_back | 4/3 | `Characters/Melantha/Interact/melantha_interact_back_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| interact_front | 4/3 | `Characters/Melantha/Interact/melantha_interact_front_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| interact_side | 4/3 | `Characters/Melantha/Interact/melantha_interact_side_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| examine_object | 4/3 | `Characters/Melantha/Narrative/melantha_examine_object_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| pensive | 4/3 | `Characters/Melantha/Narrative/melantha_pensive_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| read_grimoire | 5/4 | `Characters/Melantha/Narrative/melantha_read_grimoire_01..04.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| ritual_cast | 5/4 | `Characters/Melantha/Narrative/melantha_ritual_cast_01..04.png` | 43.4717; 80×80 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| startled | 4/3 | `Characters/Melantha/Narrative/melantha_startled_01..03.png` | 35.018867; 64×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| dream_fall | 4/3 | `Characters/Melantha/Cutscenes/melantha_dream_fall_01..03.png` | 51.92453; 96×96 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| lie_down | 5/4 | `Characters/Melantha/Cutscenes/melantha_lie_down_01..04.png` | 43.4717; 96×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| sleep | 3/2 | `Characters/Melantha/Cutscenes/melantha_sleep_01..02.png` | 51.92453; 96×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |
| wake_up | 5/4 | `Characters/Melantha/Cutscenes/melantha_wake_up_01..04.png` | 51.92453; 96×64 | antiga / MELANTHA_ACTION_NEEDS_RELINK | Não verificado |

**Pickup e reaction:** não há clips com esses nomes. `examine_object`, `pensive` e `startled` são os candidatos funcionais atuais; atribuição narrativa exata exige verificar os gatilhos. `FinalProduction/R3/Melantha/interact.png`, `ritual.png` e `surprise.png` são três alternativas R3 **sem referência GUID direta**; têm PPU 32 e pivô de base, mas são poses isoladas, não substituem automaticamente clips de 3–4 quadros. Não declarar ação R3 canônica sem aprovação visual de cada sequência. `FinalProduction/R2/M1_Walk_*` (12 PNG) está sem referência direta nos clips atuais e é MELANTHA_OLD candidata. `FinalProduction/Sheets/M1_MelanthaLocomotion_Source.png` é SOURCE; `_Runtime.png` é sheet runtime referenciada em cenas, mas os clips idle/walk atuais usam PNG individuais R3. `M2_MelanthaActions_Runtime.png` é sheet runtime disponível, sem relink comprovado aos 12 clips. Os 20 PNG antigos Melantha sem GUID direto são MELANTHA_UNUSED **apenas no índice estático**.

## UI — comparação de composição

| CATEGORY | CANONICAL recomendado | CURRENTLY_USED | ALTERNATIVES / STATUS / ACTION |
|---|---|---|---|
| MAIN MENU | U1 para logo; `UI/MainMenu/02_button_frame_normal.png`, `05_menu_selector_diamond.png`, `06_menu_moon_emblem.png`, `08_menu_center_sigil.png`, `09_menu_corner_ornament.png` para elementos separados | Esses arquivos aparecem em `MainMenu.unity`; U1 tem duas referências diretas no índice | KEEP; `01_menu_logo_beta.png` é variante antiga do título, ainda em `FinalBeta.unity`. |
| PAUSE | `UI/Screens/21_pause_panel.png` e `22_pause_title_ornament.png` são candidatos visuais | `PauseUI.prefab` usa `UI/MainMenu/07_menu_divider_ornament.png`, moldura de botão e sigilo; sem referência direta ao painel 21 | REVIEW composição e RELINK após aprovação; a sheet U3 citada antes não está no disco atual. |
| DIALOGUE | `UI/Gameplay/11_dialogue_box.png` tem **cartela de nome desenhada no alto à esquerda**; `12_portrait_frame.png` e `17/18_dialogue_cursor_*.png` | `DialogueUI.prefab` usa cursor 17; `DialogueBox` e `PortraitFrame` apontam para GUID `f60...`, cujo PNG não existe no disco. `PortraitImage` está vazio para preenchimento runtime; `CharacterName_TMP` em (310,-45), `DialogueText_TMP` em (310,-108), caixa 1560×260 | RELINK painel e moldura para 11/12 após aprovação de layout. O espaço correto para o nome é no PNG 11; testar alinhamento de texto à cartela. |
| OBJECTIVE | `UI/Gameplay/13_objective_panel.png` | `ObjectiveUI.prefab/ObjectivePanel` aponta para GUID ausente `f60...`; âncora superior direita (1,1), posição (-50,-45), tamanho 430×125; título e texto têm RectTransforms próprios | RELINK ao 13 e testar legibilidade, margens e posição em 1920×1080 e 1280×720. |
| INTERACTION | `UI/Gameplay/14_interaction_prompt_bg.png`, `15_keycap_E.png`, `16_interaction_hand_icon.png` | `InteractionPromptUI.prefab` usa 15/16, mas `PromptBackground` aponta para GUID ausente `f60...`; fundo 360×62, ícone em x=84, keycap em x=26, texto em x=122 | RELINK fundo ao 14. Possível causa visual do prompt incorreto: imagem de fundo sem asset válido; verificar também se `InteractionDetector` recebe candidato e `CanvasGroup.alpha` vira 1 em Play Mode. |
| FINAL / END | `UI/Screens/25_final_text_panel.png`, `27_final_fragment_large.png`; U1 para título principal, se exibido | `FinalBeta.unity` usa 25/27 e `UI/MainMenu/01_menu_logo_beta.png` | RELINK só o título beta para U1, após confirmar que é o objeto de título; demais painéis KEEP. |
| BUTTONS / FRAMES / ORNAMENTS / ICONS | Famílias `UI/MainMenu/02..10`, `UI/Gameplay/12`, `UI/Screens/22/24/26/29/30` | Vários componentes estão ligados; veja CSV | KEEP onde há composição aprovada; REVIEW os não conectados. `UI/MainMenu/06_menu_moon_emblem.png` e `UI/Screens/26_final_center_emblem.png` têm PNG idêntico, mas papéis diferentes. |

O `GameplayUI.prefab` também carrega overrides com o GUID ausente do painel de objetivo e do prompt. Na etapa de correção, atualizar prefab base **e** overrides; corrigir apenas a base pode não mudar a instância em cena. O código atual de `InteractionPromptUI.SetTarget` oculta o CanvasGroup sem alvo e escreve o verbo quando há alvo; esta auditoria não conclui que o GUID ausente seja a única causa de falha visível.

## Quarto

| CATEGORY | CANONICAL recomendado | CURRENTLY_USED | ALTERNATIVES | STATUS / ACTION |
|---|---|---|---|---|
| FLOOR / WALL | `FinalProduction/Tiles/Q1_RoomTiles32.png` é base runtime técnica atual | Referenciado por tiles em `Quarto` e `FinalBeta` | `Sprites/Quarto/01_Tiles/01_tile_chao_madeira.png`, `02_segmento_parede_gotica.png`, `03_canto_parede_gotica.png`; `FinalProduction/Sheets/Q1_RoomArchitecture.png` é fonte, não tile runtime | REVIEW artístico da parede com Callisto. Q1 está conectado, mas a aprovação estética foi negada no pedido; não marcar como canônico final. |
| WINDOW | `FinalProduction/R2/Q2_Window_States/window_open.png` e `window_secured.png` são estados técnicos atuais | Ambos no Quarto; aberto também no FinalBeta | `Sprites/Quarto/02_Arquitetura/01_janela_com_cortinas.png` (64×64, PPU 100) | REVIEW artístico/escala; a alternativa antiga é menor e tem PPU diferente, então não presumir que corrige a composição. |
| DOOR | `Sprites/Quarto/02_Arquitetura/02_entrada_porta_inferior.png` candidata de arquitetura | GUID direto em assets, mas não detectado como imagem diretamente em cena no índice | Q1 contém trecho de porta | REVIEW montagem. |
| FURNITURE | `FinalProduction/R2/Q2_Bed/bed.png` e `Q2_Furniture/*` | Cama e cinco dos seis móveis R2 ligados a cenas | `Sprites/Quarto/03_Moveis/*` (7 PNG, todos com alguma referência em Assets) | KEEP montados; REVIEW equivalências caso a composição esteja errada. |
| PROPS / RITUAL / DECOR | `FinalProduction/R2/Q3_Props_States/*`, `Q4_Ritual_States/*` para estados atuais | 5/12 props Q3 e 4/8 R4 ligados às cenas | `Sprites/Quarto/04_Decoracao_Props/*`, `05_Ritual/*`, `06_Gameplay_States/*` | KEEP conectados; REVIEW 16 PNG de Quarto sem vínculo GUID direto antes de qualquer descarte. |

**Parede canônica final e janela canônica final: aguardam escolha visual.** O relatório anterior confirmou existência física, mas o pedido atual registra que a aparência usada está errada. A prancha 06 compara alternativas reais; ainda falta referência visual aprovada e inspeção do enquadramento completo no jogo.

## Labirinto — 89 sprites em `Sprites/Labirinto` (22 com GUID direto; 67 sem)

| CATEGORY | CANONICAL candidato | CURRENTLY_USED | ALTERNATIVES | NEEDS_RELINK |
|---|---|---|---|---|
| FLOORS / WALLS / CORNERS | `FinalProduction/Sheets/L1_LabyrinthTiles_Runtime.png` e tiles individuais `FinalProduction/R3/LabyrinthTiles/*` | Sheet em Tile assets; 12/16 PNG R3 diretamente na cena | `Sprites/Labirinto/01_Tiles_Base/Floors/*`, `Walls/*`, `Corners/*` | REVIEW composição; não substituir tilemap só por idade |
| PILLARS / ARCHES / DOORS | `FinalProduction/R3/Decoration/*` para decoração R3; porta falsa R2 abaixo | Parte da decoração R3 e 2/13 arquitetura antiga em cena | `Sprites/Labirinto/02_Arquitetura/31..37*`, tile `30_wall_pillar_tile.png` | REVIEW |
| BRAZIERS / STATUES / CHAINS / BANNERS / RUBBLE / DECOR | `FinalProduction/R2/L3_Details_States/*` e `R3/Decoration/*` como candidatos conectados | 3/8 R2 L3 e 14/16 R3 Decoration com vínculo | `Sprites/Labirinto/03_Props/44..52*`, `02_Arquitetura/36..43*`, pisos rubble 08/09 | REVIEW variantes não montadas; props antigos 0/9 com GUID direto |
| FALSE DOOR | `FinalProduction/R2/L3_FalseDoor_States/false_door_closed/open.png` | Ambos na cena | `Sprites/Labirinto/06_Gameplay_States/01_false_door_closed.png`, `02_false_door_open.png` | KEEP R2 até comparação de estado completo |
| VOICE WELL / ECHO CORRIDOR | `FinalProduction/R2/L3_Details_States/*` + estados de `Sprites/Labirinto/06_Gameplay_States/09_well_energy_*.png` | Estados de energia referenciados na cena; poço antigo `03_Props/48_well_stone.png` sem GUID direto | `Sprites/Labirinto/03_Props/48_well_stone.png`, `04_Decals_Sigils/*` | REVIEW poço/corredor in situ |
| ARCANA / PORTALS | `Sprites/Labirinto/04_Decals_Sigils/*` e `06_Gameplay_States/*` conforme estado | 3/13 decals e 12/18 estados com vínculo de cena | Domain icons 57..63 e selos 13..18 | REVIEW iconografia não conectada |
| VFX | `Sprites/Labirinto/05_FX_Flames/*` é conjunto disponível; alternativa específica a escolher | 0/6 GUID direto nas chamas | `Sprites/VFX/*` (46 sem GUID direto) | REVIEW; não considerar efeitos aprovados sem Play Mode |

**Total comprovado de “bons nunca usados”: indeterminado.** São **67 PNG sem referência GUID direta**, não 67 aprovados ou inutilizados. A prancha 07 mostra exemplos das linhas R3, R2 e antigas. A próxima remontagem deve partir de inspeção do mapa inteiro e dos estados do puzzle.

## Domínio da Lua — 106 sprites em `Sprites/DominioDaLua` (20 com GUID direto; 86 sem)

| CATEGORY | CANONICAL candidato | CURRENTLY_USED | ALTERNATIVES | NEEDS_RELINK |
|---|---|---|---|---|
| FLOOR / WALL / CORNERS | `FinalProduction/Sheets/D1_MoonTiles_Runtime.png` + `FinalProduction/R3/MoonTiles/*` | Sheet em Tile assets; 12/16 R3 diretamente na cena | `Sprites/DominioDaLua/01_Tiles_Base/*` (12 PNG) | REVIEW composição |
| ARCHITECTURE / CRYSTALS / DECOR | `FinalProduction/R3/Decoration/*` e `R2/D5_D6_MoonProps/crystals.png` | R3 e oito props R2 ligados na cena | `Sprites/DominioDaLua/02_Arquitetura/*`, `05_Props_Interativos/43..45*` | REVIEW variações não usadas |
| GARDEN / FLOWERS / PETALS | `FinalProduction/R2/D2_Garden_States/*` como estados atuais | 6/8 R2 diretamente na cena | `Sprites/DominioDaLua/07_Puzzle_Flores/*` (só 1/14 com GUID direto), sheet `D2_LunarGarden_Runtime.png` | REVIEW cobertura dos estados, não assumir que toda pétala é obsoleta |
| MIRRORS / BLACK MIRROR | `FinalProduction/R2/D3_Mirror_Modules/*`, `D4_BlackMirror_States/*` | 1/6 módulos D3 e 4/4 D4 diretamente na cena | `Sprites/DominioDaLua/03_Puzzle_Espelhos/*` (0/8), `08_Puzzle_Espelhos/*` (6/19), `D3_Mirrors_Runtime.png` | REVIEW reflexos e rachaduras em todos os estados |
| SIGIL / FRAGMENT / PEDESTALS | `FinalProduction/R2/D5_D6_MoonProps/*` + estados conectados do conjunto antigo | R2 8/8; `04_Puzzle_Sigilo` 4/10; `05_Props_Interativos` 2/10 | `Sprites/DominioDaLua/09_Gameplay_States/*` (0/17) | REVIEW necessidade de relink por puzzle |
| PORTALS / VFX | `Sprites/DominioDaLua/02_Arquitetura/18_portal_frame_final.png` e estados/fx específicos, sujeitos a aprovação | Duas arquiteturas antigas com vínculo; `06_FX` 0/8 GUID direto | `Sprites/VFX/*` | REVIEW cena e efeitos; não afirmar ausência em runtime |

**Total comprovado de “bons nunca usados”: indeterminado.** São **86 PNG sem referência GUID direta**; os 19 do espelho, 14 das flores e 17 de gameplay merecem comparação de estados antes de definir canonização. A prancha 08 mostra alternativas R2/R3/antigas.

## Tipos e duplicações

| Tipo | Exemplo | Regra proposta |
|---|---|---|
| SOURCE | `FinalProduction/Sheets/M1_MelanthaLocomotion_Source.png`; fontes em `ArtStaging` | Preservar para edição, não ligar diretamente só por aparência. |
| RUNTIME SHEET | `M1_MelanthaLocomotion_Runtime.png`, `L1_LabyrinthTiles_Runtime.png`, `D1_MoonTiles_Runtime.png` | Usar só onde subassets/tiles existentes exigem. |
| INDIVIDUAL SPRITE / TILE | R3 Melantha, R2 props, `Sprites/*` | Preferir vínculo por GUID válido e escala/pivô verificados. |
| UI SPRITE / PORTRAIT / VFX | `UI/*`, `C1_Portraits_Pixel.png`, `Sprites/VFX/*` | Avaliar por função; UI em PPU 100 e mundo em PPU 32 não são equivalentes diretos. |

Um par de PNG com SHA-256 idêntico já identificado: `UI/MainMenu/06_menu_moon_emblem.png` e `UI/Screens/26_final_center_emblem.png`; funções diferentes, nenhuma exclusão recomendada. Sheets source/runtime diferem em finalidade e dimensões. `FinalProduction` contém versões antigas, fontes e runtime; não é uma classificação canônica automática.

## Estrutura futura proposta — **não aplicada**

```text
Assets/Art/
  Characters/{Melantha,Poe,Andrealphus,Portraits}/
  Environment/{Quarto,Labirinto,DominioLua}/
  UI/{MainMenu,Pause,Dialogue,Objective,Interaction,Final}/
  VFX/
  Sources/  (opcional; preservar IDs e proveniência)
```

Qualquer reorganização posterior deverá mover PNG **junto com `.meta`**, preservar GUID e validar clips, Tile assets, cenas, prefabs e carregamento por caminho. A aprovação da Callisto deve resolver primeiro: ações Melantha R3 versus clips antigos; parede/janela do Quarto; painel UI 11/13/14; ícones/estados de Labirinto e Domínio; e a restauração ou substituição deliberada do GUID UI ausente.

## Pranchas de comparação

1. [01 Melantha](../Evidencias/CanonicalVisualReview/01_Melantha_Comparison.png)
2. [02 Portraits](../Evidencias/CanonicalVisualReview/02_Portraits_Comparison.png)
3. [03 MainMenu](../Evidencias/CanonicalVisualReview/03_UI_MainMenu_Comparison.png)
4. [04 Dialogue](../Evidencias/CanonicalVisualReview/04_UI_Dialogue_Comparison.png)
5. [05 Objective e Interaction](../Evidencias/CanonicalVisualReview/05_UI_Objective_Interaction_Comparison.png)
6. [06 Quarto parede e janela](../Evidencias/CanonicalVisualReview/06_Quarto_Wall_Window_Comparison.png)
7. [07 Labirinto](../Evidencias/CanonicalVisualReview/07_Labyrinth_Assets_Comparison.png)
8. [08 Domínio da Lua](../Evidencias/CanonicalVisualReview/08_MoonDomain_Assets_Comparison.png)

Cada item mostrado tem ID `A###`, que aponta para o índice CSV. As pranchas usam os PNG existentes em escala de visualização; não são novos assets do jogo. **Arquivos apagados: 0. Movidos: 0. Assets gerados: 0.**
