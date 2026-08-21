# NARRATIVE IMPLEMENTATION PLAN — C2/C4D STATUS

Plano original de 45 entradas, atualizado após C4A, C4B e C4C. Esta atualização apenas classifica implementação, dependências visuais e adiamentos.

## Resumo

| Categoria | Quantidade | Status |
|---|---:|---|
| TOTAL_PLANNED_NEW_ENTRIES | 45 | baseline C2 |
| PLANNED_ENTRIES_IMPLEMENTED | 32 | IMPLEMENTED |
| INDEPENDENT_ENTRIES_IMPLEMENTED | 32 | IMPLEMENTED |
| VISUAL_DEPENDENT_PENDING | 9 | VISUAL_DEPENDENT_PENDING |
| OPTIONAL_DEFERRED | 3 | OPTIONAL_DEFERRED |
| DROPPED_WITH_REASON | 1 | L01 absorvido por entrada existente |

## Implemented

- C4B/Labirinto: `L02,L06,L07,L08,L12,L13,L15`.
- C4C/DominioLua: `D01,D04,D05,D06,D07,D08,D09,D10,D11,D12,D13,D15,D16,D17,D18,D20,D21,D22`.
- C4A/FinalBeta: `F01,F02,F03,F05,F06,F07,F08`.
- `L01` foi coberto por `DLG_LABYRINTH_WAKE`; não foi duplicado.

## Visual-dependent pending

`L04,L05,L10,L14,D02,D03,D14,D19,F04`.

Esses IDs aguardam prop final, posição final, animação final ou hook visual. `F04` é a pendência de encerramento; o restante de FinalBeta está suficiente.

## Optional deferred

`L03,L09,L11`.

São reações/interações opcionais. Não bloqueiam a leitura da vertical slice e não devem ser adicionadas por contagem.

## Matriz de puzzles após C4D

| Puzzle | INTRO | HINT | WRONG_INPUT | PARTIAL_PROGRESS | SUCCESS |
|---|---|---|---|---|---|
| Jardim | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED |
| Espelhos | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED |
| Corredor Ilusório | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED |
| Sigilo Fragmentado | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED | IMPLEMENTED |

Regras de gameplay, ordem dos puzzles e progressão persistente foram preservadas.

## Lista para C6

Reabrir somente os nove IDs `VISUAL_DEPENDENT_PENDING` no documento `NARRATIVE_CONTENT_AUDIT.md`, após a entrega visual correspondente. Não antecipar escrita, binding ou alteração de mapas.

`NEXT = C5_OBJECTIVE_UI_AUDIO_MENUS`
