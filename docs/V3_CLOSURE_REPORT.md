# Fechamento V3 — evidências de integração

## Escopo e preservação

Trabalho no projeto existente, com alterações locais preservadas. Não houve commit, push ou publicação. Bíblia narrativa V3 e dez referências visuais fornecidas pela autora são a base desta integração. O projeto continua com três níveis jogáveis (Quarto, Labirinto, DominioLua), mais MainMenu e FinalBeta.

## Implementado nesta integração

- Quarto: reposicionamento dos props oficiais, Tilemaps de piso e colisão, pontos de interação e bloqueios de móveis; ritual distingue velas apagadas/acesas.
- Labirinto e Lua: correção de importação de 56 texturas lunares, preenchimento dos pisos, bordas com colisão, props e portais. Desativados os puzzles do Labirinto duplicados na Lua.
- Objetos decorativos dos mapas usam prefixo `Art_`, evitando colisões com nomes dos objetos interativos.
- Melantha, Poe e Andrealphus: sprites e controladores de animação existentes vinculados às cenas.
- Diálogo: escala da interface corrigida, texto ampliado, retratos obtidos por recorte nativo de sprites existentes. Não foram gerados PNGs novos.
- Ecos: o prompt mostra a voz da rodada atual, usando as frases da Bíblia; índices corretos 2, 1, 3 preservados.
- Minguante: erro reinicia apenas sua sequência, preservando as demais pétalas.
- Espelhos: diálogo de solução, memória negra e segundo encontro com Andrealphus executam em sequência; a flag do encontro só é marcada ao terminar.
- FinalBeta: reutiliza a composição do Quarto, com ritual apagado, fragmento físico e sequência narrativa. Cartão final com créditos, retorno ao menu e saída.
- Objetivos: a instância persistente recebe o painel da cena sem destruir sua apresentação. Pausa controla também o fundo escurecido.

## Rastreabilidade narrativa

| Elemento | Gatilho | Estado / dependência |
|---|---|---|
| Ritual do Quarto | itens essenciais examinados e operação concluída | `RitualCompleted`; pote/brinquedo opcionais |
| Ecos | escolha correta na rodada | `EchoTrial01Complete` → `EchoTrial02Complete` → `EchoTrial03Complete` |
| Jardim | três pétalas | `GardenComplete`; erro Minguante não apaga Crescente/Cheia |
| Espelhos | Delayed, Ahead, Room | `MirrorPuzzleComplete`; segue memória negra e `DLG_D_ANDREALPHUS_02` |
| Segundo encontro | término dos diálogos encadeados | `AndrealphusMeeting02Complete` |
| FinalBeta | fragmento coletado e transição | `DLG_F_WAKE`, `DLG_F_ROOM_AFTER`, `DLG_F_FRAGMENT_CHECK`, `DLG_F_POE_SOUND`, `DLG_F_END_CARD` |

## Assets

- **KEEP:** arte oficial em `Assets/Art`, sprites individuais, clips e controllers existentes, diálogos e referências válidas.
- **REPLACE:** composição espaçada do Quarto, imports lunares inválidos e renderização de blocos/labels antigos nas cenas.
- **LEGACY:** objetos antigos desativados e arquivos preexistentes mantidos; nenhum pacote foi removido.
- **NOVOS:** Tile assets com sub-sprites nativos em `Assets/Art/Tiles/V3Closure`; prefab `Assets/Prefabs/Environment/QuartoV3Composition.prefab`. Retratos são recortes dos sprites de corpo existentes, não retratos ilustrados novos.
- **DEPENDÊNCIA VISUAL:** aprovação artística frente às referências e acabamento de retratos/ornamentação continuam distintos dos testes funcionais.

## Verificação

- EditMode: **25/25**, job `afa5b3d456fb`, 2026-09-16 00:34 UTC.
- PlayMode: **12/12**, job `f8dda1c256ee`, 2026-09-16 00:35 UTC.
- CT intermediário: **11/11**, `TestResults/ct-v3-closure-r2.json`. Inclui percurso físico de 32 pontos no Jardim sem bloqueio.
- O CT usa os serviços de cenas e interações reais, mas também reposiciona o jogador em partes do percurso. Não equivale a um playthrough humano integral.
- Áudio: 20 WAVs legíveis, todos com duração positiva; isso não substitui aprovação auditiva da mixagem.
- Capturas: `TestResults/v3-menu.png`, `v3-dialogue.png`, `v3-pause.png`; a captura da pausa antecede a correção do dimmer.
- CT ampliado para exigir conclusão de todos os diálogos de FinalBeta antes de retornar ao menu: 11/11 em `TestResults/ct-v3-final-editor.json`.
- Retomada funcional de 16/09/2026: CT repetido no Editor 11/11; build Windows separado gerado e executado com CT 11/11 e saída 0. Ver `docs/V3_FUNCTIONAL_VALIDATION.md` para evidências, avisos e limites de cobertura.

## Arquivos principais desta integração

`Assets/Editor/V3VisualClosureEditor.cs`, `Assets/Editor/V3EndCardEditor.cs`, cenas Quarto/Labirinto/DominioLua/FinalBeta, `BlackMirrorSequenceController.cs`, `PuzzleMirror.cs`, `GardenPetalPuzzle.cs`, `MoonDomainNarrativeController.cs`, `RoomRitualVisual.cs`, `FinalBetaController.cs`, `ObjectiveToastController.cs`, `PauseMenuController.cs`, `DialogueManager.cs`, `EchoCorridorPuzzle.cs`, `EchoPassageChoice.cs`, `CtEvidenceRunner.cs`, `PuzzlePlayModeTests.cs`, além dos assets e imports descritos acima. O status Git contém também muitas alterações anteriores a esta integração.
